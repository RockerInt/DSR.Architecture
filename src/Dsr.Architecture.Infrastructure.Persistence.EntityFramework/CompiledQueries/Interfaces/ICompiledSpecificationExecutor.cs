using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Specifications.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Interfaces;

/// <summary>
/// Defines an interface for executing specifications against a DbContext, with the ability to utilize compiled queries for improved performance.
/// This interface abstracts the execution logic, allowing for different implementations that can decide when to use compiled queries based on the complexity of the specification or other criteria. 
/// It provides methods for executing specifications that return a list of aggregates or a list of projected results, both asynchronously. 
/// Implementations of this interface can analyze the specification to determine if it is suitable for compilation and can cache compiled queries for reuse, optimizing the execution of complex specifications while avoiding unnecessary overhead for simpler ones.
/// </summary>
public interface ICompiledSpecificationExecutor
{
    /// <summary>
    /// Executes a specification against the provided DbContext, automatically determining whether to use a compiled query based on the complexity of the specification.
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    /// <typeparam name="TAggregate"></typeparam>
    /// <param name="context"></param>
    /// <param name="specification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<dynamic>> ExecuteDynamicAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> specification,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>;

    /// <summary>
    /// Executes a specification against the provided DbContext, automatically determining whether to use a compiled query based on the complexity of the specification.
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    /// <typeparam name="TAggregate"></typeparam>
    /// <param name="context"></param>
    /// <param name="specification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<TAggregate>> ExecuteAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> specification,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>;

    /// <summary>
    /// Executes a specification against the provided DbContext, automatically determining whether to use a compiled query based on the complexity of the specification, and projects the results to a different type.
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    /// <typeparam name="TAggregate"></typeparam>
    /// <typeparam name="TProjected"></typeparam>
    /// <param name="context"></param>
    /// <param name="specification"></param>
    /// <param name="projection"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<TProjected>> ExecuteAsync<TId, TAggregate, TProjected>(
        DbContext context,
        ISpecification<TId, TAggregate> specification,
        Expression<Func<TAggregate, TProjected>> projection,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>;

    /// <summary>
    /// Executes a specification against the provided DbContext, automatically determining whether to use a compiled query based on the complexity of the specification.
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    /// <typeparam name="TAggregate"></typeparam>
    /// <param name="context"></param>
    /// <param name="specification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<dynamic> ExecuteDynamicSingleAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> specification,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>;

    /// <summary>
    /// Executes a specification against the provided DbContext, automatically determining whether to use a compiled query based on the complexity of the specification.
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    /// <typeparam name="TAggregate"></typeparam>
    /// <param name="context"></param>
    /// <param name="specification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TAggregate?> ExecuteSingleAsync<TId, TAggregate>(
        DbContext context,
        ISpecification<TId, TAggregate> specification,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>;

    /// <summary>
    /// Executes a specification against the provided DbContext, automatically determining whether to use a compiled query based on the complexity of the specification, and projects the results to a different type.
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    /// <typeparam name="TAggregate"></typeparam>
    /// <typeparam name="TProjected"></typeparam>
    /// <param name="context"></param>
    /// <param name="specification"></param>
    /// <param name="projection"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TProjected> ExecuteSingleAsync<TId, TAggregate, TProjected>(
        DbContext context,
        ISpecification<TId, TAggregate> specification,
        Expression<Func<TAggregate, TProjected>> projection,
        CancellationToken cancellationToken)
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>;

    /// <summary>
    /// Executes a specification against the provided DbContext, automatically determining whether to use a compiled query based on the complexity of the specification, and retrieves a scalar value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <typeparam name="TAggregate"></typeparam>
    /// <param name="context"></param>
    /// <param name="specification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<T> ExecuteScalarAsync<T, TId, TAggregate>(
        DbContext context, 
        ISpecification<TId, TAggregate> specification,
        CancellationToken cancellationToken) 
        where T : struct
        where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
        where TId : IEquatable<TId>, IComparable<TId>;
}