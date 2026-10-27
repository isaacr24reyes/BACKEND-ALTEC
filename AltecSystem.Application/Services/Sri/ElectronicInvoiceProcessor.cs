using System.Text.Json;
using System.Text.RegularExpressions;
using AltecSystem.Application.DTOs.ElectronicInvoices;
using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;

namespace AltecSystem.Application.Services.Sri;

/// <summary>
/// Orquesta la emisión de una factura electrónica:
/// validar → reservar secuencial → XML → firmar → guardar → enviar al SRI → consultar autorización.
/// La factura queda guardada en cada paso, así un error de red se puede reintentar sin perderla.
/// </summary>
public class ElectronicInvoiceProcessor
{
    private const string ConsumidorFinal = "9999999999999";
    private const decimal MaxConsumidorFinal = 50m;
    private static readonly HashSet<string> FormasPagoValidas = new() { "01", "15", "16", "17", "18", "19", "20", "21" };
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private readonly IElectronicInvoiceRepository _repository;
    private readonly ISriConfiguration _config;
    private readonly ISriInvoiceXmlBuilder _xmlBuilder;
    private readonly ISriXmlSigner _signer;
    private readonly ISriWebServiceClient _sri;
    private readonly IInvoicePdfGenerator _pdf;
    private readonly IInvoiceEmailSender _email;

    public ElectronicInvoiceProcessor(IElectronicInvoiceRepository repository, ISriConfiguration config,
        ISriInvoiceXmlBuilder xmlBuilder, ISriXmlSigner signer, ISriWebServiceClient sri,
        IInvoicePdfGenerator pdf, IInvoiceEmailSender email)
    {
        _repository = repository;
        _config = config;
        _xmlBuilder = xmlBuilder;
        _signer = signer;
        _sri = sri;
        _pdf = pdf;
        _email = email;
    }

    public async Task<ElectronicInvoiceResultDto> CreateAsync(CreateElectronicInvoiceRequest request, CancellationToken ct)
    {
        var cliente = NormalizeCustomer(request.Cliente);
        var formaPago = (request.FormaPago ?? "01").Trim();
        if (!FormasPagoValidas.Contains(formaPago))
            throw new InvalidOperationException($"Forma de pago '{formaPago}' no válida.");
        if (request.Items == null || request.Items.Count == 0)
            throw new InvalidOperationException("La factura debe tener al menos un producto.");

        var settings = await _repository.GetSettingsAsync(ct);
        var ambiente = _config.Ambiente;
        var now = EcuadorNow();

        // ── Detalle y totales (IVA según la configuración del emisor; negocio popular = 0%) ──
        var items = new List<ElectronicInvoiceItem>();
        var linea = 0;
        foreach (var it in request.Items)
        {
            if (string.IsNullOrWhiteSpace(it.Descripcion))
                throw new InvalidOperationException("Todos los productos deben tener descripción.");
            if (it.Cantidad <= 0)
                throw new InvalidOperationException($"Cantidad inválida en '{it.Descripcion}'.");
            if (it.PrecioUnitario < 0 || it.Descuento < 0)
                throw new InvalidOperationException($"Precio o descuento inválido en '{it.Descripcion}'.");

            var codigo = it.Codigo;
            if (string.IsNullOrWhiteSpace(codigo) && it.ProductId.HasValue)
                codigo = (await _repository.GetProductAsync(it.ProductId.Value, ct))?.Codigo;

            var subtotal = Round2(it.Cantidad * it.PrecioUnitario - it.Descuento);
            if (subtotal < 0)
                throw new InvalidOperationException($"El descuento supera el valor de '{it.Descripcion}'.");

            items.Add(new ElectronicInvoiceItem
            {
                Id = Guid.NewGuid(),
                Linea = ++linea,
                ProductId = it.ProductId,
                CodigoPrincipal = string.IsNullOrWhiteSpace(codigo) ? "SN" : codigo.Trim(),
                Descripcion = it.Descripcion.Trim(),
                Cantidad = it.Cantidad,
                PrecioUnitario = it.PrecioUnitario,
                Descuento = Round2(it.Descuento),
                PrecioTotalSinImpuesto = subtotal,
                CodigoPorcentajeIva = settings.CodigoPorcentajeIva,
                TarifaIva = settings.TarifaIva,
                ValorIva = Round2(subtotal * settings.TarifaIva / 100m)
            });
        }

        var totalSinImpuestos = items.Sum(i => i.PrecioTotalSinImpuesto);
        var totalIva = items.Sum(i => i.ValorIva);
        var importeTotal = totalSinImpuestos + totalIva;

        if (cliente.Identificacion == ConsumidorFinal && importeTotal > MaxConsumidorFinal)
            throw new InvalidOperationException($"Para ventas mayores a ${MaxConsumidorFinal:0.00} el SRI exige los datos del cliente (no Consumidor Final).");

        // ── Cliente ──
        var customer = await _repository.UpsertCustomerAsync(new Customer
        {
            Id = Guid.NewGuid(),
            TipoIdentificacion = cliente.TipoIdentificacion!,
            Identificacion = cliente.Identificacion,
            RazonSocial = cliente.RazonSocial,
            Direccion = cliente.Direccion,
            Email = cliente.Email,
            Telefono = cliente.Telefono,
            CreatedAt = now,
            UpdatedAt = now
        }, ct);

        // ── Número y clave de acceso ──
        var secuencial = await _repository.ReserveSecuencialAsync(ambiente, ct);
        var fechaEmision = now.Date;
        var claveAcceso = SriAccessKey.Generate(fechaEmision, "01", settings.Ruc, ambiente, settings.Estab, settings.PtoEmi, secuencial);

        var invoice = new ElectronicInvoice
        {
            Id = Guid.NewGuid(),
            Ambiente = ambiente,
            Estab = settings.Estab,
            PtoEmi = settings.PtoEmi,
            Secuencial = secuencial,
            NumeroFactura = $"{settings.Estab}-{settings.PtoEmi}-{secuencial:D9}",
            ClaveAcceso = claveAcceso,
            FechaEmision = fechaEmision,
            CustomerId = customer.Id,
            TipoIdentificacionComprador = cliente.TipoIdentificacion!,
            IdentificacionComprador = cliente.Identificacion,
            RazonSocialComprador = cliente.RazonSocial,
            DireccionComprador = cliente.Direccion,
            EmailComprador = cliente.Email,
            TelefonoComprador = cliente.Telefono,
            TotalSinImpuestos = totalSinImpuestos,
            TotalDescuento = items.Sum(i => i.Descuento),
            TotalIva = totalIva,
            ImporteTotal = importeTotal,
            SaleInvoiceNumber = request.SaleInvoiceNumber,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = string.IsNullOrWhiteSpace(request.CreatedBy) ? "ADMIN" : request.CreatedBy.Trim(),
            Items = items,
            Payments = new List<ElectronicInvoicePayment>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    FormaPago = formaPago,
                    Total = importeTotal,
                    Plazo = 1,  // igual que el facturador del SRI para ventas de contado
                    UnidadTiempo = "Dias"
                }
            }
        };

        // ── XML firmado (se guarda antes de enviar para no perder la factura) ──
        invoice.XmlFirmado = _signer.Sign(_xmlBuilder.Build(invoice, settings));
        invoice.Estado = SriInvoiceStates.Firmada;

        await _repository.AddAsync(invoice, ct);
        await _repository.SaveChangesAsync(ct);

        await SendAndAuthorizeAsync(invoice, ct);
        await _repository.SaveChangesAsync(ct);

        await TrySendEmailAsync(invoice, settings, null, ct);
        await _repository.SaveChangesAsync(ct);

        return ToDto(invoice);
    }

    /// <summary>Reintenta el envío o la consulta de autorización de una factura no autorizada.</summary>
    public async Task<ElectronicInvoiceResultDto> RetryAsync(Guid id, CancellationToken ct)
    {
        var invoice = await _repository.GetAsync(id, ct)
            ?? throw new KeyNotFoundException("Factura no encontrada.");

        if (invoice.Estado is SriInvoiceStates.Firmada or SriInvoiceStates.ErrorEnvio or SriInvoiceStates.Recibida)
        {
            await SendAndAuthorizeAsync(invoice, ct);
            await _repository.SaveChangesAsync(ct);

            if (!invoice.EmailEnviado)
            {
                await TrySendEmailAsync(invoice, await _repository.GetSettingsAsync(ct), null, ct);
                await _repository.SaveChangesAsync(ct);
            }
        }

        return ToDto(invoice);
    }

    /// <summary>Histórico de facturas del ambiente configurado (o del indicado en el filtro).</summary>
    public Task<ElectronicInvoicePageDto> ListAsync(ElectronicInvoiceFilter filter, CancellationToken ct)
        => _repository.ListAsync(filter, filter.Ambiente is 1 or 2 ? filter.Ambiente.Value : _config.Ambiente, ct);

    /// <summary>
    /// Reintenta las facturas pendientes recientes (lo llama el servicio en segundo plano).
    /// Devuelve cuántas quedaron autorizadas.
    /// </summary>
    public async Task<int> RetryPendingAsync(TimeSpan maxAge, CancellationToken ct)
    {
        var ids = await _repository.GetPendingIdsAsync(_config.Ambiente, EcuadorNow() - maxAge, ct);
        var autorizadas = 0;
        foreach (var id in ids)
        {
            var result = await RetryAsync(id, ct);
            if (result.Autorizada) autorizadas++;
        }
        return autorizadas;
    }

    /// <summary>
    /// Registra la anulación hecha en SRI en línea (el SRI no ofrece un servicio para pedirla desde un sistema).
    /// Opcionalmente cancela la venta asociada y devuelve el stock, todo en un solo guardado.
    /// </summary>
    public async Task<ElectronicInvoiceResultDto> AnularAsync(Guid id, AnularElectronicInvoiceRequest request, CancellationToken ct)
    {
        var invoice = await _repository.GetAsync(id, ct)
            ?? throw new KeyNotFoundException("Factura no encontrada.");

        if (invoice.Estado == SriInvoiceStates.Anulada)
            throw new InvalidOperationException("La factura ya está anulada.");
        if (invoice.Estado != SriInvoiceStates.Autorizado)
            throw new InvalidOperationException("Solo se pueden anular facturas autorizadas por el SRI.");

        var motivo = request.Motivo?.Trim();
        if (motivo?.Length > 300) motivo = motivo[..300];

        if (request.CancelarVenta && !string.IsNullOrWhiteSpace(invoice.SaleInvoiceNumber))
            await _repository.CancelSaleAndRestoreStockAsync(invoice.SaleInvoiceNumber, ct);

        invoice.Estado = SriInvoiceStates.Anulada;
        invoice.FechaAnulacion = EcuadorNow();
        invoice.MotivoAnulacion = string.IsNullOrEmpty(motivo) ? null : motivo;
        Touch(invoice);

        await _repository.SaveChangesAsync(ct);
        return ToDto(invoice);
    }

    /// <summary>PDF (RIDE) de la factura.</summary>
    public async Task<(byte[] Pdf, string NumeroFactura)?> GetPdfAsync(Guid id, CancellationToken ct)
    {
        var invoice = await _repository.GetAsync(id, ct);
        if (invoice == null) return null;
        return (_pdf.Generate(invoice, await _repository.GetSettingsAsync(ct)), invoice.NumeroFactura);
    }

    /// <summary>Envía (o reenvía) la factura autorizada por correo. <paramref name="email"/> reemplaza al del cliente si se indica.</summary>
    public async Task<ElectronicInvoiceResultDto> SendEmailAsync(Guid id, string? email, CancellationToken ct)
    {
        var invoice = await _repository.GetAsync(id, ct)
            ?? throw new KeyNotFoundException("Factura no encontrada.");
        if (invoice.Estado != SriInvoiceStates.Autorizado)
            throw new InvalidOperationException("Solo se pueden enviar facturas autorizadas por el SRI.");
        if (!string.IsNullOrWhiteSpace(email) && !EmailRegex.IsMatch(email.Trim()))
            throw new InvalidOperationException("El correo indicado no es válido.");

        if (!await TrySendEmailAsync(invoice, await _repository.GetSettingsAsync(ct), email?.Trim(), ct))
            throw new InvalidOperationException(_lastEmailError ?? "No se pudo enviar el correo.");

        await _repository.SaveChangesAsync(ct);
        return ToDto(invoice);
    }

    private string? _lastEmailError;

    /// <summary>
    /// Envía PDF + XML autorizado al cliente, con copia oculta al correo del negocio.
    /// Un fallo de correo no invalida la factura: queda registrado y se puede reenviar.
    /// </summary>
    private async Task<bool> TrySendEmailAsync(ElectronicInvoice invoice, SriSettings settings, string? toOverride, CancellationToken ct)
    {
        _lastEmailError = null;
        if (invoice.Estado != SriInvoiceStates.Autorizado || invoice.XmlAutorizado == null)
            return false;

        var to = toOverride ?? invoice.EmailComprador;
        if (!_email.IsConfigured)
        {
            _lastEmailError = "El correo de envío no está configurado.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(to) && string.IsNullOrWhiteSpace(settings.EmailCopia))
        {
            _lastEmailError = "La factura no tiene correo de destino.";
            return false;
        }

        try
        {
            var pdf = _pdf.Generate(invoice, settings);
            await _email.SendInvoiceAsync(invoice, settings, to, settings.EmailCopia, pdf, invoice.XmlAutorizado, ct);
            invoice.EmailEnviado = true;
            Touch(invoice);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _lastEmailError = "No se pudo enviar el correo: " + ex.Message;
            var mensajes = ToDto(invoice).Mensajes;
            mensajes.Add(new SriMessageDto { Identificador = "EMAIL", Mensaje = _lastEmailError, Tipo = "ADVERTENCIA" });
            invoice.MensajesSri = JsonSerializer.Serialize(mensajes);
            Touch(invoice);
            return false;
        }
    }

    public async Task<ElectronicInvoiceResultDto?> GetAsync(Guid id, CancellationToken ct)
    {
        var invoice = await _repository.GetAsync(id, ct);
        return invoice == null ? null : ToDto(invoice);
    }

    private async Task SendAndAuthorizeAsync(ElectronicInvoice invoice, CancellationToken ct)
    {
        try
        {
            if (invoice.Estado is SriInvoiceStates.Firmada or SriInvoiceStates.ErrorEnvio)
            {
                var recepcion = await _sri.SendAsync(invoice.XmlFirmado, invoice.Ambiente, ct);
                SetMessages(invoice, recepcion.Mensajes);

                // 43 = clave de acceso registrada, 70 = en procesamiento: ya está en el SRI, solo falta autorizar
                var yaRecibida = recepcion.Mensajes.Any(m => m.Identificador is "43" or "70");
                if (recepcion.Estado == SriInvoiceStates.Recibida || yaRecibida)
                {
                    invoice.Estado = SriInvoiceStates.Recibida;
                }
                else
                {
                    invoice.Estado = SriInvoiceStates.Devuelta;
                    Touch(invoice);
                    return;
                }
            }

            if (invoice.Estado == SriInvoiceStates.Recibida)
            {
                // El SRI suele autorizar en pocos segundos
                for (var intento = 0; intento < 6; intento++)
                {
                    await Task.Delay(intento == 0 ? 1500 : 2500, ct);
                    var autorizacion = await _sri.AuthorizeAsync(invoice.ClaveAcceso, invoice.Ambiente, ct);

                    if (autorizacion.Estado == SriInvoiceStates.Autorizado)
                    {
                        invoice.Estado = SriInvoiceStates.Autorizado;
                        invoice.NumeroAutorizacion = autorizacion.NumeroAutorizacion ?? invoice.ClaveAcceso;
                        invoice.FechaAutorizacion = autorizacion.FechaAutorizacion;
                        invoice.XmlAutorizado = autorizacion.XmlAutorizacion;
                        SetMessages(invoice, autorizacion.Mensajes);
                        break;
                    }

                    if (autorizacion.Estado == SriInvoiceStates.NoAutorizado)
                    {
                        invoice.Estado = SriInvoiceStates.NoAutorizado;
                        SetMessages(invoice, autorizacion.Mensajes);
                        break;
                    }
                }
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Xml.XmlException)
        {
            // Sin conexión con el SRI: la factura queda firmada y se puede reintentar
            if (invoice.Estado == SriInvoiceStates.Firmada)
                invoice.Estado = SriInvoiceStates.ErrorEnvio;
            var mensaje = invoice.Estado == SriInvoiceStates.Recibida
                ? "El SRI está demorando en autorizar. La factura quedó recibida y se autorizará automáticamente en unos minutos."
                : "No hubo respuesta del SRI. La factura quedó guardada y se reenviará automáticamente en unos minutos.";
            SetMessages(invoice, new List<SriMessageDto> { new() { Identificador = "RED", Mensaje = mensaje, InformacionAdicional = ex.Message, Tipo = "ADVERTENCIA" } });
        }

        Touch(invoice);
    }

    // ── Validación del cliente ──

    private static InvoiceCustomerDto NormalizeCustomer(InvoiceCustomerDto? c)
    {
        if (c == null || string.IsNullOrWhiteSpace(c.Identificacion))
            throw new InvalidOperationException("Ingrese la cédula, RUC o pasaporte del cliente.");
        if (string.IsNullOrWhiteSpace(c.RazonSocial))
            throw new InvalidOperationException("Ingrese el nombre o razón social del cliente.");

        var id = c.Identificacion.Trim();
        var tipo = string.IsNullOrWhiteSpace(c.TipoIdentificacion) ? DetectType(id) : c.TipoIdentificacion.Trim();

        switch (tipo)
        {
            case "05" when !IsValidCedula(id):
                throw new InvalidOperationException("La cédula ingresada no es válida.");
            case "04" when !(id.Length == 13 && id.All(char.IsDigit) && id.EndsWith("001")):
                throw new InvalidOperationException("El RUC debe tener 13 dígitos y terminar en 001.");
            case "07" when id != ConsumidorFinal:
                throw new InvalidOperationException("Consumidor Final debe usar la identificación 9999999999999.");
        }

        var email = string.IsNullOrWhiteSpace(c.Email) ? null : c.Email.Trim();
        if (email != null && !EmailRegex.IsMatch(email))
            throw new InvalidOperationException("El correo del cliente no es válido.");

        return new InvoiceCustomerDto
        {
            TipoIdentificacion = tipo,
            Identificacion = id,
            RazonSocial = c.RazonSocial.Trim().ToUpperInvariant(),
            Direccion = string.IsNullOrWhiteSpace(c.Direccion) ? null : c.Direccion.Trim(),
            Email = email,
            Telefono = string.IsNullOrWhiteSpace(c.Telefono) ? null : c.Telefono.Trim()
        };
    }

    private static string DetectType(string id)
    {
        if (id == ConsumidorFinal) return "07";
        if (id.Length == 13 && id.All(char.IsDigit) && id.EndsWith("001")) return "04";
        if (id.Length == 10 && id.All(char.IsDigit)) return "05";
        return "06";
    }

    /// <summary>Cédula ecuatoriana: provincia 01-24 (o 30), tercer dígito &lt; 6 y verificador módulo 10.</summary>
    private static bool IsValidCedula(string id)
    {
        if (id.Length != 10 || !id.All(char.IsDigit)) return false;
        var provincia = int.Parse(id[..2]);
        if (provincia is < 1 or > 24 and not 30 || id[2] - '0' >= 6) return false;

        var sum = 0;
        for (var i = 0; i < 9; i++)
        {
            var v = (id[i] - '0') * (i % 2 == 0 ? 2 : 1);
            sum += v > 9 ? v - 9 : v;
        }
        var dv = (10 - sum % 10) % 10;
        return dv == id[9] - '0';
    }

    // ── Utilidades ──

    private static void SetMessages(ElectronicInvoice invoice, List<SriMessageDto> mensajes)
    {
        if (mensajes.Count > 0)
            invoice.MensajesSri = JsonSerializer.Serialize(mensajes);
    }

    private static void Touch(ElectronicInvoice invoice) => invoice.UpdatedAt = EcuadorNow();

    private static decimal Round2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

    private static DateTimeOffset EcuadorNow()
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
        return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);
    }

    public static ElectronicInvoiceResultDto ToDto(ElectronicInvoice i) => new()
    {
        Id = i.Id,
        NumeroFactura = i.NumeroFactura,
        ClaveAcceso = i.ClaveAcceso,
        Ambiente = i.Ambiente,
        Estado = i.Estado,
        NumeroAutorizacion = i.NumeroAutorizacion,
        FechaAutorizacion = i.FechaAutorizacion,
        FechaEmision = i.FechaEmision,
        RazonSocialComprador = i.RazonSocialComprador,
        IdentificacionComprador = i.IdentificacionComprador,
        ImporteTotal = i.ImporteTotal,
        EmailComprador = i.EmailComprador,
        EmailEnviado = i.EmailEnviado,
        FechaAnulacion = i.FechaAnulacion,
        MotivoAnulacion = i.MotivoAnulacion,
        Mensajes = string.IsNullOrWhiteSpace(i.MensajesSri)
            ? new List<SriMessageDto>()
            : JsonSerializer.Deserialize<List<SriMessageDto>>(i.MensajesSri) ?? new List<SriMessageDto>()
    };
}
