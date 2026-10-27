using AltecSystem.Application.Commands.ElectronicInvoices;
using AltecSystem.Application.DTOs.ElectronicInvoices;
using AltecSystem.Application.Interfaces;
using AltecSystem.Application.Queries.ElectronicInvoices;
using AltecSystem.Infrastructure.Sri;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AltecSystem.Api.Controllers
{
    [Route("api/facturacion")]
    [ApiController]
    public class ElectronicInvoicesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IElectronicInvoiceRepository _repository;
        private readonly SriConfiguration _sriConfig;
        private readonly ILogger<ElectronicInvoicesController> _logger;

        public ElectronicInvoicesController(IMediator mediator, IElectronicInvoiceRepository repository,
            SriConfiguration sriConfig, ILogger<ElectronicInvoicesController> logger)
        {
            _mediator = mediator;
            _repository = repository;
            _sriConfig = sriConfig;
            _logger = logger;
        }

        /// <summary>Emite una factura electrónica: firma, envía al SRI y espera la autorización.</summary>
        [HttpPost("facturas")]
        public async Task<IActionResult> Create([FromBody] CreateElectronicInvoiceRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _mediator.Send(new CreateElectronicInvoiceCommand(request), ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al emitir la factura electrónica");
                return StatusCode(500, new { error = "Ocurrió un error inesperado al emitir la factura." });
            }
        }

        /// <summary>Reintenta el envío o la autorización de una factura pendiente.</summary>
        [HttpPost("facturas/{id:guid}/reintentar")]
        public async Task<IActionResult> Retry(Guid id, CancellationToken ct)
        {
            try
            {
                return Ok(await _mediator.Send(new RetryElectronicInvoiceCommand(id), ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reintentar la factura {Id}", id);
                return StatusCode(500, new { error = "Ocurrió un error inesperado al reintentar la factura." });
            }
        }

        /// <summary>Histórico de facturas con filtros (fecha, estado, texto) y paginación.</summary>
        [HttpGet("facturas")]
        public async Task<IActionResult> List([FromQuery] ElectronicInvoiceFilter filter, CancellationToken ct)
        {
            return Ok(await _mediator.Send(new ListElectronicInvoicesQuery(filter), ct));
        }

        /// <summary>Registra la anulación hecha en SRI en línea; opcionalmente cancela la venta y devuelve el stock.</summary>
        [HttpPost("facturas/{id:guid}/anular")]
        public async Task<IActionResult> Anular(Guid id, [FromBody] AnularElectronicInvoiceRequest? body, CancellationToken ct)
        {
            try
            {
                return Ok(await _mediator.Send(new AnularElectronicInvoiceCommand(id, body ?? new AnularElectronicInvoiceRequest()), ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("facturas/{id:guid}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetElectronicInvoiceQuery(id), ct);
            return result == null ? NotFound(new { error = "Factura no encontrada." }) : Ok(result);
        }

        /// <summary>Descarga el PDF (RIDE) de la factura.</summary>
        [HttpGet("facturas/{id:guid}/pdf")]
        public async Task<IActionResult> GetPdf(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetElectronicInvoicePdfQuery(id), ct);
            if (result == null) return NotFound(new { error = "Factura no encontrada." });

            return File(result.Value.Pdf, "application/pdf", $"Factura-{result.Value.NumeroFactura}.pdf");
        }

        /// <summary>Envía (o reenvía) la factura por correo al cliente o al correo indicado.</summary>
        [HttpPost("facturas/{id:guid}/enviar-email")]
        public async Task<IActionResult> SendEmail(Guid id, [FromBody] SendInvoiceEmailRequest? body, CancellationToken ct)
        {
            try
            {
                return Ok(await _mediator.Send(new SendElectronicInvoiceEmailCommand(id, body?.Email), ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>Descarga el XML autorizado (o el firmado, si aún no está autorizada).</summary>
        [HttpGet("facturas/{id:guid}/xml")]
        public async Task<IActionResult> GetXml(Guid id, CancellationToken ct)
        {
            var invoice = await _repository.GetAsync(id, ct);
            if (invoice == null) return NotFound(new { error = "Factura no encontrada." });

            var xml = invoice.XmlAutorizado ?? invoice.XmlFirmado;
            return File(System.Text.Encoding.UTF8.GetBytes(xml), "application/xml", $"{invoice.ClaveAcceso}.xml");
        }

        /// <summary>Datos de la factura y del emisor para imprimir el ticket térmico (80 mm).</summary>
        [HttpGet("facturas/{id:guid}/ticket")]
        public async Task<IActionResult> GetTicket(Guid id, CancellationToken ct)
        {
            var invoice = await _repository.GetAsync(id, ct);
            if (invoice == null) return NotFound(new { error = "Factura no encontrada." });
            var settings = await _repository.GetSettingsAsync(ct);

            return Ok(new
            {
                emisor = new
                {
                    settings.Ruc,
                    settings.RazonSocial,
                    settings.NombreComercial,
                    settings.DirMatriz,
                    settings.DirEstablecimiento,
                    settings.ObligadoContabilidad,
                    settings.ContribuyenteRimpe
                },
                invoice.NumeroFactura,
                invoice.Ambiente,
                invoice.Estado,
                invoice.ClaveAcceso,
                invoice.NumeroAutorizacion,
                invoice.FechaEmision,
                invoice.FechaAutorizacion,
                comprador = new
                {
                    identificacion = invoice.IdentificacionComprador,
                    razonSocial = invoice.RazonSocialComprador,
                    direccion = invoice.DireccionComprador,
                    email = invoice.EmailComprador,
                    telefono = invoice.TelefonoComprador
                },
                items = invoice.Items.OrderBy(i => i.Linea).Select(i => new
                {
                    codigo = i.CodigoPrincipal,
                    i.Descripcion,
                    i.Cantidad,
                    i.PrecioUnitario,
                    i.Descuento,
                    total = i.PrecioTotalSinImpuesto
                }),
                pagos = invoice.Payments.Select(p => new { p.FormaPago, p.Total }),
                invoice.TotalSinImpuestos,
                invoice.TotalDescuento,
                invoice.TotalIva,
                invoice.ImporteTotal
            });
        }

        /// <summary>Datos de un cliente ya facturado, para autocompletar el formulario del POS.</summary>
        [HttpGet("clientes/{identificacion}")]
        public async Task<IActionResult> GetCustomer(string identificacion, CancellationToken ct)
        {
            var customer = await _repository.GetCustomerAsync(identificacion.Trim(), ct);
            if (customer == null) return NotFound(new { error = "Cliente no registrado." });

            return Ok(new
            {
                customer.TipoIdentificacion,
                customer.Identificacion,
                customer.RazonSocial,
                customer.Direccion,
                customer.Email,
                customer.Telefono
            });
        }

        /// <summary>Estado de la configuración: ambiente, emisor, próximo número y vigencia de la firma.</summary>
        [HttpGet("configuracion")]
        public async Task<IActionResult> GetConfiguration(CancellationToken ct)
        {
            var settings = await _repository.GetSettingsAsync(ct);
            string? firma = null, firmaError = null;
            DateTime? firmaVence = null;
            try
            {
                var cert = _sriConfig.GetCertificate();
                firma = cert.GetNameInfo(System.Security.Cryptography.X509Certificates.X509NameType.SimpleName, false);
                firmaVence = cert.NotAfter;
            }
            catch (Exception ex)
            {
                firmaError = ex.Message;
            }

            return Ok(new
            {
                ambiente = _sriConfig.Ambiente == 2 ? "PRODUCCION" : "PRUEBAS",
                emisor = new { settings.Ruc, settings.RazonSocial, settings.NombreComercial, settings.ContribuyenteRimpe, settings.Estab, settings.PtoEmi },
                siguienteNumero = _sriConfig.Ambiente == 2 ? settings.SiguienteSecuencialProduccion : settings.SiguienteSecuencialPruebas,
                iva = new { settings.CodigoPorcentajeIva, settings.TarifaIva },
                firma = new { titular = firma, vence = firmaVence, error = firmaError }
            });
        }
    }
}

namespace AltecSystem.Api.Controllers
{
    public class SendInvoiceEmailRequest
    {
        public string? Email { get; set; }
    }
}
