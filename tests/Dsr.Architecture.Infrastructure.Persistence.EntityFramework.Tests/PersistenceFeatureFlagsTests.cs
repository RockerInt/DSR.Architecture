using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Observability;
using Xunit;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Tests;

public class PersistenceFeatureFlagsTests
{
    [Fact]
    public void Defaults_AllFalseExceptSampleRate()
    {
        var flags = new PersistenceFeatureFlags();

        Assert.False(flags.UseBoundedCache);
        Assert.False(flags.EnableCanonicalCacheKeys);
        Assert.False(flags.UseNewSpecificationEvaluator);
        Assert.False(flags.EnforceSpecCardinality);
        Assert.False(flags.ShadowModeEnabled);
        Assert.Equal(1.0, flags.ShadowSampleRate);
    }

    [Fact]
    public void CanSetAllFlags()
    {
        var flags = new PersistenceFeatureFlags
        {
            UseBoundedCache = true,
            EnableCanonicalCacheKeys = true,
            UseNewSpecificationEvaluator = true,
            EnforceSpecCardinality = true,
            ShadowModeEnabled = true,
            ShadowSampleRate = 0.5
        };

        Assert.True(flags.UseBoundedCache);
        Assert.True(flags.EnableCanonicalCacheKeys);
        Assert.True(flags.UseNewSpecificationEvaluator);
        Assert.True(flags.EnforceSpecCardinality);
        Assert.True(flags.ShadowModeEnabled);
        Assert.Equal(0.5, flags.ShadowSampleRate);
    }
}
