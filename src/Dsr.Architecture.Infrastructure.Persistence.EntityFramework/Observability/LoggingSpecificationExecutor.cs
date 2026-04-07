using System.Diagnostics;
using System.Linq.Expressions;
using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Specifications.Interfaces;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Observability;

/// <summary>
/// Decorator that adds structured logging to specification execution.
/// Ships as part of Phase 2 (Observability) — zero behavioral change, just telemetry.
/// </summary>
internal sealed class LoggingSpecificationExecutor : ICompiledSpecificationExecutor
{
    private readonly ICompiledSpecificationExecutor _inner;
    private readonly ILogger<LoggingSpecificationExecutor> _logger;

    public LoggingSpecificationExecutor(
        ICompiledSpecificationExecutor inner,
        ILogger<LoggingSpecificationExecutor> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<List<TAggregate>> ExecuteAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> specification,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var sw = Stopwatch.StartNew();
        var aggregateType = typeof(TAggregate).Name;
        var specType = specification.GetType().Name;
        var isAnalytics = specification is IAnalyticsSpecification<TId, TAggregate>;

        CardinalityTelemetry.RecordUsage<TAggregate>(specification.SpecCardinality);

        try
        {
            var result = await _inner.ExecuteAsync(context, specification, cancellationToken);
            sw.Stop();

            _logger.LogInformation(
                "Specification executed: {AggregateType}/{SpecType}, Cardinality={Cardinality}, " +
                "HasCriteria={HasCriteria}, Includes={IncludeCount}, IsAnalytics={IsAnalytics}, " +
                "ResultCount={ResultCount}, ElapsedMs={ElapsedMs}",
                aggregateType, specType, specification.SpecCardinality,
                specification.Criteria != null,
                specification.Includes.Count, isAnalytics,
                result.Count, sw.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex,
                "Specification FAILED: {AggregateType}/{SpecType}, ElapsedMs={ElapsedMs}",
                aggregateType, specType, sw.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<List<TProjected>> ExecuteAsync<TId, TAggregate, TProjected>(
        DbContext context,
        ISpecification<TId, TAggregate> specification,
        Expression<Func<TAggregate, TProjected>> projection,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var sw = Stopwatch.StartNew();

        try
        {
            var result = await _inner.ExecuteAsync(context, specification, projection, cancellationToken);
            sw.Stop();

            _logger.LogInformation(
                "Specification (projected): {AggregateType} -> {ProjectedType}, " +
                "ResultCount={ResultCount}, ElapsedMs={ElapsedMs}",
                typeof(TAggregate).Name, typeof(TProjected).Name, result.Count, sw.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex,
                "Projected specification FAILED: {AggregateType} -> {ProjectedType}",
                typeof(TAggregate).Name, typeof(TProjected).Name);
            throw;
        }
    }

    public async Task<T> ExecuteScalarAsync<T, TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> specification,
        CancellationToken cancellationToken)
        where T : struct
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var sw = Stopwatch.StartNew();

        try
        {
            var result = await _inner.ExecuteScalarAsync<T, TId, TAggregate>(context, specification, cancellationToken);
            sw.Stop();

            _logger.LogInformation(
                "Scalar execution: {AggregateType}, ScalarType={ScalarType}, Result={Result}, " +
                "ElapsedMs={ElapsedMs}",
                typeof(TAggregate).Name, typeof(T).Name, result, sw.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex,
                "Scalar execution FAILED: {AggregateType}, ScalarType={ScalarType}",
                typeof(TAggregate).Name, typeof(T).Name);
            throw;
        }
    }

    public async Task<List<dynamic>> ExecuteDynamicAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> specification,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var sw = Stopwatch.StartNew();

        try
        {
            var result = await _inner.ExecuteDynamicAsync(context, specification, cancellationToken);
            sw.Stop();

            _logger.LogInformation(
                "Analytics/Dynamic: {AggregateType}, ResultCount={ResultCount}, ElapsedMs={ElapsedMs}",
                typeof(TAggregate).Name, result.Count, sw.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex,
                "Analytics/Dynamic execution FAILED: {AggregateType}",
                typeof(TAggregate).Name);
            throw;
        }
    }

    public async Task<dynamic> ExecuteDynamicSingleAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> specification,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var sw = Stopwatch.StartNew();

        try
        {
            var result = await _inner.ExecuteDynamicSingleAsync(context, specification, cancellationToken);
            sw.Stop();

            _logger.LogDebug(
                "DynamicSingle: {AggregateType}, ElapsedMs={ElapsedMs}",
                typeof(TAggregate).Name, sw.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex,
                "DynamicSingle execution FAILED: {AggregateType}",
                typeof(TAggregate).Name);
            throw;
        }
    }

    public async Task<TAggregate?> ExecuteSingleAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> specification,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var sw = Stopwatch.StartNew();

        try
        {
            var result = await _inner.ExecuteSingleAsync(context, specification, cancellationToken);
            sw.Stop();

            _logger.LogDebug(
                "Single: {AggregateType}, Found={Found}, ElapsedMs={ElapsedMs}",
                typeof(TAggregate).Name, result != null, sw.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex,
                "Single execution FAILED: {AggregateType}",
                typeof(TAggregate).Name);
            throw;
        }
    }

    public async Task<TProjected> ExecuteSingleAsync<TId, TAggregate, TProjected>(
        DbContext context,
        ISpecification<TId, TAggregate> specification,
        Expression<Func<TAggregate, TProjected>> projection,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var sw = Stopwatch.StartNew();

        try
        {
            var result = await _inner.ExecuteSingleAsync(context, specification, projection, cancellationToken);
            sw.Stop();

            _logger.LogDebug(
                "Single (projected): {AggregateType} -> {ProjectedType}, ElapsedMs={ElapsedMs}",
                typeof(TAggregate).Name, typeof(TProjected).Name, sw.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex,
                "Single (projected) FAILED: {AggregateType} -> {ProjectedType}",
                typeof(TAggregate).Name, typeof(TProjected).Name);
            throw;
        }
    }
}
