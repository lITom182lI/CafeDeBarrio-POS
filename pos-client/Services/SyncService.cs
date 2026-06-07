using System.Text.Json;
using CafeBarrio.POS.Data;
using CafeBarrio.POS.Dtos;

namespace CafeBarrio.POS.Services;

public sealed class SyncService : IDisposable
{
    private readonly LocalDbContext _db;
    private readonly ApiClient _api;
    private System.Threading.Timer? _timer;

    public event Action? OnSyncCompleted;

    public SyncService(LocalDbContext db, ApiClient api)
    {
        _db = db;
        _api = api;
    }

    public void Start()
        => _timer = new System.Threading.Timer(
            async _ => await SyncAsync(),
            null,
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(30));

    public async Task SyncAsync()
    {
        var pendientes = _db.PendingTransacciones
            .Where(t => !t.Sincronizada)
            .ToList();

        if (pendientes.Count == 0) return;

        bool algoCambio = false;
        foreach (var tx in pendientes)
        {
            try
            {
                var items = JsonSerializer.Deserialize<List<ItemDto>>(tx.ItemsJson) ?? [];
                var request = new CreateTransaccionRequest(
                    tx.SedeId, null, tx.MetodoPagoId, items,
                    tx.OperadorId, tx.TipoDocumento, tx.NumeroDocumento, tx.RazonSocial);
                var id = await _api.PostTransaccionAsync(request);

                tx.Sincronizada = true;
                tx.FechaSincronizacion = DateTime.UtcNow;
                tx.TransaccionIdServidor = id;
                tx.ErrorSincronizacion = null;
                algoCambio = true;
            }
            catch (Exception ex)
            {
                tx.ErrorSincronizacion = ex.Message;
            }
        }

        if (algoCambio)
        {
            await _db.SaveChangesAsync();
            OnSyncCompleted?.Invoke();
        }
    }

    public void Dispose() => _timer?.Dispose();
}
