using System.Linq.Expressions;
using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Specifications;
using Dsr.Architecture.Domain.Specifications.Enums;
using Dsr.Architecture.Domain.Specifications.Interfaces;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Extensions;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries;

/// <summary>
/// An implementation of ICompiledSpecificationExecutor that automatically decides whether to use a compiled query based on the complexity of the specification. 
/// This executor analyzes the specification using the ISpecificationComplexityAnalyzer and caches the analysis results to avoid redundant analysis. 
/// If the specification is deemed suitable for compilation, it retrieves or creates a compiled query from the CompiledQueryCache and executes it. 
/// Otherwise, it executes the query directly against the DbContext without compilation. 
/// This approach optimizes performance by leveraging compiled queries for complex specifications while avoiding unnecessary overhead for simpler ones.
/// </summary>
/// <param name="cache">The cache for compiled queries.</param>
/// <param name="analysisCache">The cache for specification complexity analysis results.</param>
/// <param name="analyzer">The analyzer used to determine specification complexity.</param>
public sealed class AutoCompiledSpecificationExecutor(
    CompiledQueryCache cache,
    SpecificationAnalysisCache analysisCache,
    ISpecificationComplexityAnalyzer analyzer)
        : ICompiledSpecificationExecutor
{
    private readonly CompiledQueryCache _cache = cache;
    private readonly SpecificationAnalysisCache _analysisCache = analysisCache;
    private readonly ISpecificationComplexityAnalyzer _analyzer = analyzer;

    /// <summary>
    /// Executes a specification against the provided DbContext, automatically determining whether to use a compiled query based on the complexity of the specification.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's unique identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <param name="context">The DbContext to execute the query against.</param>
    /// <param name="specification">The specification to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the list of matching aggregates.</returns>
    public async Task<List<TAggregate>> ExecuteAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> specification,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var key = specification.GenerateKey();

        if (!AnalyzeSpecification(key, specification))
        {
            var _query = context.BuildQuery(specification);
            return await _query.ToListAsync(cancellationToken);
        }

        var compiled = _cache.GetOrAdd(key, () => specification.Create());

        var query = (Func<DbContext, Task<List<TAggregate>>>)compiled;

        return await query(context);
    }

    /// <summary>
    /// Executes a specification against the provided DbContext, automatically determining whether to use a compiled query based on the complexity of the specification, and projects the results.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's unique identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <typeparam name="TProjected">The type of the projected results.</typeparam>
    /// <param name="context">The DbContext to execute the query against.</param>
    /// <param name="specification">The specification to execute.</param>
    /// <param name="projection">The projection expression.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the list of projected results.</returns>
    public async Task<List<TProjected>> ExecuteAsync<TId, TAggregate, TProjected>(
        DbContext context,
        ISpecification<TId, TAggregate> specification,
        Expression<Func<TAggregate, TProjected>> projection,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var key = specification.GenerateKey(projection);

        if (!AnalyzeSpecification(key, specification))
        {
            var _query = context.BuildQuery(specification, projection);
            return await _query.ToListAsync(cancellationToken);
        }

        var compiled = _cache.GetOrAdd(key, () => specification.Create(projection));

        var query = (Func<DbContext, Task<List<TProjected>>>)compiled;

        return await query(context);
    }

    /// <summary>
    /// Executes an analytics specification and returns a list of dynamic results.
    /// Supports GroupBy with aggregations.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's unique identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <param name="context">The DbContext to execute the query against.</param>
    /// <param name="specification">The analytics specification to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing a list of dynamic results.</returns>
    public async Task<List<dynamic>> ExecuteDynamicAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> specification,
        CancellationToken cancellationToken)
        where TId : IEquatable<TId>, IComparable<TId>
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
    {
        if (specification is not IAnalyticsSpecification<TId, TAggregate> analyticsSpec)
        {
            throw new InvalidOperationException("ExecuteDynamicAsync requires an AnalyticsSpecification with GroupBy and/or Aggregations.");
        }

        var key = analyticsSpec.GenerateAnalyticsKey();
        if (!AnalyzeSpecification(key, specification))
        {
            var query = context.Set<TAggregate>().BuildAnalyticsQuery(analyticsSpec);
            var results = await query.Cast<object>().ToListAsync(cancellationToken);
            return results.Cast<dynamic>().ToList();
        }

        var analyticsQuery = context.Set<TAggregate>().BuildAnalyticsQuery(analyticsSpec);
        var results2 = await analyticsQuery.Cast<object>().ToListAsync(cancellationToken);
        return results2.Cast<dynamic>().ToList();
    }

    /// <summary>
    /// Executes an analytics specification and returns a single dynamic result.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's unique identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <param name="context">The DbContext to execute the query against.</param>
    /// <param name="specification">The analytics specification to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing a single dynamic result.</returns>
    public async Task<dynamic> ExecuteDynamicSingleAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> specification,
        CancellationToken cancellationToken)
        where TId : IEquatable<TId>, IComparable<TId>
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
    {
        var results = await ExecuteDynamicAsync<TId, TAggregate>(context, specification, cancellationToken);
        return results.FirstOrDefault()!;
    }

    /// <summary>
    /// Executes a specification and returns a scalar value (e.g., Count, Sum, Avg).
    /// For scalar aggregations without GroupBy.
    /// </summary>
    /// <typeparam name="T">The type of the scalar value to return.</typeparam>
    /// <typeparam name="TId">The type of the aggregate's unique identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <param name="context">The DbContext to execute the query against.</param>
    /// <param name="specification">The specification to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the scalar value.</returns>
    public async Task<T> ExecuteScalarAsync<T, TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> specification,
        CancellationToken cancellationToken)
        where T : struct
        where TId : IEquatable<TId>, IComparable<TId>
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
    {
        if (specification is not IAnalyticsSpecification<TId, TAggregate> analyticsSpec)
        {
            throw new InvalidOperationException("ExecuteScalarAsync requires an AnalyticsSpecification with aggregations.");
        }

        var query = context.Set<TAggregate>().AsQueryable();

        // Apply base criteria
        if (specification.Criteria != null)
            query = query.Where(specification.Criteria);

        // Apply NoTracking
        if (specification.NoTracking)
            query = query.AsNoTracking();

        // Get the aggregation definition
        var aggregation = analyticsSpec.Aggregations.FirstOrDefault();
        if (aggregation == null)
        {
            throw new InvalidOperationException("AnalyticsSpecification must have at least one aggregation for ExecuteScalarAsync.");
        }

        // Execute the appropriate aggregation
        return aggregation.Type switch
        {
            AggregationType.Count => await ExecuteCountAsync<T, TId, TAggregate>(query, cancellationToken),
            AggregationType.Sum => await ExecuteSumAsync<T, TId, TAggregate>(query, aggregation, cancellationToken),
            AggregationType.Avg => await ExecuteAverageAsync<T, TId, TAggregate>(query, aggregation, cancellationToken),
            AggregationType.Max => await ExecuteMaxAsync<T, TId, TAggregate>(query, aggregation, cancellationToken),
            AggregationType.Min => await ExecuteMinAsync<T, TId, TAggregate>(query, aggregation, cancellationToken),
            _ => throw new NotSupportedException($"Aggregation type {aggregation.Type} is not supported for ExecuteScalarAsync.")
        };
    }

    /// <summary>
    /// Executes a specification and returns a single aggregate.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's unique identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <param name="context">The DbContext to execute the query against.</param>
    /// <param name="specification">The specification to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing a single aggregate or null.</returns>
    public async Task<TAggregate?> ExecuteSingleAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> specification,
        CancellationToken cancellationToken)
        where TId : IEquatable<TId>, IComparable<TId>
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
    {
        var results = await ExecuteAsync(context, specification, cancellationToken);
        return results.FirstOrDefault();
    }

    /// <summary>
    /// Executes a specification and returns a single projected aggregate.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's unique identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <typeparam name="TProjected">The type of the projected result.</typeparam>
    /// <param name="context">The DbContext to execute the query against.</param>
    /// <param name="specification">The specification to execute.</param>
    /// <param name="projection">The projection expression.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing a single projected result.</returns>
    public async Task<TProjected> ExecuteSingleAsync<TId, TAggregate, TProjected>(
        DbContext context,
        ISpecification<TId, TAggregate> specification,
        Expression<Func<TAggregate, TProjected>> projection,
        CancellationToken cancellationToken)
        where TId : IEquatable<TId>, IComparable<TId>
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
    {
        var results = await ExecuteAsync(context, specification, projection, cancellationToken);
        return results.FirstOrDefault()!;
    }

    #region Private Aggregation Helpers

    private static async Task<T> ExecuteCountAsync<T, TId, TAggregate>(
        IQueryable<TAggregate> query,
        CancellationToken cancellationToken)
        where T : struct
        where TId : IEquatable<TId>, IComparable<TId>
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
    {
        var count = await query.CountAsync(cancellationToken);
        return (T)(object)count;
    }

    private static async Task<T> ExecuteSumAsync<T, TId, TAggregate>(
        IQueryable<TAggregate> query,
        AggregationDefinition aggregation,
        CancellationToken cancellationToken)
        where T : struct
        where TId : IEquatable<TId>, IComparable<TId>
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
    {
        var selectorLambda = (LambdaExpression)aggregation.Selector;
        var result = await ExecuteSumInternalAsync(query, selectorLambda, cancellationToken);
        return (T)result;
    }

    private static async Task<object> ExecuteSumInternalAsync<TAggregate>(
        IQueryable<TAggregate> query,
        LambdaExpression selector,
        CancellationToken cancellationToken)
    {
        // Fix: Use reflection to invoke correct SumAsync overload matching the selector return type
        var method = typeof(EntityFrameworkQueryableExtensions).GetMethods()
            .Where(m => m.Name == nameof(EntityFrameworkQueryableExtensions.SumAsync)
                        && m.GetParameters().Length == 3
                        && m.GetGenericArguments().Length == 1)
            .First(m => m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments()[1] == selector.ReturnType);

        var genericMethod = method.MakeGenericMethod(typeof(TAggregate));
        var task = (Task)genericMethod.Invoke(null, [query, selector, cancellationToken])!;

        await task.ConfigureAwait(false);
        return task.GetType().GetProperty("Result")!.GetValue(task)!;
    }

    private static async Task<T> ExecuteAverageAsync<T, TId, TAggregate>(
        IQueryable<TAggregate> query,
        AggregationDefinition aggregation,
        CancellationToken cancellationToken)
        where T : struct
        where TId : IEquatable<TId>, IComparable<TId>
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
    {
        // Average always returns double
        var selectorLambda = (LambdaExpression)aggregation.Selector;
        // Fix: AverageAsync is generic only on TSource, select overload by selector type
        var avgMethod = typeof(EntityFrameworkQueryableExtensions).GetMethods()
            .Where(m => m.Name == nameof(EntityFrameworkQueryableExtensions.AverageAsync)
                        && m.GetParameters().Length == 3
                        && m.GetGenericArguments().Length == 1)
            .First(m => m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments()[1] == selectorLambda.ReturnType);

        var genericMethod = avgMethod.MakeGenericMethod(typeof(TAggregate));
        var task = (Task)genericMethod.Invoke(null, [query, selectorLambda, cancellationToken])!;
        
        await task.ConfigureAwait(false);
        var resultProperty = task.GetType().GetProperty("Result");
        var result = resultProperty!.GetValue(task);
        return result is null ? default : (T)Convert.ChangeType(result, typeof(T));
    }

    private static async Task<T> ExecuteMaxAsync<T, TId, TAggregate>(
        IQueryable<TAggregate> query,
        AggregationDefinition aggregation,
        CancellationToken cancellationToken)
        where T : struct
        where TId : IEquatable<TId>, IComparable<TId>
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
    {
        var selectorLambda = (LambdaExpression)aggregation.Selector;
        var result = await ExecuteMaxInternalAsync(query, selectorLambda, cancellationToken);
        return (T)result;
    }

    private static async Task<object> ExecuteMaxInternalAsync<TAggregate>(
        IQueryable<TAggregate> query,
        LambdaExpression selector,
        CancellationToken cancellationToken)
    {
        // Fix: MaxAsync is generic on <TSource, TResult>
        var method = typeof(EntityFrameworkQueryableExtensions).GetMethods()
            .First(m => m.Name == nameof(EntityFrameworkQueryableExtensions.MaxAsync)
                        && m.GetParameters().Length == 3
                        && m.GetGenericArguments().Length == 2);

        var genericMethod = method.MakeGenericMethod(typeof(TAggregate), selector.ReturnType);
        var task = (Task)genericMethod.Invoke(null, [query, selector, cancellationToken])!;

        await task.ConfigureAwait(false);
        return task.GetType().GetProperty("Result")!.GetValue(task)!;
    }

    private static async Task<T> ExecuteMinAsync<T, TId, TAggregate>(
        IQueryable<TAggregate> query,
        AggregationDefinition aggregation,
        CancellationToken cancellationToken)
        where T : struct
        where TId : IEquatable<TId>, IComparable<TId>
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
    {
        var selectorLambda = (LambdaExpression)aggregation.Selector;
        var result = await ExecuteMinInternalAsync(query, selectorLambda, cancellationToken);
        return (T)result;
    }

    private static async Task<object> ExecuteMinInternalAsync<TAggregate>(
        IQueryable<TAggregate> query,
        LambdaExpression selector,
        CancellationToken cancellationToken)
    {
        // Fix: MinAsync is generic on <TSource, TResult>
        var method = typeof(EntityFrameworkQueryableExtensions).GetMethods()
            .First(m => m.Name == nameof(EntityFrameworkQueryableExtensions.MinAsync)
                        && m.GetParameters().Length == 3
                        && m.GetGenericArguments().Length == 2);

        var genericMethod = method.MakeGenericMethod(typeof(TAggregate), selector.ReturnType);
        var task = (Task)genericMethod.Invoke(null, [query, selector, cancellationToken])!;

        await task.ConfigureAwait(false);
        return task.GetType().GetProperty("Result")!.GetValue(task)!;
    }

    #endregion Private Aggregation Helpers

    /// <summary>
    /// Analyzes the specification to determine if it is suitable for compilation.
    /// This method uses the ISpecificationComplexityAnalyzer to analyze the specification and caches the results in the SpecificationAnalysisCache to avoid redundant analysis for the same specification shape. 
    /// The key for caching is generated from the specification's shape, which includes the type of aggregate, criteria, includes, ordering, and other properties that affect the structure of the query. 
    /// By caching the analysis results, this method ensures that the complexity analysis is performed only once for each unique specification shape, improving the performance of subsequent executions of specifications with the same shape.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's unique identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <param name="key">The cache key generated for the specification.</param>
    /// <param name="specification">The specification to analyze.</param>
    /// <returns>True if the specification should use a compiled query; otherwise, false.</returns>
    private bool AnalyzeSpecification<TId, TAggregate>(string key, ISpecification<TId, TAggregate> specification)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
        => _analysisCache.GetOrAdd(key, () => _analyzer.Analyze(specification)).ShouldUseCompiledQuery;
}