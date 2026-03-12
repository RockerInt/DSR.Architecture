using System.Linq.Expressions;
using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Specifications.Extensions;

namespace Dsr.Architecture.Domain.Specifications;

/// <summary>
/// Base class for a specification.
/// </summary>
/// <typeparam name="TId">The type of the identifier.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregate to be filtered.</typeparam>
public class Specification<TId, TAggregate>(Expression<Func<TAggregate, bool>>? criteria = null) : ISpecification<TId, TAggregate>
    where TAggregate : IAggregateRoot<TId>
    where TId : IEquatable<TId>, IComparable<TId>
{
    /// <summary>
    /// The criteria of the specification. It is used to filter the aggregates.
    /// </summary>
    public Expression<Func<TAggregate, bool>>? Criteria { get; protected set; } = criteria;

    /// <summary>
    /// The includes of the specification. It is used to include related entities.
    /// </summary>
    public List<Expression<Func<TAggregate, object>>> Includes { get; } = [];
    
    /// <summary>
    /// The include strings of the specification. It is used to include related entities by string.
    /// </summary>
    public List<string> IncludeStrings { get; } = [];

    /// <summary>
    /// The order by of the specification. It is used to order the aggregates.
    /// </summary>
    public Expression<Func<TAggregate, object>>? OrderBy { get; protected set; }

    /// <summary>
    /// The order by descending of the specification. It is used to order the aggregates descending.
    /// </summary>
    public Expression<Func<TAggregate, object>>? OrderByDescending { get; protected set; }

    /// <summary>
    /// The take of the specification. It is used to limit the number of aggregates.
    /// </summary>
    public int? Take { get; protected set; }

    /// <summary>
    /// The skip of the specification. It is used to skip a number of aggregates.
    /// </summary>
    public int? Skip { get; protected set; }

    /// <summary>
    /// The as no tracking of the specification. It is used to specify if the query should be executed as no tracking.
    /// </summary>
    public bool AsNoTracking { get; protected set; }

    /// <summary>
    /// The as split query of the specification. It is used to specify if the query should be executed as split query.
    /// </summary>
    public bool AsSplitQuery { get; protected set; }

    /// <summary>
    /// Checks if the specification is satisfied by an aggregate.
    /// </summary>
    /// <param name="aggregate">The aggregate to check.</param>
    /// <returns>True if the specification is satisfied, false otherwise.</returns>
    public virtual bool IsSatisfiedBy(TAggregate aggregate)
    {
        if (Criteria == null)
            return true;

        var compiled = Criteria.Compile();
        return compiled(aggregate);
    }

    /// <summary>
    /// Adds an include to the specification.
    /// </summary>
    /// <param name="include">The include to add.</param>
    public void AddInclude(Expression<Func<TAggregate, object>> include)
        => Includes.Add(include);

    /// <summary>
    /// Adds an include string to the specification.
    /// </summary>
    /// <param name="includeString">The include string to add.</param>
    public void AddInclude(string includeString)
        => IncludeStrings.Add(includeString);

    /// <summary>
    /// Applies an order by to the specification.
    /// </summary>
    /// <param name="orderBy">The order by to apply.</param>
    public void ApplyOrderBy(Expression<Func<TAggregate, object>> orderBy)
        => OrderBy = orderBy;

    /// <summary>
    /// Applies an order by descending to the specification.
    /// </summary>
    /// <param name="orderByDesc">The order by descending to apply.</param>
    public void ApplyOrderByDescending(Expression<Func<TAggregate, object>> orderByDesc)
        => OrderByDescending = orderByDesc;

    /// <summary>
    /// Applies paging to the specification.
    /// </summary>
    /// <param name="skip">The skip to apply.</param>
    /// <param name="take">The take to apply.</param>
    public void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    /// <summary>
    /// Applies as no tracking to the specification.
    /// </summary>
    public void ApplyAsNoTracking()
        => AsNoTracking = true;

    /// <summary>    
    /// Applies as split query to the specification.
    /// </summary>
    public void ApplySplitQuery()
        => AsSplitQuery = true;

    /// <summary>
    /// Clones all the properties of the specification except the criteria. It is used to create a new specification based on an existing one without changing the criteria.
    /// This method is used in the And and Or methods to combine two specifications without changing the criteria of the original specifications.
    /// The criteria of the original specifications are combined using the And or Or methods of the SpecificationExtensions class.
    /// The other properties of the original specifications are cloned to the new specification.
    /// The new specification is returned with the combined criteria and the cloned properties.
    /// The original specifications are not modified.
    /// </summary>
    /// <param name="specification"></param>
    /// <returns></returns>
    private Specification<TId, TAggregate> CloneAllWithoutCriteria(Specification<TId, TAggregate> specification)
    {
        Includes.AddRange(specification.Includes);
        IncludeStrings.AddRange(specification.IncludeStrings);

        if (specification.OrderBy != null)
            OrderBy = specification.OrderBy;

        if (specification.OrderByDescending != null)
            OrderByDescending = specification.OrderByDescending;

        if (specification.Take.HasValue)
            Take = specification.Take;

        if (specification.Skip.HasValue)
            Skip = specification.Skip;

        if (specification.AsNoTracking)
            AsNoTracking = true;

        if (specification.AsSplitQuery)
            AsSplitQuery = true;

        return this;
    }

    /// <summary>
    /// Clones the specification. It is used to create a new specification based on an existing one.
    /// </summary>
    /// <param name="specification"></param>
    /// <returns></returns>
    public Specification<TId, TAggregate> Clone(Specification<TId, TAggregate> specification)
    {
        Criteria = specification.Criteria;
        CloneAllWithoutCriteria(specification);

        return this;
    }

    /// <summary>
    /// Combines the specification with another specification using an AND operator. It is used to create a new specification based on two existing ones.
    /// </summary>
    /// <param name="specification"></param>
    /// <returns></returns>
    public Specification<TId, TAggregate> And(Specification<TId, TAggregate> specification)
    {
        if (Criteria != null && specification.Criteria != null)
            Criteria = Criteria.And(specification.Criteria);
        else
            Criteria ??= specification.Criteria;
            
        CloneAllWithoutCriteria(specification);

        return this;
    }

    /// <summary>
    /// Combines the specification with another specification using an OR operator. It is used to create a new specification based on two existing ones.
    /// </summary>
    /// <param name="specification"></param>
    /// <returns></returns>
    public Specification<TId, TAggregate> Or(Specification<TId, TAggregate> specification)
    {
        if (Criteria != null && specification.Criteria != null)
            Criteria = Criteria.Or(specification.Criteria);
        else
            Criteria ??= specification.Criteria;
            
        CloneAllWithoutCriteria(specification);

        return this;
    }
}