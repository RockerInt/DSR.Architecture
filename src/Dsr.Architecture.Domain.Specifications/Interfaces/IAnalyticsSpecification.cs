using System.Linq.Expressions;
using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Specifications.Enums;

namespace Dsr.Architecture.Domain.Specifications.Interfaces;

/// <summary>
/// Defines a specification interface for analytics queries that include grouping, aggregations, and projections.
/// </summary>
/// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregate to be analyzed.</typeparam>
public interface IAnalyticsSpecification<TId, TAggregate>
    : ISpecification<TId, TAggregate>
    where TAggregate : IAggregateRoot<TId>
    where TId : IEquatable<TId>, IComparable<TId>
{
    /// <summary>
    /// Gets the expression used to group results of the query.
    /// </summary>
    LambdaExpression? GroupByExpression { get; }

    /// <summary>
    /// Gets the list of aggregation definitions to be applied to the query results.
    /// </summary>
    List<AggregationDefinition> Aggregations { get; }

    /// <summary>
    /// Gets the expression used to filter grouped query results (equivalent to the SQL HAVING clause).
    /// </summary>
    LambdaExpression? HavingExpression { get; }

    /// <summary>
    /// Gets the expression used to project query results to a different shape or type.
    /// </summary>
    LambdaExpression? Projection { get; }

    /// <summary>
    /// Asing a grouping expression to the specification.
    /// </summary>
    /// <typeparam name="TKey">The type of the grouping key.</typeparam>
    /// <param name="groupBy">The grouping expression.</param>
    IAnalyticsSpecification<TId, TAggregate> GroupBy<TKey>(Expression<Func<TAggregate, TKey>> groupBy);

    /// <summary>
    /// Adds an aggregation definition to the specification.
    /// </summary>
    /// <typeparam name="TValue">The type of the value being aggregated.</typeparam>
    /// <param name="type">The type of aggregation (Sum, Count, etc.).</param>
    /// <param name="selector">The selector expression for the value to aggregate.</param>
    /// <param name="alias">An alias for the aggregated result.</param>
    IAnalyticsSpecification<TId, TAggregate> AddAggregation<TValue>(
        AggregationType type,
        Expression<Func<TAggregate, TValue>> selector,
        string alias);

    /// <summary>
    /// Asing a filtering expression (HAVING) to grouped results.
    /// </summary>
    /// <typeparam name="TKey">The type of the grouping key.</typeparam>
    /// <param name="having">The filtering expression for grouped results.</param>
    IAnalyticsSpecification<TId, TAggregate> Having<TKey>(Expression<Func<IGrouping<TKey, TAggregate>, bool>> having);

    /// <summary>
    /// Asing a projection expression to the query results.
    /// </summary>
    /// <typeparam name="TResult">The type of the projected result.</typeparam>
    /// <param name="projection">The projection expression.</param>
    IAnalyticsSpecification<TId, TAggregate> Select<TResult>(Expression<Func<object, TResult>> projection);
}