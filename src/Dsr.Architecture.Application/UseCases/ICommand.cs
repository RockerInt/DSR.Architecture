using Dsr.Architecture.Domain.Result;

namespace Dsr.Architecture.Application.UseCases;

/// <summary>
/// Marks a use case as a command (write operation). Used for CQRS and to apply transaction behavior.
/// </summary>
/// <typeparam name="TResponse">The type of the response, typically <see cref="Result"/> or <see cref="Result{T}"/>.</typeparam>
public interface ICommand<TResponse> : IUseCase<TResponse>
    where TResponse : IResult
{
}

/// <summary>
/// Marks a use case as a command with a strongly-typed request (write operation).
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface ICommand<TRequest, TResponse> : ICommand<TResponse>, IUseCase<TRequest, TResponse>
    where TResponse : IResult
{
}
