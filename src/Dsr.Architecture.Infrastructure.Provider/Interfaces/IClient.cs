using Dsr.Architecture.Domain.Entities;

namespace Dsr.Architecture.Infrastructure.Provider.Interfaces;

/// <summary>
/// Defines the interface for a REST client.
/// </summary>
public interface IClient
{
    /// <summary>
    /// Performs a GET request to the server.
    /// </summary>
    /// <typeparam name="TResponse">The type of the expected response.</typeparam>
    /// <param name="path">The request path.</param>
    /// <param name="headers">Additional headers for the request.</param>
    /// <returns>A task resulting in a <see cref="Result{TResponse}"/> object containing the deserialized response.</returns>
    Task<Result<TResponse>> Get<TResponse>(string path, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Performs a POST request to the server with a request string.
    /// </summary>
    /// <typeparam name="TResponse">The type of the expected response.</typeparam>
    /// <param name="path">The request path.</param>
    /// <param name="request">The request body as a string.</param>
    /// <param name="headers">Additional headers for the request.</param>
    /// <returns>A task resulting in a <see cref="Result{TResponse}"/> object containing the deserialized response.</returns>
    Task<Result<TResponse>> Post<TResponse>(string path, string request, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Performs a POST request to the server with a request object.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the expected response.</typeparam>
    /// <param name="path">The request path.</param>
    /// <param name="request">The request body as an object.</param>
    /// <param name="headers">Additional headers for the request.</param>
    /// <returns>A task resulting in a <see cref="Result{TResponse}"/> object containing the deserialized response.</returns>
    Task<Result<TResponse>> Post<TRequest, TResponse>(string path, TRequest request, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Performs a PUT request to the server with a request string.
    /// </summary>
    /// <typeparam name="TResponse">The type of the expected response.</typeparam>
    /// <param name="path">The request path.</param>
    /// <param name="request">The request body as a string.</param>
    /// <param name="headers">Additional headers for the request.</param>
    /// <returns>A task resulting in a <see cref="Result{TResponse}"/> object containing the deserialized response.</returns>
    Task<Result<TResponse>> Update<TResponse>(string path, string request, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Performs a PUT request to the server with a request object.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the expected response.</typeparam>
    /// <param name="path">The request path.</param>
    /// <param name="request">The request body as an object.</param>
    /// <param name="headers">Additional headers for the request.</param>
    /// <returns>A task resulting in a <see cref="Result{TResponse}"/> object containing the deserialized response.</returns>
    Task<Result<TResponse>> Update<TRequest, TResponse>(string path, TRequest request, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Performs a DELETE request to the server.
    /// </summary>
    /// <typeparam name="TResponse">The type of the expected response.</typeparam>
    /// <param name="path">The request path.</param>
    /// <param name="headers">Additional headers for the request.</param>
    /// <returns>A task resulting in a <see cref="Result{TResponse}"/> object containing the deserialized response.</returns>
    Task<Result<TResponse>> Delete<TResponse>(string path, Dictionary<string, string>? headers = null);
}
