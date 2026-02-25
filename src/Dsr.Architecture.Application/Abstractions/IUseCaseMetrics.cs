using Dsr.Architecture.Domain.Result;

namespace Dsr.Architecture.Application.Abstractions;

/// <summary>
/// Abstraction for recording metrics about use case execution.
/// Implementations can forward data to Prometheus, OpenTelemetry, or other systems.
/// </summary>
public interface IUseCaseMetrics
{
    /// <summary>
    /// Records a single use case execution sample.
    /// </summary>
    /// <param name="useCaseName">The logical name of the use case (typically the request type).</param>
    /// <param name="status">The resulting status.</param>
    /// <param name="elapsedMilliseconds">The elapsed execution time in milliseconds.</param>
    void RecordExecution(string useCaseName, ResultStatus status, long elapsedMilliseconds);
}

