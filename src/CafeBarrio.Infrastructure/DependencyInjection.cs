using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Infrastructure.Persistence;
using CafeBarrio.Infrastructure.Persistence.Repositories;
using CafeBarrio.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CafeBarrio.Infrastructure.Security;
using CafeBarrio.Infrastructure.External;
using CafeBarrio.Infrastructure.External.Sunat;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace CafeBarrio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<AuditInterceptor>();
        services.AddDbContext<CafeBarrioDbContext>((sp, options) =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                   .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));

        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<ITransaccionRepository, TransaccionRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IOperadorRepository, OperadorRepository>();
        services.AddScoped<ISedeRepository, SedeRepository>();
        services.AddScoped<ITurnoRepository, TurnoRepository>();
        services.AddScoped<IAnulacionRepository, AnulacionRepository>();
        services.AddScoped<IMovimientoCajaRepository, MovimientoCajaRepository>();
        services.AddScoped<IConfiguracionNegocioRepository, ConfiguracionNegocioRepository>();
        services.AddScoped<ICategoriaCafeRepository, CategoriaCafeRepository>();
        services.AddScoped<IMetodoPagoRepository, MetodoPagoRepository>();
        services.AddScoped<IReportesRepository, ReportesRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<CafeBarrio.Application.Common.Interfaces.IJwtService, CafeBarrio.Infrastructure.Security.JwtService>();
        services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<CafeBarrio.Infrastructure.Persistence.Seeders.ICatalogDataSeeder, CafeBarrio.Infrastructure.Persistence.Seeders.CatalogDataSeeder>();
        services.Configure<SunatOptions>(configuration.GetSection(SunatOptions.Section));
        var sunatEnabled = configuration.GetValue<bool>($"{SunatOptions.Section}:Enabled");
        if (sunatEnabled)
        {
            services.AddHttpClient<ISunatOseApiClient, NubefactOseApiClient>((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<SunatOptions>>().Value;
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Token", opts.OseToken);
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            services.AddScoped<ISunatService, SunatOseClient>();
        }
        else
        {
            services.AddScoped<ISunatService, SunatStubService>();
        }
        services.AddHostedService<CafeBarrio.Infrastructure.BackgroundServices.SunatEmisionService>();

        return services;
    }
}
