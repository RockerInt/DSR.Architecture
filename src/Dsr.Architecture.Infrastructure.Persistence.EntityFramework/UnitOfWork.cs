using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework;

/// <summary>
/// Base class for unit of work pattern.
/// Provides a DbContext and methods to complete transactions.
/// Implements IDisposable to release resources.
/// </summary>
/// <param name="context"></param>
public abstract class UnitOfWork<TContext> : IUnitOfWork<TContext>
    where TContext : DbContext
{

    private readonly TContext _context;

    /// <summary>
    /// Gets the DbContext associated with this Unit of Work.
    /// This property provides access to the underlying database context for performing operations.
    /// </summary>
    public TContext Context => _context;

    public UnitOfWork(
        IServiceProvider serviceProvider,
        IDbContextFactory<TContext>? factory = null)
    {        
        
        if (factory is not null)
            _context = factory.CreateDbContext(); // If exist factory → use it to create a new DbContext instance (transient)
        else
            _context = serviceProvider.GetRequiredService<TContext>(); // else → resolve DbContext from the service provider (scoped or singleton)
    }

    /// <summary>
    /// Asynchronously commits changes to the database.
    /// This method saves all changes made in this context to the underlying database.
    /// It is typically called at the end of a unit of work to persist changes.
    /// If no changes are made, it will return 0.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await Context.SaveChangesAsync(cancellationToken);
    
    /// <summary>
    /// Disposes the Unit of Work and its resources.
    /// </summary>
    public void Dispose()
    {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}