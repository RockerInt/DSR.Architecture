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
    /// <param name="aggregateId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<T>> LoadAsync(TId aggregateId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Saves an aggregate to the event store, along with its uncommitted events.
    /// The expectedVersion parameter is used for optimistic concurrency control, ensuring that the aggregate has not been modified by another process since it was loaded. 
    /// If the expected version does not match the current version of the aggregate in the event store, the save operation should fail, 
    /// preventing potential conflicts and ensuring data integrity. The method should return a Result indicating the success or failure of the save operation, 
    /// allowing the caller to handle any errors or concurrency issues that may arise during the save process. 
    /// The implementation of this method should ensure that all uncommitted events from the aggregate are properly persisted to the event store, 
    /// and that the aggregate's version is updated accordingly to reflect the changes made. 
    /// This helps to maintain the integrity of the event-sourced system and ensures that all changes to 
    /// the aggregate are properly recorded and can be replayed to reconstruct the aggregate's state in the future.
    /// </summary>
    /// <param name="aggregate"></param>
    /// <param name="expectedVersion"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result> SaveAsync(
        T aggregate,
        int expectedVersion,
        CancellationToken cancellationToken = default);
}