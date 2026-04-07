using Dsr.Architecture.Domain.Aggregates;
using System.Linq.Expressions;
using Dsr.Architecture.Domain.Specifications.Enums;
using Dsr.Architecture.Domain.Specifications.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Evaluators;

/// <summary>
/// Central pipeline interface for all specification execution.
/// Replaces ICompiledSpecificationExecutor with a unified, cardinality-aware API.
/// </summary>
public interface ISpecificationEvaluator
{
    IQueryable<TAggregate> Apply<TId, TAggregate>(
        IQueryable<TAggregate> source,
        ISpecification<TId, TAggregate> spec)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>;

    Task<IReadOnlyList<TAggregate>> ExecuteListAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        CancellationToken ct = default)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>;

    Task<TProjected?> ExecuteProjectedAsync<TId, TAggregate, TProjected>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        Expression<Func<TAggregate, TProjected>> projection,
        CancellationToken ct = default)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>;

    Task<TAggregate?> ExecuteSingleAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        CancellationToken ct = default)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>;

    Task<T> ExecuteScalarAsync<T, TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        CancellationToken ct = default)
        where T : struct
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>;

    Task<IReadOnlyList<dynamic>> ExecuteAnalyticsAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        CancellationToken ct = default)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>;
}
