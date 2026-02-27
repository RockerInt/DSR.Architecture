using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Entities;
using Dsr.Architecture.Domain.Result;
using System.Linq.Expressions;

namespace Dsr.Architecture.Persistence.Abstractions;

/// <summary>
/// Defines a generic repository interface for common data access write operations on aggregates.
/// Provides methods for inserting, updating, and deleting aggregates.
/// </summary>
/// <typeparam name="TId">The type of the aggregate's unique identifier.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregate managed by this repository.</typeparam>
public interface IWriteRepository<TId, TAggregate>
    where TId : IEquatable<TId>, IComparable<TId>
    where TAggregate : IAggregateRoot<TId>
{
    #region Sync

    /// <summary>
    /// Adds a new aggregate to the repository.
    /// </summary>
    /// <param name="aggregate">The aggregate to be added.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome.</returns>
    Result Add(TAggregate aggregate);

    /// <summary>
    /// Adds multiple new aggregates to the repository.
    /// </summary>
    /// <param name="aggregates">A collection of aggregates to be added.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome.</returns>
    Result AddRange(ICollection<TAggregate> aggregates);

    /// <summary>
    /// Updates an existing aggregate in the repository.
    /// </summary>
    /// <param name="aggregate">The aggregate to be updated.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome.</returns>
    Result Update(TAggregate aggregate);

    /// <summary>
    /// Removes aggregates from the repository based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the aggregates to be removed.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome.</returns>
    Result Remove(Expression<Func<TAggregate, bool>> filterExpression);

    /// <summary>
    /// Removes an aggregate from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate to be removed.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome.</returns>
    Result RemoveById(TId id);

    /// <summary>
    /// Removes multiple aggregates from the repository based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the aggregates to be removed.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome.</returns>
    Result RemoveRange(Expression<Func<TAggregate, bool>> filterExpression);

    #endregion Sync

    #region Async

    /// <summary>
    /// Asynchronously adds a new aggregate to the repository.
    /// </summary>
    /// <param name="aggregate">The aggregate to be added.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result"/> indicating the outcome.</returns>
    Task<Result> AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously adds multiple new aggregates to the repository.
    /// </summary>
    /// <param name="aggregates">A collection of aggregates to be added.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result"/> indicating the outcome.</returns>
    Task<Result> AddRangeAsync(ICollection<TAggregate> aggregates, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously updates an existing aggregate in the repository.
    /// </summary>
    /// <param name="aggregate">The aggregate to be updated.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result"/> indicating the outcome.</returns>
    Task<Result> UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously removes aggregates from the repository based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the aggregates to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result"/> indicating the outcome.</returns>
    Task<Result> RemoveAsync(Expression<Func<TAggregate, bool>> filterExpression, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously removes an aggregate from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result"/> indicating the outcome.</returns>
    Task<Result> RemoveByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously removes multiple aggregates from the repository based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the aggregates to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result"/> indicating the outcome.</returns>
    Task<Result> RemoveRangeAsync(Expression<Func<TAggregate, bool>> filterExpression, CancellationToken cancellationToken = default);

    #endregion Async
}
