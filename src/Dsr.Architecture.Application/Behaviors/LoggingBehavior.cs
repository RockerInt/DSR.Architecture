using Dsr.Architecture.Application.UseCases;
using Dsr.Architecture.Domain.Result;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Dsr.Architecture.Application.Behaviors;

/// <summary>
/// Central excecution manager of MediatR for logging of UseCases execution
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
/// <param name="logger">Logger instance for logging information and errors</param>
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IUseCase<TResponse>
    where TResponse : IResult
{

    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;

    /// <summary>
    /// Handle method for logging the execution of the request and generating a response
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
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Handling {RequestName} {@Request}",
            requestName, request);

        var response = await next();

        stopwatch.Stop();

        if (response is IResult result && !(response.Status is ResultStatus.Ok or ResultStatus.NoContent or ResultStatus.Created))
        {
            _logger.LogWarning(
                "Handled {RequestName} with status {Status} in {Elapsed}ms with response {@Response}",
                requestName, result.Status, stopwatch.ElapsedMilliseconds, response);
        }
        else
        {
            _logger.LogInformation(
                "Handled {RequestName} successfully in {Elapsed}ms with response {@Response}",
                requestName, stopwatch.ElapsedMilliseconds, response);
        }

        return response;
    }
}