namespace Dsr.Architecture.Domain.Events;

/// <summary>
/// Interface representing a domain event in the system.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for the domain event.
    /// </summary>
    Guid EventId { get; }
    /// <summary>
    /// Date and time when the domain event occurred, in UTC.
    /// </summary>
    DateTime OccurredOnUtc { get; }
    /// <summary>
    /// Type of the domain event, typically the name of the event class.
    /// </summary>
    string EventType { get; }
    /// <summary>
    /// Version number for the domain event, useful for event versioning and compatibility.
    /// </summary>
    int Version { get; }
    /// <summary>
    /// Metadata associated with the domain event, providing contextual information about the event such 
    /// as correlation and causation identifiers, user information, and source of the event.
    /// </summary>
    public EventMetadata Metadata { get; }
}