using System.Linq.Expressions;
using System.Reflection;
using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Specifications;
using Dsr.Architecture.Domain.Specifications.Enums;
using Dsr.Architecture.Domain.Specifications.Interfaces;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Analytics;

/// <summary>
/// Provides extension methods for building Entity Framework queries from analytics specifications.
/// Handles GroupBy, Aggregations, Having clauses, and Projections.
/// </summary>
public static class AnalyticsQueryBuilder
{
    /// <summary>
    /// Builds an IQueryable from an analytics specification, supporting GroupBy and Aggregations.
    /// Returns dynamic results for grouped queries or scalar aggregations.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <param name="query">The base queryable.</param>
    /// <param name="spec">The analytics specification to apply.</param>
    /// <returns>An IQueryable of dynamic objects containing the analytics results.</returns>
    public static IQueryable<dynamic> BuildAnalyticsQuery<TId, TAggregate>(
        this IQueryable<TAggregate> query,
        IAnalyticsSpecification<TId, TAggregate> spec)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        // Apply base specification criteria (Where)
        if (spec.Criteria != null)
            query = query.Where(spec.Criteria);

        // Check if this is a scalar aggregation (no GroupBy)
        if (spec.GroupByExpression == null)
        {
            // For scalar aggregations, we return a wrapper that will be executed later
            return new ScalarAggregationWrapper<TAggregate>(query, spec.Aggregations);
        }

        // Build GroupBy query using reflection since we don't know TKey at compile time
        return BuildGroupByQueryInternal(query, spec);
    }

    /// <summary>
    /// Builds a GroupBy query using reflection to handle unknown key types.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <param name="query">The base queryable.</param>
    /// <param name="spec">The analytics specification.</param>
    /// <returns>An IQueryable of dynamic objects.</returns>
    private static IQueryable<dynamic> BuildGroupByQueryInternal<TId, TAggregate>(
        IQueryable<TAggregate> query,
        IAnalyticsSpecification<TId, TAggregate> spec)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var groupByExpr = spec.GroupByExpression!;
        var keyType = groupByExpr.ReturnType;

        // Use dynamic dispatch to handle the unknown key type
        var method = typeof(AnalyticsQueryBuilder)
            .GetMethod(nameof(BuildGroupByQueryTyped), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(typeof(TId), typeof(TAggregate), keyType);

        return (IQueryable<dynamic>)method.Invoke(null, [query, spec, groupByExpr])!;
    }

    /// <summary>
    /// Builds a typed GroupBy query.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <typeparam name="TKey">The type of the grouping key.</typeparam>
    /// <param name="query">The base queryable.</param>
    /// <param name="spec">The analytics specification.</param>
    /// <param name="groupByExpr">The GroupBy expression.</param>
    /// <returns>An IQueryable of dynamic objects.</returns>
    private static IQueryable<dynamic> BuildGroupByQueryTyped<TId, TAggregate, TKey>(
        IQueryable<TAggregate> query,
        IAnalyticsSpecification<TId, TAggregate> spec,
        LambdaExpression groupByExpr)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var typedGroupBy = (Expression<Func<TAggregate, TKey>>)groupByExpr;
        var grouped = query.GroupBy(typedGroupBy);

        var param = Expression.Parameter(typeof(IGrouping<TKey, TAggregate>), "g");

        // Get the dynamic result type from factory
        var resultType = DynamicTypeFactory.GetOrCreateGroupByResultType<TKey>(spec.Aggregations);

        // Create bindings for the new type
        var bindings = new List<MemberBinding>();

        // Bind Key
        var keyProp = resultType.GetProperty("Key")!;
        bindings.Add(Expression.Bind(keyProp, Expression.Property(param, "Key")));

        // Bind Aggregations
        foreach (var agg in spec.Aggregations)
        {
            var prop = resultType.GetProperty(agg.Alias)!;
            var aggExpr = BuildAggregationExpression(param, agg, typeof(TAggregate));
            bindings.Add(Expression.Bind(prop, aggExpr));
        }

        // Build the Select expression
        var newExpr = Expression.New(resultType.GetConstructor(Type.EmptyTypes)!);
        var memberInit = Expression.MemberInit(newExpr, bindings);

        var selectLambda = Expression.Lambda(memberInit, param);

        // Apply Select using reflection
        var selectMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "Select" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(IGrouping<TKey, TAggregate>), resultType);

        var projected = (IQueryable)selectMethod.Invoke(null, [grouped, selectLambda])!;

        // Apply Having if present
        if (spec.HavingExpression != null)
        {
            projected = ApplyHaving(projected, spec.HavingExpression!, resultType, spec);
        }

        // Convert to IQueryable<dynamic>
        return projected.Cast<object>();
    }

    /// <summary>
    /// Builds an aggregation expression for IGrouping.
    /// </summary>
    /// <param name="groupingParam">The grouping parameter expression.</param>
    /// <param name="aggregation">The aggregation definition.</param>
    /// <param name="elementType">The type of elements in the group.</param>
    /// <returns>An expression representing the aggregation operation.</returns>
    private static Expression BuildAggregationExpression(
        ParameterExpression groupingParam,
        AggregationDefinition aggregation,
        Type elementType)
    {
        return aggregation.Type switch
        {
            AggregationType.Count => BuildCountExpression(groupingParam, elementType),
            AggregationType.Sum => BuildAggregationWithSelector(groupingParam, aggregation, "Sum", elementType),
            AggregationType.Avg => BuildAggregationWithSelector(groupingParam, aggregation, "Average", elementType),
            AggregationType.Max => BuildAggregationWithSelector(groupingParam, aggregation, "Max", elementType),
            AggregationType.Min => BuildAggregationWithSelector(groupingParam, aggregation, "Min", elementType),
            _ => throw new NotSupportedException($"Aggregation type {aggregation.Type} is not supported.")
        };
    }

    /// <summary>
    /// Builds a Count expression for IGrouping.
    /// </summary>
    /// <param name="groupingParam">The grouping parameter.</param>
    /// <param name="elementType">The element type.</param>
    /// <returns>A call expression to Enumerable.Count.</returns>
    private static Expression BuildCountExpression(ParameterExpression groupingParam, Type elementType)
    {
        var countMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "Count" && m.GetParameters().Length == 1)
            .MakeGenericMethod(elementType);
        return Expression.Call(null, countMethod, groupingParam);
    }

    /// <summary>
    /// Builds an aggregation expression with selector for IGrouping.
    /// </summary>
    /// <param name="groupingParam">The grouping parameter.</param>
    /// <param name="aggregation">The aggregation definition.</param>
    /// <param name="methodName">The name of the aggregation method.</param>
    /// <param name="elementType">The element type.</param>
    /// <returns>A call expression to the specified aggregation method.</returns>
    private static Expression BuildAggregationWithSelector(
        ParameterExpression groupingParam,
        AggregationDefinition aggregation,
        string methodName,
        Type elementType)
    {
        var selectorLambda = (LambdaExpression)aggregation.Selector;
        var elementParam = Expression.Parameter(elementType, "x");

        // Replace parameter in selector
        var visitor = new ParameterReplacementVisitor(selectorLambda.Parameters[0], elementParam);
        var newSelectorBody = visitor.Visit(selectorLambda.Body);
        var newSelector = Expression.Lambda(newSelectorBody!, elementParam);

        var resultType = newSelector.ReturnType;

        // Find the Enumerable overload whose second parameter is a Func selector.
        // On .NET 6+, Min/Max have overloads that accept an IComparer as the second argument,
        // so we must filter by Func<,> explicitly to avoid binding to the wrong method.
        var candidates = typeof(Enumerable).GetMethods()
            .Where(m => m.Name == methodName && m.IsGenericMethodDefinition)
            .Select(m => new { Method = m, Parameters = m.GetParameters() })
            .Where(x => x.Parameters.Length == 2
                        && x.Parameters[1].ParameterType.IsGenericType
                        && x.Parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>))
            .ToArray();

        // Prefer a single-generic overload whose selector returns the exact result type
        // (this covers Sum/Average for int/long/double/decimal/float and their nullable variants).
        var singleGeneric = candidates.FirstOrDefault(x =>
            x.Method.GetGenericArguments().Length == 1
            && x.Parameters[1].ParameterType.GetGenericArguments()[1] == resultType);

        MethodInfo method;
        if (singleGeneric != null)
        {
            method = singleGeneric.Method.MakeGenericMethod(elementType);
        }
        else
        {
            // Fall back to the two-generic overload: Min/Max<TSource, TResult>(IEnumerable<TSource>, Func<TSource, TResult>).
            var twoGeneric = candidates.First(x => x.Method.GetGenericArguments().Length == 2);
            method = twoGeneric.Method.MakeGenericMethod(elementType, resultType);
        }

        return Expression.Call(null, method, groupingParam, newSelector);
    }

    /// <summary>
    /// Applies a Having filter to the query.
    /// </summary>
    /// <param name="query">The projected query.</param>
    /// <param name="having">The Having expression.</param>
    /// <param name="resultType">The type of the projected results.</param>
    /// <returns>The filtered IQueryable.</returns>
    private static IQueryable ApplyHaving<TId, TAggregate>(IQueryable query, LambdaExpression having, Type resultType, IAnalyticsSpecification<TId, TAggregate> spec)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
    {
        var whereMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "Where" && m.GetParameters().Length == 2)
            .MakeGenericMethod(resultType);

        var param = Expression.Parameter(resultType, "x");

        // Transform HavingExpression to work with projected result type
        // Having expects original aggregate param, replace with projection properties (Key and aliases)
        var originalParam = Expression.Parameter(typeof(TAggregate), "orig");
        var paramReplacer = new ParameterReplacementVisitor(having.Parameters[0], originalParam);
        var transformed = paramReplacer.Visit(having.Body);

        // Map aggregate properties to projection aliases/Key
        var mapper = new ProjectionPropertyMapper(param, spec.Aggregations, resultType);
        var finalBody = mapper.Visit(transformed);

        var whereLambda = Expression.Lambda(finalBody, param);

        return (IQueryable)whereMethod.Invoke(null, [query, whereLambda])!;
    }

    /// <summary>
    /// Visitor to replace aggregate selectors with projection property access.
    /// </summary>
    private class ProjectionPropertyMapper : ExpressionVisitor
    {
        private readonly ParameterExpression _projectionParam;
        private readonly IReadOnlyList<AggregationDefinition> _aggregations;
        private readonly Type _resultType;

        public ProjectionPropertyMapper(ParameterExpression projectionParam, IReadOnlyList<AggregationDefinition> aggregations, Type resultType)
        {
            _projectionParam = projectionParam;
            _aggregations = aggregations;
            _resultType = resultType;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            foreach (var agg in _aggregations)
            {
                if (IsPropertyAccessForSelector(node, agg.Selector))
                {
                    var aliasProp = _resultType.GetProperty(agg.Alias);
                    return Expression.Property(_projectionParam, aliasProp!);
                }
            }
            return base.VisitMember(node);
        }

        private bool IsPropertyAccessForSelector(MemberExpression member, LambdaExpression selector)
        {
            if (selector.Body is MemberExpression selectorMember)
            {
                return member.Member.Name == selectorMember.Member.Name;
            }
            return false;
        }
    }
}

/// <summary>
/// Wrapper for scalar aggregation results that implements IQueryable of dynamic.
/// </summary>
/// <typeparam name="T">The underlying aggregate type.</typeparam>
internal class ScalarAggregationWrapper<T> : IQueryable<object>
{
    private readonly IQueryable<T> _innerQuery;
    private readonly IReadOnlyList<AggregationDefinition> _aggregations;

    /// <summary>
    /// Initializes a new instance of the ScalarAggregationWrapper class.
    /// </summary>
    /// <param name="query">The base queryable.</param>
    /// <param name="aggregations">The list of aggregations to perform.</param>
    public ScalarAggregationWrapper(IQueryable<T> query, IReadOnlyList<AggregationDefinition> aggregations)
    {
        _innerQuery = query;
        _aggregations = aggregations;
        ElementType = typeof(object);
        Expression = query.Expression;
        Provider = new ScalarAggregationQueryProvider<T>(query.Provider, aggregations);
    }

    /// <inheritdoc />
    public Type ElementType { get; }
    /// <inheritdoc />
    public Expression Expression { get; }
    /// <inheritdoc />
    public IQueryProvider Provider { get; }

    /// <inheritdoc />
    public IEnumerator<dynamic> GetEnumerator()
    {
        var result = ExecuteScalar(_innerQuery, _aggregations[0]);
        yield return result;
    }

    /// <inheritdoc />
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Executes a scalar aggregation.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="aggregation">The aggregation to perform.</param>
    /// <returns>The result of the aggregation.</returns>
    private static object ExecuteScalar(IQueryable<T> query, AggregationDefinition aggregation)
    {
        return aggregation.Type switch
        {
            AggregationType.Count => query.Count(),
            _ => ExecuteAggregationWithSelector(query, aggregation)
        };
    }

    /// <summary>
    /// Executes an aggregation with a selector.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="aggregation">The aggregation definition.</param>
    /// <returns>The result of the aggregation.</returns>
    private static object ExecuteAggregationWithSelector(IQueryable<T> query, AggregationDefinition aggregation)
    {
        var methodName = aggregation.Type switch
        {
            AggregationType.Sum => "Sum",
            AggregationType.Avg => "Average",
            AggregationType.Max => "Max",
            AggregationType.Min => "Min",
            _ => throw new NotSupportedException()
        };

        var selectorLambda = (LambdaExpression)aggregation.Selector;
        var method = typeof(Queryable).GetMethods()
            .Where(m => m.Name == methodName)
            .First(m => m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T));

        return method.Invoke(null, [query, selectorLambda])!;
    }
}

/// <summary>
/// Query provider for scalar aggregation results.
/// </summary>
/// <typeparam name="T">The underlying aggregate type.</typeparam>
internal class ScalarAggregationQueryProvider<T> : IQueryProvider
{
    private readonly IQueryProvider _innerProvider;
    private readonly IReadOnlyList<AggregationDefinition> _aggregations;

    /// <summary>
    /// Initializes a new instance of the ScalarAggregationQueryProvider class.
    /// </summary>
    /// <param name="innerProvider">The inner query provider.</param>
    /// <param name="aggregations">The list of aggregations.</param>
    public ScalarAggregationQueryProvider(IQueryProvider innerProvider, IReadOnlyList<AggregationDefinition> aggregations)
    {
        _innerProvider = innerProvider;
        _aggregations = aggregations;
    }

    /// <inheritdoc />
    public IQueryable CreateQuery(Expression expression) => throw new NotSupportedException();
    /// <inheritdoc />
    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => throw new NotSupportedException();
    /// <inheritdoc />
    public object Execute(Expression expression) => throw new NotSupportedException();
    /// <inheritdoc />
    public TResult Execute<TResult>(Expression expression) => throw new NotSupportedException();
}
