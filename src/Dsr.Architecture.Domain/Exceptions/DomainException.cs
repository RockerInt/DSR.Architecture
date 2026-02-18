using Dsr.Architecture.Domain.Validation;

namespace Dsr.Architecture.Domain.Exceptions;

/// <summary>
/// Base class for exceptions that occur within the domain layer. 
/// This class can be extended to create specific domain exceptions that include additional context or information about the error. 
/// The DomainError property can be used to provide structured information about the error, which can be useful for logging, debugging, 
/// or providing feedback to the user. By using a base DomainException class, you can ensure that all domain-related exceptions are 
/// handled consistently and can be easily identified and managed within the application.
/// </summary>
/// <param name="message"></param>
/// <param name="domainError"></param>
public abstract class DomainException(string message, DomainError? domainError = null) : Exception(message)
{
    /// <summary>
    /// Gets the DomainError associated with this exception, providing structured information about the error that occurred within the domain layer.
    /// </summary>
    public DomainError? DomainError { get; } = domainError;
}