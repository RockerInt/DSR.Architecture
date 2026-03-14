using System.Linq.Expressions;
using Dsr.Architecture.Domain.Aggregates;

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
}