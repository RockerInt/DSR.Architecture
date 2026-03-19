using System.Linq.Expressions;
using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Specifications.Enums;
using Dsr.Architecture.Domain.Specifications.Extensions;
using Dsr.Architecture.Domain.Specifications.Interfaces;

namespace Dsr.Architecture.Domain.Specifications;

/// <summary>
/// Base class for a specification that can be used to filter and shape queries for aggregates.
/// </summary>
/// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregate to be filtered.</typeparam>
public class Specification<TId, TAggregate>(Expression<Func<TAggregate, bool>>? criteria = null) : ISpecification<TId, TAggregate>
    where TAggregate : IAggregateRoot<TId>
    where TId : IEquatable<TId>, IComparable<TId>
{
    #region Properties

    /// <summary>
    /// Gets or sets the criteria of the specification. It is used to filter the aggregates.
    /// </summary>
    public Expression<Func<TAggregate, bool>>? Criteria { get; protected set; } = criteria;

    /// <summary>
    /// Gets the list of include expressions for the specification. It is used to include related entities in the query.
    /// </summary>
    public List<Expression<Func<TAggregate, object>>> Includes { get; } = [];

    /// <summary>
    /// Gets the list of include strings for the specification. It is used to include related entities by their string names.
    /// </summary>
    public List<string> IncludeStrings { get; } = [];

    /// <summary>
    /// Gets the primary ordering expression for the specification.
    /// </summary>
    public Expression<Func<TAggregate, object>>? OrderByExpression { get; protected set; }

    /// <summary>
    /// Gets the primary descending ordering expression for the specification.
    /// </summary>
    public Expression<Func<TAggregate, object>>? OrderByDescendingExpression { get; protected set; }

    /// <summary>
    /// Gets the number of records to return (Take).
    /// </summary>
    public int? Take { get; protected set; }

    /// <summary>
    /// Gets the number of records to skip (Skip).
    /// </summary>
    public int? Skip { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the query should be executed without change tracking.
    /// Defaults to true.
    /// </summary>
    public bool NoTracking { get; protected set; } = true;


    /// <summary>
    /// Gets a value indicating whether the query should be executed as a split query.
    /// </summary>
    public bool SplitQuery { get; protected set; }

    /// <summary>
    /// Gets the expected cardinality of the specification result.
    /// Defaults to <see cref="SpecificationResultCardinality.List"/>.
    /// </summary>
    public SpecificationResultCardinality SpecCardinality { get; protected set; } = SpecificationResultCardinality.List;

    #endregion Properties

    #region Properties Methods

    /// <summary>
    /// Checks if the specification is satisfied by a specific aggregate instance.
    /// </summary>
    /// <param name="aggregate">The aggregate instance to check.</param>
    /// <returns>True if the aggregate satisfies the criteria; otherwise, false.</returns>
    public virtual bool IsSatisfiedBy(TAggregate aggregate)
    {
        if (Criteria == null)
            return true;

        var compiled = Criteria.Compile();
        return compiled(aggregate);
    }

    /// <summary>
    /// Adds an include expression to the specification.
    /// </summary>
    /// <param name="include">The include expression to add.</param>
    public ISpecification<TId, TAggregate> AddInclude(Expression<Func<TAggregate, object>> include)
    {
        Includes.Add(include);
        return this;
    }

    /// <summary>
    /// Adds an include string to the specification.
    /// </summary>
    /// <param name="includeString">The include string to add.</param>
    public ISpecification<TId, TAggregate> AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
        return this;
    }

    /// <summary>
    /// Applies an ordering expression to the specification.
    /// </summary>
    /// <param name="orderBy">The ordering expression to apply.</param>
    public void ApplyOrder(Expression<Func<TAggregate, object>> orderBy)
        => OrderByExpression = orderBy;

    /// <summary>
    /// Applies a descending ordering expression to the specification.
    /// </summary>
    /// <param name="orderByDesc">The descending ordering expression to apply.</param>
    public void ApplyOrderDescending(Expression<Func<TAggregate, object>> orderByDesc)
        => OrderByDescendingExpression = orderByDesc;

    /// <summary>
    /// Applies paging parameters (Skip and Take) to the specification.
    /// </summary>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="take">The number of records to take.</param>
    public void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    /// <summary>
    /// Applies paging parameters based on page number and page size.
    /// Pages start from 1.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of records per page.</param>
    public void ApplyPagingPerPages(int page, int pageSize)
    {
        Skip = (page - 1) * pageSize;
        Take = pageSize;
    }

    /// <summary>
    /// Applies the expected cardinality for the specification result.
    /// </summary>
    /// <param name="cardinality">The expected cardinality (List, Single, First, etc.).</param>
    public void ApplyCardinality(SpecificationResultCardinality cardinality)
        => SpecCardinality = cardinality;

    /// <summary>
    /// Asing a criteria expression to the specification. 
    /// </summary>
    /// <param name="criteria">The criteria expression to asing.</param>
    /// <returns>Self specification</returns>
    public ISpecification<TId, TAggregate> Where(Expression<Func<TAggregate, bool>> criteria)
    {
        Criteria = criteria;
        return this;
    }

    /// <summary>
    /// Asing an ordering expression to the specification.
    /// </summary>
    /// <param name="orderBy">The ordering expression to asing.</param>
    /// <returns>Self specification</returns>
    public ISpecification<TId, TAggregate> OrderBy(Expression<Func<TAggregate, object>> orderBy)
    {
        ApplyOrder(orderBy);
        return this;
    }

    /// <summary>
    /// Asing a descending ordering expression to the specification.
    /// </summary>
    /// <param name="orderByDesc">The descending ordering expression to asing.</param>
    /// <returns>Self specification</returns>
    public ISpecification<TId, TAggregate> OrderByDescending(Expression<Func<TAggregate, object>> orderByDesc)
    {
        ApplyOrderDescending(orderByDesc);
        return this;
    }

    /// <summary>
    /// Asing the paging parameters (Skip and Take) to the specification.
    /// </summary>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="take">The number of records to take.</param>
    /// <returns>Self specification</returns>
    public ISpecification<TId, TAggregate> Paging(int skip, int take)
    {
        ApplyPaging(skip, take);
        return this;
    }

    /// <summary>
    /// Asing the paging parameters based on page number and page size.
    /// Pages start from 1.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <returns>Self specification</returns>
    public ISpecification<TId, TAggregate> PagingPerPages(int page, int pageSize)
    {
        ApplyPagingPerPages(page, pageSize);
        return this;
    }

    /// <summary>
    /// Configures the specification to use no tracking for the query.
    /// </summary>
    /// <returns>Self specification</returns>
    public ISpecification<TId, TAggregate> AsNoTracking()
    {
        NoTracking = true;
        return this;
    }

    /// <summary>    
    /// Configures the specification to use a split query.
    /// </summary>
    /// <returns>Self specification</returns>
    public ISpecification<TId, TAggregate> AsSplitQuery()
    {
        SplitQuery = true;
        return this;
    }

    /// <summary>
    /// Asing the expected cardinality for the specification result.
    /// </summary>
    /// <param name="cardinality">The expected cardinality (List, Single, First, etc.).</param>
    /// <returns>Self specification</returns>
    public ISpecification<TId, TAggregate> Cardinality(SpecificationResultCardinality cardinality)
    {
        ApplyCardinality(cardinality);
        return this;
    }

    #endregion Properties Methods

    #region Operations

    /// <summary>
    /// Clones all properties of the given specification into the current instance, excluding the criteria.
    /// </summary>
    /// <param name="specification">The source specification to clone properties from.</param>
    /// <returns>The current specification instance with updated properties.</returns>
    private Specification<TId, TAggregate> CloneAllWithoutCriteria(ISpecification<TId, TAggregate> specification)
    {
        Includes.AddRange(specification.Includes);
        IncludeStrings.AddRange(specification.IncludeStrings);

        if (specification.OrderByExpression != null)
            OrderByExpression = specification.OrderByExpression;

        if (specification.OrderByDescendingExpression != null)
            OrderByDescendingExpression = specification.OrderByDescendingExpression;

        if (specification.Take.HasValue)
            Take = specification.Take;

        if (specification.Skip.HasValue)
            Skip = specification.Skip;

        if (specification.NoTracking)
            NoTracking = true;

        if (specification.SplitQuery)
            SplitQuery = true;

        SpecCardinality = specification.SpecCardinality;

        return this;
    }

    /// <summary>
    /// Clones the properties and criteria of the given specification into the current instance.
    /// </summary>
    /// <param name="specification">The source specification to clone from.</param>
    /// <returns>The current specification instance with updated properties and criteria.</returns>
    public Specification<TId, TAggregate> Clone(ISpecification<TId, TAggregate> specification)
    {
        Criteria = specification.Criteria;
        CloneAllWithoutCriteria(specification);

        return this;
    }

    /// <summary>
    /// Combines this specification with another using a logical AND operator.
    /// </summary>
    /// <param name="specification">The specification to combine with.</param>
    /// <returns>A new specification representing the combination.</returns>
    public Specification<TId, TAggregate> And(ISpecification<TId, TAggregate> specification)
    {
        var newSpec = new Specification<TId, TAggregate>();
        if (Criteria != null && specification.Criteria != null)
            newSpec.Criteria = Criteria.And(specification.Criteria);
        else
            newSpec.Criteria = Criteria ?? specification.Criteria;

        newSpec.CloneAllWithoutCriteria(specification);

        return newSpec;
    }

    /// <summary>
    /// Combines this specification with another using a logical OR operator.
    /// </summary>
    /// <param name="specification">The specification to combine with.</param>
    /// <returns>A new specification representing the combination.</returns>
    public Specification<TId, TAggregate> Or(Specification<TId, TAggregate> specification)
    {
        var newSpec = new Specification<TId, TAggregate>();
        if (Criteria != null && specification.Criteria != null)
            newSpec.Criteria = Criteria.Or(specification.Criteria);
        else
            newSpec.Criteria = Criteria ?? specification.Criteria;

        newSpec.CloneAllWithoutCriteria(specification);

        return newSpec;
    }

    #endregion Operations
}