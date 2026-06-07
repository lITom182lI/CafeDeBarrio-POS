using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Infrastructure.Persistence;
using CafeBarrio.Infrastructure.Persistence.Repositories;
using CafeBarrio.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CafeBarrio.Infrastructure.Security;
using CafeBarrio.Infrastructure.External;

namespace CafeBarrio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<AuditInterceptor>();
        services.AddDbContext<CafeBarrioDbContext>((sp, options) =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                   .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));

        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<ITransaccionRepository, TransaccionRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IOperadorRepository, OperadorRepository>();
        services.AddScoped<ITurnoRepository, TurnoRepository>();
        services.AddScoped<IAnulacionRepository, AnulacionRepository>();
        services.AddScoped<IMovimientoCajaRepository, MovimientoCajaRepository>();
        services.AddScoped<IConfiguracionNegocioRepository, ConfiguracionNegocioRepository>();
        services.AddScoped<ICategoriaCafeRepository, CategoriaCafeRepository>();
        services.AddScoped<IMetodoPagoRepository, MetodoPagoRepository>();
        services.AddScoped<IReportesRepository, ReportesRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddSingleton<CafeBarrio.Application.Common.Interfaces.IJwtService, CafeBarrio.Infrastructure.Security.JwtService>();
        services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISunatService, SunatStubService>();

        return services;
    }
}
