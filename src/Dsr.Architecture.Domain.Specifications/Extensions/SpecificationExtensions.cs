using System.Linq.Expressions;

namespace Dsr.Architecture.Domain.Specifications.Extensions;

/// <summary>
/// Provides extension methods for combining LINQ expressions.
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Combines two expressions using a logical AND operator.
    /// </summary>
    /// <typeparam name="T">The type of the expression parameter.</typeparam>
    /// <param name="left">The first expression.</param>
    /// <param name="right">The second expression.</param>
    /// <returns>A new expression representing the logical AND of both input expressions.</returns>
    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var param = Expression.Parameter(typeof(T));

        var body = Expression.AndAlso(
            Expression.Invoke(left, param),
            Expression.Invoke(right, param));

        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    /// <summary>
    /// Combines two expressions using a logical OR operator.
    /// </summary>
    /// <typeparam name="T">The type of the expression parameter.</typeparam>
    /// <param name="left">The first expression.</param>
    /// <param name="right">The second expression.</param>
    /// <returns>A new expression representing the logical OR of both input expressions.</returns>
    public static Expression<Func<T, bool>> Or<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var param = Expression.Parameter(typeof(T));

        var body = Expression.OrElse(
            Expression.Invoke(left, param),
            Expression.Invoke(right, param));

        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}