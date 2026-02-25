namespace Dsr.Architecture.Application.Abstractions;

/// <summary>
/// Interface for a cache service that provides methods to get, set, and remove cached data.
/// This abstraction allows for different caching implementations (e.g., in-memory, distributed cache) to be used interchangeably in the application.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Asynchronously retrieves a value from the cache based on the specified key. If the key does not exist in the cache, it returns null.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<T?> GetAsync<T>(string key);
    /// <summary>
    /// Asynchronously sets a value in the cache with the specified key and optional expiration time. 
    /// If the expiration time is not provided, the cache entry will persist indefinitely (or until the cache's default expiration policy is applied).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiration"></param>
    /// <returns></returns>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    /// <summary>
    /// Asynchronously removes a value from the cache based on the specified key. If the key does not exist, this method should complete without error.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task RemoveAsync(string key);
}
