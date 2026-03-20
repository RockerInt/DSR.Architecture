using System.Collections.Concurrent;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries;

/// <summary>
/// Implements a cache for specification analysis results. It is used to cache the results of analyzing specifications to determine if they are suitable for compiled queries.
/// </summary>
public sealed class SpecificationAnalysisCache
{
    private readonly ConcurrentDictionary<string, SpecificationComplexityResult> _cache = new();

    /// <summary>
    /// Gets a specification analysis result from the cache or adds it if it does not exist. 
    /// </summary>
    /// <param name="key">The cache key, usually the specification fingerprint.</param>
    /// <param name="factory">A factory function that performs complexity analysis if not found in cache.</param>
    /// <returns>The result of the specification complexity analysis.</returns>
    public SpecificationComplexityResult GetOrAdd(
        string key,
        Func<SpecificationComplexityResult> factory)
        => _cache.GetOrAdd(key, _ => factory());
}