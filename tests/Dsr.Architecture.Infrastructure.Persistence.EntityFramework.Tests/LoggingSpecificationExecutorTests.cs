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

public class LoggingSpecificationExecutorTests
{
    private readonly DbContextOptions<TestDbContext> _options;

    public LoggingSpecificationExecutorTests()
    {
        _options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase($"test_{Guid.NewGuid()}")
            .Options;
    }

    private LoggingSpecificationExecutor CreateLoggingExecutor()
    {
        var cache = new CompiledQueryCache();
        var analysisCache = new SpecificationAnalysisCache();
        var analyzer = new FakeComplexityAnalyzer();
        var inner = new AutoCompiledSpecificationExecutor(cache, analysisCache, analyzer);
        return new LoggingSpecificationExecutor(inner, NullLogger<LoggingSpecificationExecutor>.Instance);
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
    public async Task ExecuteAsync_LogsAndReturnsResults()
    {
        await SeedTestData();
        var executor = CreateLoggingExecutor();
        var spec = new TestSpecification();

        await using var ctx = new TestDbContext(_options);
        var result = await executor.ExecuteAsync<int, TestAggregate>(ctx, spec, CancellationToken.None);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task ExecuteAsync_WithCriteria_LogsAndFilters()
    {
        await SeedTestData();
        var executor = CreateLoggingExecutor();
        var spec = new TestSpecification(name: "Two");

        await using var ctx = new TestDbContext(_options);
        var result = await executor.ExecuteAsync<int, TestAggregate>(ctx, spec, CancellationToken.None);

        Assert.Single(result);
    }

    [Fact]
    public async Task ExecuteScalarAsync_LogsAndReturnsCount()
    {
        await SeedTestData();
        var executor = CreateLoggingExecutor();
        var spec = new TestAnalyticsSpec();

        await using var ctx = new TestDbContext(_options);
        var result = await executor.ExecuteScalarAsync<int, int, TestAggregate>(ctx, spec, CancellationToken.None);

        Assert.Equal(3, result);
    }

    [Fact]
    public async Task ExecuteSingleAsync_LogsAndReturnsResult()
    {
        await SeedTestData();
        var executor = CreateLoggingExecutor();
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
        var executor = CreateLoggingExecutor();
        var spec = new TestSpecification(name: "NonExistent");

        await using var ctx = new TestDbContext(_options);
        var result = await executor.ExecuteSingleAsync<int, TestAggregate>(ctx, spec, CancellationToken.None);

        Assert.Null(result);
    }
}
