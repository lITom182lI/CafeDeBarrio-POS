using System.Text;
using CafeBarrio.Application;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using CafeBarrio.Domain.Entities;
using CafeBarrio.Infrastructure;
using CafeBarrio.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CafeBarrio.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(new CompactJsonFormatter())
    .WriteTo.File(new CompactJsonFormatter(), "logs/cafebarrio-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key no configurado.");

var knownPlaceholders = new[]
{
    "REEMPLAZAR_CON_SECRETO_MINIMO_32_CARACTERES",
    "OVERRIDE_VIA_ENV_VAR",
    "your-secret-key",
    "changeme",
    "secret"
};

if (jwtKey.Length < 32)
    throw new InvalidOperationException(
        "Jwt:Key debe tener mínimo 32 caracteres.");

if (knownPlaceholders.Any(p =>
        jwtKey.Contains(p, StringComparison.OrdinalIgnoreCase)))
    throw new InvalidOperationException(
        "Jwt:Key contiene un valor placeholder. " +
        "Configura un secreto real antes de arrancar.");

static void RequireConfig(IConfiguration cfg, string key)
{
    var value = cfg[key];
    if (string.IsNullOrWhiteSpace(value) || 
        value.StartsWith("OVERRIDE_VIA_ENV_VAR") ||
        value.StartsWith("REEMPLAZAR_") ||
        value.StartsWith("DEV_JWT_KEY") ||
        value.StartsWith("AQUI_"))
    {
        throw new InvalidOperationException(
            $"Configuración requerida no establecida o con valor placeholder: '{key}'. " +
            $"Configura la variable de entorno antes de iniciar en producción.");
    }
}

if (!builder.Environment.IsDevelopment())
{
    RequireConfig(builder.Configuration, "ConnectionStrings:DefaultConnection");
    RequireConfig(builder.Configuration, "Cors:AllowedOrigin");
    RequireConfig(builder.Configuration, "Jwt:Key");

    var connStr = builder.Configuration["ConnectionStrings:DefaultConnection"] ?? "";
    if (connStr.Contains("TrustServerCertificate=True", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException(
            "TrustServerCertificate=True está prohibido en producción. " +
            "Configura un certificado TLS válido en el servidor SQL Server.");
}
builder.Host.UseSerilog();

var allowedOrigins = (builder.Configuration["Cors:AllowedOrigin"]
                     ?? "http://localhost:5173,http://localhost:5174")
                     .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(options =>
    options.AddPolicy("Dashboard", policy =>
        policy.WithOrigins(allowedOrigins)
              .WithHeaders("Content-Type", "Authorization", "X-Operator-Id")
              .WithMethods("GET", "POST", "PUT", "DELETE")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer     = builder.Configuration["Jwt:Issuer"],
            ValidAudience   = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var stampClaim = context.Principal?.FindFirst("security_stamp")?.Value;
                var idClaim    = context.Principal?.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var roleClaim  = context.Principal?.FindFirst(
                    System.Security.Claims.ClaimTypes.Role)?.Value;

                if (stampClaim is null || idClaim is null) return;
                if (!int.TryParse(idClaim, out var id)) { context.Fail("Invalid token"); return; }

                var db = context.HttpContext.RequestServices
                    .GetRequiredService<CafeBarrioDbContext>();

                if (roleClaim == "Operador")
                {
                    var operador = await db.Operadores.FindAsync(id);
                    if (operador is null || operador.SecurityStamp != stampClaim)
                        context.Fail("Token revocado. Vuelve a iniciar sesión.");
                }
                else
                {
                    var usuario = await db.Usuarios.FindAsync(id);
                    if (usuario is null || usuario.SecurityStamp != stampClaim)
                        context.Fail("Token revocado. Inicia sesión nuevamente.");
                }
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.RequireHeaderSymmetry = false;
    options.ForwardLimit = 1;
    options.KnownProxies.Clear();
    options.KnownNetworks.Clear();

    var trustedProxies = builder.Configuration
        .GetSection("ReverseProxy:TrustedProxyIPs")
        .Get<string[]>() ?? [];

    foreach (var ip in trustedProxies)
    {
        if (System.Net.IPAddress.TryParse(ip.Trim(), out var parsed))
            options.KnownProxies.Add(parsed);
    }
});

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("login-policy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit          = 20,
                Window               = TimeSpan.FromMinutes(15),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit           = 0
            }));

    options.AddPolicy("pin-policy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Request.Headers.TryGetValue("X-Operator-Id", out var opId)
                ? $"{httpContext.Connection.RemoteIpAddress}:{opId}"
                : httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit          = 10,
                Window               = TimeSpan.FromMinutes(10),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit           = 0
            }));

    options.AddPolicy("api-write-policy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit          = 200,
                Window               = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit           = 0
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers.RetryAfter = "600";
        await context.HttpContext.Response.WriteAsync(
            "{\"message\":\"Demasiados intentos. Intente en 10 minutos.\"}", token);
    };
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── OpenTelemetry ─────────────────────────────────────────────────────
var otelEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r
        .AddService(
            serviceName:    "CafeBarrio.API",
            serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.0.0"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation(o => o.RecordException = true)
            .AddHttpClientInstrumentation()
            .AddSqlClientInstrumentation();

        if (builder.Environment.IsDevelopment())
            tracing.AddConsoleExporter();

        if (!string.IsNullOrWhiteSpace(otelEndpoint))
            tracing.AddOtlpExporter(o => o.Endpoint = new Uri(otelEndpoint));
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        if (builder.Environment.IsDevelopment())
            metrics.AddConsoleExporter();

        if (!string.IsNullOrWhiteSpace(otelEndpoint))
            metrics.AddOtlpExporter(o => o.Endpoint = new Uri(otelEndpoint));
    });

builder.Services.AddHttpClient();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CafeBarrioDbContext>("database", tags: new[] { "ready" })
    .AddCheck<CafeBarrio.Infrastructure.HealthChecks.SunatHealthCheck>(
        "sunat-ose",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
        tags: new[] { "detail" });

var app = builder.Build();

app.UseForwardedHeaders();

// Seeder de datos de referencia para instalacion nueva
using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<CafeBarrioDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    // Aplicar migraciones pendientes
    if (app.Environment.IsDevelopment())
    {
        db.Database.Migrate();
    }
    else
    {
        var pending = db.Database.GetPendingMigrations().ToList();
        if (pending.Count > 0)
            throw new InvalidOperationException(
                $"Hay {pending.Count} migración(es) pendiente(s): " +
                string.Join(", ", pending) +
                ". Ejecuta 'dotnet ef database update' antes de arrancar.");
    }


}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
{
    var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    var correlationId = ctx.Items["CorrelationId"]?.ToString() ?? "unknown";
    if (ex is not null)
        Log.Error(ex, "Unhandled exception [{CorrelationId}] on {Method} {Path}",
            correlationId, ctx.Request.Method, ctx.Request.Path);

    ctx.Response.StatusCode  = StatusCodes.Status500InternalServerError;
    ctx.Response.ContentType = "application/problem+json";
    await ctx.Response.WriteAsync(
        $"{{\"type\":\"https://tools.ietf.org/html/rfc7807\"," +
        $"\"title\":\"Error interno del servidor.\"," +
        $"\"status\":500," +
        $"\"correlationId\":\"{correlationId}\"}}");
}));

if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.Append("X-Frame-Options",        "DENY");
    ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    ctx.Response.Headers.Append("Referrer-Policy",        "strict-origin-when-cross-origin");
    ctx.Response.Headers.Append("Content-Security-Policy","default-src 'self'; frame-ancestors 'none'");
    await next();
});



app.Use(async (ctx, next) =>
{
    var correlationId = ctx.Request.Headers["X-Correlation-ID"].FirstOrDefault()
        ?? Guid.NewGuid().ToString("N");
    ctx.Items["CorrelationId"] = correlationId;
    ctx.Response.Headers.Append("X-Correlation-ID", correlationId);
    await next();
});
app.UseCors("Dashboard");
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<CafeBarrio.Infrastructure.Persistence.Seeders.ICatalogDataSeeder>().SeedAsync();
}

app.MapControllers();
// Liveness: el proceso está vivo — sin checks, siempre 200 si el proceso responde.
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

// Readiness: solo DB — bloquea tráfico si la BD no responde.
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = hc => hc.Tags.Contains("ready")
});

// Detail: todos los checks incluyendo SUNAT (informacional).
app.MapHealthChecks("/health/detail", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true
});

// Alias de compatibilidad — redirige a readiness.
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = hc => hc.Tags.Contains("ready")
});
app.Run();
