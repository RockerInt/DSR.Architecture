namespace Dsr.Architecture.Application.Abstractions;

/// <summary>
/// Provides the current time (UTC) for the application. Implemented by the host; use in use case handlers
/// and domain services instead of DateTime.UtcNow for testability and consistent time handling.
/// </summary>
public interface ITimeProviderService
{
    /// <summary>
    /// Gets the current date and time in UTC.
    /// </summary>
    DateTime UtcNow { get; }
}
