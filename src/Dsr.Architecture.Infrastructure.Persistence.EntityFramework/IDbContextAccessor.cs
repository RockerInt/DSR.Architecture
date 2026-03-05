using Microsoft.EntityFrameworkCore;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework;

/// <summary>
/// Defines an interface for accessing DbContext instances within the application.
/// </summary>
public interface IDbContextAccessor
{
    /// <summary>
    /// Gets a read-only collection of DbContext instances that are currently being tracked by the application.
    /// </summary>
    IReadOnlyCollection<DbContext> DbContexts { get; }
    /// <summary>
    /// Gets an existing DbContext of the specified type or creates a new one if it does not exist.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <returns></returns>
    TContext GetOrAdd<TContext>() where TContext : DbContext;
}