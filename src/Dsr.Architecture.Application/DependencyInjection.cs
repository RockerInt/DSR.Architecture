using Dsr.Architecture.Application.Abstractions;
using Dsr.Architecture.Application.Behaviors;
using Dsr.Architecture.Application.Policies;
using Dsr.Architecture.Application.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dsr.Architecture.Application;

/// <summary>
/// Static class for dependency injection of application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds application services to the specified <see cref="IServiceCollection"/> with default pipeline configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        => services.AddApplicationServices(_ => { });

    /// <summary>
    /// Adds application services to the specified <see cref="IServiceCollection"/>, allowing configuration of pipeline behaviors.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configurePipeline">Delegate used to configure which behaviors are applied in the MediatR pipeline.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        Action<ApplicationPipelineOptions> configurePipeline)
    {
        // Central error policy
        // Use TryAdd to allow consumers to easily override the default policy.
        // This makes the library more robust, as it will respect any pre-existing
        // registration for IErrorPolicy.
        services.TryAddSingleton<IErrorPolicy, DefaultErrorPolicy>();

        var pipelineOptions = new ApplicationPipelineOptions();
        configurePipeline(pipelineOptions);

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(ValidationBehavior<,>).Assembly);

            // Pipeline behaviors (outermost → innermost):
            // Validation → Authorization → Logging → Metrics → ExceptionHandling → Idempotency → Transaction → Handler
            if (pipelineOptions.UseValidation)
                config.AddOpenBehavior(typeof(ValidationBehavior<,>));

            if (pipelineOptions.UseAuthorization)
                config.AddOpenBehavior(typeof(AuthorizationBehavior<,>));

            if (pipelineOptions.UseLogging)
                config.AddOpenBehavior(typeof(LoggingBehavior<,>));

            if (pipelineOptions.UseMetrics)
                config.AddOpenBehavior(typeof(MetricsBehavior<,>));

            // Exception handling behavior is always applied.
            config.AddOpenBehavior(typeof(ExceptionHandlingBehavior<,>));

            if (pipelineOptions.UseIdempotency)
                config.AddOpenBehavior(typeof(IdempotencyBehavior<,>));

            if (pipelineOptions.UseTransaction)
                config.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        return services;
    }
}
