using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Result;
using Dsr.Architecture.Domain.Specifications.Interfaces;
using System.Linq.Expressions;

namespace Dsr.Architecture.Persistence.Abstractions;

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
    /// Retrieves all aggregates from the repository.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <returns>A <see cref="Result{IEnumerable{TAggregate}}"/> with a collection of all aggregates.</returns>
    Result<IEnumerable<TAggregate>> List(ISpecification<TId, TAggregate> specification);

    /// <summary>
    /// Retrieves and projects aggregates that match the specified specification to a different type.
    /// </summary>
    /// <typeparam name="TProjected">The type to project the aggregates to.</typeparam>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="projection">An expression to project the filtered aggregates to <typeparamref name="TProjected"/>.</param>
    /// <returns>A <see cref="Result{IEnumerable{TProjected}}"/> with a collection of projected aggregates.</returns>
    Result<IEnumerable<TProjected>> List<TProjected>(
        ISpecification<TId, TAggregate> specification,
        Expression<Func<TAggregate, TProjected>> projection);

    /// <summary>
    /// Retrieves a list of dynamic results based on an analytics specification.
    /// </summary>
    /// <param name="specification">The analytics specification to execute.</param>
    /// <returns>A <see cref="Result{List{dynamic}}"/> with a list of dynamic results.</returns>
    Result<IEnumerable<dynamic>> ListDynamic(ISpecification<TId, TAggregate> specification);

    /// <summary>
    /// Finds the individual aggregate based on the SpecificationResultCardinality(First, FirstOrDefault, Single, SingleOrDefault) that matches the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <returns>A <see cref="Result{TAggregate}"/> with the first matching aggregate.</returns>
    Result<TAggregate> Get(ISpecification<TId, TAggregate> specification);

    /// <summary>
    /// Retrieves an aggregate by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate to retrieve.</param>
    /// <returns>A <see cref="Result{TAggregate}"/> with the retrieved aggregate.</returns>
    Result<TAggregate> GetById(TId id);

    /// <summary>
    /// Retrieves a scalar value based on the given specification.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <returns>A <see cref="Result{T}"/> with the retrieved scalar value.</returns>
    Result<T> GetScalar<T>(ISpecification<TId, TAggregate> specification) where T : struct;

    /// <summary>
    /// Checks if any aggregate matches the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <returns>True if at least one aggregate matches; otherwise, false.</returns>
    Result<bool> Any(ISpecification<TId, TAggregate> specification);

    /// <summary>
    /// Counts the number of aggregates that match the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <returns>The total number of matching aggregates.</returns>
    Result<int> Count(ISpecification<TId, TAggregate> specification);

    #endregion Sync

    #region Async

    /// <summary>
    /// Asynchronously retrieves aggregates that match the specified specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{IEnumerable{TAggregate}}"/> with a collection of matching aggregates.</returns>
    Task<Result<IEnumerable<TAggregate>>> ListAsync(
        ISpecification<TId, TAggregate> specification, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves and projects aggregates that match the specified specification to a different type.
    /// </summary>
    /// <typeparam name="TProjected">The type to project the aggregates to.</typeparam>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="projection">An expression to project the filtered aggregates to <typeparamref name="TProjected"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{IEnumerable{TProjected}}"/> with a collection of projected aggregates.</returns>
    Task<Result<IEnumerable<TProjected>>> ListAsync<TProjected>(
        ISpecification<TId, TAggregate> specification,
        Expression<Func<TAggregate, TProjected>> projection, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a list of dynamic results based on an analytics specification.
    /// </summary>
    /// <param name="specification">The analytics specification to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{List{dynamic}}"/> with a list of dynamic results.</returns>
    Task<Result<IEnumerable<dynamic>>> ListDynamicAsync(ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously finds the aggregate that matches the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{TAggregate}"/> with the first matching aggregate.</returns>
    Task<Result<TAggregate>> GetAsync(ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves an aggregate by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate to retrieve.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{TAggregate}"/> with the retrieved aggregate.</returns>
    Task<Result<TAggregate>> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a scalar value based on the given specification.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with the retrieved scalar value.</returns>
    Task<Result<T>> GetScalarAsync<T>(ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken = default) where T : struct;

    /// <summary>
    /// Asynchronously checks if any aggregate matches the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{TAggregate}"/> with the first matching aggregate.</returns>
    Task<Result<bool>> AnyAsync(ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Asynchronously counts the number of aggregates that match the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{TAggregate}"/> with the first matching aggregate.</returns>
    Task<Result<int>> CountAsync(ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken = default);

    #endregion Async
}
