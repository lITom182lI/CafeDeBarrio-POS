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
builder.Host.UseSerilog();

var allowedOrigins = (builder.Configuration["Cors:AllowedOrigin"]
                     ?? "http://localhost:5173,http://localhost:5174")
                     .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(options =>
    options.AddPolicy("Dashboard", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

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
    });

builder.Services.AddAuthorization();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
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

builder.Services.AddHealthChecks()
    .AddDbContextCheck<CafeBarrioDbContext>("database");

var app = builder.Build();

// Seeder de datos de referencia para instalacion nueva
using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<CafeBarrioDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    // Aplicar migraciones pendientes
    db.Database.Migrate();

    // ── Sede ─────────────────────────────────────────────────────────────
    if (!db.Sedes.Any())
    {
        db.Sedes.Add(new Sede
        {
            Nombre      = "Café de Barrio",
            Direccion   = "Jr. Principal 123",
            Distrito    = "Lima",
            Ciudad      = "Lima",
            EsPrincipal = true,
            Activa      = true
        });
        db.SaveChanges();
    }

    // ── Datos de referencia independientes ───────────────────────────────
    if (!db.TiposCliente.Any())
        db.TiposCliente.Add(new TipoCliente { Nombre = "Regular" });

    if (!db.MetodosPago.Any())
    {
        db.MetodosPago.AddRange(
            new MetodoPago { Nombre = "Efectivo", Activo = true, EsEfectivo = true  },
            new MetodoPago { Nombre = "Tarjeta",  Activo = true, EsEfectivo = false },
            new MetodoPago { Nombre = "Yape",     Activo = true, EsEfectivo = false },
            new MetodoPago { Nombre = "Plin",     Activo = true, EsEfectivo = false }
        );
    }

    if (!db.CategoriasCafe.Any())
    {
        db.CategoriasCafe.AddRange(
            new CategoriaCafe { Codigo = "CAF", Nombre = "Cafes",   Activa = true },
            new CategoriaCafe { Codigo = "BEB", Nombre = "Bebidas", Activa = true },
            new CategoriaCafe { Codigo = "COM", Nombre = "Comida",  Activa = true }
        );
    }

    db.SaveChanges();

    // ── Cliente "Mostrador" (para ventas sin cliente identificado) ────────
    if (!db.Clientes.Any())
    {
        var tipoId = db.TiposCliente.First().TipoClienteId;
        db.Clientes.Add(new Cliente
        {
            TipoClienteId = tipoId,
            Nombre        = "Mostrador",
            Apellido      = string.Empty,
            Email         = "mostrador@cafedebarrio.local",
            FechaRegistro = DateOnly.FromDateTime(DateTime.UtcNow),
            Activo        = true
        });
        db.SaveChanges();
    }

    // ── ConfiguracionNegocio (IGV régimen general: 16% + IPM 2% = 18%) ──
    if (!db.ConfiguracionesNegocio.Any())
    {
        var sedeId = db.Sedes.First().SedeId;
        db.ConfiguracionesNegocio.Add(new ConfiguracionNegocio
        {
            SedeId        = sedeId,
            TasaIGV       = 0.16m,
            TasaIPM       = 0.02m,
            FechaVigencia = DateTime.UtcNow,
            Activo        = true
        });
        db.SaveChanges();
    }

    // ── Usuario admin ────────────────────────────────────────────────────
    if (!db.Usuarios.Any())
    {
        db.Usuarios.Add(new Usuario
        {
            Email        = "admin@cafedebarrio.com",
            PasswordHash = hasher.Hash(
                builder.Configuration["Seed:AdminPassword"]
                ?? throw new InvalidOperationException("Seed:AdminPassword no configurado.")),
            Rol          = "Admin",
            Activo       = true
        });
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
{
    var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    if (ex is not null)
        Log.Error(ex, "Unhandled exception on {Method} {Path}",
            ctx.Request.Method, ctx.Request.Path);

    ctx.Response.StatusCode  = StatusCodes.Status500InternalServerError;
    ctx.Response.ContentType = "application/problem+json";
    await ctx.Response.WriteAsync(
        "{\"type\":\"https://tools.ietf.org/html/rfc7807\"," +
        "\"title\":\"Error interno del servidor.\"," +
        "\"status\":500}");
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

app.UseCors("Dashboard");
app.UseHttpsRedirection();
app.UseForwardedHeaders();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
