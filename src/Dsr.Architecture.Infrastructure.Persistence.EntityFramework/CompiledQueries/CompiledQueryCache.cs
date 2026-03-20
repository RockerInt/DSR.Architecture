using System.Collections.Concurrent;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries;

/// <summary>
/// Implements a cache for compiled queries. It is used to cache the compiled queries for specifications to improve performance. 
/// The cache is thread-safe and can be used in a multi-threaded environment.
/// </summary>
public sealed class CompiledQueryCache
{
    private readonly ConcurrentDictionary<string, Delegate> _cache = new();

    /// <summary>
    /// Gets a compiled query from the cache or adds it if it does not exist. 
    /// </summary>
    /// <param name="key">The cache key, usually the specification fingerprint.</param>
    /// <param name="factory">A factory function that creates the compiled query delegate if not found in cache.</param>
    /// <returns>The compiled query delegate.</returns>
    public Delegate GetOrAdd(string key, Func<Delegate> factory)
        => _cache.GetOrAdd(key, _ => factory());
}