namespace Dsr.Architecture.Domain.Validation;

/// <summary>
/// Defines the severity of an error that occurred during validation, allowing for differentiation between informational 
/// messages, warnings, errors, and critical issues.
/// </summary>
public enum ErrorSeverity
{
    Info = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}
