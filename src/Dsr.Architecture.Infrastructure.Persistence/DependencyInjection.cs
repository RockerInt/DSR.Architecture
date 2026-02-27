using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dsr.Architecture.Infrastructure.Persistence;

/// <summary>
/// Static class for dependency injection of persistence services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds persistence services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddPersistenceServicesBase(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PersistenceSettings>(settings =>
        {
            configuration.GetSection("PersistenceSettings").Bind(settings);

            if (string.IsNullOrEmpty(settings.DatabaseProvider))
                settings.DatabaseProvider = configuration["PersistenceSettings:DatabaseProvider"];

            if (string.IsNullOrEmpty(settings.DatabaseProvider))
                settings.DatabaseProvider = "sqlite"; // Fallback to a default provider if not provided

            if (string.IsNullOrEmpty(settings.DatabaseName))
                settings.DatabaseName = configuration["PersistenceSettings:DatabaseName"];

            if (string.IsNullOrEmpty(settings.DatabaseName))
                settings.DatabaseName = "WebAppBD"; // Fallback to a default name if not provided

            if (string.IsNullOrEmpty(settings.ReadDatabaseName))
                settings.ReadDatabaseName = configuration["PersistenceSettings:ReadDatabaseName"];

            if (string.IsNullOrEmpty(settings.ReadDatabaseName))
                settings.ReadDatabaseName = settings.DatabaseName; // Fallback to the main database name if not provided

            if (string.IsNullOrEmpty(settings.ConnectionStringName))
                settings.ConnectionStringName = configuration["PersistenceSettings:ConnectionStringName"];

            if (string.IsNullOrEmpty(settings.ConnectionStringName))
                settings.ConnectionStringName = settings.DatabaseName; // Fallback to a default name if not provided

            if (string.IsNullOrEmpty(settings.ReadConnectionStringName))
                settings.ReadConnectionStringName = configuration["PersistenceSettings:ReadConnectionStringName"];

            if (string.IsNullOrEmpty(settings.ReadConnectionStringName))
                settings.ReadConnectionStringName = settings.ConnectionStringName; // Fallback to the main connection string name if not provided

            var connectionString = configuration.GetConnectionString(settings.ConnectionStringName) ?? configuration.GetConnectionString(settings.DatabaseName);
            settings.ConnectionString = connectionString ?? settings.ConnectionString ?? configuration["PersistenceSettings:ConnectionString"] 
                ?? throw new InvalidOperationException("Connection string is not configured.");

            var readConnectionString = configuration.GetConnectionString(settings.ReadConnectionStringName) ?? configuration.GetConnectionString(settings.ReadDatabaseName);
            settings.ReadConnectionString = readConnectionString ?? settings.ReadConnectionString;
        });

        services.AddSingleton<IPersistenceSettings>(serviceProvider =>
            serviceProvider.GetRequiredService<IOptions<PersistenceSettings>>().Value);

        return services;
    }
}
