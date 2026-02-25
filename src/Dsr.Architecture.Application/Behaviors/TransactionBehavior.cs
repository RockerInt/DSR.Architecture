using Dsr.Architecture.Application.UseCases;
using Dsr.Architecture.Domain.Abstractions.Repositories;
using Dsr.Architecture.Domain.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Dsr.Architecture.Application.Behaviors;

/// <summary>
/// Pipeline behavior that commits the unit of work after a successful command execution.
/// Only runs for <see cref="ICommand{TResponse}"/>; queries are not wrapped in a transaction.
/// </summary>
/// <typeparam name="TRequest">The request type (must be ICommand).</typeparam>
/// <typeparam name="TResponse">The response type (IResult).</typeparam>
public class TransactionBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
    where TResponse : IResult
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger = logger;

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        var response = await next();

        if (response.IsSuccess)
        {
            try
            {
                await _unitOfWork.CompleteAsync(cancellationToken);
                _logger.LogDebug("Committed unit of work for {RequestName}", requestName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to commit unit of work for {RequestName}", requestName);
                return (TResponse)(object)Result.CriticalError("Failed to persist changes.");
            }
        }

        return response;
    }
}
