using System.Globalization;
using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AltecSystem.Infrastructure.Sri;

/// <summary>
/// RIDE de la factura electrónica en PDF (A4), con la misma disposición que el RIDE del
/// facturador del SRI: logo y datos del emisor, cuadro de autorización con código de barras,
/// datos del comprador, detalle, información adicional, formas de pago y totales.
/// </summary>
public class RideInvoicePdfGenerator : IInvoicePdfGenerator
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private static readonly Lazy<byte[]> Logo = new(LoadLogo);

    private static readonly Dictionary<string, string> FormasPago = new()
    {
        ["01"] = "SIN UTILIZACION DEL SISTEMA FINANCIERO",
        ["15"] = "COMPENSACIÓN DE DEUDAS",
        ["16"] = "TARJETA DE DÉBITO",
        ["17"] = "DINERO ELECTRÓNICO",
        ["18"] = "TARJETA PREPAGO",
        ["19"] = "TARJETA DE CRÉDITO",
        ["20"] = "OTROS CON UTILIZACION DEL SISTEMA FINANCIERO",
        ["21"] = "ENDOSO DE TÍTULOS"
    };

    static RideInvoicePdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(ElectronicInvoice invoice, SriSettings settings)
    {
        return Document.Create(doc => doc.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(24);
            page.DefaultTextStyle(t => t.FontSize(8).FontColor(Colors.Black));

            page.Content().Column(col =>
            {
                col.Spacing(10);
                col.Item().Element(c => Header(c, invoice, settings));
                col.Item().Element(c => Buyer(c, invoice));
                col.Item().Element(c => Details(c, invoice));
                col.Item().Element(c => Footer(c, invoice));
            });
        })).GeneratePdf();
    }

    // ── Cabecera: emisor (izquierda) y autorización (derecha) ──
    private static void Header(IContainer container, ElectronicInvoice inv, SriSettings s)
    {
        container.Row(row =>
        {
            row.Spacing(10);

            row.RelativeItem().Column(left =>
            {
                left.Spacing(6);
                left.Item().Height(110).AlignCenter().AlignMiddle().Image(Logo.Value).FitArea();
                left.Item().Border(1).Padding(8).Column(e =>
                {
                    e.Spacing(5);
                    e.Item().Text(s.RazonSocial).FontSize(9);
                    if (!string.IsNullOrWhiteSpace(s.NombreComercial))
                        e.Item().Text(s.NombreComercial).FontSize(9);
                    e.Item().Row(r => { r.ConstantItem(62).Text("Dirección\nMatriz:"); r.RelativeItem().Text(s.DirMatriz).FontSize(7); });
                    e.Item().Row(r => { r.ConstantItem(62).Text("Dirección\nSucursal:"); r.RelativeItem().Text(s.DirEstablecimiento).FontSize(7); });
                    if (!string.IsNullOrWhiteSpace(s.ContribuyenteEspecial))
                        e.Item().Text($"Contribuyente Especial Nro: {s.ContribuyenteEspecial}");
                    e.Item().Row(r => { r.RelativeItem().Text("OBLIGADO A LLEVAR CONTABILIDAD"); r.ConstantItem(30).Text(s.ObligadoContabilidad); });
                    if (!string.IsNullOrWhiteSpace(s.AgenteRetencion))
                        e.Item().Text($"Agente de Retención Resolución No. {s.AgenteRetencion}");
                    if (!string.IsNullOrWhiteSpace(s.ContribuyenteRimpe))
                        e.Item().PaddingTop(4).Text(s.ContribuyenteRimpe);
                });
            });

            row.RelativeItem().Border(1).Padding(10).Column(a =>
            {
                a.Spacing(7);
                a.Item().Row(r => { r.ConstantItem(55).Text("R.U.C.:").FontSize(11); r.RelativeItem().Text(s.Ruc).FontSize(13); });
                a.Item().Text("FACTURA").FontSize(12);
                a.Item().Row(r => { r.ConstantItem(55).Text("No."); r.RelativeItem().Text(inv.NumeroFactura); });
                a.Item().Text("NÚMERO DE AUTORIZACIÓN");
                a.Item().Text(inv.NumeroAutorizacion ?? "PENDIENTE DE AUTORIZACIÓN").FontSize(7.5f);
                a.Item().Row(r =>
                {
                    r.ConstantItem(130).Text("FECHA Y HORA DE\nAUTORIZACIÓN:");
                    r.RelativeItem().Text(inv.FechaAutorizacion.HasValue
                        ? EcuadorTime(inv.FechaAutorizacion.Value).ToString("dd/MM/yyyy HH:mm:ss", Inv)
                        : "—");
                });
                a.Item().Row(r => { r.ConstantItem(130).Text("AMBIENTE:"); r.RelativeItem().Text(inv.Ambiente == 2 ? "PRODUCCIÓN" : "PRUEBAS"); });
                a.Item().Row(r => { r.ConstantItem(130).Text("EMISIÓN:"); r.RelativeItem().Text("NORMAL"); });
                a.Item().Text("CLAVE DE ACCESO");
                a.Item().Height(38).Svg(Code128.ToSvg(inv.ClaveAcceso));
                a.Item().AlignCenter().Text(inv.ClaveAcceso).FontSize(7);
            });
        });
    }

    // ── Datos del comprador ──
    private static void Buyer(IContainer container, ElectronicInvoice inv)
    {
        container.Border(1).Padding(6).Column(c =>
        {
            c.Spacing(4);
            c.Item().Row(r => { r.ConstantItem(170).Text("Razón Social / Nombres y Apellidos:"); r.RelativeItem().Text(inv.RazonSocialComprador); });
            c.Item().Row(r =>
            {
                r.ConstantItem(80).Text("Identificación");
                r.ConstantItem(110).Text(inv.IdentificacionComprador);
                r.ConstantItem(90).Text("Placa / Matrícula:");
                r.RelativeItem().Text("");
            });
            c.Item().Row(r =>
            {
                r.ConstantItem(80).Text("Fecha");
                r.ConstantItem(110).Text(inv.FechaEmision.ToString("dd/MM/yyyy", Inv));
                r.ConstantItem(90).Text("Guía");
                r.RelativeItem().Text("");
            });
            c.Item().Row(r => { r.ConstantItem(80).Text("Dirección:"); r.RelativeItem().Text(inv.DireccionComprador ?? ""); });
        });
    }

    // ── Detalle ──
    private static void Details(IContainer container, ElectronicInvoice inv)
    {
        container.Table(t =>
        {
            t.ColumnsDefinition(c =>
            {
                c.ConstantColumn(52);  // Cod. Principal
                c.ConstantColumn(40);  // Cod. Auxiliar
                c.ConstantColumn(45);  // Cantidad
                c.RelativeColumn(3);   // Descripción
                c.RelativeColumn(1.4f);// Detalle adicional
                c.ConstantColumn(48);  // Precio unitario
                c.ConstantColumn(40);  // Subsidio
                c.ConstantColumn(48);  // Precio sin subsidio
                c.ConstantColumn(42);  // Descuento
                c.ConstantColumn(50);  // Precio total
            });

            t.Header(h =>
            {
                foreach (var title in new[] { "Cod.\nPrincipal", "Cod.\nAuxiliar", "Cantidad", "Descripción", "Detalle Adicional",
                             "Precio Unitario", "Subsidio", "Precio sin Subsidio", "Descuento", "Precio Total" })
                    h.Cell().Border(1).Padding(3).AlignCenter().AlignMiddle().Text(title).FontSize(7);
            });

            foreach (var item in inv.Items.OrderBy(i => i.Linea))
            {
                Cell(t).AlignCenter().Text(item.CodigoPrincipal);
                Cell(t).Text("");
                Cell(t).AlignCenter().Text(Qty(item.Cantidad));
                Cell(t).Text(item.Descripcion);
                Cell(t).Text("");
                Cell(t).AlignRight().Text(Qty(item.PrecioUnitario));
                Cell(t).AlignRight().Text("0.00");
                Cell(t).AlignRight().Text("0.00");
                Cell(t).AlignRight().Text(Money(item.Descuento));
                Cell(t).AlignRight().Text(Money(item.PrecioTotalSinImpuesto));
            }
        });
    }

    private static IContainer Cell(TableDescriptor t) => t.Cell().Border(1).Padding(3);

    // ── Información adicional, formas de pago y totales ──
    private static void Footer(IContainer container, ElectronicInvoice inv)
    {
        var subtotal0 = inv.Items.Where(i => i.TarifaIva == 0).Sum(i => i.PrecioTotalSinImpuesto);

        container.Row(row =>
        {
            row.Spacing(10);

            row.RelativeItem().Column(left =>
            {
                left.Spacing(8);

                left.Item().Border(1).Column(info =>
                {
                    info.Item().BorderBottom(1).Padding(3).AlignCenter().Text("Información Adicional");
                    var adicionales = new List<(string, string?)>
                    {
                        ("Telefono:", inv.TelefonoComprador),
                        ("Email:", inv.EmailComprador)
                    };
                    foreach (var (nombre, valor) in adicionales.Where(a => !string.IsNullOrWhiteSpace(a.Item2)))
                        info.Item().Padding(3).Row(r => { r.ConstantItem(90).Text(nombre).FontSize(7); r.RelativeItem().Text(valor!).FontSize(7); });
                    if (adicionales.All(a => string.IsNullOrWhiteSpace(a.Item2)))
                        info.Item().Padding(3).Text("—").FontSize(7);
                });

                left.Item().PaddingRight(40).Table(t =>
                {
                    t.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(1); });
                    t.Header(h =>
                    {
                        h.Cell().Border(1).Padding(3).AlignCenter().Text("Forma de pago");
                        h.Cell().Border(1).Padding(3).AlignCenter().Text("Valor");
                    });
                    foreach (var p in inv.Payments)
                    {
                        var desc = FormasPago.TryGetValue(p.FormaPago, out var d) ? d : "OTROS";
                        t.Cell().Border(1).Padding(3).Text($"{p.FormaPago} - {desc}").FontSize(7);
                        t.Cell().Border(1).Padding(3).AlignRight().Text(Money(p.Total)).FontSize(7);
                    }
                });
            });

            row.ConstantItem(250).Column(right =>
            {
                right.Spacing(8);
                right.Item().Table(t =>
                {
                    t.ColumnsDefinition(c => { c.RelativeColumn(); c.ConstantColumn(70); });
                    void Line(string label, decimal value)
                    {
                        t.Cell().Border(1).Padding(3).Text(label);
                        t.Cell().Border(1).Padding(3).AlignRight().Text(Money(value));
                    }

                    Line("SUBTOTAL 0%", subtotal0);
                    if (inv.TotalIva > 0)
                        Line("SUBTOTAL CON IVA", inv.TotalSinImpuestos - subtotal0);
                    Line("SUBTOTAL NO OBJETO DE IVA", 0);
                    Line("SUBTOTAL EXENTO DE IVA", 0);
                    Line("SUBTOTAL SIN IMPUESTOS", inv.TotalSinImpuestos);
                    Line("TOTAL DESCUENTO", inv.TotalDescuento);
                    Line("ICE", 0);
                    if (inv.TotalIva > 0)
                        Line("IVA", inv.TotalIva);
                    Line("IRBPNR", 0);
                    Line("PROPINA", 0);
                    Line("VALOR TOTAL", inv.ImporteTotal);
                });

                right.Item().Border(1).Padding(3).Column(sub =>
                {
                    sub.Item().Row(r => { r.RelativeItem().Text("VALOR TOTAL SIN SUBSIDIO"); r.ConstantItem(60).AlignRight().Text("0.00"); });
                    sub.Item().PaddingTop(6).Row(r =>
                    {
                        r.RelativeItem().Text("AHORRO POR SUBSIDIO:\n(Incluye IVA cuando corresponda)");
                        r.ConstantItem(60).AlignRight().Text("0.00");
                    });
                });
            });
        });
    }

    private static string Money(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero).ToString("0.00", Inv);
    private static string Qty(decimal v) => Math.Round(v, 6, MidpointRounding.AwayFromZero).ToString("0.00####", Inv);

    private static DateTimeOffset EcuadorTime(DateTimeOffset value) =>
        TimeZoneInfo.ConvertTime(value, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));

    private static byte[] LoadLogo()
    {
        using var stream = typeof(RideInvoicePdfGenerator).Assembly
            .GetManifestResourceStream("AltecSystem.Infrastructure.Resources.logo-altec.jpeg")
            ?? throw new InvalidOperationException("No se encontró el logo embebido del RIDE.");
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}
