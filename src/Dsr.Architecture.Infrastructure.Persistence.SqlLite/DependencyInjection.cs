using Dsr.Architecture.Infrastructure.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dsr.Architecture.Infrastructure.Persistence.SqlLite;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        var persistenceSettings = services.BuildServiceProvider().GetRequiredService<IPersistenceSettings>();

        services.AddDbContext<SqlLiteDbContext>(options =>
            options.UseSqlite(persistenceSettings.ConnectionString));

        services.AddScoped(typeof(IRepository<,>), typeof(SqlLiteRepository<,>)); 

        return services;
    }
}