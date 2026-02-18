namespace Dsr.Architecture.Domain.Validation.Extensions;

/// <summary>
/// Provides extension methods for validating numeric inputs and adding validation errors to the ValidationCollector.
/// </summary>
public static class ValidationCollectorNumericExtensions
{
    /// <summary>
    /// Validates that the provided numeric value is not negative. If the value is negative, 
    /// it adds a domain error to the collection with a message indicating that the parameter cannot be negative. 
    /// The error includes an identifier (the parameter name), a message, a code ("negative_value") to categorize the error type, 
    /// and metadata containing the actual value that failed validation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="paramName"></param>
    public static ValidationCollector Negative<T>(this ValidationCollector validation, T value, string paramName)
        where T : struct, IComparable<T>
            => validation.AddIf(value.CompareTo(default!) < 0, paramName,
                $"{paramName} cannot be negative.",
                "negative_value",
                ErrorType.Validation,
                ErrorSeverity.Error,
                new Dictionary<string, object?>
                {
                    { "ActualValue", value }
                });
    /// <summary>
    /// Validates that the provided numeric value is not negative or zero. 
    /// If the value is negative or zero, it adds a domain error to the collection with a message indicating 
    /// that the parameter must be greater than zero. 
    /// The error includes an identifier (the parameter name), a message, a code ("invalid_range") to categorize the error type, 
    /// and metadata containing the actual value that failed validation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="paramName"></param>
    public static ValidationCollector GreaterThan<T>(this ValidationCollector validation, T value, T min, string paramName)
        where T : struct, IComparable<T>
            => validation.AddIf(value.CompareTo(min) <= 0, paramName,
                $"{paramName} must be greater than {min}.",
                "invalid_range",
                ErrorType.Validation,
                ErrorSeverity.Error,
                new Dictionary<string, object?>
                {
                    { "ActualValue", value }
                });
    /// <summary>
    /// Validates that the provided numeric value is within a specified range (between min and max). 
    /// If the value is outside the range, it adds a domain error to the collection with a message indicating that the parameter must be between the specified minimum and maximum values. 
    /// The error includes an identifier (the parameter name), a message, a code ("out_of_range_value") to categorize the error type, and metadata containing the actual value that failed validation along with the defined minimum and maximum values. 
    /// This method is useful for validating input data that must fall within specific numeric boundaries, such as age, price, quantity, etc.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="paramName"></param>
    public static ValidationCollector Between<T>(this ValidationCollector validation, T value, T min, T max, string paramName)
        where T : IComparable<T>
            => validation.AddIf(value.CompareTo(min) < 0 || value.CompareTo(max) > 0, paramName,
                $"{paramName} must be between {min} and {max}.",
                "out_of_range_value",
                ErrorType.Validation,
                ErrorSeverity.Error,
                new Dictionary<string, object?>
                {
                    { "ActualValue", value },
                    { "MinValue", min },
                    { "MaxValue", max }
                });
}
