using System.Linq.Expressions;
using System.Text;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries;

/// <summary>
/// Computes a structural hash for an expression tree. 
/// This is used to generate a unique key for caching compiled queries based on the structure of the expression, rather than its specific parameter values.
/// </summary>
public sealed class ExpressionStructuralHasher : ExpressionVisitor
{
    private readonly StringBuilder _builder = new();

    /// <summary>
    /// Computes a hash for the given expression by visiting its structure and building a string representation that captures the shape of the expression tree.
    /// </summary>
    /// <param name="expression">The expression to compute the hash for.</param>
    /// <returns>A string representation of the expression's structure.</returns>
    public string ComputeHash(Expression expression)
    {
        Visit(expression);
        return _builder.ToString();
    }

    /// <summary>
    /// Overrides the Visit methods for different expression types to build a string representation that captures the structure of the expression tree.
    /// </summary>
    /// <param name="node">The binary expression node to visit.</param>
    /// <returns>The visited expression.</returns>
    protected override Expression VisitBinary(BinaryExpression node)
    {
        _builder.Append($"BIN:{node.NodeType}");
        Visit(node.Left);
        Visit(node.Right);
        return node;
    }

    /// <summary>
    /// Overrides the VisitMember method to include the member name in the hash, and visits the expression that represents the object whose member is being accessed.
    /// </summary>
    /// <param name="node">The member expression node to visit.</param>
    /// <returns>The visited expression.</returns>
    protected override Expression VisitMember(MemberExpression node)
    {
        _builder.Append($"MEM:{node.Member.Name}");
        Visit(node.Expression);
        return node;
    }

    /// <summary>
    /// Overrides the VisitConstant method to include the constant value in the hash.
    /// </summary>
    /// <param name="node">The constant expression node to visit.</param>
    /// <returns>The visited expression.</returns>
    protected override Expression VisitConstant(ConstantExpression node)
    {
        _builder.Append($"CONST:{node.Value}");
        return node;
    }

    /// <summary>
    /// Overrides the VisitParameter method to include the parameter type in the hash.
    /// </summary>
    /// <param name="node">The parameter expression node to visit.</param>
    /// <returns>The visited expression.</returns>
    protected override Expression VisitParameter(ParameterExpression node)
    {
        _builder.Append($"PARAM:{node.Type.Name}");
        return node;
    }

    /// <summary>
    /// Overrides the VisitMethodCall method to include the method name in the hash, and visits all arguments to capture the structure of method calls in the expression tree.
    /// </summary>
    /// <param name="node">The method call expression node to visit.</param>
    /// <returns>The visited expression.</returns>
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        _builder.Append($"CALL:{node.Method.Name}");

        foreach (var arg in node.Arguments)
            Visit(arg);

        return node;
    }
}