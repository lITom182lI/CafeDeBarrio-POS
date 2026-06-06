using CafeBarrio.POS.Data;
using CafeBarrio.POS.Forms;
using CafeBarrio.POS.Services;
using Microsoft.Extensions.Configuration;

ApplicationConfiguration.Initialize();

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var apiUrl = config["ApiBaseUrl"] ?? "http://localhost:5138";
var sedeId = int.TryParse(config["SedeId"], out var s) ? s : 1;

var db = new LocalDbContext();
db.Database.EnsureCreated();

var apiClient = new ApiClient(apiUrl);
var posService = new PosService(db, apiClient, sedeId);
var syncService = new SyncService(db, apiClient);

var form = new FormPOS(posService, syncService);
form.SetApiCheck(() => apiClient.IsAvailableAsync());

Application.Run(form);

db.Dispose();
