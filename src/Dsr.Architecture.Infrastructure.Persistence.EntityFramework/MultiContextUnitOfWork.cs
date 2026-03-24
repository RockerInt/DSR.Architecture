using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework;

/// <summary>
/// Implementation of a multi-context unit of work pattern.
/// This class enables managing transactions across multiple DbContext instances using the IDbContextAccessor.
/// It ensures that operations performed across different contexts are part of the same transaction when possible.
/// </summary>
/// <param name="accessor">The DbContext accessor to manage multiple contexts.</param>
public class MultiContextUnitOfWork(IDbContextAccessor accessor) : ITransactionalEFUnitOfWork
{   
    private readonly IDbContextAccessor _accessor = accessor;

    /// <summary>
    /// Gets the DbContext accessor associated with this unit of work.
    /// </summary>
    public IDbContextAccessor Accessor => _accessor;

    /// <summary>
    /// Executes a series of operations within a shared transaction across all tracked DbContexts.
    /// This method uses the first registered DbContext to start a transaction and then joins other contexts to it.
    /// </summary>
    /// <param name="operation">The operation to execute within the transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no DbContexts are registered.</exception>
    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        var contexts = _accessor.DbContexts.ToList();

        if (contexts.Count == 0)
            throw new InvalidOperationException("No DbContexts registered in scope.");

        var primaryContext = contexts.First();

        var strategy = primaryContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await primaryContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var dbTransaction = transaction.GetDbTransaction();

                // Share transaction
                foreach (var context in contexts.Skip(1))
                {
                    await context.Database.UseTransactionAsync(dbTransaction, cancellationToken);
                }

                await operation(cancellationToken);

                // Save all dynamically
                foreach (var context in contexts)
                {
                    await context.SaveChangesAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    /// <summary>
    /// Asynchronously saves changes made in all tracked DbContexts to the database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the total number of state entries written to the database.</returns>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var total = 0;

        foreach (var context in _accessor.DbContexts)
        {
            total += await context.SaveChangesAsync(cancellationToken);
        }

        return total;
    }

    /// <summary>
    /// Disposes all tracked DbContext instances and releases resources.
    /// </summary>
    public void Dispose()
    {
        foreach (var context in _accessor.DbContexts)
        {
            context.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}