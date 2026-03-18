using System.Linq.Expressions;
using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Specifications.Enums;
using Dsr.Architecture.Domain.Specifications.Interfaces;

namespace Dsr.Architecture.Domain.Specifications;

/// <summary>
/// Base class for analytics specifications that provide grouping, aggregation, and projection capabilities.
/// </summary>
/// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregate to be analyzed.</typeparam>
public class AnalyticsSpecification<TId, TAggregate>(Expression<Func<TAggregate, bool>>? criteria = null)
    : Specification<TId, TAggregate>(criteria), IAnalyticsSpecification<TId, TAggregate>, ISpecification<TId, TAggregate>
    where TAggregate : IAggregateRoot<TId>
    where TId : IEquatable<TId>, IComparable<TId>
{
    /// <summary>
    /// Gets the expression used to group results of the query.
    /// </summary>
    public LambdaExpression? GroupByExpression { get; protected set; }

    /// <summary>
    /// Gets the list of aggregation definitions to be applied to the query results.
    /// </summary>
    public List<AggregationDefinition> Aggregations { get; } = [];

    /// <summary>
    /// Gets the expression used to filter grouped query results (equivalent to the SQL HAVING clause).
    /// </summary>
    public LambdaExpression? HavingExpression { get; protected set; }

    /// <summary>
    /// Gets the expression used to project query results to a different shape or type.
    /// </summary>
    public LambdaExpression? Projection { get; protected set; }

    /// <summary>
    /// Applies a grouping expression to the specification.
    /// </summary>
    /// <typeparam name="TKey">The type of the grouping key.</typeparam>
    /// <param name="groupBy">The grouping expression.</param>
    protected void ApplyGroupBy<TKey>(Expression<Func<TAggregate, TKey>> groupBy)
        => GroupByExpression = groupBy;

    /// <summary>
    /// Adds an aggregation definition to the specification.
    /// </summary>
    /// <typeparam name="TValue">The type of the value being aggregated.</typeparam>
    /// <param name="type">The type of aggregation (Sum, Count, etc.).</param>
    /// <param name="selector">The selector expression for the value to aggregate.</param>
    /// <param name="alias">An alias for the aggregated result.</param>
    protected void AddAggregation<TValue>(
        AggregationType type,
        Expression<Func<TAggregate, TValue>> selector,
        string alias)
        => Aggregations.Add(
            new AggregationDefinition(
                type,
                selector,
                alias));    

    /// <summary>
    /// Applies a filtering expression (HAVING) to grouped results.
    /// </summary>
    /// <typeparam name="TKey">The type of the grouping key.</typeparam>
    /// <param name="having">The filtering expression for grouped results.</param>
    protected void ApplyHaving<TKey>(Expression<Func<IGrouping<TKey, TAggregate>, bool>> having)
        => HavingExpression = having;

    /// <summary>
    /// Applies a projection expression to the query results.
    /// </summary>
    /// <typeparam name="TResult">The type of the projected result.</typeparam>
    /// <param name="projection">The projection expression.</param>
    protected void ApplyProjection<TResult>(Expression<Func<object, TResult>> projection)
        => Projection = projection;
}