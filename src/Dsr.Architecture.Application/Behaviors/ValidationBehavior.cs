using Dsr.Architecture.Application.UseCases;
using Dsr.Architecture.Domain.Result;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Dsr.Architecture.Application.Behaviors;


/// <summary>
/// Central excecution manager of MediatR for validation of UseCases with FluentValidation
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
/// <param name="validators">Collection of validators for the request</param>
/// <param name="logger">Logger instance for logging information and errors</param>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IUseCase<TResponse>
    where TResponse : IResult
{
    /// <summary>
    /// Collection of validators for the request
    /// </summary>
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    /// <summary>
    /// Logger instance for logging information and errors
    /// </summary>
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger = logger;

    /// <summary>
    /// Handle method for processing the validation using FluentValidation
    /// </summary>
    /// <param name="request"></param>
    /// <param name="next"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.AsErrors())
            .ToList();

        if (errors.Count != 0)
        {
            _logger.LogError($"Validation for {typeof(TRequest).Name} with errors: {string.Join(", ", errors.Select(e => $"Property: {e.Identifier}, Error: {e.Message}"))}");
            return (TResponse)(object)Result.Invalid(errors);
        }

        return await next();
    }
}
