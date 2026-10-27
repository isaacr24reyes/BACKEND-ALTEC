using AltecSystem.Application.Services.Sri;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AltecSystem.Infrastructure.BackgroundJobs;

/// <summary>
/// Cada pocos minutos reintenta las facturas que quedaron sin autorizar (SRI lento o sin conexión)
/// y envía el correo cuando se autorizan, sin depender de que el usuario tenga la ventana abierta.
/// </summary>
public class PendingInvoiceRetryService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan MaxAge = TimeSpan.FromDays(7);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PendingInvoiceRetryService> _logger;

    public PendingInvoiceRetryService(IServiceScopeFactory scopeFactory, ILogger<PendingInvoiceRetryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Espera inicial para no competir con el arranque de la aplicación
        try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); }
        catch (OperationCanceledException) { return; }

        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<ElectronicInvoiceProcessor>();
                var autorizadas = await processor.RetryPendingAsync(MaxAge, stoppingToken);
                if (autorizadas > 0)
                    _logger.LogInformation("Reintento automático: {Count} factura(s) autorizada(s)", autorizadas);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // Firma o base de datos no disponibles: se vuelve a intentar en el siguiente ciclo
                _logger.LogWarning(ex, "Reintento automático de facturas falló");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false) is true);
    }
}
