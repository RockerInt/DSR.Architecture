using Dsr.Architecture.Infrastructure.Persistence.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dsr.Architecture.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PersistenceSettings>(settings => configuration.GetSection("PersistenceSettings").Bind(settings));
        services.AddSingleton<IPersistenceSettings>(serviceProvider =>
            serviceProvider.GetRequiredService<IOptions<PersistenceSettings>>().Value);

        return services;
    }
}