using System.Linq.Expressions;

namespace Dsr.Architecture.Domain.Specifications.Extensions;

public static class SpecificationExpressionExtensions
{
    /*public static IQueryable<T> Apply<T>(this IQueryable<T> query, ISpecification<T> specification)
    {
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

        return query;
    }*/ 
    // TODO: Implement this method in a extension method for the IQueryable<T> class in Infrastructure.Persistence.EntityFramework layer.

    /// <summary>
    /// Combines two expressions with an AND operator.
    /// </summary>
    /// <typeparam name="T">The type of the expression.</typeparam>
    /// <param name="left">The left expression.</param>
    /// <param name="right">The right expression.</param>
    /// <returns>A new expression with the two expressions combined with an AND operator.</returns>
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
    /// Combines two expressions with an OR operator.
    /// </summary>
    /// <typeparam name="T">The type of the expression.</typeparam>
    /// <param name="left">The left expression.</param>
    /// <param name="right">The right expression.</param>
    /// <returns>A new expression with the two expressions combined with an OR operator.</returns>
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