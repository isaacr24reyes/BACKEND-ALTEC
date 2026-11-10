using AltecSystem.Application.Commands.ImportedQuotations;
using AltecSystem.Application.Interfaces;
using AltecSystem.Application.Queries.ImportedQuotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AltecSystem.Api.Controllers
{
    [Route("api/cotizaciones-importadas")]
    [ApiController]
    public class ImportedQuotationsController : ControllerBase
    {
        private const long MaxRequestBytes = 60L * 1024 * 1024; // margen sobre el máximo de 50 MB del archivo

        private readonly IMediator _mediator;
        private readonly ILogger<ImportedQuotationsController> _logger;
        private readonly IQuotationImageExtractor _imageExtractor;
        private readonly IMemoryCache _cache;

        // Evita que varias fotos pedidas a la vez lean el mismo Excel grande en paralelo
        private static readonly SemaphoreSlim ImagesLock = new(1, 1);

        public ImportedQuotationsController(IMediator mediator, ILogger<ImportedQuotationsController> logger,
            IQuotationImageExtractor imageExtractor, IMemoryCache cache)
        {
            _mediator = mediator;
            _logger = logger;
            _imageExtractor = imageExtractor;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] string? buscar)
        {
            try
            {
                var result = await _mediator.Send(new GetImportedQuotationsQuery
                {
                    Desde = desde,
                    Hasta = hasta,
                    Buscar = buscar
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar importaciones");
                return StatusCode(500, new { error = "Ocurrió un error inesperado." });
            }
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MaxRequestBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
        public async Task<IActionResult> Upload([FromForm] UploadImportedQuotationCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar la importación '{Nombre}'", command.Nombre);
                return StatusCode(500, new { error = "Ocurrió un error inesperado al guardar la importación." });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            try
            {
                var detail = await _mediator.Send(new GetImportedQuotationDetailQuery(id));
                if (detail == null)
                    return NotFound(new { error = "Importación no encontrada." });

                return Ok(detail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el detalle de la importación {Id}", id);
                return StatusCode(500, new { error = "Ocurrió un error inesperado." });
            }
        }

        /// <summary>Guarda los costos extras de la importación (aduana, IVA, Alibaba, impuesto TC...).</summary>
        [HttpPut("{id:guid}/costos-extras")]
        public async Task<IActionResult> UpdateExtraCosts(Guid id, [FromBody] UpdateExtraCostsRequest body)
        {
            try
            {
                var ok = await _mediator.Send(new UpdateImportedQuotationExtraCostsCommand(id, body.CostosExtras, body.Nota));
                if (!ok)
                    return NotFound(new { error = "Importación no encontrada." });

                return Ok(await _mediator.Send(new GetImportedQuotationDetailQuery(id)));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar los costos extras de la importación {Id}", id);
                return StatusCode(500, new { error = "Ocurrió un error inesperado al guardar los costos extras." });
            }
        }

        /// <summary>Filas del Excel que tienen foto (para pedir cada una con /imagenes/{fila}).</summary>
        [HttpGet("{id:guid}/imagenes")]
        public async Task<IActionResult> GetImageRows(Guid id)
        {
            try
            {
                var images = await GetImagesAsync(id);
                if (images == null)
                    return NotFound(new { error = "Importación no encontrada." });
                _logger.LogDebug("Fotos de la importación {Id}: {Count} filas con foto", id, images.Count);
                return Ok(images.Keys.OrderBy(k => k));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer las fotos de la importación {Id}", id);
                return StatusCode(500, new { error = "No se pudieron leer las fotos del Excel." });
            }
        }

        /// <summary>Foto de una fila, tal como viene incrustada en el Excel.</summary>
        [HttpGet("{id:guid}/imagenes/{fila:int}")]
        public async Task<IActionResult> GetImage(Guid id, int fila)
        {
            try
            {
                var images = await GetImagesAsync(id);
                if (images == null || !images.TryGetValue(fila, out var image))
                {
                    _logger.LogDebug("Foto {Fila} de la importación {Id}: no existe", fila, id);
                    return NotFound();
                }

                _logger.LogDebug("Foto {Fila} de la importación {Id}: entregada ({Bytes} bytes, origen {Origin})",
                    fila, id, image.Bytes.Length, Request.Headers.Origin.ToString());
                // El Excel guardado no cambia: el navegador puede reutilizar la foto
                Response.Headers.CacheControl = "public, max-age=604800, immutable";
                // La misma foto se pide como <img> (sin Origin) y por fetch para el PDF (con Origin):
                // sin Vary el navegador reutiliza la copia sin cabeceras CORS y bloquea el fetch
                Response.Headers.Append("Vary", "Origin");
                return File(image.Bytes, image.ContentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer la foto {Fila} de la importación {Id}", fila, id);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Lee el Excel una sola vez y deja sus fotos en memoria unos minutos,
        /// para no descargar el archivo completo de la BD por cada foto.
        /// </summary>
        private async Task<Dictionary<int, QuotationImage>?> GetImagesAsync(Guid id)
        {
            var key = $"cotizacion-importada-fotos:{id}";
            if (_cache.TryGetValue(key, out Dictionary<int, QuotationImage>? cached))
                return cached;

            await ImagesLock.WaitAsync();
            try
            {
                // Otro pedido pudo haberlo leído mientras se esperaba
                if (_cache.TryGetValue(key, out cached))
                    return cached;

                var started = DateTime.UtcNow;
                var file = await _mediator.Send(new GetImportedQuotationFileQuery(id));
                if (file == null)
                    return null;

                var images = _imageExtractor.Extract(file.Archivo);
                _logger.LogInformation("Excel de la importación {Id} leído: {Count} fotos en {Ms} ms",
                    id, images.Count, (int)(DateTime.UtcNow - started).TotalMilliseconds);
                _cache.Set(key, images, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(10),
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                });
                return images;
            }
            finally
            {
                ImagesLock.Release();
            }
        }

        [HttpGet("{id:guid}/archivo")]
        public async Task<IActionResult> Download(Guid id)
        {
            try
            {
                var file = await _mediator.Send(new GetImportedQuotationFileQuery(id));
                if (file == null)
                    return NotFound(new { error = "Importación no encontrada." });

                return File(file.Archivo, file.ContentType, file.NombreArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar la importación {Id}", id);
                return StatusCode(500, new { error = "Ocurrió un error inesperado." });
            }
        }
    }
}
