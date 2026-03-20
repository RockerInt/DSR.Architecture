using System.Linq.Expressions;
using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Specifications;
using Dsr.Architecture.Domain.Specifications.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Extensions;

/// <summary>
/// Provides extension methods for creating compiled queries from specifications. 
/// This class contains a method that creates a compiled query from a specification using the EF.CompileAsyncQuery method. 
/// The compiled query is created by building a query from the specification using the BuildQuery extension method and then converting it to a list. 
/// The resulting compiled query can be executed asynchronously to retrieve the results of the specification.
/// </summary>
public static class CompiledQueryFactory
{
    /// <summary>
    /// Creates a compiled query from a specification. 
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <param name="spec">The specification to compile.</param>
    /// <returns>A delegate representing the compiled query.</returns>
    public static Func<DbContext, Task<List<TAggregate>>> Create<TId, TAggregate>(
        this ISpecification<TId, TAggregate> spec)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
        => EF.CompileAsyncQuery((DbContext context) =>
            context.BuildQuery(spec).ToList());

    /// <summary>
    /// Creates a compiled query from a specification with a projection. 
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <typeparam name="TProjected">The type of the projected results.</typeparam>
    /// <param name="spec">The specification to compile.</param>
    /// <param name="projection">The projection expression.</param>
    /// <returns>A delegate representing the compiled query with projection.</returns>
    public static Func<DbContext, Task<List<TProjected>>> Create<TId, TAggregate, TProjected>(
        this ISpecification<TId, TAggregate> spec,
        Expression<Func<TAggregate, TProjected>> projection)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>
        => EF.CompileAsyncQuery((DbContext context) =>
            context.BuildQuery(spec, projection).ToList());
}