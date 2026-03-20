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
    /// <typeparam name="TId"></typeparam>
    /// <typeparam name="TAggregate"></typeparam>
    /// <param name="spec"></param>
    /// <returns></returns>
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

        return new SpecificationComplexityResult
        {
            Score = score,
            ShouldUseCompiledQuery = score <= MaxScore,
            Reason = score <= MaxScore
                ? "simple Specification"
                : "highly complex Specification with multiple includes, ordering, pagination, or split query"
        };
    }
}