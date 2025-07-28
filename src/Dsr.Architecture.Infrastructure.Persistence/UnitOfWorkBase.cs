using Dsr.Architecture.Domain.Entities;
using Dsr.Architecture.Domain.Interfaces;
using Dsr.Architecture.Infrastructure.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dsr.Architecture.Infrastructure.Persistence;

/// <summary>
/// Base class for unit of work pattern.
/// Provides a DbContext and methods to complete transactions.
/// Implements IDisposable to release resources.
/// </summary>
/// <param name="context"></param>
public abstract class UnitOfWorkBase(DbContext context) : IUnitOfWork
{
    /// <summary>
    /// Gets the DbContext associated with this Unit of Work.
    /// This property provides access to the underlying database context for performing operations.
    /// </summary>
    public DbContext Context { get; } = context;

    /// <summary>
    /// Asynchronously commits changes to the database.
    /// This method saves all changes made in this context to the underlying database.
    /// It is typically called at the end of a unit of work to persist changes.
    /// If no changes are made, it will return 0.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
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