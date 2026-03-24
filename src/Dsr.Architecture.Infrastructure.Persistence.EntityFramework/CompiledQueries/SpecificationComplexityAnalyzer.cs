using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Specifications;
using Dsr.Architecture.Domain.Specifications.Interfaces;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Interfaces;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries;

/// <summary>
/// Analyzes the complexity of a specification to determine whether it is suitable for use with compiled queries.
/// The analysis is based on a scoring system that evaluates various aspects of the specification, such as the presence of criteria, includes, ordering, pagination, and whether it uses split queries.
/// </summary>
public sealed class SpecificationComplexityAnalyzer
    : ISpecificationComplexityAnalyzer
{
    private const int MaxScore = 6;

    /// <summary>
    /// Analyzes the given specification and calculates a complexity score based on its properties.
    /// The score is used to determine whether the specification should be executed using a compiled query or not.
    /// </summary>
    /// <typeparam name="TId">The type of the identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate root.</typeparam>
    /// <param name="spec">The specification to analyze.</param>
    /// <returns>A <see cref="SpecificationComplexityResult"/> containing the calculated score and whether a compiled query should be used.</returns>
    public SpecificationComplexityResult Analyze<TId, TAggregate>(
        ISpecification<TId, TAggregate> spec)
        where TAggregate : IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        int score = 0;

        if (spec.Criteria != null)
            score += 1;

        score += spec.Includes.Count * 3;
        score += spec.IncludeStrings.Count * 3;

        if (spec.OrderBy != null || spec.OrderByDescending != null)
            score += 2;

        if (spec.Skip.HasValue)
            score += 1;

        if (spec.Take.HasValue)
            score += 1;

        if (spec.SplitQuery)
            score += 3;

        // Analytics specification adds significant complexity
        if (spec is IAnalyticsSpecification<TId, TAggregate> analyticsSpec)
        {
            score += AnalyzeAnalyticsComplexity(analyticsSpec);
        }

        return new SpecificationComplexityResult
        {
            Score = score,
            ShouldUseCompiledQuery = score <= MaxScore,
            Reason = score <= MaxScore
                ? "simple Specification"
                : "highly complex Specification with multiple includes, ordering, pagination, or analytics operations"
        };
    }

    /// <summary>
    /// Analyzes the complexity of an analytics specification.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <param name="spec">The analytics specification to analyze.</param>
    /// <returns>The complexity score for analytics-specific features.</returns>
    private int AnalyzeAnalyticsComplexity<TId, TAggregate>(
        IAnalyticsSpecification<TId, TAggregate> spec)
        where TAggregate : IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        int score = 0;

        // GroupBy is complex
        if (spec.GroupByExpression != null)
            score += 4;

        // Each aggregation adds complexity
        score += spec.Aggregations.Count * 2;

        // Having clause adds complexity
        if (spec.HavingExpression != null)
            score += 3;

        // Custom projection adds complexity
        if (spec.Projection != null)
            score += 2;

        return score;
    }
}