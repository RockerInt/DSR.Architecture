namespace Dsr.Architecture.Domain.Events;

/// <summary>
/// Defines a contract for dispatching domain events. Implementations of this interface are responsible for handling the dispatching 
/// of domain events to their respective handlers, ensuring that the events are processed in a timely and efficient manner. 
/// This may involve using an event bus, message queue, or other mechanisms to ensure that events are delivered to the appropriate handlers, 
/// even in distributed systems. The dispatcher should also handle any exceptions that may occur during event processing and ensure 
/// that the system remains consistent and reliable.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(
        IEnumerable<IDomainEvent> events,
        CancellationToken cancellationToken = default);
}