namespace Dsr.Architecture.Infrastructure.Persistence.Interfaces;

/// <summary>
/// Defines a contract for a Unit of Work.
/// A Unit of Work is responsible for coordinating the work of multiple repositories
/// and ensuring that changes are committed to the database in a single transaction.
/// It also provides a way to manage the lifetime of the database context.
/// This interface should be implemented by classes that manage database operations
/// and provide a way to commit changes.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the DbContext associated with this Unit of Work.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
}