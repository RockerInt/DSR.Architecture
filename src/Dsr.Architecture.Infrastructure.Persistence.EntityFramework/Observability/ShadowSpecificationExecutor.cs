using System.Diagnostics;
using System.Linq.Expressions;
using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Specifications.Enums;
using Dsr.Architecture.Domain.Specifications.Interfaces;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Interfaces;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Evaluators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Observability;

/// <summary>
/// Shadow mode executor: runs both old and new pipelines concurrently,
/// returns the old pipeline result to the caller, and compares results in background.
///
/// This is the CORE migration safety mechanism. Zero behavioral change to users
/// while the new pipeline is validated against production traffic.
/// </summary>
internal sealed class ShadowSpecificationExecutor : ICompiledSpecificationExecutor
{
    private readonly ICompiledSpecificationExecutor _primary;    // OLD pipeline (returned to caller)
    private readonly ISpecificationEvaluator _candidate;          // NEW pipeline (compared in background)
    private readonly ILogger<ShadowSpecificationExecutor> _logger;
    private readonly PersistenceFeatureFlags _flags;
    private readonly IServiceProvider _serviceProvider;
    private readonly Random _random;

    public ShadowSpecificationExecutor(
        ICompiledSpecificationExecutor primary,
        ISpecificationEvaluator candidate,
        ILogger<ShadowSpecificationExecutor> logger,
        PersistenceFeatureFlags flags,
        IServiceProvider serviceProvider)
    {
        _primary = primary;
        _candidate = candidate;
        _logger = logger;
        _flags = flags;
        _serviceProvider = serviceProvider;
        _random = new Random();
    }

    private bool ShouldSample()
    {
        if (!_flags.ShadowModeEnabled) return false;
        return _random.NextDouble() < _flags.ShadowSampleRate;
    }

    public async Task<List<TAggregate>> ExecuteAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var primaryResult = await _primary.ExecuteAsync(context, spec, cancellationToken);

        if (ShouldSample())
        {
            _ = ExecuteShadowListAsync(context, spec, primaryResult, typeof(TAggregate).Name, spec.GetType().Name, spec.SpecCardinality, cancellationToken);
        }

        return primaryResult;
    }

    public async Task<List<TProjected>> ExecuteAsync<TId, TAggregate, TProjected>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        Expression<Func<TAggregate, TProjected>> projection,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var primaryResult = await _primary.ExecuteAsync(context, spec, projection, cancellationToken);

        if (ShouldSample())
        {
            _ = ExecuteShadowProjectedAsync(context, spec, projection, primaryResult, typeof(TAggregate).Name, spec.GetType().Name, spec.SpecCardinality, cancellationToken);
        }

        return primaryResult;
    }

    public async Task<T> ExecuteScalarAsync<T, TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        CancellationToken cancellationToken)
        where T : struct
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var primaryResult = await _primary.ExecuteScalarAsync<T, TId, TAggregate>(context, spec, cancellationToken);

        if (ShouldSample())
        {
            _ = ExecuteShadowScalarAsync<T, TId, TAggregate>(context, spec, primaryResult, typeof(TAggregate).Name, spec.GetType().Name, cancellationToken);
        }

        return primaryResult;
    }

    public async Task<List<dynamic>> ExecuteDynamicAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var primaryResult = await _primary.ExecuteDynamicAsync(context, spec, cancellationToken);

        if (ShouldSample())
        {
            _ = ExecuteShadowDynamicAsync(context, spec, primaryResult, typeof(TAggregate).Name, spec.GetType().Name, cancellationToken);
        }

        return primaryResult;
    }

    public async Task<dynamic> ExecuteDynamicSingleAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        return await _primary.ExecuteDynamicSingleAsync(context, spec, cancellationToken);
    }

    public async Task<TAggregate?> ExecuteSingleAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var primaryResult = await _primary.ExecuteSingleAsync(context, spec, cancellationToken);

        if (ShouldSample())
        {
            _ = ExecuteShadowSingleAsync(context, spec, primaryResult, typeof(TAggregate).Name, spec.GetType().Name, cancellationToken);
        }

        return primaryResult;
    }

    public async Task<TProjected> ExecuteSingleAsync<TId, TAggregate, TProjected>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        Expression<Func<TAggregate, TProjected>> projection,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var primaryResult = await _primary.ExecuteSingleAsync(context, spec, projection, cancellationToken);

        if (ShouldSample())
        {
            _ = ExecuteShadowSingleProjectedAsync(context, spec, projection, primaryResult, typeof(TAggregate).Name, spec.GetType().Name, cancellationToken);
        }

        return primaryResult;
    }

    // ===== SHADOW EXECUTION METHODS =====

    private async Task ExecuteShadowListAsync<TId, TAggregate>(
        DbContext _originalContext,
        ISpecification<TId, TAggregate> spec,
        List<TAggregate> primaryResult,
        string aggregateType,
        string specType,
        SpecificationResultCardinality cardinality,
        CancellationToken ct)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            await using var shadowContext = scope.ServiceProvider.GetRequiredService<DbContext>();

            var sw = Stopwatch.StartNew();
            var candidateResult = await _candidate.ExecuteListAsync(shadowContext, spec, ct);
            sw.Stop();

            LogComparison(primaryResult.Count, candidateResult.Count, aggregateType, specType, cardinality, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SHADOW ERROR: Candidate pipeline failed for {AggregateType}/{SpecType}",
                aggregateType, specType);
        }
    }

    private async Task ExecuteShadowProjectedAsync<TId, TAggregate, TProjected>(
        DbContext _originalContext,
        ISpecification<TId, TAggregate> spec,
        Expression<Func<TAggregate, TProjected>> projection,
        List<TProjected> primaryResult,
        string aggregateType,
        string specType,
        SpecificationResultCardinality cardinality,
        CancellationToken ct)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            await using var shadowContext = scope.ServiceProvider.GetRequiredService<DbContext>();

            var sw = Stopwatch.StartNew();
            var candidateResult = await _candidate.ExecuteProjectedAsync(shadowContext, spec, projection, ct);
            sw.Stop();

            var candidateCount = candidateResult != null ? 1 : 0;
            LogComparison(primaryResult.Count, candidateCount, aggregateType, specType, cardinality, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SHADOW ERROR: Candidate projected pipeline failed for {AggregateType}/{SpecType}",
                aggregateType, specType);
        }
    }

    private async Task ExecuteShadowScalarAsync<T, TId, TAggregate>(
        DbContext _originalContext,
        ISpecification<TId, TAggregate> spec,
        T primaryResult,
        string aggregateType,
        string specType,
        CancellationToken ct)
        where T : struct
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            await using var shadowContext = scope.ServiceProvider.GetRequiredService<DbContext>();

            var sw = Stopwatch.StartNew();
            var candidateResult = await _candidate.ExecuteScalarAsync<T, TId, TAggregate>(shadowContext, spec, ct);
            sw.Stop();

            var match = Math.Abs(Convert.ToDouble(primaryResult) - Convert.ToDouble(candidateResult)) < 0.0001;
            if (!match)
            {
                _logger.LogWarning(
                    "SHADOW MISMATCH (Scalar): {AggregateType}/{SpecType} — Primary={Primary}, Candidate={Candidate}, ElapsedMs={ElapsedMs}",
                    aggregateType, specType, primaryResult, candidateResult, sw.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SHADOW ERROR: Candidate scalar pipeline failed for {AggregateType}/{SpecType}",
                aggregateType, specType);
        }
    }

    private async Task ExecuteShadowDynamicAsync<TId, TAggregate>(
        DbContext _originalContext,
        ISpecification<TId, TAggregate> spec,
        List<dynamic> primaryResult,
        string aggregateType,
        string specType,
        CancellationToken ct)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            await using var shadowContext = scope.ServiceProvider.GetRequiredService<DbContext>();

            var sw = Stopwatch.StartNew();
            var candidateResult = await _candidate.ExecuteAnalyticsAsync(shadowContext, spec, ct);
            sw.Stop();

            LogComparison(primaryResult.Count, candidateResult.Count, aggregateType, specType, SpecificationResultCardinality.List, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SHADOW ERROR: Candidate dynamic pipeline failed for {AggregateType}/{SpecType}",
                aggregateType, specType);
        }
    }

    private async Task ExecuteShadowSingleAsync<TId, TAggregate>(
        DbContext _originalContext,
        ISpecification<TId, TAggregate> spec,
        TAggregate? primaryResult,
        string aggregateType,
        string specType,
        CancellationToken ct)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            await using var shadowContext = scope.ServiceProvider.GetRequiredService<DbContext>();

            var sw = Stopwatch.StartNew();
            var candidateResult = await _candidate.ExecuteSingleAsync(shadowContext, spec, ct);
            sw.Stop();

            var match = primaryResult == null ? candidateResult == null
                : candidateResult != null && CollectionComparer.AreEqual([primaryResult], [candidateResult]);

            if (!match)
            {
                _logger.LogWarning(
                    "SHADOW MISMATCH (Single): {AggregateType}/{SpecType} — PrimaryFound={PrimaryFound}, CandidateFound={CandidateFound}, ElapsedMs={ElapsedMs}",
                    aggregateType, specType, primaryResult != null, candidateResult != null, sw.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SHADOW ERROR: Candidate single pipeline failed for {AggregateType}/{SpecType}",
                aggregateType, specType);
        }
    }

    private async Task ExecuteShadowSingleProjectedAsync<TId, TAggregate, TProjected>(
        DbContext _originalContext,
        ISpecification<TId, TAggregate> spec,
        Expression<Func<TAggregate, TProjected>> projection,
        TProjected primaryResult,
        string aggregateType,
        string specType,
        CancellationToken ct)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            await using var shadowContext = scope.ServiceProvider.GetRequiredService<DbContext>();

            var sw = Stopwatch.StartNew();
            var candidateResult = await _candidate.ExecuteProjectedAsync(shadowContext, spec, projection, ct);
            sw.Stop();

            var match = primaryResult == null ? candidateResult == null
                : candidateResult != null && CollectionComparer.AreEqual(
                    new object?[] { primaryResult },
                    new object?[] { candidateResult });

            if (!match)
            {
                _logger.LogWarning(
                    "SHADOW MISMATCH (SingleProjected): {AggregateType}/{SpecType} — ElapsedMs={ElapsedMs}",
                    aggregateType, specType, sw.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SHADOW ERROR: Candidate single projected pipeline failed for {AggregateType}/{SpecType}",
                aggregateType, specType);
        }
    }

    private void LogComparison(int primaryCount, int candidateCount, string aggregateType, string specType, SpecificationResultCardinality cardinality, long elapsedMs)
    {
        if (primaryCount != candidateCount)
        {
            _logger.LogWarning(
                "SHADOW MISMATCH: {AggregateType}/{SpecType} — PrimaryCount={PrimaryCount}, CandidateCount={CandidateCount}, Cardinality={Cardinality}, CandidateElapsedMs={ElapsedMs}",
                aggregateType, specType, primaryCount, candidateCount, cardinality, elapsedMs);
        }
        else
        {
            _logger.LogDebug(
                "SHADOW MATCH: {AggregateType}/{SpecType} — Count={Count}, CandidateElapsedMs={ElapsedMs}",
                aggregateType, specType, primaryCount, elapsedMs);
        }
    }
}
