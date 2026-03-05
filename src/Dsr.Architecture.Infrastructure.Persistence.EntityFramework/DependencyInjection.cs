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
    /// Adds EntityFramework persistence services to the specified IServiceCollection.
    /// This method registers the UnitOfWork and repository implementations for the specified DbContext type.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddFullPersistence<TContext>(
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
    {
        services.AddDbContext<TContext>(options);

        services.AddScoped(provider =>
        {
            var accessor = provider.GetRequiredService<IDbContextAccessor>();
            return accessor.GetOrAdd<TContext>();
        });

        return services;
    }

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
