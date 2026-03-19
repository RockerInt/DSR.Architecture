using System.Linq.Expressions;
using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Specifications.Enums;

namespace Dsr.Architecture.Domain.Specifications.Interfaces;

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
    Expression<Func<TAggregate, object>>? OrderByExpression { get; }

    /// <summary>
    /// Gets the primary descending ordering expression for the query.
    /// </summary>
    Expression<Func<TAggregate, object>>? OrderByDescendingExpression{ get; }

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
    bool NoTracking { get; }

    /// <summary>
    /// Gets a value indicating whether the query should be executed as a split query.
    /// </summary>
    bool SplitQuery { get; }

    /// <summary>
    /// Gets the expected cardinality of the specification result (e.g., List, Single, First).
    /// </summary>
    SpecificationResultCardinality SpecCardinality { get; }

    /// <summary>
    /// Checks if the specification is satisfied by a specific aggregate instance.
    /// </summary>
    /// <param name="aggregate">The aggregate instance to check.</param>
    /// <returns>True if the aggregate satisfies the criteria; otherwise, false.</returns>
    bool IsSatisfiedBy(TAggregate aggregate);

    /// <summary>
    /// Asing a criteria expression to the specification. 
    /// </summary>
    /// <param name="criteria">The criteria expression to asing.</param>
    /// <returns>Self specification</returns>
    ISpecification<TId, TAggregate> Where(Expression<Func<TAggregate, bool>> criteria);

    /// <summary>
    /// Adds an include expression to the specification.
    /// </summary>
    /// <param name="include">The include expression to add.</param>
    /// <returns>Self specification</returns>
    ISpecification<TId, TAggregate> AddInclude(Expression<Func<TAggregate, object>> include);

    /// <summary>
    /// Adds an include string to the specification.
    /// </summary>
    /// <param name="includeString">The include string to add.</param>
    /// <returns>Self specification</returns>
    ISpecification<TId, TAggregate> AddInclude(string includeString);

    /// <summary>
    /// Asing an ordering expression to the specification.
    /// </summary>
    /// <param name="orderBy">The ordering expression to asing.</param>
    /// <returns>Self specification</returns>
    ISpecification<TId, TAggregate> OrderBy(Expression<Func<TAggregate, object>> orderBy);

    /// <summary>
    /// Asing a descending ordering expression to the specification.
    /// </summary>
    /// <param name="orderByDesc">The descending ordering expression to asing.</param>
    /// <returns>Self specification</returns>
    ISpecification<TId, TAggregate> OrderByDescending(Expression<Func<TAggregate, object>> orderByDesc);

    /// <summary>
    /// Asing the paging parameters (Skip and Take) to the specification.
    /// </summary>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="take">The number of records to take.</param>
    /// <returns>Self specification</returns>
    ISpecification<TId, TAggregate> Paging(int skip, int take);

    /// <summary>
    /// Asing the paging parameters based on page number and page size.
    /// Pages start from 1.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <returns>Self specification</returns>
    ISpecification<TId, TAggregate> PagingPerPages(int page, int pageSize);

    /// <summary>
    /// Configures the specification to use no tracking for the query.
    /// </summary>
    /// <returns>Self specification</returns>
    ISpecification<TId, TAggregate> AsNoTracking();

    /// <summary>    
    /// Configures the specification to use a split query.
    /// </summary>
    /// <returns>Self specification</returns>
    ISpecification<TId, TAggregate> AsSplitQuery();

    /// <summary>
    /// Asing the expected cardinality for the specification result.
    /// </summary>
    /// <param name="cardinality">The expected cardinality (List, Single, First, etc.).</param>
    /// <returns>Self specification</returns>
    ISpecification<TId, TAggregate> Cardinality(SpecificationResultCardinality cardinality);
}