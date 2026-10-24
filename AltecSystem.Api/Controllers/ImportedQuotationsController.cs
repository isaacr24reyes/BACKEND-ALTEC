using AltecSystem.Application.Commands.ImportedQuotations;
using AltecSystem.Application.Queries.ImportedQuotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AltecSystem.Api.Controllers
{
    [Route("api/cotizaciones-importadas")]
    [ApiController]
    public class ImportedQuotationsController : ControllerBase
    {
        private const long MaxRequestBytes = 60L * 1024 * 1024; // margen sobre el máximo de 50 MB del archivo

        private readonly IMediator _mediator;
        private readonly ILogger<ImportedQuotationsController> _logger;

        public ImportedQuotationsController(IMediator mediator, ILogger<ImportedQuotationsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
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
                _logger.LogError(ex, "Error al listar cotizaciones importadas");
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
                _logger.LogError(ex, "Error al guardar la cotización importada '{Nombre}'", command.Nombre);
                return StatusCode(500, new { error = "Ocurrió un error inesperado al guardar la cotización." });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            try
            {
                var detail = await _mediator.Send(new GetImportedQuotationDetailQuery(id));
                if (detail == null)
                    return NotFound(new { error = "Cotización no encontrada." });

                return Ok(detail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el detalle de la cotización importada {Id}", id);
                return StatusCode(500, new { error = "Ocurrió un error inesperado." });
            }
        }

        [HttpGet("{id:guid}/archivo")]
        public async Task<IActionResult> Download(Guid id)
        {
            try
            {
                var file = await _mediator.Send(new GetImportedQuotationFileQuery(id));
                if (file == null)
                    return NotFound(new { error = "Cotización no encontrada." });

                return File(file.Archivo, file.ContentType, file.NombreArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar la cotización importada {Id}", id);
                return StatusCode(500, new { error = "Ocurrió un error inesperado." });
            }
        }
    }
}
