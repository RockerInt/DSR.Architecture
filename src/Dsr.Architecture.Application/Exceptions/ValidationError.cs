namespace Dsr.Architecture.Application.Exceptions;

/// <summary>
/// Class representing a validation error.
/// </summary>
/// <remarks>
/// Constructor for the ValidationError class.
/// </remarks>
/// <param name="propertyName">The name of the property that caused the validation error.</param>
/// <param name="errorMessage">The error message associated with the validation error.</param>
internal class ValidationError(string propertyName, string errorMessage)
{
    /// <summary>
    /// The name of the property that caused the validation error.
    /// </summary>
    public readonly string PropertyName = propertyName;

    /// <summary>
    /// The error message associated with the validation error.
    /// </summary>
    public readonly string ErrorMessage = errorMessage;
}
