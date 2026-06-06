using System.Text;
using CafeBarrio.Application;
using CafeBarrio.Domain.Entities;
using CafeBarrio.Infrastructure;
using CafeBarrio.Infrastructure.Persistence;
using CafeBarrio.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigin = builder.Configuration["Cors:AllowedOrigin"]
                    ?? "http://localhost:5173";
builder.Services.AddCors(options =>
    options.AddPolicy("Dashboard", policy =>
        policy.WithOrigins(allowedOrigin)
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

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("login-policy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit          = 5,
                Window               = TimeSpan.FromMinutes(5),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit           = 0
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers.RetryAfter = "300";
        await context.HttpContext.Response.WriteAsync(
            "{\"message\":\"Demasiados intentos. Intente en 5 minutos.\"}", token);
    };
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Seeder de datos de referencia para instalacion nueva
using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<CafeBarrioDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

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
            new MetodoPago { Nombre = "Efectivo", Activo = true },
            new MetodoPago { Nombre = "Tarjeta",  Activo = true }
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
            PasswordHash = hasher.Hash("Admin2026!"),
            Rol          = "Admin",
            Activo       = true
        });
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors("Dashboard");
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
