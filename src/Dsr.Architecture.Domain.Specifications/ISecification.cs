using System.Linq.Expressions;

namespace Dsr.Architecture.Domain.Specifications;

/// <summary>
/// Defines a specification for a query.
/// </summary>
/// <typeparam name="T">The type of the entity to be filtered.</typeparam>
public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// The includes of the specification. It is used to include related entities.
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// The include strings of the specification. It is used to include related entities by string.
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// The order by of the specification. It is used to order the entities.
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// The order by descending of the specification. It is used to order the entities descending.
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

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
    /// Checks if the specification is satisfied by an entity.
    /// </summary>
    /// <param name="entity">The entity to check.</param>
    /// <returns>True if the specification is satisfied, false otherwise.</returns>
    bool IsSatisfiedBy(T entity);
}