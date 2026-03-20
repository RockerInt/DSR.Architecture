using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework;

/// <summary>
/// Scoped implementation of IDbContextAccessor.
/// This class tracks DbContext instances within a specific service scope.
/// </summary>
public class ScopedDbContextAccessor(IServiceProvider serviceProvider) : IDbContextAccessor
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly List<DbContext> _dbContexts = [];

    /// <summary>
    /// Gets a read-only collection of DbContext instances that are currently being tracked by the application within the current scope.
    /// </summary>
    public IReadOnlyCollection<DbContext> DbContexts => _dbContexts;

    /// <summary>
    /// Gets an existing DbContext of the specified type or creates a new one if it does not exist, and starts tracking it.
    /// </summary>
    /// <typeparam name="TContext">The type of the DbContext to get or add.</typeparam>
    /// <returns>The DbContext instance of type <typeparamref name="TContext"/>.</returns>
    public TContext GetOrAdd<TContext>() where TContext : DbContext
    {
        var context = _serviceProvider.GetRequiredService<TContext>();

        if (!_dbContexts.Contains(context))
            _dbContexts.Add(context);

        return context;
    }
}