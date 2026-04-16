using System.Linq.Expressions;
using Dsr.Architecture.Domain.Specifications.Enums;
using Dsr.Architecture.Domain.Specifications.Interfaces;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Interfaces;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Evaluators;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Tests;

public class CanarySpecificationExecutorTests
{
    private readonly DbContextOptions<TestDbContext> _options;

    public CanarySpecificationExecutorTests()
    {
        _options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase($"test_{Guid.NewGuid()}")
            .Options;
    }

    private CanarySpecificationExecutor CreateCanaryExecutor()
    {
        var cache = new CompiledQueryCache();
        var analysisCache = new SpecificationAnalysisCache();
        var analyzer = new FakeComplexityAnalyzer();
        var flags = new PersistenceFeatureFlags();
        var evaluator = new SpecificationEvaluator(cache, analysisCache, analyzer, flags, NullLogger<SpecificationEvaluator>.Instance);
        return new CanarySpecificationExecutor(evaluator, cache, analysisCache, analyzer);
    }

    private async Task SeedTestData()
    {
        await using var ctx = new TestDbContext(_options);
        ctx.AddRange(
            new TestAggregate(1, "One"),
            new TestAggregate(2, "Two"),
            new TestAggregate(3, "Three"));
        await ctx.SaveChangesAsync();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAllEntities()
    {
        await SeedTestData();
        var executor = CreateCanaryExecutor();
        var spec = new TestSpecification();

        await using var ctx = new TestDbContext(_options);
        var result = await executor.ExecuteAsync<int, TestAggregate>(ctx, spec, CancellationToken.None);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task ExecuteAsync_WithCriteria_FiltersCorrectly()
    {
        await SeedTestData();
        var executor = CreateCanaryExecutor();
        var spec = new TestSpecification(name: "Two");

        await using var ctx = new TestDbContext(_options);
        var result = await executor.ExecuteAsync<int, TestAggregate>(ctx, spec, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Two", result[0].Name);
    }

    [Fact]
    public async Task ExecuteSingleAsync_ReturnsMatchingEntity()
    {
        await SeedTestData();
        var executor = CreateCanaryExecutor();
        var spec = new TestSpecification(name: "One");

        await using var ctx = new TestDbContext(_options);
        var result = await executor.ExecuteSingleAsync<int, TestAggregate>(ctx, spec, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("One", result!.Name);
    }

    [Fact]
    public async Task ExecuteSingleAsync_NoMatch_ReturnsNull()
    {
        await SeedTestData();
        var executor = CreateCanaryExecutor();
        var spec = new TestSpecification(name: "NonExistent");

        await using var ctx = new TestDbContext(_options);
        var result = await executor.ExecuteSingleAsync<int, TestAggregate>(ctx, spec, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteScalarAsync_Count_ReturnsCorrectValue()
    {
        await SeedTestData();
        var executor = CreateCanaryExecutor();
        var spec = new TestAnalyticsSpec();

        await using var ctx = new TestDbContext(_options);
        var result = await executor.ExecuteScalarAsync<int, int, TestAggregate>(ctx, spec, CancellationToken.None);

        Assert.Equal(3, result);
    }

    [Fact]
    public async Task ExecuteDynamicSingleAsync_WithoutProjection_ReturnsEntity()
    {
        await SeedTestData();
        var executor = CreateCanaryExecutor();
        var spec = new TestSpecification(name: "Three");

        await using var ctx = new TestDbContext(_options);
        var result = await executor.ExecuteDynamicSingleAsync<int, TestAggregate>(ctx, spec, CancellationToken.None);

        Assert.NotNull(result);
        var agg = (TestAggregate)result;
        Assert.Equal("Three", agg.Name);
    }
}
