using Dsr.Architecture.Application.Abstractions;
using Dsr.Architecture.Application.UseCases;
using Dsr.Architecture.Domain.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Dsr.Architecture.Application.Behaviors;

/// <summary>
/// Pipeline behavior that enforces idempotency for commands implementing <see cref="IIdempotentCommand"/>.
/// Uses an <see cref="IIdempotencyStore"/> to detect and short-circuit duplicate executions.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class IdempotencyBehavior<TRequest, TResponse>(
    IEnumerable<IIdempotencyStore> stores,
    ILogger<IdempotencyBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>, IIdempotentCommand
    where TResponse : IResult
{
    private readonly IIdempotencyStore? _store = stores.FirstOrDefault();
    private readonly ILogger<IdempotencyBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // If no store is configured, behave as a no-op.
        if (_store is null)
        {
            return await next();
        }

        var key = request.IdempotencyKey;

        if (string.IsNullOrWhiteSpace(key))
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;

        var existing = await _store.GetAsync(key, cancellationToken);

        if (existing is not null)
        {
            _logger.LogInformation(
                "Idempotent command {RequestName} with key {Key} detected; returning stored result.",
                requestName,
                key);

            return (TResponse)existing;
        }

        var response = await next();

        try
        {
            await _store.SaveAsync(key, response, cancellationToken);

            _logger.LogDebug(
                "Stored result for idempotent command {RequestName} with key {Key}",
                requestName,
                key);
        }
        catch (Exception ex)
        {
            // Failing to store idempotent result should not break the primary operation.
            _logger.LogError(
                ex,
                "Failed to store idempotent result for {RequestName} with key {Key}",
                requestName,
                key);
        }

        return response;
    }
}

