namespace Dsr.Architecture.Persistence.Abstractions;

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
    /// Commits all changes made in the context of this unit of work to the database.
    /// This method ensures that all operations performed through the repositories
    /// are saved atomically, meaning that either all changes are committed or none are.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{Int32}"/> representing the asynchronous operation, containing the number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}