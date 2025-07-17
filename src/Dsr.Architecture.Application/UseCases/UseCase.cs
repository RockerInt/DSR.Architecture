using Dsr.Architecture.Application.Interfaces;
using MediatR;

namespace Dsr.Architecture.Application.UseCases;

/// <summary>
/// Abstract base class for a use case with a dynamic request.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <remarks>
/// Constructor for UseCase.
/// </remarks>
/// <param name="request">Optional dynamic request object.</param>
public abstract class UseCase<TResponse>(dynamic? request = null) : IUseCase<TResponse>
{
    /// <summary>
    /// The dynamic request object.
    /// </summary>
    public dynamic? Request { get; set; } = request;
}

/// <summary>
/// Abstract base class for a use case with a strongly-typed request.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public abstract class UseCase<TRequest, TResponse> : UseCase<TResponse>, IUseCase<TRequest, TResponse>
{
    /// <summary>
    /// Constructor for UseCase with a strongly-typed request.
    /// </summary>
    /// <param name="request">The strongly-typed request object.</param>
    public UseCase(TRequest? request) : base(request)
        => Request = request;

    /// <summary>
    /// The strongly-typed request object.
    /// </summary>
    public new TRequest? Request
    {
        get => (TRequest?)base.Request;
        set => base.Request = value;
    }
}
