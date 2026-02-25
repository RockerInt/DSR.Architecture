using Dsr.Architecture.Domain.Result;
using MediatR;

namespace Dsr.Architecture.Application.UseCases;

/// <summary>
/// Defines a use case with a response.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IUseCase<TResponse> : IRequest<TResponse>
    where TResponse : IResult
{
    /// <summary>
    /// Gets or sets the request.
    /// </summary>
    public dynamic? Request { get; set; }
}

/// <summary>
/// Defines a use case with a request and a response.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IUseCase<TRequest, TResponse> : IUseCase<TResponse>
    where TResponse : IResult
{
    /// <summary>
    /// Gets or sets the request.
    /// </summary>
    public new TRequest? Request { get; set; }
}
