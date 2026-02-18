namespace Dsr.Architecture.Domain.Validation.Extensions;

/// <summary>
/// Provides extension methods for validating basic conditions such as null checks and empty GUIDs, and adding validation errors to the ValidationCollector.
/// </summary>
public static class ValidationCollectorBasicExtensions
{
    /// <summary>
    /// Validates that the provided input is not null. If the input is null, 
    /// it adds a domain error to the collection with a message indicating that the parameter cannot be null. 
    /// The error includes an identifier (the parameter name), a message, and a code ("null_value") to categorize the error type. 
    /// This method is commonly used to ensure that required parameters are provided and to prevent null reference exceptions in the application. 
    /// It can be used in a fluent manner as part of a validation chain when validating input data for entities, value objects, or use case requests.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="paramName"></param>
    public static ValidationCollector Null(this ValidationCollector validation, object? input, string paramName)
            => validation.AddIf(input is null, paramName, 
                $"{paramName} cannot be null.",
                "null_value");
    
    /// <summary>
    /// Validates that the provided GUID value is not empty (Guid.Empty). If the value is empty, 
    /// it adds a domain error to the collection with a message indicating that the parameter cannot be an empty GUID. 
    /// The error includes an identifier (the parameter name), a message, and a code ("empty_guid") to categorize the error type. 
    /// This method is commonly used to ensure that GUID parameters,
    /// </summary>
    /// <param name="value"></param>
    /// <param name="paramName"></param>
    public static ValidationCollector EmptyGuid(this ValidationCollector validation, Guid value, string paramName)
            => validation.AddIf(value == Guid.Empty, paramName,
                $"{paramName} cannot be empty.",
                "empty_guid");
}
