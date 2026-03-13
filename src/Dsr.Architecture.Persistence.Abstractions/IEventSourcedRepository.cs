using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Result;

namespace Dsr.Architecture.Persistence.Abstractions;

/// <summary>
/// Defines a repository interface for event-sourced aggregates. 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TId"></typeparam>
public interface IEventSourcedRepository<T, TId>
    where T : AggregateRoot<TId>
    where TId : IEquatable<TId>, IComparable<TId>
{
    /// <summary>
    /// Loads an aggregate from the event store by its identifier.
    /// </summary>
    /// <param name="aggregateId">The unique identifier of the aggregate to load.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with the loaded aggregate.</returns>
    Task<Result<T>> LoadAsync(TId aggregateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves an aggregate to the event store, along with its uncommitted events.
    /// </summary>
    /// <remarks>
    /// The <paramref name="expectedVersion"/> parameter is used for optimistic concurrency control.
    /// If the version does not match the current version in the store, the operation fails.
    /// </remarks>
    /// <param name="aggregate">The aggregate root containing uncommitted events to save.</param>
    /// <param name="expectedVersion">The version the aggregate is expected to have in the store for concurrency control.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, indicating the outcome of the save process.</returns>
    Task<Result> SaveAsync(
        T aggregate,
        int expectedVersion,
        CancellationToken cancellationToken = default);
}