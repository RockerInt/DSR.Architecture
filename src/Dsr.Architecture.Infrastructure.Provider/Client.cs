using Dsr.Architecture.Domain.Entities;
using Dsr.Architecture.Infrastructure.Provider.Interfaces;
using Dsr.Architecture.Utilities;

namespace Dsr.Architecture.Infrastructure.Provider;

/// <summary>
/// Class for REST communication with WebAPIs
/// </summary>
public class Client : IClient
{
    /// <summary>
    /// Base address of the HTTP client
    /// </summary>
    private string BaseAddress { get; set; }

    /// <summary>
    /// Base headers for all HTTP requests
    /// </summary>
    private Dictionary<string, string>? BaseHeaders { get; set; }

    /// <summary>
    /// HTTP client constructor
    /// </summary>
    /// <param name="baseAddress">Base address for requests</param>
    /// <param name="baseHeaders">Optional base headers</param>
    public Client(string baseAddress, Dictionary<string, string>? baseHeaders = null)
        => (BaseAddress, BaseHeaders) = (baseAddress, baseHeaders);

    /// <summary>
    /// Performs a GET request to the server
    /// </summary>
    /// <typeparam name="TResponse">Type of the expected response</typeparam>
    /// <param name="path">Request path</param>
    /// <param name="headers">Additional headers for the request</param>
    /// <returns>A task resulting in a Result object containing the deserialized response</returns>
    public async Task<Result<TResponse>> Get<TResponse>(string path, Dictionary<string, string>? headers = null)
    {
        var result = await WebUtilities.ConectAsync(Utilities.Enums.Method.Get, BaseAddress, path, null, ValidateHeaders(headers));

        return new Result<TResponse>(result.MapResponse<TResponse>());
    }

    /// <summary>
    /// Performs a POST request to the server with a request string
    /// </summary>
    /// <typeparam name="TResponse">Type of the expected response</typeparam>
    /// <param name="path">Request path</param>
    /// <param name="request">Request body as a string</param>
    /// <param name="headers">Additional headers for the request</param>
    /// <returns>A task resulting in a Result object containing the deserialized response</returns>
    public async Task<Result<TResponse>> Post<TResponse>(string path, string request, Dictionary<string, string>? headers = null)
    {
        var result = await WebUtilities.ConectAsync(Utilities.Enums.Method.Post, BaseAddress, path, request, ValidateHeaders(headers));

        return new Result<TResponse>(result.MapResponse<TResponse>());
    }

    /// <summary>
    /// Performs a POST request to the server with a request object
    /// </summary>
    /// <typeparam name="TRequest">Type of the request object</typeparam>
    /// <typeparam name="TResponse">Type of the expected response</typeparam>
    /// <param name="path">Request path</param>
    /// <param name="request">Request body as an object</param>
    /// <param name="headers">Additional headers for the request</param>
    /// <returns>A task resulting in a Result object containing the deserialized response</returns>
    public async Task<Result<TResponse>> Post<TRequest, TResponse>(string path, TRequest request, Dictionary<string, string>? headers = null)
    {
        var result = await WebUtilities.ConectAsync(Utilities.Enums.Method.Post, BaseAddress, path, request, ValidateHeaders(headers));

        return new Result<TResponse>(result.MapResponse<TResponse>());
    }

    /// <summary>
    /// Performs a PUT request to the server with a request string
    /// </summary>
    /// <typeparam name="TResponse">Type of the expected response</typeparam>
    /// <param name="path">Request path</param>
    /// <param name="request">Request body as a string</param>
    /// <param name="headers">Additional headers for the request</param>
    /// <returns>A task resulting in a Result object containing the deserialized response</returns>
    public async Task<Result<TResponse>> Update<TResponse>(string path, string request, Dictionary<string, string>? headers = null)
    {
        var result = await WebUtilities.ConectAsync(Utilities.Enums.Method.Put, BaseAddress, path, request, ValidateHeaders(headers));

        return new Result<TResponse>(result.MapResponse<TResponse>());
    }

    /// <summary>
    /// Performs a PUT request to the server with a request object
    /// </summary>
    /// <typeparam name="TRequest">Type of the request object</typeparam>
    /// <typeparam name="TResponse">Type of the expected response</typeparam>
    /// <param name="path">Request path</param>
    /// <param name="request">Request body as an object</param>
    /// <param name="headers">Additional headers for the request</param>
    /// <returns>A task resulting in a Result object containing the deserialized response</returns>
    public async Task<Result<TResponse>> Update<TRequest, TResponse>(string path, TRequest request, Dictionary<string, string>? headers = null)
    {
        var result = await WebUtilities.ConectAsync(Utilities.Enums.Method.Put, BaseAddress, path, request, ValidateHeaders(headers));

        return new Result<TResponse>(result.MapResponse<TResponse>());
    }

    /// <summary>
    /// Performs a DELETE request to the server
    /// </summary>
    /// <typeparam name="TResponse">Type of the expected response</typeparam>
    /// <param name="path">Request path</param>
    /// <param name="headers">Additional headers for the request</param>
    /// <returns>A task resulting in a Result object containing the deserialized response</returns>
    public async Task<Result<TResponse>> Delete<TResponse>(string path, Dictionary<string, string>? headers = null)
    {
        var result = await WebUtilities.ConectAsync(Utilities.Enums.Method.Delete, BaseAddress, path, null, ValidateHeaders(headers));

        return new Result<TResponse>(result.MapResponse<TResponse>());
    }

    /// <summary>
    /// Validates and combines the base headers with the provided headers
    /// </summary>
    /// <param name="headers">Additional headers for the request</param>
    /// <returns>A dictionary with the combined headers</returns>
    private Dictionary<string, string>? ValidateHeaders(Dictionary<string, string>? headers)
    {
        if (headers == null) return BaseHeaders;
        if (BaseHeaders == null) return headers;

        var newHeaders = new Dictionary<string, string>(BaseHeaders);

        foreach (var header in headers)
        {
            newHeaders[header.Key] = header.Value;
        }
        return newHeaders;
    }
}
