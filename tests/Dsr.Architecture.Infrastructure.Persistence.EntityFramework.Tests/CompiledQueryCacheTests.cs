using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries;
using Xunit;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Tests;

public class CompiledQueryCacheTests
{
    [Fact]
    public void DefaultCtor_GetOrAdd_CachesAndReturns()
    {
        var cache = new CompiledQueryCache();
        var callCount = 0;
        Delegate factory() { callCount++; return (Func<int>)(() => 42); }

        var first = cache.GetOrAdd("key1", factory);
        var second = cache.GetOrAdd("key1", factory);

        Assert.Same(first, second);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void DefaultCtor_TryGet_ReturnsFalseWhenMissing()
    {
        var cache = new CompiledQueryCache();

        var found = cache.TryGet("missing", out var compiled);

        Assert.False(found);
        Assert.Null(compiled);
    }

    [Fact]
    public void DefaultCtor_TryGet_ReturnsTrueAfterAdd()
    {
        var cache = new CompiledQueryCache();
        Delegate expected = (Func<int>)(() => 99);
        cache.GetOrAdd("key2", () => expected);

        var found = cache.TryGet("key2", out var compiled);

        Assert.True(found);
        Assert.Same(expected, compiled);
    }

    [Fact]
    public void DefaultCtor_DifferentKeys_StoreSeparately()
    {
        var cache = new CompiledQueryCache();
        Delegate d1 = (Func<int>)(() => 1);
        Delegate d2 = (Func<int>)(() => 2);

        cache.GetOrAdd("a", () => d1);
        cache.GetOrAdd("b", () => d2);

        cache.TryGet("a", out var resultA);
        cache.TryGet("b", out var resultB);

        Assert.Same(d1, resultA);
        Assert.Same(d2, resultB);
    }
}
