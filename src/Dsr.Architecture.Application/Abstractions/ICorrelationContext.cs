namespace Dsr.Architecture.Application.Abstractions;

/// <summary>
/// Provides access to the current correlation identifier for the request.
/// This is typically populated by the host (e.g. API middleware) and consumed
/// by logging, metrics, and result handling.
/// </summary>
public interface ICorrelationContext
{
    /// <summary>
    /// Gets or sets the correlation identifier for the current logical operation.
    /// </summary>
    string? CorrelationId { get; set; }
}

