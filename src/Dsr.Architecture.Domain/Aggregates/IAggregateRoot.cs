using Dsr.Architecture.Domain.Entities;
using Dsr.Architecture.Domain.Events;

namespace Dsr.Architecture.Domain.Aggregates;

/// <summary>
/// Defines the contract for an aggregate root in the domain model, which is responsible for maintaining 
/// the integrity of a cluster of related entities and value objects. 
/// An aggregate root is the main entry point for accessing and modifying the state of the aggregate, 
/// and it ensures that all business rules and invariants are enforced when changes are made. 
/// The interface includes properties for tracking the version of the aggregate and any domain events that have been raised, 
/// as well as a method for clearing domain events after they have been processed.
/// </summary>
public interface IAggregateRoot<TId> : IEntity<TId>
{
    /// <summary>
    /// Gets the version of the aggregate, which is used for optimistic concurrency control to ensure that updates to 
    /// the aggregate are based on the most recent state.
    /// </summary>
    int Version { get; }
    /// <summary>
    /// Gets a read-only collection of domain events that have been raised by the aggregate. 
    /// These events represent significant changes to the state of the aggregate and can be used for event sourcing, auditing, 
    /// and integration with other parts of the system.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    /// <summary>
    /// Clears the collection of domain events after they have been processed. 
    /// This is typically called after the events have been published to an event bus or stored in an event store, 
    /// to ensure that the aggregate does not retain events that have already been handled.
    /// </summary>
    void ClearDomainEvents();
}