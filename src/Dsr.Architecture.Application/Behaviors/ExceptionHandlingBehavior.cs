using Dsr.Architecture.Application.Abstractions;
using Dsr.Architecture.Application.UseCases;
using Dsr.Architecture.Domain.Exceptions;
using Dsr.Architecture.Domain.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Dsr.Architecture.Application.Behaviors;

/// <summary>
/// Central excecution manager of MediatR for error handling in UseCases
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
/// <param name="logger">Logger instance for logging information and errors</param>
/// <param name="errorPolicy">Central error policy for mapping and logging decisions</param>
public class ExceptionHandlingBehavior<TRequest, TResponse>(
    ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger,
    IErrorPolicy errorPolicy)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IUseCase<TResponse>
    where TResponse : IResult
{
    /// <summary>
    /// Logger instance for logging information and errors
    /// </summary>
    private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger = logger;

    /// <summary>
    /// Centralized error policy used to transform and classify errors.
    /// </summary>
    private readonly IErrorPolicy _errorPolicy = errorPolicy;

    /// <summary>
    /// Handle method for processing the request and generating a response
    /// </summary>
    /// <param name="request">The request object</param>
    /// <param name="next">Delegate to the next action in the pipeline</param>
    /// <param name="cancellationToken">Token for cancelling the request</param>
    /// <returns>A task that represents the asynchronous operation, containing the response</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (DomainException ex)
        {
            var decision = _errorPolicy.HandleDomainException(ex);

            if (decision.LogAsError)
            {
                _logger.LogError(ex,
                    "Domain exception on {RequestName} with status {Status} and http {HttpStatus}",
                    typeof(TRequest).Name,
                    decision.Result.Status,
                    decision.HttpStatusCode);
            }
            else if (decision.LogAsWarning)
            {
                _logger.LogWarning(ex,
                    "Domain exception on {RequestName} with status {Status} and http {HttpStatus}",
                    typeof(TRequest).Name,
                    decision.Result.Status,
                    decision.HttpStatusCode);
            }

            return (TResponse)(object)decision.Result;
        }
        catch (Exception ex)
        {
            var decision = _errorPolicy.HandleException(ex);

            if (decision.LogAsError)
            {
                _logger.LogError(ex,
                    "Unhandled exception on {RequestName} with status {Status} and http {HttpStatus}",
                    typeof(TRequest).Name,
                    decision.Result.Status,
                    decision.HttpStatusCode);
            }
            else if (decision.LogAsWarning)
            {
                _logger.LogWarning(ex,
                    "Unhandled exception on {RequestName} with status {Status} and http {HttpStatus}",
                    typeof(TRequest).Name,
                    decision.Result.Status,
                    decision.HttpStatusCode);
            }

            return (TResponse)(object)decision.Result;
        }
    }
}