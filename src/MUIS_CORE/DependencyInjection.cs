using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MUIS_CORE.Behaviors;

namespace MUIS_CORE;

public static class DependencyInjection
{
    /// <param name="applicationAssembly">
    ///   Ensamblado donde están los Handlers de MediatR del proyecto.
    ///   Si es null, se usa el ensamblado llamador. Nunca queda sin registrar.
    /// </param>
    /// <param name="enableValidation">
    ///   true: agrega ValidationBehavior al pipeline (requiere Validators registrados).
    ///   false: MediatR sin validación automática.
    /// </param>
    public static IServiceCollection AddMuisCore(
        this IServiceCollection services,
        Assembly? applicationAssembly = null,
        bool enableValidation = false)
    {
        // MediatR siempre se registra — independientemente de los parámetros.
        // El ensamblado del llamador es el fallback si no se provee uno explícito.
        var assembly = applicationAssembly ?? Assembly.GetCallingAssembly();

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);

            if (enableValidation)
                config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return services;
    }
}
