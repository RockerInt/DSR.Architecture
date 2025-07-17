using MediatR;

namespace Dsr.Architecture.Application.Interfaces;

public interface IUseCase<TResponse> : IRequest<TResponse>
{
    public dynamic? Request { get; set; }
}
public interface IUseCase<TRequest, TResponse> : IUseCase<TResponse>
{
    public new TRequest? Request { get; set; }
}