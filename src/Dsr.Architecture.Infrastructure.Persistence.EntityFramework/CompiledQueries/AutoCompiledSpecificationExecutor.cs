using System.Linq.Expressions;
using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Specifications;
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
    /// Not implemented. Executes a specification and returns a list of dynamic results.
    /// </summary>
    public Task<List<dynamic>> ExecuteDynamicAsync<TId, TAggregate>(DbContext context, ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken)
        where TId : IEquatable<TId>, IComparable<TId>
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Not implemented. Executes a specification and returns a single dynamic result.
    /// </summary>
    public Task<dynamic> ExecuteDynamicSingleAsync<TId, TAggregate>(DbContext context, ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken)
        where TId : IEquatable<TId>, IComparable<TId>
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Not implemented. Executes a specification and returns a scalar value.
    /// </summary>
    public Task<T> ExecuteScalarAsync<T, TId, TAggregate>(DbContext context, ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken)
        where T : struct
        where TId : IEquatable<TId>, IComparable<TId>
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Not implemented. Executes a specification and returns a single aggregate.
    /// </summary>
    public Task<TAggregate?> ExecuteSingleAsync<TId, TAggregate>(DbContext context, ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken)
        where TId : IEquatable<TId>, IComparable<TId>
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Not implemented. Executes a specification and returns a single projected aggregate.
    /// </summary>
    public Task<TProjected> ExecuteSingleAsync<TId, TAggregate, TProjected>(DbContext context, ISpecification<TId, TAggregate> specification, Expression<Func<TAggregate, TProjected>> projection, CancellationToken cancellationToken)
        where TId : IEquatable<TId>, IComparable<TId>
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
    {
        throw new NotImplementedException();
    }

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