using System.Linq.Expressions;
using System.Reflection;
using Dsr.Architecture.Domain.Specifications;
using Dsr.Architecture.Domain.Specifications.Enums;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Analytics;

/// <summary>
/// Builds Expression Trees for dynamic aggregation operations.
/// This class generates aggregation calls for Sum, Count, Avg, Max, Min.
/// </summary>
public static class AggregationExpressionBuilder
{
    /// <summary>
    /// Gets the return type for an aggregation operation based on the aggregation definition.
    /// </summary>
    /// <param name="aggregation">The aggregation definition containing the type and selector.</param>
    /// <returns>The CLR type of the aggregation result.</returns>
    public static Type GetAggregationReturnType(AggregationDefinition aggregation)
    {
        var selectorType = aggregation.Selector.ReturnType;
        return aggregation.Type switch
        {
            AggregationType.Count => typeof(int),
            AggregationType.Sum => selectorType,
            AggregationType.Avg => typeof(double),
            AggregationType.Max => selectorType,
            AggregationType.Min => selectorType,
            _ => typeof(object)
        };
    }

    /// <summary>
    /// Gets the return type for an aggregation operation with nullable handling.
    /// </summary>
    /// <param name="type">The type of aggregation (Sum, Count, etc.).</param>
    /// <param name="selectorReturnType">The return type of the selector expression.</param>
    /// <returns>The CLR type of the aggregation result.</returns>
    /// <exception cref="NotSupportedException">Thrown if the aggregation type is not supported.</exception>
    public static Type GetAggregationReturnType(AggregationType type, Type selectorReturnType)
    {
        return type switch
        {
            AggregationType.Count => typeof(int),
            AggregationType.Sum => selectorReturnType,
            AggregationType.Avg => typeof(double),
            AggregationType.Max => selectorReturnType,
            AggregationType.Min => selectorReturnType,
            _ => throw new NotSupportedException($"Aggregation type {type} is not supported.")
        };
    }

    /// <summary>
    /// Gets the Enumerable aggregation method for the specified aggregation type.
    /// </summary>
    /// <param name="type">The type of aggregation.</param>
    /// <param name="elementType">The type of the elements in the collection.</param>
    /// <param name="resultType">Optional expected result type.</param>
    /// <returns>The MethodInfo representing the Enumerable aggregation method.</returns>
    /// <exception cref="NotSupportedException">Thrown if the aggregation type is not supported.</exception>
    public static MethodInfo GetEnumerableAggregationMethod(AggregationType type, Type elementType, Type? resultType = null)
    {
        var methodName = type switch
        {
            AggregationType.Count => "Count",
            AggregationType.Sum => "Sum",
            AggregationType.Avg => "Average",
            AggregationType.Max => "Max",
            AggregationType.Min => "Min",
            _ => throw new NotSupportedException($"Aggregation type {type} is not supported.")
        };

        var methods = typeof(Enumerable).GetMethods()
            .Where(m => m.Name == methodName);

        if (type == AggregationType.Count)
        {
            // Count has overloads without selector
            return methods.First(m => m.GetParameters().Length == 1)
                .MakeGenericMethod(elementType);
        }

        // For other aggregations, we need the version with selector
        return methods.First(m => m.GetParameters().Length == 2)
            .MakeGenericMethod(elementType);
    }
}

/// <summary>
/// Visitor that replaces parameter expressions in expression trees.
/// </summary>
internal class ParameterReplacementVisitor : ExpressionVisitor
{
    private readonly ParameterExpression _oldParameter;
    private readonly ParameterExpression _newParameter;

    /// <summary>
    /// Initializes a new instance of the ParameterReplacementVisitor class.
    /// </summary>
    /// <param name="oldParameter">The parameter to be replaced.</param>
    /// <param name="newParameter">The new parameter to use instead.</param>
    public ParameterReplacementVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
    {
        _oldParameter = oldParameter;
        _newParameter = newParameter;
    }

    /// <summary>
    /// Visits the parameter expression and replaces it if it matches the old parameter.
    /// </summary>
    /// <param name="node">The parameter expression to visit.</param>
    /// <returns>The original or the new parameter expression.</returns>
    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == _oldParameter ? _newParameter : base.VisitParameter(node);
    }
}
