using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries;
using Xunit;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Tests;

public class SpecificationAnalysisCacheTests
{
    [Fact]
    public void GetOrAdd_CachesResult()
    {
        var cache = new SpecificationAnalysisCache();
        var callCount = 0;

        var result1 = cache.GetOrAdd("key1", () =>
        {
            callCount++;
            return new SpecificationComplexityResult { Score = 5, ShouldUseCompiledQuery = true, Reason = "test" };
        });

        var result2 = cache.GetOrAdd("key1", () =>
        {
            callCount++;
            return new SpecificationComplexityResult { Score = 99, ShouldUseCompiledQuery = false, Reason = "other" };
        });

        Assert.Equal(1, callCount);
        Assert.Same(result1, result2);
        Assert.Equal(5, result1.Score);
    }

    [Fact]
    public void GetOrAdd_DifferentKeys_StoresSeparately()
    {
        var cache = new SpecificationAnalysisCache();

        var result1 = cache.GetOrAdd("a", () => new SpecificationComplexityResult { Score = 1 });
        var result2 = cache.GetOrAdd("b", () => new SpecificationComplexityResult { Score = 2 });

        Assert.Equal(1, result1.Score);
        Assert.Equal(2, result2.Score);
    }
}
