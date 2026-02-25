using Dsr.Architecture.Domain.Exceptions;
using Dsr.Architecture.Domain.Result;
using Dsr.Architecture.Domain.Validation;

namespace Dsr.Architecture.Application.Abstractions;

/// <summary>
/// Represents a centralized error policy that decides how to:
/// - Transform errors into domain <see cref="Result"/>
/// - Map domain errors to HTTP status codes
/// - Classify and handle technical errors
/// - Drive logging, retry behavior, and replay handling.
/// </summary>
public interface IErrorPolicy
{
    /// <summary>
    /// Handles a domain exception and produces a policy decision.
    /// </summary>
    ErrorPolicyDecision HandleDomainException(DomainException exception);

    /// <summary>
    /// Handles a domain validation or business error.
    /// </summary>
    ErrorPolicyDecision HandleDomainError(DomainError error);

    /// <summary>
    /// Handles an unexpected technical exception.
    /// </summary>
    ErrorPolicyDecision HandleException(Exception exception);

    /// <summary>
    /// Maps a domain <see cref="IResult"/> to an HTTP status code and logging / retry hints.
    /// </summary>
    ErrorPolicyDecision FromResult(IResult result);
}

/// <summary>
/// Describes how the system should react to a particular error.
/// </summary>
/// <param name="Result">The domain result to return.</param>
/// <param name="HttpStatusCode">The suggested HTTP status code.</param>
/// <param name="LogAsError">Whether to log as error.</param>
/// <param name="LogAsWarning">Whether to log as warning (if not error).</param>
/// <param name="ShouldRetry">Whether the caller may safely retry.</param>
/// <param name="RetryAfter">Optional suggested delay before retry.</param>
/// <param name="IsReplay">Whether the error corresponds to a replay/duplicate operation.</param>
public sealed record ErrorPolicyDecision(
    Result Result,
    int HttpStatusCode,
    bool LogAsError,
    bool LogAsWarning,
    bool ShouldRetry,
    TimeSpan? RetryAfter,
    bool IsReplay);

