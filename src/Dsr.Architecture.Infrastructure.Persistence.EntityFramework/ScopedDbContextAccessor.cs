using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework;

/// <summary>
/// Defines an interface for accessing DbContext instances within the application.
/// </summary>
public class ScopedDbContextAccessor(IServiceProvider serviceProvider) : IDbContextAccessor
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly List<DbContext> _dbContexts = [];

    /// <summary>
    /// Gets a read-only collection of DbContext instances that are currently being tracked by the application.
    /// </summary>
    public IReadOnlyCollection<DbContext> DbContexts => _dbContexts;

    /// <summary>
    /// Gets an existing DbContext of the specified type or creates a new one if it does not exist.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <returns></returns>
    public TContext GetOrAdd<TContext>() where TContext : DbContext
    {
        var context = _serviceProvider.GetRequiredService<TContext>();

        if (!_dbContexts.Contains(context))
            _dbContexts.Add(context);

        return context;
    }
}