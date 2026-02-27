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

    /// <summary>
    /// Begins a new transaction. This method should be called before performing any operations that need to be part of a transaction.
    /// The implementation of this method should ensure that a transaction is started on the underlying database context, 
    /// allowing subsequent operations to be executed within the scope of that transaction. 
    /// This is particularly important for ensuring that multiple operations are treated as a single unit of work, 
    /// where either all operations succeed or all operations fail, maintaining data integrity and consistency. 
    /// The method should also handle any exceptions that may occur during the transaction initiation process, 
    /// ensuring that appropriate error handling and logging mechanisms are in place to manage any issues that may arise when starting a transaction. 
    /// Additionally, the implementation should ensure that any resources associated with the transaction are properly managed and released, preventing potential resource leaks and ensuring efficient use of system resources.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Commits the current transaction. This method should be called after all operations that need to be part of the transaction have been performed successfully.
    /// The implementation of this method should ensure that the current transaction is committed on the underlying database context, finalizing all changes made during the transaction. 
    /// This is crucial for ensuring that all operations performed within the transaction are persisted to the database as a single unit of work, maintaining data integrity and consistency. 
    /// The method should also handle any exceptions that may occur during the transaction commit process, ensuring that appropriate error handling and logging mechanisms are in place to 
    /// manage any issues that may arise when committing a transaction. Additionally, the implementation should ensure that any resources associated with the transaction are properly managed and released, 
    /// preventing potential resource leaks and ensuring efficient use of system resources.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Rolls back the current transaction. This method should be called if any operation within the transaction fails or if there is a need to revert changes made during the transaction.
    /// The implementation of this method should ensure that the current transaction is rolled back on the underlying database context, undoing all changes made during the transaction. 
    /// This is essential for maintaining data integrity and consistency, especially in scenarios where multiple operations are performed as part of a single unit of work, and any failure should result in the entire transaction being reverted. 
    /// The method should also handle any exceptions that may occur during the transaction rollback process, ensuring that appropriate error handling and logging mechanisms are in place to manage any issues that may arise when rolling back a transaction. 
    /// Additionally, the implementation should ensure that any resources associated with the transaction are properly managed and released, preventing potential resource leaks and ensuring efficient use of system resources.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}