using Dsr.Architecture.Domain.Result;

namespace Dsr.Architecture.Application.Abstractions;

/// <summary>
/// Defines an authorization component for a specific use case request type.
/// Implementations decide whether the current user is allowed to execute the request.
/// </summary>
/// <typeparam name="TRequest">The request type to authorize.</typeparam>
public interface IUseCaseAuthorizer<in TRequest>
    where TRequest : notnull
{
    /// <summary>
    /// Authorizes the given request.
    /// Return <see cref="Result.Success"/> when authorized, or
    /// <see cref="Result.Forbidden"/> / <see cref="Result.Unauthorized"/> when not.
    /// </summary>
    Task<Result> AuthorizeAsync(TRequest request, CancellationToken cancellationToken = default);
}

