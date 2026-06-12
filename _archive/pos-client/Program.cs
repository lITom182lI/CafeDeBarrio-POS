using System.Configuration;
using CafeBarrio.POS.Data;
using CafeBarrio.POS.Forms;
using CafeBarrio.POS.Services;
using Microsoft.Extensions.Configuration;

ApplicationConfiguration.Initialize();

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();
var apiBaseUrl = System.Configuration.ConfigurationManager.AppSettings["ApiBaseUrl"]
                 ?? config["ApiBaseUrl"]
                 ?? "http://localhost:5138";
var acceptSelfSigned = bool.TryParse(
    System.Configuration.ConfigurationManager.AppSettings["AcceptSelfSigned"], out var val) && val;
var printTicket = !bool.TryParse(
    System.Configuration.ConfigurationManager.AppSettings["PrintTicket"],
    out var pt) || pt;
var sedeId = int.TryParse(config["SedeId"], out var s) ? s : 1;

// ── Verificación de primera ejecución ────────────────────────────────
if (string.IsNullOrWhiteSpace(apiBaseUrl) ||
    apiBaseUrl.Contains("AQUI") ||
    apiBaseUrl.Contains("AQUI_IP"))
{
    MessageBox.Show(
        "Configuracion inicial requerida.\n\n" +
        "El sistema no esta configurado todavia.\n\n" +
        "Pasos:\n" +
        "  1. Abra la carpeta de instalacion\n" +
        "  2. Edite el archivo App.config con el Bloc de notas\n" +
        "  3. Cambie ApiBaseUrl por la IP del servidor\n" +
        "     Ejemplo: https://192.168.1.10:7240\n\n" +
        "Consulte CONFIGURACION.md para instrucciones detalladas.",
        "Cafe de Barrio POS — Primera ejecucion",
        MessageBoxButtons.OK,
        MessageBoxIcon.Information);
    return;
}
// ─────────────────────────────────────────────────────────────────────

var db = new LocalDbContext();
db.Database.EnsureCreated();

var apiClient = new ApiClient(apiBaseUrl, acceptSelfSigned);
var posService = new PosService(db, apiClient, sedeId);
var syncService = new SyncService(db, apiClient);

using var loginForm = new FormLogin(apiClient);
if (loginForm.ShowDialog() != DialogResult.OK)
    return;

var form = new FormPOS(posService, syncService, printTicket, loginForm.OperadorId);
form.SetApiCheck(() => apiClient.IsAvailableAsync());

Application.Run(form);

db.Dispose();
