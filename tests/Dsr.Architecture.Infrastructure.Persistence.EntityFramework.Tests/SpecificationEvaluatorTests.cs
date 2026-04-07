using System.Linq.Expressions;
using Dsr.Architecture.Domain.Aggregates;
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

public class SpecificationEvaluatorTests
{
    private readonly DbContextOptions<TestDbContext> _options;

    public SpecificationEvaluatorTests()
    {
        var dbName = $"test_{Guid.NewGuid()}";
        _options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
    }

    private static ISpecificationEvaluator CreateEvaluator()
        => CreateEvaluatorInternal(false);

    private static ISpecificationEvaluator CreateCardinalityEvaluator()
        => CreateEvaluatorInternal(true);

    private static ISpecificationEvaluator CreateEvaluatorInternal(bool enforceCardinality)
    {
        var cache = new CompiledQueryCache();
        var analysisCache = new SpecificationAnalysisCache();
        var flags = new PersistenceFeatureFlags { EnforceSpecCardinality = enforceCardinality };
        var mockAnalyzer = new FakeComplexityAnalyzer();
        return new SpecificationEvaluator(cache, analysisCache, mockAnalyzer, flags, NullLogger<SpecificationEvaluator>.Instance);
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
    public async Task ExecuteListAsync_ReturnsAllWhenNoCriteria()
    {
        await SeedTestData();
        var evaluator = CreateEvaluator();
        var spec = new TestSpecification();

        await using var ctx = new TestDbContext(_options);
        var result = await evaluator.ExecuteListAsync(ctx, spec);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task ExecuteListAsync_FiltersByCriteria()
    {
        await SeedTestData();
        var evaluator = CreateEvaluator();
        var spec = new TestSpecification(name: "One");

        await using var ctx = new TestDbContext(_options);
        var result = await evaluator.ExecuteListAsync(ctx, spec);

        Assert.Single(result);
        Assert.Equal("One", result[0].Name);
    }

    [Fact]
    public async Task ExecuteListAsync_AppliesPaging()
    {
        await SeedTestData();
        var evaluator = CreateEvaluator();
        var spec = new TestSpecification(skip: 1, take: 2);

        await using var ctx = new TestDbContext(_options);
        var result = await evaluator.ExecuteListAsync(ctx, spec);

        Assert.Equal(2, result.Count);
        Assert.Equal("Two", result[0].Name);
    }

    [Fact]
    public async Task ExecuteSingleAsync_ReturnsFirstOrDefault_WhenNoCardinalityEnforced()
    {
        await SeedTestData();
        var evaluator = CreateEvaluator();
        var spec = new TestSpecification(name: "One");

        await using var ctx = new TestDbContext(_options);
        var result = await evaluator.ExecuteSingleAsync(ctx, spec);

        Assert.NotNull(result);
        Assert.Equal("One", result!.Name);
    }

    [Fact]
    public async Task ExecuteSingleAsync_EnforcesCardinality_First_ThrowsOnEmpty()
    {
        await SeedTestData();
        var evaluator = CreateCardinalityEvaluator();
        var spec = new TestSpecification(name: "NonExistent", cardinality: SpecificationResultCardinality.First);

        await using var ctx = new TestDbContext(_options);

        await Assert.ThrowsAsync<InvalidOperationException>(() => evaluator.ExecuteSingleAsync(ctx, spec));
    }

    [Fact]
    public async Task ExecuteSingleAsync_EnforcesCardinality_Single_ThrowsOnMultiple()
    {
        await SeedTestData();
        var evaluator = CreateCardinalityEvaluator();
        var spec = new TestSpecification(cardinality: SpecificationResultCardinality.Single);

        await using var ctx = new TestDbContext(_options);

        await Assert.ThrowsAsync<InvalidOperationException>(() => evaluator.ExecuteSingleAsync(ctx, spec));
    }

    [Fact]
    public async Task ExecuteSingleAsync_EnforcesCardinality_SingleOrDefault_ReturnsNullOnEmpty()
    {
        await SeedTestData();
        var evaluator = CreateCardinalityEvaluator();
        var spec = new TestSpecification(name: "NonExistent", cardinality: SpecificationResultCardinality.SingleOrDefault);

        await using var ctx = new TestDbContext(_options);
        var result = await evaluator.ExecuteSingleAsync(ctx, spec);

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteScalarAsync_ReturnsCount()
    {
        await SeedTestData();
        var evaluator = CreateEvaluator();
        var param = Expression.Parameter(typeof(TestAggregate));
        var spec = new TestAnalyticsSpec();

        await using var ctx = new TestDbContext(_options);
        var result = await evaluator.ExecuteScalarAsync<int, int, TestAggregate>(ctx, spec);

        Assert.Equal(3, result);
    }

    [Fact]
    public async Task CanaryExecuteAsync_ReturnsMatchingResults()
    {
        await SeedTestData();
        var canaryExecutor = CreateCanaryExecutor();
        var spec = new TestSpecification(name: "One");

        await using var ctx = new TestDbContext(_options);
        var result = await canaryExecutor.ExecuteAsync<int, TestAggregate>(ctx, spec, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("One", result[0].Name);
    }

    [Fact]
    public async Task CanaryExecuteScalarAsync_ReturnsCorrectCount()
    {
        await SeedTestData();
        var canaryExecutor = CreateCanaryExecutor();
        var spec = new TestAnalyticsSpec();

        await using var ctx = new TestDbContext(_options);
        var result = await canaryExecutor.ExecuteScalarAsync<int, int, TestAggregate>(ctx, spec, CancellationToken.None);

        Assert.Equal(3, result);
    }

    private CanarySpecificationExecutor CreateCanaryExecutor()
    {
        var evaluator = CreateEvaluator();
        var cache = new CompiledQueryCache();
        var analysisCache = new SpecificationAnalysisCache();
        var analyzer = new FakeComplexityAnalyzer();
        return new CanarySpecificationExecutor(evaluator, cache, analysisCache, analyzer);
    }
}

// ===== Test Infrastructure =====

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<TestAggregate> Aggregates => Set<TestAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<Dsr.Architecture.Domain.Validation.ValidationCollector>();
        modelBuilder.Ignore<Dsr.Architecture.Domain.Validation.DomainError>();
    }
}

public class TestAggregate : AggregateRoot<int>
{
    public string Name { get; private set; } = string.Empty;

    public TestAggregate() : base(0) { }
    public TestAggregate(int id, string name) : base(id) => Name = name;
}

public class TestSpecification : Specification<int, TestAggregate>
{
    public TestSpecification(
        string? name = null,
        int? skip = null,
        int? take = null,
        Expression<Func<TestAggregate, object>>? orderBy = null,
        SpecificationResultCardinality cardinality = SpecificationResultCardinality.List)
        : base(null)
    {
        if (name != null)
            Criteria = x => x.Name == name;

        if (orderBy != null)
            ApplyOrder(orderBy);

        if (skip.HasValue)
            ApplyPaging(skip.Value, take ?? int.MaxValue);

        if (cardinality != SpecificationResultCardinality.List)
            ApplyCardinality(cardinality);
    }
}

public class TestAnalyticsSpec : Specification<int, TestAggregate>, IAnalyticsSpecification<int, TestAggregate>
{
    private readonly List<AggregationDefinition> _aggregations = [];

    public List<AggregationDefinition> Aggregations => _aggregations;
    public LambdaExpression? GroupByExpression { get; private set; }
    public LambdaExpression? HavingExpression { get; }
    public LambdaExpression? Projection { get; private set; }

    public TestAnalyticsSpec() : base(null)
    {
        var param = Expression.Parameter(typeof(TestAggregate), "x");
        var lambda = Expression.Lambda<Func<TestAggregate, int>>(Expression.Constant(1), param);
        _aggregations.Add(new AggregationDefinition(AggregationType.Count, lambda, "TotalCount"));
    }

    public IAnalyticsSpecification<int, TestAggregate> GroupBy<TKey>(Expression<Func<TestAggregate, TKey>> groupBy)
    {
        GroupByExpression = groupBy;
        return this;
    }

    public IAnalyticsSpecification<int, TestAggregate> AddAggregation<TValue>(AggregationType type, Expression<Func<TestAggregate, TValue>> selector, string alias)
    {
        _aggregations.Add(new AggregationDefinition(type, selector, alias));
        return this;
    }

    public IAnalyticsSpecification<int, TestAggregate> Having<TKey>(Expression<Func<IGrouping<TKey, TestAggregate>, bool>> having)
    {
        return this;
    }

    public IAnalyticsSpecification<int, TestAggregate> Select<TResult>(Expression<Func<object, TResult>> projection)
    {
        Projection = projection;
        return this;
    }
}

public class FakeComplexityAnalyzer : ISpecificationComplexityAnalyzer
{
    public SpecificationComplexityResult Analyze<TId, TAggregate>(ISpecification<TId, TAggregate> spec)
        where TAggregate : IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
        => new() { ShouldUseCompiledQuery = false, Score = 1 };
}
