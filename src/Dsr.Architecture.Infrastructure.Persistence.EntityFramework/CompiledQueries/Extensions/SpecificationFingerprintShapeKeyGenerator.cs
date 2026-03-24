using System.Linq.Expressions;
using System.Text;
using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Specifications.Interfaces;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Extensions;

/// <summary>
/// Generates a unique key representing the "shape" of a specification, used for caching compiled queries. 
/// The key is based on the type of the aggregate and the structure of the specification's criteria and includes. 
/// This allows for efficient retrieval of compiled queries that match the same shape, even if the specific parameter values differ.
/// </summary>
public static class SpecificationFingerprintShapeKeyGenerator
{
    /// <summary>
    /// Generates a unique key for a given specification by analyzing its structure.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <param name="spec">The specification to generate a key for.</param>
    /// <returns>A string fingerprint representing the query shape.</returns>
    public static string GenerateKey<TId, TAggregate>(
        this ISpecification<TId, TAggregate> spec)
        where TAggregate : IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var builder = new StringBuilder();

        builder.Append(typeof(TAggregate).FullName);

        if (spec.Criteria != null)
        {
            var visitor = new ExpressionFingerprintVisitor();
            var normalized = visitor.Visit(spec.Criteria);

            var hasher = new ExpressionStructuralHasher();
            builder.Append("|WHERE:");
            builder.Append(hasher.ComputeHash(normalized));
        }

        if (spec.Includes.Count > 0)
            builder.Append($"|INC:{spec.Includes.Count}");

        if (spec.IncludeStrings.Count > 0)
            builder.Append($"|INCS:{spec.IncludeStrings.Count}");

        if (spec.OrderBy != null)
            builder.Append("|OB");

        if (spec.OrderByDescending != null)
            builder.Append("|OBD");

        if (spec.Skip.HasValue)
            builder.Append("|SKIP");

        if (spec.Take.HasValue)
            builder.Append("|TAKE");

        if (spec.NoTracking)
            builder.Append("|NT");

        if (spec.SplitQuery)
            builder.Append("|SQ");

        return builder.ToString();
    }

    /// <summary>
    /// Generates a unique key for a given specification with a projection.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <typeparam name="TProjected">The type of the projected results.</typeparam>
    /// <param name="spec">The specification to generate a key for.</param>
    /// <param name="projection">The projection expression to include in the fingerprint.</param>
    /// <returns>A string fingerprint representing the query and projection shape.</returns>
    public static string GenerateKey<TId, TAggregate, TProjected>(
        this ISpecification<TId, TAggregate> spec,
        Expression<Func<TAggregate, TProjected>> projection)
        where TAggregate : IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var builder = new StringBuilder();

        var visitor = new ExpressionFingerprintVisitor();
        var normalized = visitor.Visit(projection);

        var hasher = new ExpressionStructuralHasher();
        builder.Append("|SELECT:");
        builder.Append(hasher.ComputeHash(normalized));
        builder.Append(spec.GenerateKey());

        return builder.ToString();
    }

    /// <summary>
    /// Generates a unique key for an analytics specification, including GroupBy, Aggregations, and Having.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <param name="spec">The analytics specification to generate a key for.</param>
    /// <returns>A string fingerprint representing the analytics query shape.</returns>
    public static string GenerateAnalyticsKey<TId, TAggregate>(
        this IAnalyticsSpecification<TId, TAggregate> spec)
        where TAggregate : IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var builder = new StringBuilder();
        var visitor = new ExpressionFingerprintVisitor();
        var hasher = new ExpressionStructuralHasher();

        builder.Append(typeof(TAggregate).FullName);
        builder.Append("|ANALYTICS");

        // Include base specification key
        builder.Append(((ISpecification<TId, TAggregate>)spec).GenerateKey());

        // GroupBy expression
        if (spec.GroupByExpression != null)
        {
            var normalized = visitor.Visit(spec.GroupByExpression);
            builder.Append("|GB:");
            builder.Append(hasher.ComputeHash(normalized));
        }

        // Aggregations
        foreach (var agg in spec.Aggregations)
        {
            builder.Append($"|AGG:{agg.Type}:{agg.Alias}:");
            var normalized = visitor.Visit(agg.Selector);
            builder.Append(hasher.ComputeHash(normalized));
        }

        // Having expression
        if (spec.HavingExpression != null)
        {
            var normalized = visitor.Visit(spec.HavingExpression);
            builder.Append("|HAVING:");
            builder.Append(hasher.ComputeHash(normalized));
        }

        // Custom projection
        if (spec.Projection != null)
        {
            var normalized = visitor.Visit(spec.Projection);
            builder.Append("|PROJ:");
            builder.Append(hasher.ComputeHash(normalized));
        }

        return builder.ToString();
    }
}