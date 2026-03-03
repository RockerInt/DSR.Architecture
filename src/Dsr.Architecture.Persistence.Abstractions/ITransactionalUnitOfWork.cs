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
    /// <param name="operation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);
}