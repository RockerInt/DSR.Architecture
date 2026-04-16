using System.Linq.Expressions;
using Dsr.Architecture.Domain.Specifications;
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

public class SpecificationEvaluatorAdditionalTests
{
    private readonly DbContextOptions<TestDbContext> _options;

    public SpecificationEvaluatorAdditionalTests()
    {
        _options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase($"test_{Guid.NewGuid()}")
            .Options;
    }

    private static ISpecificationEvaluator CreateEvaluator(bool enforceCardinality = false)
    {
        var cache = new CompiledQueryCache();
        var analysisCache = new SpecificationAnalysisCache();
        var flags = new PersistenceFeatureFlags { EnforceSpecCardinality = enforceCardinality };
        var analyzer = new FakeComplexityAnalyzer();
        return new SpecificationEvaluator(cache, analysisCache, analyzer, flags, NullLogger<SpecificationEvaluator>.Instance);
    }

    private async Task SeedTestData()
    {
        await using var ctx = new TestDbContext(_options);
        ctx.AddRange(
            new TestAggregate(1, "Alpha"),
            new TestAggregate(2, "Beta"),
            new TestAggregate(3, "Gamma"));
        await ctx.SaveChangesAsync();
    }

    // ===== Apply tests =====

    [Fact]
    public async Task Apply_WithOrderByAsc_ReturnsSortedResults()
    {
        await SeedTestData();
        var evaluator = CreateEvaluator();
        var spec = new TestSpecification(orderBy: x => x.Name);

        await using var ctx = new TestDbContext(_options);
        var query = evaluator.Apply<int, TestAggregate>(ctx.Set<TestAggregate>(), spec);
        var result = await query.ToListAsync();

        Assert.Equal("Alpha", result[0].Name);
        Assert.Equal("Beta", result[1].Name);
        Assert.Equal("Gamma", result[2].Name);
    }

    [Fact]
    public async Task Apply_WithCriteria_Filters()
    {
        await SeedTestData();
        var evaluator = CreateEvaluator();
        var spec = new TestSpecification(name: "Beta");

        await using var ctx = new TestDbContext(_options);
        var query = evaluator.Apply<int, TestAggregate>(ctx.Set<TestAggregate>(), spec);
        var result = await query.ToListAsync();

        Assert.Single(result);
        Assert.Equal("Beta", result[0].Name);
    }

    [Fact]
    public async Task Apply_WithPaging_SkipsAndTakes()
    {
        await SeedTestData();
        var evaluator = CreateEvaluator();
        var spec = new TestSpecification(skip: 1, take: 1);

        await using var ctx = new TestDbContext(_options);
        var query = evaluator.Apply<int, TestAggregate>(ctx.Set<TestAggregate>(), spec);
        var result = await query.ToListAsync();

        Assert.Single(result);
    }

    // ===== ExecuteSingleAsync – no cardinality enforcement, returns null on empty =====

    [Fact]
    public async Task ExecuteSingleAsync_NoMatch_NoEnforcement_ReturnsNull()
    {
        await SeedTestData();
        var evaluator = CreateEvaluator(enforceCardinality: false);
        var spec = new TestSpecification(name: "NonExistent");

        await using var ctx = new TestDbContext(_options);
        var result = await evaluator.ExecuteSingleAsync(ctx, spec);

        Assert.Null(result);
    }

    // ===== ExecuteSingleAsync – cardinality = FirstOrDefault on empty =====

    [Fact]
    public async Task ExecuteSingleAsync_EnforcedFirstOrDefault_ReturnsNullOnEmpty()
    {
        await SeedTestData();
        var evaluator = CreateEvaluator(enforceCardinality: true);
        var spec = new TestSpecification(name: "NonExistent", cardinality: SpecificationResultCardinality.FirstOrDefault);

        await using var ctx = new TestDbContext(_options);
        var result = await evaluator.ExecuteSingleAsync(ctx, spec);

        Assert.Null(result);
    }

    // ===== ExecuteSingleAsync – cardinality = default (List) falls back to FirstOrDefault =====

    [Fact]
    public async Task ExecuteSingleAsync_EnforcedDefaultCardinality_ReturnsSome()
    {
        await SeedTestData();
        var evaluator = CreateEvaluator(enforceCardinality: true);
        // List cardinality goes to default branch → FirstOrDefault
        var spec = new TestSpecification();

        await using var ctx = new TestDbContext(_options);
        var result = await evaluator.ExecuteSingleAsync(ctx, spec);

        Assert.NotNull(result);
    }

    // ===== ExecuteScalarAsync – non-analytics throws =====

    [Fact]
    public async Task ExecuteScalarAsync_NonAnalytics_ThrowsInvalidOp()
    {
        await SeedTestData();
        var evaluator = CreateEvaluator();
        var spec = new TestSpecification();

        await using var ctx = new TestDbContext(_options);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => evaluator.ExecuteScalarAsync<int, int, TestAggregate>(ctx, spec));
    }

    // ===== ExecuteAnalyticsAsync – non-analytics spec falls back to list =====

    [Fact]
    public async Task ExecuteAnalyticsAsync_NonAnalyticsSpec_FallsBackToList()
    {
        await SeedTestData();
        var evaluator = CreateEvaluator();
        var spec = new TestSpecification();

        await using var ctx = new TestDbContext(_options);
        var result = await evaluator.ExecuteAnalyticsAsync(ctx, spec);

        // Falls back to ExecuteListAsync → Cast<dynamic>
        Assert.Equal(3, result.Count);
    }

    // ===== ExecuteAnalyticsAsync – analytics with no GroupBy and no aggregations returns empty =====

    [Fact]
    public async Task ExecuteAnalyticsAsync_NoGroupByNoAggregations_ReturnsEmpty()
    {
        await SeedTestData();
        var evaluator = CreateEvaluator();
        var spec = new EmptyAnalyticsSpec();

        await using var ctx = new TestDbContext(_options);
        var result = await evaluator.ExecuteAnalyticsAsync(ctx, spec);

        Assert.Empty(result);
    }

    // ===== ExecuteAnalyticsAsync – scalar aggregation without GroupBy returns single dict item =====

    [Fact]
    public async Task ExecuteAnalyticsAsync_ScalarAggregationCount_ReturnsSingleDictItem()
    {
        await SeedTestData();
        var evaluator = CreateEvaluator();
        var spec = new TestAnalyticsSpec();

        await using var ctx = new TestDbContext(_options);
        var result = await evaluator.ExecuteAnalyticsAsync(ctx, spec);

        Assert.Single(result);
        var dict = (IDictionary<string, object?>)result[0];
        Assert.Equal(3, dict["TotalCount"]);
    }

    // ===== ExecuteListAsync – returns all items when NoCriteria =====

    [Fact]
    public async Task ExecuteListAsync_NoCriteria_ReturnsAll()
    {
        await SeedTestData();
        var evaluator = CreateEvaluator();
        var spec = new TestSpecification();

        await using var ctx = new TestDbContext(_options);
        var result = await evaluator.ExecuteListAsync(ctx, spec);

        Assert.Equal(3, result.Count);
    }

    // ===== ExecuteSingleAsync – Enforced Single with exactly one match succeeds =====

    [Fact]
    public async Task ExecuteSingleAsync_EnforcedSingle_ExactlyOneMatch_Succeeds()
    {
        await SeedTestData();
        var evaluator = CreateEvaluator(enforceCardinality: true);
        var spec = new TestSpecification(name: "Alpha", cardinality: SpecificationResultCardinality.Single);

        await using var ctx = new TestDbContext(_options);
        var result = await evaluator.ExecuteSingleAsync(ctx, spec);

        Assert.NotNull(result);
        Assert.Equal("Alpha", result!.Name);
    }

    // ===== ExecuteSingleAsync – Enforced Single with zero matches throws =====

    [Fact]
    public async Task ExecuteSingleAsync_EnforcedSingle_ZeroMatches_Throws()
    {
        await SeedTestData();
        var evaluator = CreateEvaluator(enforceCardinality: true);
        var spec = new TestSpecification(name: "NonExistent", cardinality: SpecificationResultCardinality.Single);

        await using var ctx = new TestDbContext(_options);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => evaluator.ExecuteSingleAsync(ctx, spec));
    }

    // ===== ExecuteSingleAsync – Enforced SingleOrDefault multiple throws =====

    [Fact]
    public async Task ExecuteSingleAsync_EnforcedSingleOrDefault_MultipleMatches_Throws()
    {
        await SeedTestData();
        var evaluator = CreateEvaluator(enforceCardinality: true);
        // No name filter → 3 matches
        var spec = new TestSpecification(cardinality: SpecificationResultCardinality.SingleOrDefault);

        await using var ctx = new TestDbContext(_options);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => evaluator.ExecuteSingleAsync(ctx, spec));
    }

    // ===== ExecuteSingleAsync – Enforced First with match succeeds =====

    [Fact]
    public async Task ExecuteSingleAsync_EnforcedFirst_WithMatch_ReturnsFirst()
    {
        await SeedTestData();
        var evaluator = CreateEvaluator(enforceCardinality: true);
        var spec = new TestSpecification(cardinality: SpecificationResultCardinality.First);

        await using var ctx = new TestDbContext(_options);
        var result = await evaluator.ExecuteSingleAsync(ctx, spec);

        Assert.NotNull(result);
    }
}

/// <summary>
/// Analytics spec with no aggregations and no GroupBy, for testing the empty-result path.
/// </summary>
public class EmptyAnalyticsSpec : Specification<int, TestAggregate>, IAnalyticsSpecification<int, TestAggregate>
{
    public List<AggregationDefinition> Aggregations { get; } = [];
    public LambdaExpression? GroupByExpression => null;
    public LambdaExpression? HavingExpression => null;
    public LambdaExpression? Projection => null;

    public EmptyAnalyticsSpec() : base(null) { }

    public IAnalyticsSpecification<int, TestAggregate> GroupBy<TKey>(Expression<Func<TestAggregate, TKey>> groupBy) => this;
    public IAnalyticsSpecification<int, TestAggregate> AddAggregation<TValue>(AggregationType type, Expression<Func<TestAggregate, TValue>> selector, string alias) => this;
    public IAnalyticsSpecification<int, TestAggregate> Having<TKey>(Expression<Func<IGrouping<TKey, TestAggregate>, bool>> having) => this;
    public IAnalyticsSpecification<int, TestAggregate> Select<TResult>(Expression<Func<object, TResult>> projection) => this;
}
