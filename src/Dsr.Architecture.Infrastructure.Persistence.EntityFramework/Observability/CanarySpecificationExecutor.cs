using System.Linq.Expressions;
using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Specifications.Interfaces;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Interfaces;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Evaluators;
using Microsoft.EntityFrameworkCore;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Observability;

/// <summary>
/// Canary executor: routes all calls through the new SpecificationEvaluator pipeline.
/// Used when UseNewSpecificationEvaluator flag is true. Wraps ISpecificationEvaluator
/// to match the ICompiledSpecificationExecutor interface signature.
/// </summary>
internal sealed class CanarySpecificationExecutor : ICompiledSpecificationExecutor
{
    private readonly ISpecificationEvaluator _evaluator;
    private readonly CompiledQueries.CompiledQueryCache _cache;
    private readonly SpecificationAnalysisCache _analysis;
    private readonly ISpecificationComplexityAnalyzer _analyzer;

    public CanarySpecificationExecutor(
        ISpecificationEvaluator evaluator,
        CompiledQueries.CompiledQueryCache cache,
        SpecificationAnalysisCache analysis,
        ISpecificationComplexityAnalyzer analyzer)
    {
        _evaluator = evaluator;
        _cache = cache;
        _analysis = analysis;
        _analyzer = analyzer;
    }

    public async Task<List<TAggregate>> ExecuteAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
        => (await _evaluator.ExecuteListAsync(context, spec, cancellationToken)).ToList();

    public async Task<List<TProjected>> ExecuteAsync<TId, TAggregate, TProjected>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        Expression<Func<TAggregate, TProjected>> projection,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var result = await _evaluator.ExecuteProjectedAsync(context, spec, projection, cancellationToken);
        return result != null ? [result] : [];
    }

    public async Task<TAggregate?> ExecuteSingleAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
        => await _evaluator.ExecuteSingleAsync(context, spec, cancellationToken);

    public async Task<List<dynamic>> ExecuteDynamicAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
        => (await _evaluator.ExecuteAnalyticsAsync(context, spec, cancellationToken)).Cast<dynamic>().ToList();

    public async Task<dynamic> ExecuteDynamicSingleAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var projection = (spec as IAnalyticsSpecification<TId, TAggregate>)?.Projection;
        if (projection != null)
        {
            // For analytics with projection, execute as list and return single
            var results = await _evaluator.ExecuteListAsync(context, spec, cancellationToken);
            return results.FirstOrDefault() ?? default!;
        }

        // Fallback: execute as single
        return await _evaluator.ExecuteSingleAsync(context, spec, cancellationToken) ?? default!;
    }

    public async Task<T> ExecuteScalarAsync<T, TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        CancellationToken cancellationToken)
        where T : struct
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
        => await _evaluator.ExecuteScalarAsync<T, TId, TAggregate>(context, spec, cancellationToken);

    public async Task<TProjected> ExecuteSingleAsync<TId, TAggregate, TProjected>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        Expression<Func<TAggregate, TProjected>> projection,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    => await _evaluator.ExecuteProjectedAsync(context, spec, projection, cancellationToken)
        ?? throw new InvalidOperationException("No results found for the given specification.");
}
