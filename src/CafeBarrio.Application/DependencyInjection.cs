using CafeBarrio.Application.Features.Transacciones.Commands.CreateTransaccion;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MUIS_CORE.Behaviors;

namespace CafeBarrio.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssemblyContaining<CreateTransaccionCommandValidator>();
        return services;
    }
}
