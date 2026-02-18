namespace Dsr.Architecture.Domain.Events;

/// <summary>
/// Abstract base class for domain events.
/// </summary>
public abstract record DomainEvent(EventMetadata Metadata) : IDomainEvent
{
    /// <summary>
    /// Unique identifier for the domain event.
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();
    /// <summary>
    /// Date and time when the domain event occurred, in UTC.
    /// </summary>
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
    /// <summary>
    /// Type of the domain event, typically the name of the event class.
    /// </summary>
    public virtual string EventType => GetType().Name;
    /// <summary>
    /// Version number for the domain event, useful for event versioning and compatibility.
    /// </summary>
    public virtual int Version => 1;
    /// <summary>
    /// Metadata associated with the domain event, providing contextual information about the event such 
    /// as correlation and causation identifiers, user information, and source of the event.
    /// </summary>
    public EventMetadata Metadata { get; } = Metadata 
            ?? throw new ArgumentNullException(nameof(Metadata));
}
