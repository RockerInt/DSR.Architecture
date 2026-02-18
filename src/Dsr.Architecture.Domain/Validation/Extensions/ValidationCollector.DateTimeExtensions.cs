namespace Dsr.Architecture.Domain.Validation.Extensions;

/// <summary>
/// Provides extension methods for validating date and time inputs and adding validation errors to the ValidationCollector.
/// </summary>
public static class ValidationCollectorDateTimeExtensions
{
    /// <summary>
    /// Validates that the provided DateTime value is not the default value (DateTime.MinValue). If the value is the default,
    /// it adds a domain error to the collection with a message indicating that the parameter cannot be the default date. 
    /// The error includes an identifier (the parameter name), a message, and a code ("default_date") to categorize the error type. 
    /// This method is useful for validating DateTime inputs such as event dates, deadlines, timestamps, etc., where a valid date must be provided and the default value is not acceptable. 
    /// It ensures that the date is properly initialized and represents a meaningful point in time before it is processed further in the application.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="paramName"></param>
    public static ValidationCollector DefaultDate(this ValidationCollector validation, DateTime value, string paramName)
        => validation.AddIf(value == default, paramName, $"{paramName} cannot be default date.", "default_date");
    /// <summary>
    /// Validates that the provided DateTime value is in the future. If the value is not in the future (i.e., it is in the past or present),
    /// it adds a domain error to the collection with a message indicating that the parameter must be in the future. 
    /// The error includes an identifier (the parameter name), a message, and a code ("invalid_future_date") to categorize the error type. 
    /// This method is useful for validating DateTime inputs such as scheduled dates, expiration dates, etc., where a future date is required for validity. 
    /// It ensures that the date represents a point in time that has not yet occurred before it is processed further in the application. 
    /// The validation is typically performed against the current UTC date and time to ensure consistency across different time zones and to avoid issues related to local time discrepancies.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="paramName"></param>
    public static ValidationCollector MustBeFuture(this ValidationCollector validation, DateTime value, string paramName)
        => validation.AddIf(
            value <= DateTime.UtcNow, 
            paramName, 
            $"{paramName} must be in the future.", 
            "invalid_future_date",
            ErrorType.Validation,
            ErrorSeverity.Error,
            new Dictionary<string, object?>
            {
                { "ActualValue", value }
            });
    /// <summary>
    /// Validates that the provided DateTime value is in the past. If the value is not in the past (i.e., it is in the future or present),
    /// it adds a domain error to the collection with a message indicating that the parameter must be in the past. 
    /// The error includes an identifier (the parameter name), a message, and a code ("invalid_past_date") to categorize the error type. 
    /// This method is useful for validating DateTime inputs such as birth dates, historical event dates, etc., where a past date is required for validity. 
    /// It ensures that the date represents a point in time that has already occurred before it is processed further in the application. 
    /// The validation is typically performed against the current UTC date and time to ensure consistency across different time zones and to avoid issues related to local time discrepancies   
    /// </summary>
    /// <param name="value"></param>
    /// <param name="paramName"></param>
    public static ValidationCollector MustBePast(this ValidationCollector validation, DateTime value, string paramName)
        => validation.AddIf(
            value >= DateTime.UtcNow, 
            paramName, 
            $"{paramName} must be in the past.", 
            "invalid_past_date",
            ErrorType.Validation,
            ErrorSeverity.Error,
            new Dictionary<string, object?>
            {
                { "ActualValue", value }
            });
}
