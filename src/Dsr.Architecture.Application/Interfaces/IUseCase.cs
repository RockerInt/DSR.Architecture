using MediatR;

namespace Dsr.Architecture.Application.Interfaces;

/// <summary>
/// Defines a use case with a response.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IUseCase<TResponse> : IRequest<TResponse>
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
{
    /// <summary>
    /// Gets or sets the request.
    /// </summary>
    public new TRequest? Request { get; set; }
}
