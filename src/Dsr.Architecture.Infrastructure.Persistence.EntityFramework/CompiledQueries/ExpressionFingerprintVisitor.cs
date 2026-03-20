using System.Linq.Expressions;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries;

/// <summary>
/// An expression visitor that generates a "fingerprint" of an expression by replacing parameter expressions with standardized placeholders. 
/// This is used to identify expressions that are structurally the same, regardless of the specific parameter names or values. 
/// It is useful for caching compiled queries based on the shape of the expression tree.
/// </summary>
public sealed class ExpressionFingerprintVisitor : ExpressionVisitor
{
    private readonly Dictionary<ParameterExpression, ParameterExpression> _map = [];
    private int _index = 0;

    /// <summary>
    /// Visits a parameter expression and replaces it with a standardized placeholder. 
    /// If the parameter has already been encountered, it returns the same placeholder for that parameter.
    /// </summary>
    /// <param name="node">The parameter expression node to visit.</param>
    /// <returns>A standardized parameter expression.</returns>
    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (!_map.TryGetValue(node, out var replacement))
        {
            replacement = Expression.Parameter(node.Type, $"p{_index++}");
            _map[node] = replacement;
        }

        return replacement;
    }
}