using System.Collections.Concurrent;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries;

/// <summary>
/// Implements a cache for compiled queries. Thread-safe for concurrent access.
///
/// Two modes: ConcurrentDictionary (default, unbounded) or IMemoryCache-backed (bounded).
/// The DI layer decides which backing to use via the constructor overload.
/// </summary>
public sealed class CompiledQueryCache
{
    private readonly ConcurrentDictionary<string, Delegate>? _dictionary;
    private readonly Func<string, Func<Delegate>, Delegate>? _getOrAddFn;
    private readonly Func<string, (bool, Delegate?)>? _tryGetFn;

    /// <summary>
    /// Creates a cache backed by an unbounded ConcurrentDictionary (default).
    /// </summary>
    public CompiledQueryCache()
    {
        _dictionary = new ConcurrentDictionary<string, Delegate>();
    }

    /// <summary>
    /// Creates a cache that delegates to an external implementation.
    /// </summary>
    internal CompiledQueryCache(
        Func<string, Func<Delegate>, Delegate> getOrAdd,
        Func<string, (bool, Delegate?)> tryGet)
    {
        _getOrAddFn = getOrAdd;
        _tryGetFn = tryGet;
    }

    /// <summary>
    /// Attempts to retrieve a compiled query without side effects.
    /// Returns true if found, false otherwise.
    /// </summary>
    public bool TryGet(string key, out Delegate? compiled)
    {
        if (_tryGetFn != null)
        {
            var (found, value) = _tryGetFn(key);
            compiled = value;
            return found;
        }

        return _dictionary!.TryGetValue(key, out compiled);
    }

    /// <summary>
    /// Gets a compiled query or creates one via factory.
    /// </summary>
    public Delegate GetOrAdd(string key, Func<Delegate> factory)
    {
        if (_getOrAddFn != null)
            return _getOrAddFn(key, factory);

        return _dictionary!.GetOrAdd(key, _ => factory());
    }
}
