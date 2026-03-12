using System.Linq.Expressions;
using Dsr.Architecture.Domain.Aggregates;

namespace Dsr.Architecture.Domain.Specifications;

/// <summary>
/// Defines a specification for a query.
/// </summary>
/// <typeparam name="T">The type of the aggregate to be filtered.</typeparam>
public interface ISpecification<TId, TAggregate>
    where TAggregate : IAggregateRoot<TId>
    where TId : IEquatable<TId>, IComparable<TId>
{
    Expression<Func<TAggregate, bool>>? Criteria { get; }

    /// <summary>
    /// The includes of the specification. It is used to include related entities.
    /// </summary>
    List<Expression<Func<TAggregate, object>>> Includes { get; }

    /// <summary>
    /// The include strings of the specification. It is used to include related entities by string.
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// The order by of the specification. It is used to order the entities.
    /// </summary>
    Expression<Func<TAggregate, object>>? OrderBy { get; }

    /// <summary>
    /// The order by descending of the specification. It is used to order the entities descending.
    /// </summary>
    Expression<Func<TAggregate, object>>? OrderByDescending { get; }

    /// <summary>
    /// The take of the specification. It is used to limit the number of entities.
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// The skip of the specification. It is used to skip a number of entities.
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// The as no tracking of the specification. It is used to specify if the query should be executed as no tracking.
    /// </summary>
    bool AsNoTracking { get; }

    /// <summary>
    /// The as split query of the specification. It is used to specify if the query should be executed as split query.
    /// </summary>
    bool AsSplitQuery { get; }

    /// <summary>
    /// Checks if the specification is satisfied by an aggregate.
    /// </summary>
    /// <param name="aggregate">The aggregate to check.</param>
    /// <returns>True if the specification is satisfied, false otherwise.</returns>
    bool IsSatisfiedBy(TAggregate aggregate);
}