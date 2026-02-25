using Dsr.Architecture.Application.Abstractions;
using Dsr.Architecture.Application.UseCases;
using Dsr.Architecture.Domain.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Dsr.Architecture.Application.Behaviors;

/// <summary>
/// Pipeline behavior that performs authorization checks before executing a use case handler.
/// It delegates decision logic to one or more <see cref="IUseCaseAuthorizer{TRequest}"/> implementations.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class AuthorizationBehavior<TRequest, TResponse>(
    IEnumerable<IUseCaseAuthorizer<TRequest>> authorizers,
    ILogger<AuthorizationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IUseCase<TResponse>
    where TResponse : IResult
{
    private readonly IReadOnlyCollection<IUseCaseAuthorizer<TRequest>> _authorizers = authorizers.ToList();
    private readonly ILogger<AuthorizationBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_authorizers.Count == 0)
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;

        foreach (var authorizer in _authorizers)
        {
            var result = await authorizer.AuthorizeAsync(request, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning(
                    "Authorization failed for {RequestName} with status {Status}",
                    requestName,
                    result.Status);

                return (TResponse)(object)result;
            }
        }

        return await next();
    }
}

