using System.Linq.Expressions;
using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Specifications.Interfaces;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Analytics;
using Microsoft.EntityFrameworkCore;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Extensions;

/// <summary>
/// Provides extension methods for building Entity Framework queries from specifications. This allows for translating the criteria, includes, ordering, and other properties of a specification into an IQueryable that can be executed against the database.
/// </summary>
public static class SpecificationQueryBuilder
{
    /// <summary>
    /// Builds an IQueryable from a given specification.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <param name="context">The database context.</param>
    /// <param name="spec">The specification to apply.</param>
    /// <returns>An IQueryable for the aggregate type with the specification applied.</returns>
    public static IQueryable<TAggregate> BuildQuery<TId, TAggregate>(
        this DbContext context,
        ISpecification<TId, TAggregate> spec)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
        => context.Set<TAggregate>().BuildQuery(spec);

    /// <summary>
    /// Builds an IQueryable from a given specification with projection.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <typeparam name="TProjected">The type of the projected results.</typeparam>
    /// <param name="context">The database context.</param>
    /// <param name="spec">The specification to apply.</param>
    /// <param name="projection">The projection expression.</param>
    /// <returns>An IQueryable for the projected results.</returns>
    public static IQueryable<TProjected> BuildQuery<TId, TAggregate, TProjected>(
        this DbContext context,
        ISpecification<TId, TAggregate> spec,
        Expression<Func<TAggregate, TProjected>> projection)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
        => context.Set<TAggregate>().BuildQuery(spec, projection);

    /// <summary>
    /// Applies specification criteria and properties to an existing queryable.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <param name="query">The source queryable.</param>
    /// <param name="spec">The specification to apply.</param>
    /// <returns>The modified queryable.</returns>
    public static IQueryable<TAggregate> BuildQuery<TId, TAggregate>(
        this IQueryable<TAggregate> query,
        ISpecification<TId, TAggregate> spec)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        if (spec.Criteria != null)
            query = query.Where(spec.Criteria);

        foreach (var include in spec.Includes)
            query = query.Include(include);

        foreach (var include in spec.IncludeStrings)
            query = query.Include(include);

        if (spec.OrderByExpression != null)
            query = query.OrderBy(spec.OrderByExpression);

        if (spec.OrderByDescendingExpression != null)
            query = query.OrderByDescending(spec.OrderByDescendingExpression);

        if (spec.Skip.HasValue)
            query = query.Skip(spec.Skip.Value);

        if (spec.Take.HasValue)
            query = query.Take(spec.Take.Value);

        if (spec.NoTracking)
            query = query.AsNoTracking();

        if (spec.SplitQuery)
            query = query.AsSplitQuery();

        return query;
    }
    
    /// <summary>
    /// Applies specification criteria and properties to an existing queryable with projection.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <typeparam name="TProjected">The type of the projected results.</typeparam>
    /// <param name="query">The source queryable.</param>
    /// <param name="spec">The specification to apply.</param>
    /// <param name="projection">The projection expression.</param>
    /// <returns>The modified queryable for projected results.</returns>
    public static IQueryable<TProjected> BuildQuery<TId, TAggregate, TProjected>(
        this IQueryable<TAggregate> query,
        ISpecification<TId, TAggregate> spec,
        Expression<Func<TAggregate, TProjected>> projection)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
        => query.BuildQuery(spec).Select(projection);

    #region Analytics Specification Support

    /// <summary>
    /// Builds an IQueryable for analytics specifications, supporting GroupBy and Aggregations.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <param name="context">The database context.</param>
    /// <param name="spec">The analytics specification to apply.</param>
    /// <returns>An IQueryable for dynamic results containing aggregated data.</returns>
    public static IQueryable<dynamic> BuildAnalyticsQuery<TId, TAggregate>(
        this DbContext context,
        IAnalyticsSpecification<TId, TAggregate> spec)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
        => context.Set<TAggregate>().BuildAnalyticsQuery(spec);

    /// <summary>
    /// Builds an IQueryable for analytics specifications from an existing queryable.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <param name="query">The source queryable.</param>
    /// <param name="spec">The analytics specification to apply.</param>
    /// <returns>An IQueryable for dynamic results containing aggregated data.</returns>
    public static IQueryable<dynamic> BuildAnalyticsQuery<TId, TAggregate>(
        this IQueryable<TAggregate> query,
        IAnalyticsSpecification<TId, TAggregate> spec)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
        => AnalyticsQueryBuilder.BuildAnalyticsQuery(query, spec);

    /// <summary>
    /// Determines if a specification is an analytics specification with GroupBy or Aggregations.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <param name="spec">The specification to check.</param>
    /// <returns>True if the specification is an analytics specification with aggregations.</returns>
    public static bool IsAnalyticsSpecification<TId, TAggregate>(this ISpecification<TId, TAggregate> spec)
        where TAggregate : IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
        => spec is IAnalyticsSpecification<TId, TAggregate> analyticsSpec
           && analyticsSpec.Aggregations.Count > 0;

    #endregion Analytics Specification Support
}