using System.Net;
using Dsr.Architecture.Application.Abstractions;
using Dsr.Architecture.Domain.Exceptions;
using Dsr.Architecture.Domain.Result;
using Dsr.Architecture.Domain.Validation;

namespace Dsr.Architecture.Application.Policies;

/// <summary>
/// Default implementation of <see cref="IErrorPolicy"/> providing a centralized strategy for:
/// - Transforming exceptions and domain errors into <see cref="Result"/>
/// - Mapping domain errors and result statuses to HTTP codes
/// - Deciding logging level and retry/replay semantics.
/// </summary>
public sealed class DefaultErrorPolicy : IErrorPolicy
{
    public ErrorPolicyDecision HandleDomainException(DomainException exception)
    {
        if (exception.DomainError is null)
        {
            var fallback = Result.Error(exception.Message);
            return BuildFromResult(fallback, null, isTechnical: false);
        }

        return HandleDomainError(exception.DomainError);
    }

    public ErrorPolicyDecision HandleDomainError(DomainError error)
    {
        // Base Result from DomainError
        var result = error.Type switch
        {
            ErrorType.Validation => Result.Invalid(new Error { Code = error.Code, Identifier = error.Identifier, Message = error.Message, Type = error.Type, Severity = error.Severity, Metadata = error.Metadata }),
            ErrorType.BusinessRule => Result.Error(error.Message),
            ErrorType.Conflict => Result.Conflict(error.Message),
            ErrorType.NotFound => Result.NotFound(error.Message),
            ErrorType.Unauthorized => Result.Unauthorized(error.Message),
            ErrorType.Technical => Result.Error(error.Message),
            _ => Result.Error(error.Message)
        };

        return BuildFromResult(result, error, isTechnical: error.Type is ErrorType.Technical);
    }

    public ErrorPolicyDecision HandleException(Exception exception)
    {
        // Transient / technical exceptions that are often safe to retry
        if (exception is TimeoutException or TaskCanceledException or OperationCanceledException)
        {
            var result = Result.Unavailable("The operation timed out. Please retry.");
            return new ErrorPolicyDecision(
                result,
                (int)HttpStatusCode.ServiceUnavailable,
                LogAsError: false,
                LogAsWarning: true,
                ShouldRetry: true,
                RetryAfter: TimeSpan.FromSeconds(5),
                IsReplay: false);
        }

        // Generic technical error
        var generic = Result.CriticalError("Unexpected server error.");
        return new ErrorPolicyDecision(
            generic,
            (int)HttpStatusCode.InternalServerError,
            LogAsError: true,
            LogAsWarning: false,
            ShouldRetry: false,
            RetryAfter: null,
            IsReplay: false);
    }

    public ErrorPolicyDecision FromResult(IResult result)
        => BuildFromResult(result, null, isTechnical: false);

    private static ErrorPolicyDecision BuildFromResult(IResult result, DomainError? error, bool isTechnical)
    {
        var status = result.Status;
        var httpStatus = status switch
        {
            ResultStatus.Ok or ResultStatus.Created or ResultStatus.NoContent => (int)HttpStatusCode.OK,
            ResultStatus.Invalid => (int)HttpStatusCode.BadRequest,
            ResultStatus.NotFound => (int)HttpStatusCode.NotFound,
            ResultStatus.Forbidden => (int)HttpStatusCode.Forbidden,
            ResultStatus.Unauthorized => (int)HttpStatusCode.Unauthorized,
            ResultStatus.Conflict => (int)HttpStatusCode.Conflict,
            ResultStatus.Unavailable => (int)HttpStatusCode.ServiceUnavailable,
            ResultStatus.CriticalError => (int)HttpStatusCode.InternalServerError,
            ResultStatus.Error => (int)HttpStatusCode.InternalServerError,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var isReplay = error is not null &&
                       error.Type is ErrorType.Conflict &&
                       (error.Code.Contains("Replay", StringComparison.OrdinalIgnoreCase)
                        || error.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase));

        var shouldRetry = status is ResultStatus.Unavailable && !isReplay;

        var logAsError = status is ResultStatus.Error or ResultStatus.CriticalError;
        var logAsWarning = !logAsError && status is not ResultStatus.Ok and not ResultStatus.Created and not ResultStatus.NoContent;

        // Ensure we always return a non-generic Result instance
        var concreteResult = result switch
        {
            Result r => r,
            _ => Result.Error(result.ErrorMessages.FirstOrDefault() ?? "Error")
        };

        return new ErrorPolicyDecision(
            concreteResult,
            httpStatus,
            LogAsError: logAsError || isTechnical,
            LogAsWarning: logAsWarning && !logAsError,
            ShouldRetry: shouldRetry,
            RetryAfter: shouldRetry ? TimeSpan.FromSeconds(5) : null,
            IsReplay: isReplay);
    }
}

