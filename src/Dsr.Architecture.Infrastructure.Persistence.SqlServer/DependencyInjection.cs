using Dsr.Architecture.Infrastructure.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dsr.Architecture.Infrastructure.Persistence.SqlServer;

/// <summary>
/// Static class for dependency injection of SQL Server persistence services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds SQL Server persistence services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        var persistenceSettings = services.BuildServiceProvider().GetRequiredService<IPersistenceSettings>();

        services.AddDbContext<SqlServerDbContext>(options =>
            options.UseSqlServer(persistenceSettings.ConnectionString));

        services.AddScoped(typeof(IRepository<,>), typeof(SqlServerRepository<,>));

        return services;
    }
}
