using System.Text.RegularExpressions;

namespace Dsr.Architecture.Domain.Validation.Extensions;

/// <summary>
/// Provides extension methods for validating string inputs and adding validation errors to the ValidationCollector.
/// </summary>
public static class ValidationCollectorStringExtensions
{
    /// <summary>
    /// Validates that the provided string input has a minimum length. If the input length is less than the specified minimum length,
    /// it adds a domain error to the collection with a message indicating that the parameter must be at least the specified number of characters. 
    /// The error includes an identifier (the parameter name), a message, a code ("min_length") to categorize the error type, and metadata containing the actual input value along with the defined minimum length. 
    /// This method is useful for validating string inputs such as names, descriptions, passwords, etc., where a certain minimum length is required for validity and to ensure that the input contains sufficient information.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="minLength"></param>
    /// <param name="paramName"></param>
    public static ValidationCollector MinLength(this ValidationCollector validation, string input, int minLength, string paramName)
    => validation.AddIf(input == null || input?.Length < minLength, paramName,
            $"{paramName} must be at least {minLength} characters long.",
            "min_length",
            ErrorType.Validation,
            ErrorSeverity.Error,
            new Dictionary<string, object?>
            {
                { "ActualValue", input },
                { "MinLength", minLength }
            });
    /// <summary>
    /// Validates that the provided string input does not exceed a maximum length. If the input length exceeds the specified maximum length,
    /// it adds a domain error to the collection with a message indicating that the parameter cannot exceed the specified number of characters. 
    /// The error includes an identifier (the parameter name), a message, a code ("max_length") to categorize the error type, and metadata containing the actual input value along with the defined maximum length. 
    /// This method is useful for validating string inputs such as names, descriptions, comments, etc., where a certain maximum length is required to ensure data integrity and prevent issues such as database truncation or UI display problems.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="maxLength"></param>
    /// <param name="paramName"></param>
    public static ValidationCollector MaxLength(this ValidationCollector validation, string input, int maxLength, string paramName)
            => validation.AddIf(input?.Length > maxLength, paramName,
                $"{paramName} cannot exceed {maxLength} characters.",
                "max_length",
                ErrorType.Validation,
                ErrorSeverity.Error,
                new Dictionary<string, object?>
                {
                    { "ActualValue", input },
                    { "MaxLength", maxLength }
                });
    /// <summary>
    /// Validates that the provided string input has an exact length. If the input length does not match the specified length,
    /// it adds a domain error to the collection with a message indicating that the parameter must be exactly the specified number of characters. 
    /// The error includes an identifier (the parameter name), a message, a code ("exact_length") to categorize the error type, and metadata containing the actual input value along with the defined exact length. 
    /// This method is useful for validating string inputs such as codes, identifiers, fixed-format strings, etc., where a specific length is required for validity and consistency across the application.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="length"></param>
    /// <param name="paramName"></param>
    public static ValidationCollector ExactLength(this ValidationCollector validation, string input, int length, string paramName)
            => validation.AddIf(input?.Length != length, paramName,
                $"{paramName} must be exactly {length} characters.",
                "exact_length",
                ErrorType.Validation,
                ErrorSeverity.Error,
                new Dictionary<string, object?>
                {
                    { "ActualValue", input },
                    { "ExactLength", length }
                });
    /// <summary>
    /// Validates that the provided string input matches a specified regular expression pattern. If the input does not match the pattern,
    /// it adds a domain error to the collection with a message indicating that the parameter format is invalid. 
    /// The error includes an identifier (the parameter name), a message, a code ("invalid_format") to categorize the error type, and metadata containing the actual input value along with the defined pattern. 
    /// This method is useful for validating string inputs such as email addresses, phone numbers, postal codes, etc., where a specific format is required for validity. 
    /// The regular expression pattern can be customized to enforce the desired format rules for the input string.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="pattern"></param>
    /// <param name="paramName"></param>
    public static ValidationCollector Matches(this ValidationCollector validation, string input, string pattern, string paramName)
            => validation.AddIf(!Regex.IsMatch(input ?? string.Empty, pattern), paramName,
                $"{paramName} has an invalid format.",
                "invalid_format",
                ErrorType.Validation,
                ErrorSeverity.Error,
                new Dictionary<string, object?>
                {
                    { "ActualValue", input },
                    { "Pattern", pattern }
                });
    

    /// <summary>
    /// Validates that the provided string input is in a valid email format. If the input does not match a basic email pattern,
    /// it adds a domain error to the collection with a message indicating that the parameter has an invalid email format. 
    /// The error includes an identifier (the parameter name), a message, a code ("invalid_email_format") to categorize the error type, and metadata containing the actual input value. 
    /// This method is useful for validating string inputs that are expected to be email addresses, ensuring that they conform to a basic email structure 
    /// (e.g., containing an "@" symbol and a domain) to prevent invalid email entries in the application. 
    /// Note that this method uses a simple regular expression for email validation, which may not cover all valid email formats but serves as a basic check for common email patterns. 
    /// For more comprehensive email validation, a more complex regular expression or a dedicated email validation library may be used.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="paramName"></param>
    public static ValidationCollector Email(this ValidationCollector validation, string input, string paramName)
            => validation.AddIf(!Regex.IsMatch(input ?? string.Empty, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"), paramName,
                $"{paramName} has an invalid email format.",
                "invalid_email_format",
                ErrorType.Validation,
                ErrorSeverity.Error,
                new Dictionary<string, object?>
                {
                    { "ActualValue", input }
                });
}
