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
public abstract class MultiContextUnitOfWork(IDbContextAccessor accessor) : ITransactionalEFUnitOfWork
{   
    private readonly IDbContextAccessor _accessor = accessor;

    public IDbContextAccessor Accessor => _accessor;

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

                // Compartir transacción
                foreach (var context in contexts.Skip(1))
                {
                    await context.Database.UseTransactionAsync(dbTransaction, cancellationToken);
                }

                await operation(cancellationToken);

                // Guardar todos dinámicamente
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
    /// Disposes the Unit of Work and its resources.
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