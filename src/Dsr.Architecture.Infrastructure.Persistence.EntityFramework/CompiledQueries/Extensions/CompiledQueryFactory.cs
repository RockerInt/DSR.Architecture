using System.Linq.Expressions;
using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Specifications.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Extensions;

/// <summary>
/// 
/// </summary>
public static class CompiledQueryFactory
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    /// <typeparam name="TAggregate"></typeparam>
    /// <param name="spec"></param>
    /// <returns></returns>
    public static Func<DbContext, Task<List<TAggregate>>> Create<TId, TAggregate>(
        this ISpecification<TId, TAggregate> spec)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
        => EF.CompileAsyncQuery((DbContext ctx) => ctx.BuildQuery(spec).ToList());

    public static Func<DbContext, Task<List<TProjected>>> Create<TId, TAggregate, TProjected>(
        this ISpecification<TId, TAggregate> spec,
        Expression<Func<TAggregate, TProjected>> projection)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
        => EF.CompileAsyncQuery((DbContext ctx) => ctx.BuildQuery(spec, projection).ToList());

    /// <summary>
    /// Analytics dynamic (single expr).
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    /// <typeparam name="TAggregate"></typeparam>
    /// <param name="spec"></param>
    /// <returns></returns>
    public static Func<DbContext, List<object>> CreateDynamic<TId, TAggregate>(
        this IAnalyticsSpecification<TId, TAggregate> spec)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
        => EF.CompileQuery((DbContext ctx) => ctx.Set<TAggregate>().BuildAnalyticsQuery(spec).Cast<object>().ToList());
}
