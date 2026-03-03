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
    /// This method should ensure that all operations performed through the repositories
    /// are saved atomically, meaning that either all changes are committed or none are.
    /// The implementation of this method should handle any necessary transaction management
    /// to ensure data integrity and consistency. It should also handle any exceptions that may occur during
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}