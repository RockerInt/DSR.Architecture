using System.Linq.Expressions;
using Dsr.Architecture.Domain.Aggregates;

namespace Dsr.Architecture.Domain.Specifications;

/// <summary>
/// Defines a specification interface for querying aggregates. 
/// This interface allows for defining criteria, includes, ordering, and other properties that can be used to build queries against a data source.
/// </summary>
/// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregate to be filtered.</typeparam>
public interface ISpecification<TId, TAggregate>
    where TAggregate : IAggregateRoot<TId>
    where TId : IEquatable<TId>, IComparable<TId>
{
    /// <summary>
    /// Gets the criteria of the specification. It is used to filter the aggregates.
    /// </summary>
    Expression<Func<TAggregate, bool>>? Criteria { get; }

    /// <summary>
    /// Gets the includes of the specification. It is used to include related entities in the query.
    /// </summary>
    List<Expression<Func<TAggregate, object>>> Includes { get; }

    /// <summary>
    /// Gets the include strings of the specification. It is used to include related entities by their string names.
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Gets the primary ordering expression for the query.
    /// </summary>
    Expression<Func<TAggregate, object>>? OrderBy { get; }

    /// <summary>
    /// Gets the primary descending ordering expression for the query.
    /// </summary>
    Expression<Func<TAggregate, object>>? OrderByDescending { get; }

    /// <summary>
    /// Gets the number of records to return (Take).
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Gets the number of records to skip (Skip).
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Gets a value indicating whether the query should be executed without change tracking.
    /// </summary>
    bool AsNoTracking { get; }

    /// <summary>
    /// Gets a value indicating whether the query should be executed as a split query.
    /// </summary>
    bool AsSplitQuery { get; }

    /// <summary>
    /// Gets the expected cardinality of the specification result (e.g., List, Single, First).
    /// </summary>
    SpecificationResultCardinality Cardinality { get; }

    /// <summary>
    /// Checks if the specification is satisfied by a specific aggregate instance.
    /// </summary>
    /// <param name="aggregate">The aggregate instance to check.</param>
    /// <returns>True if the aggregate satisfies the criteria; otherwise, false.</returns>
    bool IsSatisfiedBy(TAggregate aggregate);
}