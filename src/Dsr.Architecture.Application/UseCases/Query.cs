using Dsr.Architecture.Domain.Result;

namespace Dsr.Architecture.Application.UseCases;

/// <summary>
/// Abstract base class for a use case with a dynamic request.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <remarks>
/// Constructor for UseCase.
/// </remarks>
/// <param name="request">Optional request object (stored as dynamic for runtime flexibility).</param>
public abstract class Query<TResponse>(object? request = null) : UseCase<TResponse>(request), IQuery<TResponse>, IUseCase<TResponse>
    where TResponse : IResult
{
}

/// <summary>
/// Abstract base class for a use case with a strongly-typed request.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public abstract class Query<TRequest, TResponse> : UseCase<TRequest, TResponse>, IQuery<TRequest, TResponse>, IUseCase<TRequest, TResponse>
    where TResponse : IResult
{
    /// <summary>
    /// Constructor for UseCase with a strongly-typed request.
    /// </summary>
    /// <param name="request">The strongly-typed request object.</param>
    public Query(TRequest? request) : base(request)
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
