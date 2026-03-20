using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Specifications.Interfaces;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Interfaces;

/// <summary>
/// Defines an interface for analyzing the complexity of a specification.
/// Implementations of this interface evaluate a specification and return a result indicating its complexity level 
/// and whether it is suitable for use with compiled queries.
/// </summary>
public interface ISpecificationComplexityAnalyzer
{
    /// <summary>
    /// Analyzes the given specification and calculates a complexity score based on its properties.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's unique identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <param name="spec">The specification to analyze.</param>
    /// <returns>A <see cref="SpecificationComplexityResult"/> containing the analysis outcome.</returns>
    SpecificationComplexityResult Analyze<TId, TAggregate>(
        ISpecification<TId, TAggregate> spec)
        where TAggregate : IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>;
}