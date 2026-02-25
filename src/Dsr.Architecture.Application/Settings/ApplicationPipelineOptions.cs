namespace Dsr.Architecture.Application.Settings;

/// <summary>
/// Configuration options for the MediatR pipeline behaviors in the Application layer.
/// </summary>
public sealed class ApplicationPipelineOptions
{
    /// <summary>
    /// Enables the validation behavior in the pipeline.
    /// </summary>
    public bool UseValidation { get; set; } = true;

    /// <summary>
    /// Enables the authorization behavior in the pipeline.
    /// </summary>
    public bool UseAuthorization { get; set; } = true;

    /// <summary>
    /// Enables the logging behavior in the pipeline.
    /// </summary>
    public bool UseLogging { get; set; } = true;

    /// <summary>
    /// Enables the metrics behavior in the pipeline.
    /// </summary>
    public bool UseMetrics { get; set; } = true;

    /// <summary>
    /// Enables the idempotency behavior in the pipeline.
    /// </summary>
    public bool UseIdempotency { get; set; } = true;

    /// <summary>
    /// Enables the transaction behavior in the pipeline.
    /// </summary>
    public bool UseTransaction { get; set; } = true;
}

