namespace Dsr.Architecture.Persistence.Abstractions;

/// <summary>
/// Defines a contract for a Unit of Work that supports transactional operations.
/// </summary>
public interface ITransactionalUnitOfWork : IUnitOfWork
{
    /// <summary>
    /// Executes the specified operation within a transaction. 
    /// If the operation completes successfully, the transaction is committed; otherwise, it is rolled back.
    /// </summary>
    /// <param name="operation">The operation to be executed within the transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);
}