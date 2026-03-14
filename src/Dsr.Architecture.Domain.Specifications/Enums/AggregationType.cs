namespace Dsr.Architecture.Domain.Specifications.Enums;

/// <summary>
/// Defines the types of aggregations that can be applied in an analytics specification.
/// </summary>
public enum AggregationType
{
    /// <summary>
    /// Calculates the sum of the values.
    /// </summary>
    Sum,

    /// <summary>
    /// Calculates the count of the items.
    /// </summary>
    Count,

    /// <summary>
    /// Finds the maximum value.
    /// </summary>
    Max,

    /// <summary>
    /// Finds the minimum value.
    /// </summary>
    Min,

    /// <summary>
    /// Calculates the average of the values.
    /// </summary>
    Avg
}