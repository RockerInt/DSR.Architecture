using Dsr.Architecture.Domain.Result;

namespace Dsr.Architecture.Domain.Validation;

/// <summary>
/// A utility class for collecting validation errors in a fluent manner. 
/// It provides methods to validate various conditions such as null checks, empty GUIDs, numeric ranges, and business rules. 
/// The collected errors can be converted into a Result object for standardized error handling across the application.
/// </summary>
public sealed class ValidationCollector
{
    /// <summary>
    /// A list to store collected domain errors during validation. 
    /// Each error includes details such as the identifier, message, code, type, severity, and optional metadata.
    /// </summary>
    private readonly List<DomainError> _errors = [];
    /// <summary>>
    /// Gets a read-only list of collected domain errors. 
    /// This allows external code to access the errors without modifying the internal list.
    /// </summary>    
    public IReadOnlyList<DomainError> Errors => _errors;
    /// <summary>
    /// Indicates whether any validation errors have been collected. 
    /// This is true if there are one or more errors in the collection, and false if the collection is empty. 
    /// This property can be used to quickly check if validation has failed without needing to inspect the individual errors.
    /// </summary>
    public bool HasErrors => _errors.Count != 0;
    /// <summary>
    /// Provides a fluent interface for performing validations. 
    /// This property returns the current instance of the ValidationCollector, allowing for method chaining when performing multiple validations in a single statement. 
    /// For example, you can call validation methods like Null, EmptyGuid, NegativeOrZero, etc., in a fluent manner to collect errors as needed.
    /// </summary>
    public ValidationCollector Against => this;
    /// <summary>
    /// Adds a new domain error to the collection with the specified details. 
    /// This method is used internally by the various validation methods to record errors when validation conditions are not met. 
    /// The parameters include an identifier for the error, a descriptive message, 
    /// a code representing the error type, the severity of the error, and optional metadata for additional context.
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="message"></param>
    /// <param name="code"></param>
    /// <param name="type"></param>
    /// <param name="severity"></param>
    /// <param name="metadata"></param>
    public ValidationCollector Add(
        string identifier,
        string message,
        string code,
        ErrorType type = ErrorType.Validation,
        ErrorSeverity severity = ErrorSeverity.Error,
        Dictionary<string, object?>? metadata = null)
    {
        _errors.Add(new DomainError
        {
            Identifier = identifier,
            Message = message,
            Code = code,
            Type = type,
            Severity = severity,
            Metadata = metadata
        });
        return this;
    }
    /// <summary>
    /// Adds a new domain error to the collection if the specified condition is true. 
    /// This method is a convenient way to perform conditional validation checks and record errors only when certain conditions are met. 
    /// The parameters include the condition to evaluate, an identifier for the error, a descriptive message, 
    /// a code representing the error type, the severity of the error, and optional metadata for additional context. 
    /// This allows for flexible validation logic where errors are only added when specific conditions indicate a validation failure, 
    /// making it easier to manage complex validation scenarios in a clean and organized manner.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="identifier"></param>
    /// <param name="message"></param>
    /// <param name="code"></param>
    /// <param name="type"></param>
    /// <param name="severity"></param>
    /// <param name="metadata"></param>
    public ValidationCollector AddIf(
        bool condition,
        string identifier,
        string message,
        string code,
        ErrorType type = ErrorType.Validation,
        ErrorSeverity severity = ErrorSeverity.Error,
        Dictionary<string, object?>? metadata = null)
    {
        if (condition)
            Add(identifier, message, code, type, severity, metadata);
        return this;
    }
    public Result.Result ToResult()
    {
        if (!HasErrors)
            return Result.Result.Success();

        var errors = _errors.Select(e =>
            new Error
            {
                Identifier = e.Identifier,
                Message = e.Message,
                Code = e.Code,
                Type = e.Type,
                Severity = e.Severity,
                Metadata = e.Metadata
            }).ToList();

        return Result.Result.Invalid(errors);
    }
}
