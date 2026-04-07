using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Interfaces;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Evaluators;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Observability;
using Dsr.Architecture.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework;

/// <summary>
/// Static class for dependency injection of EntityFramework persistence services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds compiled query services to the specified IServiceCollection.
    /// Registers CompiledQueryCache, SpecificationAnalysisCache, SpecificationComplexityAnalyzer,
    /// and AutoCompiledSpecificationExecutor wrapped in a logging decorator for observability.
    /// Reads feature flags from configuration to enable optional features.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">Optional configuration for feature flags.
    /// Flags live under "FeatureFlags:Persistence" in config.
    /// Environment variable overrides: PERSISTENCE_FF__USEBOUNDEDCACHE, etc.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddCompiledQueriesPersistence(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        var flags = ReadFeatureFlags(configuration);

        services.AddSingleton(flags);

        if (flags.UseBoundedCache)
        {
            services.AddMemoryCache(options => options.SizeLimit = 10_000);
            services.AddSingleton<CompiledQueryCache>(sp =>
            {
                var boundedCache = new BoundedCompiledQueryCache(
                    sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>(),
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<BoundedCompiledQueryCache>>());

                return new CompiledQueryCache(
                    getOrAdd: boundedCache.GetOrAdd,
                    tryGet: key =>
                    {
                        var found = boundedCache.TryGet(key, out var compiled);
                        return (found, compiled);
                    });
            });
        }
        else
        {
            services.AddSingleton<CompiledQueryCache>();
        }

        services.AddSingleton<SpecificationAnalysisCache>()
                .AddSingleton<ISpecificationComplexityAnalyzer, SpecificationComplexityAnalyzer>()
                .AddScoped<ISpecificationEvaluator, SpecificationEvaluator>()
                .AddScoped<ICompiledSpecificationExecutor>(sp =>
                {
                    var inner = new AutoCompiledSpecificationExecutor(
                        sp.GetRequiredService<CompiledQueryCache>(),
                        sp.GetRequiredService<SpecificationAnalysisCache>(),
                        sp.GetRequiredService<ISpecificationComplexityAnalyzer>());
                    var log = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LoggingSpecificationExecutor>>();

                    // Phase 5: Canary — use new pipeline instead of old
                    if (flags.UseNewSpecificationEvaluator)
                    {
                        return new CanarySpecificationExecutor(
                            sp.GetRequiredService<ISpecificationEvaluator>(),
                            sp.GetRequiredService<CompiledQueryCache>(),
                            sp.GetRequiredService<SpecificationAnalysisCache>(),
                            sp.GetRequiredService<ISpecificationComplexityAnalyzer>());
                    }

                    // Phase 4: Shadow mode — old primary, new shadow for comparison
                    if (flags.ShadowModeEnabled)
                    {
                        return new ShadowSpecificationExecutor(
                            inner,
                            sp.GetRequiredService<ISpecificationEvaluator>(),
                            sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ShadowSpecificationExecutor>>(),
                            flags,
                            sp);
                    }

                    return new LoggingSpecificationExecutor(inner, log);
                });

        return services;
    }


    /// <summary>
    /// Adds EntityFramework persistence services to the specified IServiceCollection.
    /// This method registers the UnitOfWork and repository implementations for the specified DbContext type.
    /// </summary>
    /// <typeparam name="TContext">The type of the database context.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="options">Optional configuration for the database context.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddUnifiedPersistence<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? options = null)
        where TContext : DbContext
    {
        if (options is not null)
            services.AddDbContext<TContext>(options);

        services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();
        services.AddScoped(typeof(IRepository<,>), typeof(EFRepository<,,>));

        return services;
    }

    /// <summary>
    /// Adds read-only EntityFramework persistence services to the specified IServiceCollection.
    /// This method registers the UnitOfWork and read-only repository implementations for the specified DbContext type.
    /// </summary>
    /// <typeparam name="TContext">The type of the DbContext.</typeparam>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="options">Optional configuration for the DbContext.</param>
    /// <returns>The modified IServiceCollection.</returns>
    public static IServiceCollection AddReadOnlyPersistence<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? options = null)
        where TContext : DbContext
    {
        if (options is not null)
            services.AddDbContext<TContext>(options);

        services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();
        services.AddScoped(typeof(IReadRepository<,>), typeof(ReadEFRepository<,,>));

        return services;
    }

    /// <summary>
    /// Adds write-only EntityFramework persistence services to the specified IServiceCollection.
    /// This method registers the UnitOfWork and write-only repository implementations for the specified DbContext type.
    /// </summary>
    /// <typeparam name="TContext">The type of the DbContext.</typeparam>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="options">Optional configuration for the DbContext.</param>
    /// <returns>The modified IServiceCollection.</returns>
    public static IServiceCollection AddWritePersistence<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? options = null)
        where TContext : DbContext
    {
        if (options is not null)
            services.AddDbContext<TContext>(options);

        services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();
        services.AddScoped(typeof(IWriteRepository<,>), typeof(WriteEFRepository<,,>));

        return services;
    }

    /// <summary>
    /// Adds a tracked DbContext to the specified IServiceCollection.
    /// This method registers the DbContext and ensures that it is tracked by the IDbContextAccessor
    /// for use in transactions and unit of work patterns.
    /// </summary>
    /// <typeparam name="TContext">The type of the database context.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="options">Configuration for the database context.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddTrackedDbContext<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> options)
        where TContext : DbContext
        => services.AddDbContext<TContext>(options)
                   .AddScoped(provider =>
                   {
                        var accessor = provider.GetRequiredService<IDbContextAccessor>();
                        return accessor.GetOrAdd<TContext>();
                   });

    /// <summary>
    /// Adds a multi-context unit of work implementation to the specified IServiceCollection.
    /// This method registers repositories and the MultiContextUnitOfWork, which allows for managing transactions across
    /// multiple DbContexts using the IDbContextAccessor to access the registered contexts.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddMultiContextUnitOfWork(this IServiceCollection services)
        => services.AddScoped<ITransactionalEFUnitOfWork, MultiContextUnitOfWork>()
                   .AddScoped(typeof(IRepository<,>), typeof(EFRepository<,,>))
                   .AddScoped(typeof(IReadRepository<,>), typeof(ReadEFRepository<,,>))
                   .AddScoped(typeof(IWriteRepository<,>), typeof(WriteEFRepository<,,>));

    private static PersistenceFeatureFlags ReadFeatureFlags(IConfiguration? configuration)
    {
        var flags = new PersistenceFeatureFlags();
        if (configuration is null) return flags;

        var section = configuration.GetSection("FeatureFlags:Persistence");
        if (!section.Exists()) return flags;

        foreach (var child in section.GetChildren())
        {
            switch (child.Key)
            {
                case "UseBoundedCache":
                    flags.UseBoundedCache = child.Value == "true";
                    break;
                case "EnableCanonicalCacheKeys":
                    flags.EnableCanonicalCacheKeys = child.Value == "true";
                    break;
                case "UseNewSpecificationEvaluator":
                    flags.UseNewSpecificationEvaluator = child.Value == "true";
                    break;
                case "EnforceSpecCardinality":
                    flags.EnforceSpecCardinality = child.Value == "true";
                    break;
                case "ShadowModeEnabled":
                    flags.ShadowModeEnabled = child.Value == "true";
                    break;
                case "ShadowSampleRate":
                    if (double.TryParse(child.Value, out var rate))
                        flags.ShadowSampleRate = rate;
                    break;
            }
        }

        return flags;
    }
}
