namespace Dsr.Architecture.Domain.Validation;

/// <summary>
/// Represents an error that occurred during domain validation, encapsulating details about the error such as its 
/// code, message, type, severity, and any additional metadata.
/// </summary>
public class DomainError
{
    /// <summary>
    /// A unique code that identifies the specific error, allowing for consistent handling and localization of error messages across the application.
    /// </summary>
    public string Code { get; init; } = default!;
    /// <summary>
    /// A unique identifier for the error instance, which can be used for tracking and correlation purposes, 
    /// especially in scenarios where multiple errors may occur and need to be distinguished from one another.
    /// </summary>
    public string Identifier { get; init; } = default!;
    /// <summary>
    /// A human-readable message that describes the error, providing context and information about what went wrong during validation. 
    /// This message can be used for logging, debugging, and displaying error information to users or developers, helping to facilitate understanding and resolution of the issue.
    /// </summary>
    public string Message { get; init; } = default!;
    /// <summary>
    /// The type of error that occurred during validation, allowing for categorization and handling of different error scenarios in a consistent manner.
    /// </summary>
    public ErrorType Type { get; init; } = ErrorType.Validation;
    /// <summary>
    /// The severity of the error that occurred during validation, allowing for differentiation between informational messages, warnings, errors, and critical issues.
    /// </summary>
    public ErrorSeverity Severity { get; init; } = ErrorSeverity.Error;
    /// <summary>
    /// Additional metadata associated with the error, which can include any relevant information that provides further context about the error, 
    /// such as the property or field that caused the error, the value that was invalid, or any other details that may be helpful for debugging and resolving the issue.
    /// </summary>
    public Dictionary<string, object?>? Metadata { get; init; }
}
