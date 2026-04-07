using System.Linq.Expressions;
using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Specifications;
using Dsr.Architecture.Domain.Specifications.Enums;
using Dsr.Architecture.Domain.Specifications.Interfaces;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Extensions;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Evaluators;

/// <summary>
/// Central specification evaluator that unifies query execution.
/// Phase 4: This is a thin wrapper around the existing pipeline.
/// It will be enhanced in later phases with cardinality enforcement,
/// direct scalar aggregation, and simplified analytics.
///
/// Currently delegates to the existing SpecificationQueryBuilder and
/// compiled query infrastructure — zero behavioral change until Phase 5+.
/// </summary>
internal sealed class SpecificationEvaluator : ISpecificationEvaluator
{
    private readonly CompiledQueries.CompiledQueryCache _cache;
    private readonly SpecificationAnalysisCache _analysisCache;
    private readonly ISpecificationComplexityAnalyzer _analyzer;
    private readonly Observability.PersistenceFeatureFlags _flags;
    private readonly ILogger<SpecificationEvaluator> _logger;

    public SpecificationEvaluator(
        CompiledQueries.CompiledQueryCache cache,
        SpecificationAnalysisCache analysisCache,
        ISpecificationComplexityAnalyzer analyzer,
        Observability.PersistenceFeatureFlags flags,
        ILogger<SpecificationEvaluator> logger)
    {
        _cache = cache;
        _analysisCache = analysisCache;
        _analyzer = analyzer;
        _flags = flags;
        _logger = logger;
    }

    public IQueryable<TAggregate> Apply<TId, TAggregate>(
        IQueryable<TAggregate> source,
        ISpecification<TId, TAggregate> spec)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var query = source;

        // Apply criteria
        if (spec.Criteria != null)
            query = query.Where(spec.Criteria);

        // Apply includes
        foreach (var include in spec.Includes)
            query = query.Include(include);

        foreach (var include in spec.IncludeStrings)
            query = query.Include(include);

        // Apply ordering (EF Core: OrderBy must come first)
        if (spec.OrderByExpression != null)
            query = query.OrderBy(spec.OrderByExpression);
        else if (spec.OrderByDescendingExpression != null)
            query = query.OrderByDescending(spec.OrderByDescendingExpression);

        // Apply paging
        if (spec.Skip.HasValue)
            query = query.Skip(spec.Skip.Value);
        if (spec.Take.HasValue)
            query = query.Take(spec.Take.Value);

        // Apply tracking
        if (spec.NoTracking)
            query = query.AsNoTracking();

        if (spec.SplitQuery)
            query = query.AsSplitQuery();

        return query;
    }

    public async Task<IReadOnlyList<TAggregate>> ExecuteListAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        CancellationToken ct = default)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var key = spec.GenerateKey();

        // Decide: compiled vs dynamic (same logic as AutoCompiledSpecificationExecutor)
        if (!ShouldCompile(key, spec))
        {
            return await context.BuildQuery(spec).ToListAsync(ct);
        }

        var compiled = _cache.GetOrAdd(key, () => spec.Create()) as Func<DbContext, Task<List<TAggregate>>>
            ?? throw new InvalidOperationException($"Compiled query for key {key} is not a valid delegate type.");

        return await compiled(context);
    }

    public async Task<TProjected?> ExecuteProjectedAsync<TId, TAggregate, TProjected>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        Expression<Func<TAggregate, TProjected>> projection,
        CancellationToken ct = default)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var key = spec.GenerateKey(projection);

        if (!ShouldCompile(key, spec))
        {
            var result = await context.BuildQuery(spec, projection).ToListAsync(ct);
            return result.FirstOrDefault();
        }

        var compiled = _cache.GetOrAdd(key, () => spec.Create(projection)) as Func<DbContext, Task<List<TProjected>>>
            ?? throw new InvalidOperationException($"Compiled projection for key {key} is not a valid delegate type.");

        var results = await compiled(context);
        return results.FirstOrDefault();
    }

    public async Task<TAggregate?> ExecuteSingleAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        CancellationToken ct = default)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        // Phase 4: Always uses FirstOrDefault (safe default)
        // Phase 5+: Will enforce SpecCardinality when EnforceSpecCardinality is true
        if (_flags.EnforceSpecCardinality)
        {
            return spec.SpecCardinality switch
            {
                SpecificationResultCardinality.First => await GetFirstAsync(context, spec, ct),
                SpecificationResultCardinality.FirstOrDefault => await GetFirstOrDefaultAsync(context, spec, ct),
                SpecificationResultCardinality.Single => await GetSingleAsync(context, spec, ct),
                SpecificationResultCardinality.SingleOrDefault => await GetSingleOrDefaultAsync(context, spec, ct),
                _ => await GetFirstOrDefaultAsync(context, spec, ct),
            };
        }

        return await GetFirstOrDefaultAsync(context, spec, ct);
    }

    public async Task<T> ExecuteScalarAsync<T, TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        CancellationToken ct = default)
        where T : struct
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        // Phase 4: Delegates to existing AutoCompiledSpecificationExecutor scalar path
        // Phase 5+: Will execute directly without analytics wrapper
        if (spec is IAnalyticsSpecification<TId, TAggregate> analytics)
        {
            var query = context.Set<TAggregate>().AsQueryable();
            if (spec.Criteria != null)
                query = query.Where(spec.Criteria);
            if (spec.NoTracking)
                query = query.AsNoTracking();

            var aggregation = analytics.Aggregations.FirstOrDefault();
            if (aggregation == null)
                throw new InvalidOperationException("AnalyticsSpecification must have at least one aggregation.");

            return aggregation.Type switch
            {
                AggregationType.Count => (T)(object)await query.CountAsync(ct),
                AggregationType.Sum => await ExecuteSum<T, TAggregate>(query, aggregation.Selector, ct),
                AggregationType.Avg => await ExecuteAvg<T, TAggregate>(query, aggregation.Selector, ct),
                AggregationType.Max => await ExecuteMax<T, TAggregate>(query, aggregation.Selector, ct),
                AggregationType.Min => await ExecuteMin<T, TAggregate>(query, aggregation.Selector, ct),
                _ => throw new NotSupportedException($"Aggregation type {aggregation.Type} is not supported.")
            };
        }

        throw new InvalidOperationException("ExecuteScalarAsync requires an AnalyticsSpecification.");
    }

    public async Task<IReadOnlyList<dynamic>> ExecuteAnalyticsAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> spec,
        CancellationToken ct = default)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        if (spec is not IAnalyticsSpecification<TId, TAggregate> analytics)
        {
            // Non-analytics spec: treat as regular list query
            return (await ExecuteListAsync(context, spec, ct)).Cast<dynamic>().ToList();
        }

        // No GroupBy + no aggregations: empty result
        if (analytics.GroupByExpression == null && analytics.Aggregations.Count == 0)
        {
            return Array.Empty<dynamic>();
        }

        // Scalar aggregations (no GroupBy)
        if (analytics.GroupByExpression == null)
        {
            // Execute each aggregation and wrap in a dictionary-like dynamic
            var query = context.Set<TAggregate>().AsQueryable();
            if (spec.Criteria != null) query = query.Where(spec.Criteria);
            if (spec.NoTracking) query = query.AsNoTracking();

            var results = new List<dynamic>();
            var item = new Dictionary<string, object?>();

            foreach (var agg in analytics.Aggregations)
            {
                item[agg.Alias] = agg.Type switch
                {
                    AggregationType.Count => await query.CountAsync(ct),
                    AggregationType.Sum => await ExecuteSumInternal<long, TAggregate>(query, agg.Selector, ct),
                    AggregationType.Avg => await ExecuteAvgInternal<double, TAggregate>(query, agg.Selector, ct),
                    AggregationType.Max => await ExecuteMaxInternal<object?, TAggregate>(query, agg.Selector, ct),
                    AggregationType.Min => await ExecuteMinInternal<object?, TAggregate>(query, agg.Selector, ct),
                    _ => throw new NotSupportedException()
                };
            }

            results.Add(item);
            return results;
        }

        // GroupBy path: delegate to existing AnalyticsQueryBuilder
        var query2 = context.Set<TAggregate>().BuildAnalyticsQuery(analytics);
        var results2 = await query2.Cast<object>().ToListAsync(ct);
        return results2.Cast<dynamic>().ToList();
    }

    private bool ShouldCompile<TId, TAggregate>(string key, ISpecification<TId, TAggregate> spec)
        where TAggregate : IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
        => _analysisCache.GetOrAdd(key, () => _analyzer.Analyze(spec)).ShouldUseCompiledQuery;

    // ===== Terminal operation helpers =====

    private async Task<TAggregate?> GetFirstOrDefaultAsync<TId, TAggregate>(
        DbContext context, ISpecification<TId, TAggregate> spec, CancellationToken ct)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
        => await Apply(context.Set<TAggregate>(), spec).FirstOrDefaultAsync(ct);

    private async Task<TAggregate> GetFirstAsync<TId, TAggregate>(
        DbContext context, ISpecification<TId, TAggregate> spec, CancellationToken ct)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
        => await Apply(context.Set<TAggregate>(), spec).FirstAsync(ct);

    private async Task<TAggregate?> GetSingleAsync<TId, TAggregate>(
        DbContext context, ISpecification<TId, TAggregate> spec, CancellationToken ct)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var query = Apply(context.Set<TAggregate>(), spec);
        var results = await query.ToListAsync(ct);
        return results switch
        {
            [] => throw new InvalidOperationException("Sequence contains no matching element."),
            [var single] => single,
            _ => throw new InvalidOperationException("Sequence contains more than one matching element.")
        };
    }

    private async Task<TAggregate?> GetSingleOrDefaultAsync<TId, TAggregate>(
        DbContext context, ISpecification<TId, TAggregate> spec, CancellationToken ct)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var query = Apply(context.Set<TAggregate>(), spec);
        var results = await query.ToListAsync(ct);
        if (results.Count > 1)
            throw new InvalidOperationException("Sequence contains more than one matching element.");
        return results.Count == 0 ? default : results[0];
    }

    // ===== Aggregation helpers =====

    private static async Task<T> ExecuteSum<T, TAggregate>(IQueryable<TAggregate> query, LambdaExpression selector, CancellationToken ct)
        where T : struct
    {
        var elementType = selector.ReturnType;
        var result = await ExecuteAggregationAsync(query, nameof(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.SumAsync), typeof(TAggregate), elementType, selector, ct);
        return (T)Convert.ChangeType(result, typeof(T));
    }

    private static async Task<T> ExecuteAvg<T, TAggregate>(IQueryable<TAggregate> query, LambdaExpression selector, CancellationToken ct)
        where T : struct
    {
        var elementType = selector.ReturnType;
        var result = await ExecuteAggregationAsync(query, "AverageAsync", typeof(TAggregate), elementType, selector, ct);
        return (T)Convert.ChangeType(result, typeof(T));
    }

    private static async Task<T> ExecuteMax<T, TAggregate>(IQueryable<TAggregate> query, LambdaExpression selector, CancellationToken ct)
        where T : struct
    {
        var elementType = selector.ReturnType;
        var result = await ExecuteAggregationAsync(query, nameof(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.MaxAsync), typeof(TAggregate), elementType, selector, ct, isTwoGeneric: true);
        return (T)Convert.ChangeType(result, typeof(T));
    }

    private static async Task<T> ExecuteMin<T, TAggregate>(IQueryable<TAggregate> query, LambdaExpression selector, CancellationToken ct)
        where T : struct
    {
        var elementType = selector.ReturnType;
        var result = await ExecuteAggregationAsync(query, nameof(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.MinAsync), typeof(TAggregate), elementType, selector, ct, isTwoGeneric: true);
        return (T)Convert.ChangeType(result, typeof(T));
    }

    private static async Task<T> ExecuteSumInternal<T, TAggregate>(IQueryable<TAggregate> query, LambdaExpression selector, CancellationToken ct)
        where T : struct
        => await ExecuteSum<T, TAggregate>(query, selector, ct);

    private static async Task<T> ExecuteAvgInternal<T, TAggregate>(IQueryable<TAggregate> query, LambdaExpression selector, CancellationToken ct)
        where T : struct
        => await ExecuteAvg<T, TAggregate>(query, selector, ct);

    private static async Task<object?> ExecuteMaxInternal<T, TAggregate>(IQueryable<TAggregate> query, LambdaExpression selector, CancellationToken ct)
    {
        var method = typeof(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(m => m.Name == nameof(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.MaxAsync) && m.IsGenericMethodDefinition)
            .First(m => m.GetGenericArguments().Length == 2);
        var typed = method.MakeGenericMethod(typeof(TAggregate), selector.ReturnType);
        var task = (Task)typed.Invoke(null, new object[] { query, selector, ct })!;
        await task.ConfigureAwait(false);
        return task.GetType().GetProperty("Result")!.GetValue(task);
    }

    private static async Task<object?> ExecuteMinInternal<T, TAggregate>(IQueryable<TAggregate> query, LambdaExpression selector, CancellationToken ct)
    {
        var method = typeof(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(m => m.Name == nameof(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.MinAsync) && m.IsGenericMethodDefinition)
            .First(m => m.GetGenericArguments().Length == 2);
        var typed = method.MakeGenericMethod(typeof(TAggregate), selector.ReturnType);
        var task = (Task)typed.Invoke(null, new object[] { query, selector, ct })!;
        await task.ConfigureAwait(false);
        return task.GetType().GetProperty("Result")!.GetValue(task);
    }

    private static async Task<object> ExecuteAggregationAsync(
        IQueryable query,
        string methodName,
        Type sourceType,
        Type selectorType,
        LambdaExpression selector,
        CancellationToken ct,
        bool isTwoGeneric = false)
    {
        var method = typeof(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(m => m.Name == methodName && m.IsGenericMethodDefinition)
            .First(m => m.GetGenericArguments().Length == (isTwoGeneric ? 2 : 1));

        var genericArgs = isTwoGeneric
            ? new[] { sourceType, selectorType }
            : new[] { sourceType };

        var typedMethod = method.MakeGenericMethod(genericArgs);
        var args = new object[] { query, selector, ct };
        var task = (Task)typedMethod.Invoke(null, args)!;
        await task.ConfigureAwait(false);
        return task.GetType().GetProperty("Result")!.GetValue(task)!;
    }
}
