using Dsr.Architecture.Application.Exceptions;
using Dsr.Architecture.Domain.Entities;
using Dsr.Architecture.Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Dsr.Architecture.Application.UseCases;

/// <summary>
/// Central excecution manager of MediatR for UseCases
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
/// <param name="validators">Collection of validators for the request</param>
/// <param name="logger">Logger instance for logging information and errors</param>
public class UseCaseBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators, ILogger<UseCaseBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IUseCase<TResponse>
    where TResponse : Result
{
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
        var context = new ValidationContext<TRequest>(request);

        // Perform validation
        var validationFailures = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var errors = validationFailures
            .Where(validationResult => !validationResult.IsValid)
            .SelectMany(validationResult => validationResult.Errors)
            .Select(validationFailure => new ValidationError(
                validationFailure.PropertyName,
                validationFailure.ErrorMessage))
            .ToList();

        var error = string.Empty;
        var errorCode = 0;
        Exception? exception = null;

        try
        {
            // If there are validation errors, throw an exception
            if (errors.Count != 0)
                throw new Exceptions.ValidationException(errors);

            // Log the start of the execution
            logger.LogInformation($"Start excecution of {typeof(TRequest).Name}");
            var watch = System.Diagnostics.Stopwatch.StartNew();

            // Invoke the next handler in the pipeline
            var response = await next();
            watch.Stop();

            // Log the end of the execution
            logger.LogInformation($"Finish excecution of {typeof(TRequest).Name} with {watch.ElapsedMilliseconds}ms");

            return response;
        }
        catch (Exceptions.ValidationException ex)
        {
            // Handle validation exception
            exception = ex;
            errorCode = 1;
            error = $"Validation error in {typeof(TRequest).Name}: {Environment.NewLine}{string.Join(", ", ex.Errors.Select(x => $"Property: {x.PropertyName}, Error: {x.ErrorMessage}"))}";
        }
        catch (Exception ex)
        {
            // Handle general exceptions
            exception = ex;
            errorCode = 2;
            error = $"Error in excecution of {typeof(TRequest).Name}";
        }

        // Create a response with error information
        var result = Activator.CreateInstance<TResponse>();
        if (exception is not null)
        {
            result.ErrorMessage = error ?? "Unhandle exception";
            result.ResultCode = errorCode;
            logger.LogError(exception, message: result.ErrorMessage);
        }
        return result;
    }
}