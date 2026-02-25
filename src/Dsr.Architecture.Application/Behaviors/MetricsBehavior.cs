using System.Diagnostics;
using Dsr.Architecture.Application.Abstractions;
using Dsr.Architecture.Application.UseCases;
using Dsr.Architecture.Domain.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Dsr.Architecture.Application.Behaviors;

/// <summary>
/// Pipeline behavior that records basic metrics for each use case execution.
/// Delegates to one or more <see cref="IUseCaseMetrics"/> implementations.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class MetricsBehavior<TRequest, TResponse>(
    IEnumerable<IUseCaseMetrics> metrics,
    ILogger<MetricsBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IUseCase<TResponse>
    where TResponse : IResult
{
    private readonly IReadOnlyCollection<IUseCaseMetrics> _metrics = metrics.ToList();
    private readonly ILogger<MetricsBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_metrics.Count == 0)
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();

        foreach (var metric in _metrics)
        {
            try
            {
                metric.RecordExecution(requestName, response.Status, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to record metrics for {RequestName}",
                    requestName);
            }
        }

        return response;
    }
}

