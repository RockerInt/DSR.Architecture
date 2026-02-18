namespace Dsr.Architecture.Domain.Validation;

/// <summary>
/// Defines the type of error that occurred during validation, allowing for categorization and handling of different error scenarios in a consistent manner.
/// </summary>
public enum ErrorType
{
    Validation = 1,
    BusinessRule = 2,
    Conflict = 3,
    NotFound = 4,
    Unauthorized = 5,
    Technical = 6
}
