using System.Linq.Expressions;
using Dsr.Architecture.Domain.Specifications.Enums;

namespace Dsr.Architecture.Domain.Specifications;

/// <summary>
/// Represents the definition of an aggregation to be applied in an analytics specification.
/// </summary>
public class AggregationDefinition(
    AggregationType type,
    LambdaExpression selector,
    string alias)
{
    /// <summary>
    /// Gets the type of aggregation (e.g., Sum, Count, Avg).
    /// </summary>
    public AggregationType Type { get; } = type;

    /// <summary>
    /// Gets the selector expression that specifies the field to be aggregated.
    /// </summary>
    public LambdaExpression Selector { get; } = selector;

    /// <summary>
    /// Gets the alias for the aggregated value in the result.
    /// </summary>
    public string Alias { get; } = alias;
}