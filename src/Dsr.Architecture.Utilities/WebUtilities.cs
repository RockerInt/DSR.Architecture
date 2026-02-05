using Dsr.Architecture.Utilities.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Dsr.Architecture.Utilities;

/// <summary>
/// Utility class for handling HTTP requests and JSON serialization.
/// </summary>
public static class WebUtilities
{
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
    {
        string httpContent = JsonConvert.SerializeObject(data);
        using var client = new HttpClient { BaseAddress = new Uri(baseAddress) };

        if (headers != null)
        {
            foreach (var entry in headers)
            {
                client.DefaultRequestHeaders.Add(entry.Key, entry.Value);
            }
        }
        else
        {
            client.DefaultRequestHeaders.Accept.Clear();
        }

        StringContent? stringContent = !string.IsNullOrEmpty(httpContent) ? new StringContent(httpContent, Encoding.UTF8, "application/json") : null;

        return method switch
        {
            Method.Get => await client.GetAsync(path),
            Method.Put => await client.PutAsync(path, stringContent),
            Method.Delete => await client.DeleteAsync(path),
            _ => await client.PostAsync(path, stringContent),
        };
    }

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

        if (headers != null)
        {
            foreach (var entry in headers)
            {
                client.DefaultRequestHeaders.Add(entry.Key, entry.Value);
            }
        }
        else
        {
            client.DefaultRequestHeaders.Accept.Clear();
        }

        StringContent? stringContent = !string.IsNullOrEmpty(httpContent) ? new StringContent(httpContent, Encoding.UTF8, "application/json") : null;

        return method switch
        {
            Method.Get => await client.GetAsync(path),
            Method.Put => await client.PutAsync(path, stringContent),
            Method.Delete => await client.DeleteAsync(path),
            _ => await client.PostAsync(path, stringContent),
        };
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
    {
        string httpContent = JsonConvert.SerializeObject(data);

        if (headers != null)
        {
            foreach (var entry in headers)
            {
                client.DefaultRequestHeaders.Add(entry.Key, entry.Value);
            }
        }
        else
        {
            client.DefaultRequestHeaders.Accept.Clear();
        }

        StringContent? stringContent = !string.IsNullOrEmpty(httpContent) ? new StringContent(httpContent, Encoding.UTF8, "application/json") : null;

        return method switch
        {
            Method.Get => await client.GetAsync(path),
            Method.Put => await client.PutAsync(path, stringContent),
            Method.Delete => await client.DeleteAsync(path),
            _ => await client.PostAsync(path, stringContent),
        };
    }

    /// <summary>
    /// Asynchronously sends an HTTP request with the specified method, base address, path, and content string.
    /// </summary>
    /// <param name="method">HTTP method to be used.</param>
    /// <param name="client">HTTP client to be used.</param>
    /// <param name="path">Path of the HTTP request.</param>
    /// <param name="httpContent">Content to be sent with the request as a string.</param>
    /// <param name="headers">Optional headers to be included in the request.</param>
    /// <returns>HTTP response message.</returns>
    public async static Task<HttpResponseMessage> ConectAsync(Method method, HttpClient client, string path, string? httpContent, Dictionary<string, string>? headers = null)
    {
        if (headers != null)
        {
            foreach (var entry in headers)
            {
                client.DefaultRequestHeaders.Add(entry.Key, entry.Value);
            }
        }
        else
        {
            client.DefaultRequestHeaders.Accept.Clear();
        }

        StringContent? stringContent = !string.IsNullOrEmpty(httpContent) ? new StringContent(httpContent, Encoding.UTF8, "application/json") : null;

        return method switch
        {
            Method.Get => await client.GetAsync(path),
            Method.Put => await client.PutAsync(path, stringContent),
            Method.Delete => await client.DeleteAsync(path),
            _ => await client.PostAsync(path, stringContent),
        };
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
        => ConectAsync(method, baseAddress, path, data, headers).Result;

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
        => ConectAsync(method, baseAddress, path, httpContent, headers).Result;

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
        => ConectAsync(method, client, path, data, headers).Result;

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
        => ConectAsync(method, client, path, httpContent, headers).Result;

    /// <summary>
    /// Validates the content of an HTTP response.
    /// </summary>
    /// <param name="httpResponse">HTTP response to validate.</param>
    /// <returns>Content of the response as a string.</returns>
    public static string? ValidateContent(this HttpResponseMessage httpResponse)
    {
        string? resp = null;
        if (httpResponse.Content.Headers.ContentLength > 0)
        {
            Stream stream = httpResponse.Content.ReadAsStreamAsync().Result;
            StreamReader sr = new(stream);
            resp = sr.ReadToEnd();
        }
        return resp;
    }


    /// <summary>
    /// Maps an HTTP response to an entity of type T.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="response">HTTP response to map.</param>
    /// <returns>Mapped entity.</returns>
    public static T? MapResponse<T>(this HttpResponseMessage response)
        => response.IsSuccessStatusCode
            ? response.ValidateContent().ToEntitySimple<T>()
            : throw HttpCallError(response);

    /// <summary>
    /// Maps an HTTP response to a list of entities of type T.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="response">HTTP response to map.</param>
    /// <returns>List of mapped entities.</returns>
    public static List<T>? MapListResponse<T>(this HttpResponseMessage response)
        => response.IsSuccessStatusCode
            ? response.ValidateContent().ToEntityListSimple<T>()
            : throw HttpCallError(response);

    /// <summary>
    /// Generates an exception based on the HTTP response.
    /// </summary>
    /// <param name="response">HTTP response that caused the error.</param>
    /// <returns>Generated exception.</returns>
    public static Exception HttpCallError(this HttpResponseMessage response)
        => new($"StatusCode: {Convert.ToInt16(response.StatusCode)}, {Environment.NewLine} Message: {response.ValidateContent()}");

}


