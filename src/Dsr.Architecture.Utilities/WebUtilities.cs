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
    // JSON settings to ignore circular references and format with indentation.
    private static readonly JsonSerializerSettings _settings = new()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        Formatting = Formatting.Indented
    };

    // Enum to define HTTP methods.
    public enum Method
    {
        Get,
        Post,
        Put,
        Delete
    }

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
    /// Serializes an object to JSON using the specified settings.
    /// </summary>
    /// <param name="obj">Object to serialize.</param>
    /// <param name="settings">Optional JSON serialization settings.</param>
    /// <returns>Serialized JSON string.</returns>
    public static string JsonSerialize(this object obj, JsonSerializerSettings? settings = null)
        => JsonConvert.SerializeObject(obj, settings ?? _settings);

    /// <summary>
    /// Deserializes a JSON string to an entity of type T.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="obj">JSON string to deserialize.</param>
    /// <returns>Deserialized entity.</returns>
    public static T ToEntity<T>(this string obj) where T : class, new()
    {
        T? returnObj = ToEntitySimple<T>(obj);

        if (returnObj != null) return returnObj;

        returnObj = new();
        PropertyInfo[] piProperties = typeof(T).GetProperties();
        JObject jObj = JObject.Parse(obj);

        foreach (PropertyInfo piProperty in piProperties)
        {
            try
            {
                string? stringValue = (string?)jObj.SelectToken(piProperty.Name);
                if (stringValue != null)
                {
                    TypeConverter tc = TypeDescriptor.GetConverter(piProperty.PropertyType);
                    piProperty.SetValue(returnObj, tc.ConvertFromString(null, CultureInfo.InvariantCulture, stringValue));
                }
            }
            catch { }
        }

        return returnObj;
    }

    /// <summary>
    /// Deserializes a JSON string to a simple entity of type T.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="obj">JSON string to deserialize.</param>
    /// <returns>Deserialized entity.</returns>
    public static T? ToEntitySimple<T>(this string? obj)
        => obj != null ? JsonConvert.DeserializeObject<T>(obj) : default;

    /// <summary>
    /// Deserializes a JSON string to a list of entities of type T.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <typeparam name="D">Type of the sub-entity for nested lists.</typeparam>
    /// <param name="objs">JSON string to deserialize.</param>
    /// <returns>List of deserialized entities.</returns>
    public static List<T> ToEntityList<T, D>(this string objs) where T : class, new() where D : class, new()
    {
        PropertyInfo[] piProperties = typeof(T).GetProperties();
        List<T> returnObjList = new();
        var jArray = JsonConvert.DeserializeObject<dynamic[]>(objs);

        if (jArray != null)
        {
            foreach (var obj in jArray)
            {
                if (obj != null)
                {
                    JObject jObj = JObject.Parse(obj.ToString());
                    T preReturnObj = new();
                    foreach (PropertyInfo piProperty in piProperties)
                    {
                        if (!typeof(System.Collections.IList).IsAssignableFrom(piProperty.PropertyType))
                        {
                            string? stringValue = (string?)jObj.SelectToken(piProperty.Name);
                            if (stringValue != null)
                            {
                                TypeConverter tc = TypeDescriptor.GetConverter(piProperty.PropertyType);
                                piProperty.SetValue(preReturnObj, tc.ConvertFromString(null, CultureInfo.InvariantCulture, stringValue));
                            }
                        }
                        else if (!typeof(T).Equals(typeof(D)))
                        {
                            if (!string.IsNullOrWhiteSpace(jObj.SelectToken(piProperty.Name)?.ToString()))
                            {
                                JArray jArraySub = JArray.Parse(jObj.SelectToken(piProperty.Name)?.ToString() ?? string.Empty);
                                List<D> asingObjSub = ToEntityList<D, D>(jArraySub.ToString());
                                piProperty.SetValue(preReturnObj, asingObjSub);
                            }
                        }
                    }
                    returnObjList.Add(preReturnObj);
                }
            }
        }
        return returnObjList;
    }

    /// <summary>
    /// Deserializes a JSON string to a list of simple entities of type T.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="objs">JSON string to deserialize.</param>
    /// <returns>List of deserialized entities.</returns>
    public static List<T>? ToEntityListSimple<T>(this string? objs)
        => objs != null ? JsonConvert.DeserializeObject<List<T>>(objs) : null;

    /// <summary>
    /// Deserializes a JSON string to a dictionary.
    /// </summary>
    /// <typeparam name="T">Type of the key.</typeparam>
    /// <typeparam name="D">Type of the value.</typeparam>
    /// <param name="objs">JSON string to deserialize.</param>
    /// <returns>Deserialized dictionary.</returns>
    public static Dictionary<T, D>? ToDictionary<T, D>(this string objs) where T : struct
        => JsonConvert.DeserializeObject<Dictionary<T, D>>(objs);

    /// <summary>
    /// Tries to parse a JSON string to an object of type T.
    /// </summary>
    /// <typeparam name="T">Type of the object.</typeparam>
    /// <param name="json">JSON string to parse.</param>
    /// <param name="result">Parsed object, if successful.</param>
    /// <returns>True if parsing was successful, otherwise false.</returns>
    public static bool TryParseJson<T>(this string json, out T? result)
    {
        bool success = true;

        if (string.IsNullOrEmpty(json))
        {
            success = false;
            result = default;
            return success;
        }

        var settings = new JsonSerializerSettings
        {
            Error = (sender, args) => { success = false; args.ErrorContext.Handled = true; },
            MissingMemberHandling = MissingMemberHandling.Error
        };

        result = JsonConvert.DeserializeObject<T>(json, settings);
        return success;
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


