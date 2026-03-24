using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework;

/// <summary>
/// Implementation of the unit of work pattern for a specific Entity Framework DbContext.
/// This class provides a coordinated way to manage transactions and save changes for a single DbContext.
/// </summary>
/// <typeparam name="TContext">The type of the DbContext managed by this unit of work.</typeparam>
public class UnitOfWork<TContext> : IUnitOfWork<TContext>
    where TContext : DbContext
{

    private readonly TContext _context;

    /// <summary>
    /// Gets the DbContext associated with this Unit of Work.
    /// This property provides access to the underlying database context for performing operations.
    /// </summary>
    public TContext Context => _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork{TContext}"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve the DbContext.</param>
    /// <param name="factory">An optional DbContext factory to create a new context instance.</param>
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
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the number of state entries written to the database.</returns>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await Context.SaveChangesAsync(cancellationToken);
    
    /// <summary>
    /// Disposes the Unit of Work and releases the underlying DbContext resources.
    /// </summary>
    public void Dispose()
    {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}