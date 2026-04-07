using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries;

/// <summary>
/// Bounded compiled query cache using IMemoryCache with eviction policies.
/// Replaces the unbounded ConcurrentDictionary to prevent memory leaks.
/// </summary>
public sealed class BoundedCompiledQueryCache
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<BoundedCompiledQueryCache> _logger;
    private const string CacheLogKeyPrefix = "__compiled_query__";

    public BoundedCompiledQueryCache(
        IMemoryCache cache,
        ILogger<BoundedCompiledQueryCache> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a compiled query if already cached.
    /// </summary>
    public bool TryGet(string key, out Delegate compiled)
        => _cache.TryGetValue(key, out compiled!);

    /// <summary>
    /// Gets or creates a compiled query with bounded TTL and size.
    /// </summary>
    public Delegate GetOrAdd(string key, Func<Delegate> factory) =>
        _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            entry.SlidingExpiration = TimeSpan.FromMinutes(10);
            entry.Size = 1;
            _logger.LogDebug(
                "Compiled query cache miss, compiling: {Key}",
                key.Length > 100 ? key[..100] + "..." : key);
            return factory();
        })!;
}
