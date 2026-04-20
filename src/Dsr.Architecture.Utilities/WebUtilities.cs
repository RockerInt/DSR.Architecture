using Dsr.Architecture.Utilities.Enums;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Dsr.Architecture.Utilities;

/// <summary>
/// Utility class for handling HTTP requests and JSON serialization.
/// </summary>
public static class WebUtilities
{
    private static readonly HashSet<string> ContentHeaderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Allow",
        "Content-Disposition",
        "Content-Encoding",
        "Content-Language",
        "Content-Length",
        "Content-Location",
        "Content-MD5",
        "Content-Range",
        "Content-Type",
        "Expires",
        "Last-Modified"
    };

    /// <summary>
    /// Maps a <see cref="Method"/> to the corresponding <see cref="HttpMethod"/>.
    /// </summary>
    private static HttpMethod ToHttpMethod(Method method) => method switch
    {
        Method.Get => HttpMethod.Get,
        Method.Put => HttpMethod.Put,
        Method.Delete => HttpMethod.Delete,
        _ => HttpMethod.Post,
    };

    /// <summary>
    /// Builds a per-request <see cref="HttpRequestMessage"/> with headers applied to the message itself,
    /// so that no caller-supplied headers (including sensitive ones such as Authorization) are ever
    /// attached to a shared <see cref="HttpClient.DefaultRequestHeaders"/> collection.
    /// </summary>
    private static HttpRequestMessage BuildRequest(Method method, string path, HttpContent? content, Dictionary<string, string>? headers)
    {
        var request = new HttpRequestMessage(ToHttpMethod(method), path);

        if (content is not null)
            request.Content = content;

        if (headers is not null)
        {
            foreach (var entry in headers)
            {
                // Content headers must go on the content, not the request.
                if (ContentHeaderNames.Contains(entry.Key))
                {
                    if (request.Content is not null)
                        request.Content.Headers.TryAddWithoutValidation(entry.Key, entry.Value);
                    continue;
                }

                request.Headers.TryAddWithoutValidation(entry.Key, entry.Value);
            }
        }

        return request;
    }

    /// <summary>
    /// Creates a JSON <see cref="StringContent"/> payload from a serialized string.
    /// </summary>
    private static StringContent? CreateJsonContent(string? httpContent)
        => string.IsNullOrEmpty(httpContent)
            ? null
            : new StringContent(httpContent, Encoding.UTF8, "application/json");

    /// <summary>
    /// Asynchronously sends an HTTP request with the specified method, base address, path, and data.
    /// </summary>
    /// <typeparam name="T">Type of the data to be sent.</typeparam>
    /// <param name="method">HTTP method to be used.</param>
    /// <param name="baseAddress">Base address of the HTTP request.</param>
    /// <param name="path">Path of the HTTP request.</param>
    /// <param name="data">Data to be sent with the request.</param>
    /// <param name="headers">Optional headers to be included in the request.</param>
    /// <returns>HTTP response message.</returns>
    public async static Task<HttpResponseMessage> ConectAsync<T>(Method method, string baseAddress, string path, T data, Dictionary<string, string>? headers = null)
        => await ConectAsync(method, baseAddress, path, JsonConvert.SerializeObject(data), headers);

    /// <summary>
    /// Asynchronously sends an HTTP request with the specified method, base address, path, and content string.
    /// </summary>
    /// <param name="method">HTTP method to be used.</param>
    /// <param name="baseAddress">Base address of the HTTP request.</param>
    /// <param name="path">Path of the HTTP request.</param>
    /// <param name="httpContent">Content to be sent with the request as a string.</param>
    /// <param name="headers">Optional headers to be included in the request.</param>
    /// <returns>HTTP response message.</returns>
    public async static Task<HttpResponseMessage> ConectAsync(Method method, string baseAddress, string path, string? httpContent, Dictionary<string, string>? headers = null)
    {
        using var client = new HttpClient { BaseAddress = new Uri(baseAddress) };
        using var request = BuildRequest(method, path, CreateJsonContent(httpContent), headers);
        return await client.SendAsync(request);
    }

    /// <summary>
    /// Asynchronously sends an HTTP request with the specified method, base address, path, and data.
    /// </summary>
    /// <typeparam name="T">Type of the data to be sent.</typeparam>
    /// <param name="method">HTTP method to be used.</param>
    /// <param name="client">HTTP client to be used.</param>
    /// <param name="path">Path of the HTTP request.</param>
    /// <param name="data">Data to be sent with the request.</param>
    /// <param name="headers">Optional headers to be included in the request.</param>
    /// <returns>HTTP response message.</returns>
    public async static Task<HttpResponseMessage> ConectAsync<T>(Method method, HttpClient client, string path, T data, Dictionary<string, string>? headers = null)
        => await ConectAsync(method, client, path, JsonConvert.SerializeObject(data), headers);

    /// <summary>
    /// Asynchronously sends an HTTP request with the specified method, base address, path, and content string.
    /// Headers are applied to the per-call <see cref="HttpRequestMessage"/> instead of the shared
    /// <see cref="HttpClient.DefaultRequestHeaders"/> to avoid cross-call leakage of sensitive headers.
    /// </summary>
    /// <param name="method">HTTP method to be used.</param>
    /// <param name="client">HTTP client to be used.</param>
    /// <param name="path">Path of the HTTP request.</param>
    /// <param name="httpContent">Content to be sent with the request as a string.</param>
    /// <param name="headers">Optional headers to be included in the request.</param>
    /// <returns>HTTP response message.</returns>
    public async static Task<HttpResponseMessage> ConectAsync(Method method, HttpClient client, string path, string? httpContent, Dictionary<string, string>? headers = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        using var request = BuildRequest(method, path, CreateJsonContent(httpContent), headers);
        return await client.SendAsync(request);
    }

    /// <summary>
    /// Synchronously sends an HTTP request with the specified method, base address, path, and data.
    /// </summary>
    /// <typeparam name="T">Type of the data to be sent.</typeparam>
    /// <param name="method">HTTP method to be used.</param>
    /// <param name="baseAddress">Base address of the HTTP request.</param>
    /// <param name="path">Path of the HTTP request.</param>
    /// <param name="data">Data to be sent with the request.</param>
    /// <param name="headers">Optional headers to be included in the request.</param>
    /// <returns>HTTP response message.</returns>
    public static HttpResponseMessage Conect<T>(Method method, string baseAddress, string path, T data, Dictionary<string, string>? headers = null)
        => ConectAsync(method, baseAddress, path, data, headers).GetAwaiter().GetResult();

    /// <summary>
    /// Synchronously sends an HTTP request with the specified method, base address, path, and content string.
    /// </summary>
    /// <param name="method">HTTP method to be used.</param>
    /// <param name="baseAddress">Base address of the HTTP request.</param>
    /// <param name="path">Path of the HTTP request.</param>
    /// <param name="httpContent">Content to be sent with the request as a string.</param>
    /// <param name="headers">Optional headers to be included in the request.</param>
    /// <returns>HTTP response message.</returns>
    public static HttpResponseMessage Conect(Method method, string baseAddress, string path, string? httpContent, Dictionary<string, string>? headers = null)
        => ConectAsync(method, baseAddress, path, httpContent, headers).GetAwaiter().GetResult();

    /// <summary>
    /// Synchronously sends an HTTP request with the specified method, base address, path, and data.
    /// </summary>
    /// <typeparam name="T">Type of the data to be sent.</typeparam>
    /// <param name="method">HTTP method to be used.</param>
    /// <param name="client">HTTP client to be used.</param>
    /// <param name="path">Path of the HTTP request.</param>
    /// <param name="data">Data to be sent with the request.</param>
    /// <param name="headers">Optional headers to be included in the request.</param>
    /// <returns>HTTP response message.</returns>
    public static HttpResponseMessage Conect<T>(Method method, HttpClient client, string path, T data, Dictionary<string, string>? headers = null)
        => ConectAsync(method, client, path, data, headers).GetAwaiter().GetResult();

    /// <summary>
    /// Synchronously sends an HTTP request with the specified method, base address, path, and content string.
    /// </summary>
    /// <param name="method">HTTP method to be used.</param>
    /// <param name="client">HTTP client to be used.</param>
    /// <param name="path">Path of the HTTP request.</param>
    /// <param name="httpContent">Content to be sent with the request as a string.</param>
    /// <param name="headers">Optional headers to be included in the request.</param>
    /// <returns>HTTP response message.</returns>
    public static HttpResponseMessage Conect(Method method, HttpClient client, string path, string? httpContent, Dictionary<string, string>? headers = null)
        => ConectAsync(method, client, path, httpContent, headers).GetAwaiter().GetResult();
}
