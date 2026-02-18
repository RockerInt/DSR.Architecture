namespace Dsr.Architecture.Domain.Validation.Extensions;

/// <summary>
/// Provides extension methods for adding technical errors to the ValidationCollector.
/// </summary>
public static class ValidationCollectorTechnicalExtensions
{
    /// <summary>
    /// Adds a custom technical error to the collection with the specified identifier, message, and code. 
    /// The error is categorized as a technical error with a severity level of critical. 
    /// This method can be used to record technical issues that occur during validation or processing, such as exceptions, system failures, or other unexpected conditions that are not related to business rules but still need to be captured and handled appropriately within the application. 
    /// It allows for consistent error handling and reporting of technical issues alongside validation errors, ensuring that all relevant information is collected for troubleshooting and resolution.
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="message"></param>
    /// <param name="code"></param>
    public static void Technical(
        this ValidationCollector validation,
        string identifier,
        string message,
        string code)
        => validation.Add(identifier,
            message,
            code,
            ErrorType.Technical,
            ErrorSeverity.Critical);
}
