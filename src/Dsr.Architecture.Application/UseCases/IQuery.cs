using Dsr.Architecture.Domain.Result;

namespace Dsr.Architecture.Application.UseCases;

/// <summary>
/// Marks a use case as a query (read-only operation). Used for CQRS; no transaction/Unit of Work is applied.
/// </summary>
/// <typeparam name="TResponse">The type of the response, typically <see cref="Result{T}"/> or <see cref="PagedResult{T}"/>.</typeparam>
public interface IQuery<TResponse> : IUseCase<TResponse>
    where TResponse : IResult
{
}

/// <summary>
/// Marks a use case as a query with a strongly-typed request (read-only operation).
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IQuery<TRequest, TResponse> : IQuery<TResponse>, IUseCase<TRequest, TResponse>
    where TResponse : IResult
{
}
