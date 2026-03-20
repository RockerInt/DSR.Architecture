using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Interfaces;
using Dsr.Architecture.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework;

/// <summary>
/// Static class for dependency injection of EntityFramework persistence services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds compiled query services to the specified IServiceCollection.
    /// This method registers the CompiledQueryCache, SpecificationComplexityAnalyzer, and AutoCompiledSpecificationExecutor 
    /// as singleton services for managing and executing compiled queries based on specifications.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCompiledQueriesPersistence(this IServiceCollection services)
        => services.AddSingleton<CompiledQueryCache>()
                   .AddSingleton<SpecificationAnalysisCache>()
                   .AddSingleton<ISpecificationComplexityAnalyzer, SpecificationComplexityAnalyzer>()
                   .AddSingleton<ICompiledSpecificationExecutor, AutoCompiledSpecificationExecutor>();


    /// <summary>
    /// Adds EntityFramework persistence services to the specified IServiceCollection.
    /// This method registers the UnitOfWork and repository implementations for the specified DbContext type.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
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
    /// <typeparam name="TContext"></typeparam>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <returns></returns>
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
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddMultiContextUnitOfWork(this IServiceCollection services)
        => services.AddScoped<ITransactionalEFUnitOfWork, MultiContextUnitOfWork>()
                   .AddScoped(typeof(IRepository<,>), typeof(EFRepository<,,>))
                   .AddScoped(typeof(IReadRepository<,>), typeof(ReadEFRepository<,,>))
                   .AddScoped(typeof(IWriteRepository<,>), typeof(WriteEFRepository<,,>));
}
