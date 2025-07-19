using Dsr.Architecture.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace Dsr.Architecture.Application;

/// <summary>
/// Static class for dependency injection of application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds application services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        => services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(UseCaseBehavior<,>).Assembly);
            config.AddOpenBehavior(typeof(UseCaseBehavior<,>));
        });
}

