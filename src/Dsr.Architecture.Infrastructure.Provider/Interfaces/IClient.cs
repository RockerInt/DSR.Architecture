using Dsr.Architecture.Domain.Entities;

namespace Dsr.Architecture.Infrastructure.Provider.Interfaces;

public interface IClient
{
    Task<Result<TResponse>> Get<TResponse>(string path, Dictionary<string, string>? headers = null);
    Task<Result<TResponse>> Post<TResponse>(string path, string request, Dictionary<string, string>? headers = null);
    Task<Result<TResponse>> Post<TRequest, TResponse>(string path, TRequest request, Dictionary<string, string>? headers = null);
    Task<Result<TResponse>> Update<TResponse>(string path, string request, Dictionary<string, string>? headers = null);
    Task<Result<TResponse>> Update<TRequest, TResponse>(string path, TRequest request, Dictionary<string, string>? headers = null);
    Task<Result<TResponse>> Delete<TResponse>(string path, Dictionary<string, string>? headers = null);
}