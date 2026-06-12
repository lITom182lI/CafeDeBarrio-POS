using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CafeBarrio.Infrastructure.BackgroundServices;

public sealed class SunatEmisionService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SunatEmisionService> _log;
    private readonly TimeSpan _intervalo = TimeSpan.FromSeconds(30);

    public SunatEmisionService(IServiceScopeFactory scopeFactory, ILogger<SunatEmisionService> log)
    {
        _scopeFactory = scopeFactory;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarPendientesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _log.LogError(ex,
                    "SunatBackgroundService: error en ciclo de polling. " +
                    "Se reintentará en el próximo intervalo.");
            }

            await Task.Delay(_intervalo, stoppingToken)
                      .ContinueWith(_ => { }, CancellationToken.None);
        }
    }

    private const int MaxRetries = 3;

    private async Task ProcesarPendientesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db    = scope.ServiceProvider.GetRequiredService<CafeBarrioDbContext>();
        var sunat = scope.ServiceProvider.GetRequiredService<ISunatService>();

        var pendientes = await db.Transacciones
            .Where(t => t.SunatEstado == "Pendiente" && t.SunatIntentos < MaxRetries)
            .Include(t => t.Detalles).ThenInclude(d => d.Producto)
            .OrderBy(t => t.Fecha)
            .Take(10)
            .ToListAsync(ct);

        foreach (var tx in pendientes)
        {
            tx.SunatIntentos++;
            try
            {
                var items = tx.Detalles.Select(d => new BoletaItem(
                    d.Producto.Nombre, d.Cantidad, d.PrecioUnitario, d.SubtotalLinea)).ToList();

                var resultado = await sunat.EmitirBoletaAsync(new EmitirBoletaRequest(
                    tx.TransaccionId, tx.Fecha,
                    items, tx.Subtotal, tx.Impuesto, tx.Total,
                    tx.Canal, tx.TipoDocumento, tx.NumeroDocumento, tx.RazonSocial), ct);

                if (resultado.Emitida)
                {
                    tx.SunatEstado      = "Emitida";
                    tx.SunatNumeroSerie = resultado.NumeroSerie;
                    tx.SunatError       = null;
                    _log.LogInformation("[SUNAT-BG] Tx {Id}: Emitida — {NumeroSerie}",
                        tx.TransaccionId, resultado.NumeroSerie);
                }
                else if (resultado.Retryable && tx.SunatIntentos < MaxRetries)
                {
                    // HTTP 5xx transitorio: dejar Pendiente para próximo ciclo
                    tx.SunatError = resultado.Error;
                    _log.LogWarning("[SUNAT-BG] Tx {Id} HTTP-5xx transitorio intento {N}/{Max} — reintentará",
                        tx.TransaccionId, tx.SunatIntentos, MaxRetries);
                }
                else if (resultado.Retryable)
                {
                    // Agotó reintentos tras HTTP 5xx persistente
                    tx.SunatEstado = "DeadLetter";
                    tx.SunatError  = resultado.Error;
                    _log.LogError("[SUNAT-BG] Tx {Id} → DeadLetter tras {N} intentos (5xx): {Error}",
                        tx.TransaccionId, tx.SunatIntentos, resultado.Error);
                }
                else
                {
                    // HTTP 4xx: falla permanente, sin retry
                    tx.SunatEstado = "NoEmitida";
                    tx.SunatError  = resultado.Error;
                    _log.LogWarning("[SUNAT-BG] Tx {Id} → NoEmitida (4xx permanente): {Error}",
                        tx.TransaccionId, resultado.Error);
                }
            }
            catch (Exception ex)
            {
                tx.SunatError = ex.Message;
                if (tx.SunatIntentos >= MaxRetries)
                {
                    tx.SunatEstado = "DeadLetter";
                    _log.LogError("[SUNAT-BG] Tx {Id} → DeadLetter tras {N} intentos: {Error}",
                        tx.TransaccionId, tx.SunatIntentos, ex.Message);
                }
                else
                {
                    _log.LogWarning("[SUNAT-BG] Tx {Id} fallida intento {N}/{Max}: {Error}",
                        tx.TransaccionId, tx.SunatIntentos, MaxRetries, ex.Message);
                }
            }

            await db.SaveChangesAsync(ct);
        }
    }
}
