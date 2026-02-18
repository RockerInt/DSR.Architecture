namespace Dsr.Architecture.Domain.Validation.Extensions;

/// <summary>
/// Provides extension methods for validating collection inputs and adding validation errors to the ValidationCollector.
/// </summary>
public static class ValidationCollectorCollectionsExtensions
{
    /// <summary>
    /// Validates that the provided collection is not null or empty. If the collection is null or contains no elements,
    /// it adds a domain error to the collection with a message indicating that the parameter cannot be null or empty. 
    /// The error includes an identifier (the parameter name), a message, and a code ("empty_collection") to categorize the error type. 
    /// This method is useful for validating collection inputs such as lists of items, related entities, etc., where at least one element is required for validity. 
    /// It ensures that the collection is properly initialized and contains data before it is processed further in the application.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <param name="paramName"></param>
    public static ValidationCollector NullOrEmpty<T>(this ValidationCollector validation, IEnumerable<T>? collection, string paramName)
        => validation.AddIf(
            collection is null || !collection.Any(), 
            paramName,
            $"{paramName} cannot be null or empty.", 
            "empty_collection",
            ErrorType.Validation,
            ErrorSeverity.Error);
}
