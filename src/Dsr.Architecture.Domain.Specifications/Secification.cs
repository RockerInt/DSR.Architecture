using System.Linq.Expressions;

namespace Dsr.Architecture.Domain.Specifications;

/// <summary>
/// Abstract base class for a specification.
/// </summary>
/// <typeparam name="T">The type of the entity to be filtered.</typeparam>
public abstract class Specification<T>(Expression<Func<T, bool>>? criteria = null) : ISpecification<T>
{
    /// <summary>
    /// The criteria of the specification. It is used to filter the entities.
    /// </summary>
    public Expression<Func<T, bool>>? Criteria { get; protected set; } = criteria;

    /// <summary>
    /// The includes of the specification. It is used to include related entities.
    /// </summary>
    public List<Expression<Func<T, object>>> Includes { get; } = [];
    
    /// <summary>
    /// The include strings of the specification. It is used to include related entities by string.
    /// </summary>
    public List<string> IncludeStrings { get; } = [];

    /// <summary>
    /// The order by of the specification. It is used to order the entities.
    /// </summary>
    public Expression<Func<T, object>>? OrderBy { get; protected set; }

    /// <summary>
    /// The order by descending of the specification. It is used to order the entities descending.
    /// </summary>
    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }

    /// <summary>
    /// The take of the specification. It is used to limit the number of entities.
    /// </summary>
    public int? Take { get; protected set; }

    /// <summary>
    /// The skip of the specification. It is used to skip a number of entities.
    /// </summary>
    public int? Skip { get; protected set; }

    /// <summary>
    /// The as no tracking of the specification. It is used to specify if the query should be executed as no tracking.
    /// </summary>
    public bool AsNoTracking { get; protected set; }

    /// <summary>
    /// Checks if the specification is satisfied by an entity.
    /// </summary>
    /// <param name="entity">The entity to check.</param>
    /// <returns>True if the specification is satisfied, false otherwise.</returns>
    public virtual bool IsSatisfiedBy(T entity)
    {
        if (Criteria == null)
            return true;

        var compiled = Criteria.Compile();
        return compiled(entity);
    }

    /// <summary>
    /// Adds an include to the specification.
    /// </summary>
    /// <param name="include">The include to add.</param>
    protected void AddInclude(Expression<Func<T, object>> include)
        => Includes.Add(include);

    /// <summary>
    /// Adds an include string to the specification.
    /// </summary>
    /// <param name="includeString">The include string to add.</param>
    protected void AddInclude(string includeString)
        => IncludeStrings.Add(includeString);

    /// <summary>
    /// Applies an order by to the specification.
    /// </summary>
    /// <param name="orderBy">The order by to apply.</param>
    protected void ApplyOrderBy(Expression<Func<T, object>> orderBy)
        => OrderBy = orderBy;

    /// <summary>
    /// Applies an order by descending to the specification.
    /// </summary>
    /// <param name="orderByDesc">The order by descending to apply.</param>
    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDesc)
        => OrderByDescending = orderByDesc;

    /// <summary>
    /// Applies paging to the specification.
    /// </summary>
    /// <param name="skip">The skip to apply.</param>
    /// <param name="take">The take to apply.</param>
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    /// <summary>
    /// Applies as no tracking to the specification.
    /// </summary>
    protected void ApplyAsNoTracking()
        => AsNoTracking = true;
}