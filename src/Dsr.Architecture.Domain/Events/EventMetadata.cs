namespace Dsr.Architecture.Domain.Events;

/// <summary>
/// Represents metadata associated with a domain event, providing contextual information 
/// about the event such as correlation and causation identifiers, user information, and source of the event.
/// </summary>
/// <param name="CorrelationId"></param>
/// <param name="CausationId"></param>
/// <param name="UserId"></param>
/// <param name="Source"></param>
public sealed record EventMetadata(
    Guid CorrelationId,
    Guid? CausationId,
    string? UserId = null,
    string? Source = null)
{
    /// <summary>
    /// Gets the correlation identifier for the event, used to correlate related events and operations across the system.
    /// </summary>
    public Guid CorrelationId { get; } = CorrelationId;
    /// <summary>
    /// Gets the causation identifier for the event, used to identify the cause of the event, 
    /// such as a command or another event that triggered it.
    /// </summary>
    public Guid? CausationId { get; } = CausationId;
    /// <summary>
    /// Gets the user identifier associated with the event, representing the user who initiated the action that caused the event. 
    /// This can be used for auditing and tracking purposes.
    /// </summary>
    public string? UserId { get; } = UserId;
    /// <summary>
    /// Gets the source of the event, which can be used to identify the origin of the event, 
    /// such as a specific service, module, or component within the system.
    /// </summary>
    public string? Source { get; } = Source;
}