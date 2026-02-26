using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Result;
using System.Linq.Expressions;

namespace Dsr.Architecture.Domain.Abstractions.Repositories;

/// <summary>
/// Defines a generic repository interface for common data access read operations on aggregates.
/// Provides methods for searching and retrieving aggregates.
/// </summary>
/// <typeparam name="TId">The type of the aggregate's unique identifier.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregate managed by this repository.</typeparam>
public interface IReadRepository<TId, TAggregate>
    where TId : IEquatable<TId>, IComparable<TId>
    where TAggregate : IAggregateRoot<TId>
{
    #region Sync

    /// <summary>
    /// Provides an <see cref="IQueryable{TAggregate}"/> for building complex queries against the aggregate collection.
    /// </summary>
    /// <returns>An <see cref="IQueryable{TAggregate}"/> instance.</returns>
    IQueryable<TAggregate> AsQueryable();

    /// <summary>
    /// Retrieves all aggregates from the repository.
    /// </summary>
    /// <returns>A <see cref="Result{T}"/> with a collection of all aggregates.</returns>
    Result<IEnumerable<TAggregate>> GetAll();

    /// <summary>
    /// Retrieves aggregates that match the specified filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the aggregates.</param>
    /// <returns>A <see cref="Result{T}"/> with a collection of matching aggregates.</returns>
    Result<IEnumerable<TAggregate>> GetBy(
        Expression<Func<TAggregate, bool>> filterExpression);

    /// <summary>
    /// Retrieves and projects aggregates that match the specified filter expression to a different type.
    /// </summary>
    /// <typeparam name="TProjected">The type to project the aggregates to.</typeparam>
    /// <param name="filterExpression">An expression to filter the aggregates.</param>
    /// <param name="projectionExpression">An expression to project the filtered aggregates to <typeparamref name="TProjected"/>.</param>
    /// <returns>A <see cref="Result{T}"/> with a collection of projected aggregates.</returns>
    Result<IEnumerable<TProjected>> GetBy<TProjected>(
        Expression<Func<TAggregate, bool>> filterExpression,
        Expression<Func<TAggregate, TProjected>> projectionExpression);

    /// <summary>
    /// Finds the first aggregate that matches the given filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the aggregates.</param>
    /// <returns>A <see cref="Result{T}"/> with the first matching aggregate.</returns>
    Result<TAggregate> First(Expression<Func<TAggregate, bool>> filterExpression);

    /// <summary>
    /// Retrieves an aggregate by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate to retrieve.</param>
    /// <returns>A <see cref="Result{T}"/> with the retrieved aggregate.</returns>
    Result<TAggregate> GetById(TId id);

    #endregion Sync

    #region Async

    /// <summary>
    /// Asynchronously retrieves all aggregates from the repository.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with a collection of all aggregates.</returns>
    Task<Result<IEnumerable<TAggregate>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves aggregates that match the specified filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the aggregates.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with a collection of matching aggregates.</returns>
    Task<Result<IEnumerable<TAggregate>>> GetByAsync(
        Expression<Func<TAggregate, bool>> filterExpression, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves and projects aggregates that match the specified filter expression to a different type.
    /// </summary>
    /// <typeparam name="TProjected">The type to project the aggregates to.</typeparam>
    /// <param name="filterExpression">An expression to filter the aggregates.</param>
    /// <param name="projectionExpression">An expression to project the filtered aggregates to <typeparamref name="TProjected"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with a collection of projected aggregates.</returns>
    Task<Result<IEnumerable<TProjected>>> GetByAsync<TProjected>(
        Expression<Func<TAggregate, bool>> filterExpression,
        Expression<Func<TAggregate, TProjected>> projectionExpression, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously finds the first aggregate that matches the given filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the aggregates.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with the first matching aggregate.</returns>
    Task<Result<TAggregate>> FirstAsync(Expression<Func<TAggregate, bool>> filterExpression, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves an aggregate by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate to retrieve.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with the retrieved aggregate.</returns>
    Task<Result<TAggregate>> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    #endregion Async
}
