namespace Dsr.Architecture.Domain.Events;

/// <summary>
/// Defines an interface for accessing the current event context, which includes metadata about the domain event being processed.
/// </summary>
public interface IEventContextAccessor
{
    EventMetadata GetCurrent();
}