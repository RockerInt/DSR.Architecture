using System.Linq.Expressions;
using Dsr.Architecture.Domain.Specifications;
using Dsr.Architecture.Domain.Specifications.Enums;
using Dsr.Architecture.Domain.Specifications.Interfaces;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries;
using Xunit;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Tests;

public class SpecificationComplexityAnalyzerTests
{
    private readonly SpecificationComplexityAnalyzer _analyzer = new();

    [Fact]
    public void Analyze_EmptySpec_ReturnsLowScore()
    {
        var spec = new TestSpecification();

        var result = _analyzer.Analyze<int, TestAggregate>(spec);

        Assert.True(result.ShouldUseCompiledQuery);
        Assert.Equal(0, result.Score);
        Assert.Contains("simple", result.Reason);
    }

    [Fact]
    public void Analyze_WithCriteria_AddsOne()
    {
        var spec = new TestSpecification(name: "test");

        var result = _analyzer.Analyze<int, TestAggregate>(spec);

        Assert.Equal(1, result.Score);
        Assert.True(result.ShouldUseCompiledQuery);
    }

    [Fact]
    public void Analyze_WithPaging_AddsScore()
    {
        var spec = new TestSpecification(skip: 0, take: 10);

        var result = _analyzer.Analyze<int, TestAggregate>(spec);

        // skip=1 + take=1 = 2
        Assert.Equal(2, result.Score);
        Assert.True(result.ShouldUseCompiledQuery);
    }

    [Fact]
    public void Analyze_WithOrderBy_AddsTwo()
    {
        var spec = new TestSpecification(orderBy: x => x.Name);

        var result = _analyzer.Analyze<int, TestAggregate>(spec);

        Assert.Equal(2, result.Score);
    }

    [Fact]
    public void Analyze_AnalyticsSpec_WithAggregation_AddsHighScore()
    {
        var spec = new TestAnalyticsSpec();

        var result = _analyzer.Analyze<int, TestAggregate>(spec);

        // Analytics: 1 aggregation * 2 = 2
        Assert.True(result.Score >= 2);
    }

    [Fact]
    public void Analyze_HighComplexity_ShouldNotCompile()
    {
        // Create analytics spec with GroupBy + multiple aggregations
        var spec = new TestAnalyticsSpecWithGroupBy();

        var result = _analyzer.Analyze<int, TestAggregate>(spec);

        // GroupBy=4 + 2 aggregations * 2 = 8, total > 6
        Assert.True(result.Score > 6);
        Assert.False(result.ShouldUseCompiledQuery);
        Assert.Contains("complex", result.Reason);
    }
}

/// <summary>
/// Analytics spec that includes a GroupBy expression for high complexity testing.
/// </summary>
public class TestAnalyticsSpecWithGroupBy : Specification<int, TestAggregate>, IAnalyticsSpecification<int, TestAggregate>
{
    private readonly List<AggregationDefinition> _aggregations = [];

    public List<AggregationDefinition> Aggregations => _aggregations;
    public LambdaExpression? GroupByExpression { get; }
    public LambdaExpression? HavingExpression { get; }
    public LambdaExpression? Projection { get; }

    public TestAnalyticsSpecWithGroupBy() : base(null)
    {
        var param = Expression.Parameter(typeof(TestAggregate), "x");
        var nameProp = Expression.Property(param, nameof(TestAggregate.Name));
        GroupByExpression = Expression.Lambda<Func<TestAggregate, string>>(nameProp, param);
        _aggregations.Add(new AggregationDefinition(
            AggregationType.Count,
            Expression.Lambda<Func<TestAggregate, int>>(Expression.Constant(1), param),
            "TotalCount"));
        _aggregations.Add(new AggregationDefinition(
            AggregationType.Sum,
            Expression.Lambda<Func<TestAggregate, int>>(Expression.Constant(1), param),
            "SumValue"));
    }

    public IAnalyticsSpecification<int, TestAggregate> GroupBy<TKey>(Expression<Func<TestAggregate, TKey>> groupBy) => this;
    public IAnalyticsSpecification<int, TestAggregate> AddAggregation<TValue>(AggregationType type, Expression<Func<TestAggregate, TValue>> selector, string alias) => this;
    public IAnalyticsSpecification<int, TestAggregate> Having<TKey>(Expression<Func<IGrouping<TKey, TestAggregate>, bool>> having) => this;
    public IAnalyticsSpecification<int, TestAggregate> Select<TResult>(Expression<Func<object, TResult>> projection) => this;
}
