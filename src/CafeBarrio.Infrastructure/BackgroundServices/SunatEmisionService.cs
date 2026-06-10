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
            await ProcesarPendientesAsync(stoppingToken);
            await Task.Delay(_intervalo, stoppingToken);
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

                tx.SunatEstado      = resultado.Emitida ? "Emitida" : "NoEmitida";
                tx.SunatNumeroSerie = resultado.NumeroSerie;
                tx.SunatError       = resultado.Emitida ? null : resultado.Error;

                _log.LogInformation("[SUNAT-BG] Tx {Id}: {Estado} (intento {N})",
                    tx.TransaccionId, tx.SunatEstado, tx.SunatIntentos);
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
