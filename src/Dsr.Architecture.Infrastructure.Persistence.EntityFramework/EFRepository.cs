using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Result;
using Dsr.Architecture.Domain.Specifications.Interfaces;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Interfaces;
using Dsr.Architecture.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework;

/// <summary>
/// Repository implementation for managing aggregates using Entity Framework Core.
/// This repository acts as a composite wrapper, delegating read operations to <see cref="ReadEFRepository{TContext, TId, TAggregate}"/>
/// and write operations to <see cref="WriteEFRepository{TContext, TId, TAggregate}"/>.
/// </summary>
/// <typeparam name="TContext">The type of the DbContext.</typeparam>
/// <typeparam name="TId">The type of the aggregate's unique identifier.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregate managed by this repository.</typeparam>
public class EFRepository<TContext, TId, TAggregate> : IRepository<TId, TAggregate>, IReadRepository<TId, TAggregate>, IWriteRepository<TId, TAggregate>
    where TContext : DbContext
    where TId : IEquatable<TId>, IComparable<TId>
    where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
{
    private readonly ReadEFRepository<TContext, TId, TAggregate> _readRepository; 
    private readonly WriteEFRepository<TContext, TId, TAggregate> _writeRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="EFRepository{TContext, TId, TAggregate}"/> class with the specified unit of work, specification executor, and loggers.
    /// </summary>
    /// <param name="unitOfWork">The unit of work containing the DbContext.</param>
    /// <param name="executor">The executor for compiled specifications.</param>
    /// <param name="loggerReader">The logger for the read repository.</param>
    /// <param name="loggerWriter">The logger for the write repository.</param>
    public EFRepository(
        IUnitOfWork<TContext> unitOfWork, 
        ICompiledSpecificationExecutor executor, 
        ILogger<ReadEFRepository<TContext, TId, TAggregate>> loggerReader,
        ILogger<WriteEFRepository<TContext, TId, TAggregate>> loggerWriter)
    {
        _readRepository = new ReadEFRepository<TContext, TId, TAggregate> (unitOfWork, executor, loggerReader);
        _writeRepository = new WriteEFRepository<TContext, TId, TAggregate> (unitOfWork, loggerWriter);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EFRepository{TContext, TId, TAggregate}"/> class with the specified transactional unit of work, specification executor, and loggers.
    /// </summary>
    /// <param name="unitOfWork">The transactional unit of work containing the DbContexts.</param>
    /// <param name="executor">The executor for compiled specifications.</param>
    /// <param name="loggerReader">The logger for the read repository.</param>
    /// <param name="loggerWriter">The logger for the write repository.</param>
    public EFRepository(
        ITransactionalEFUnitOfWork unitOfWork, 
        ICompiledSpecificationExecutor executor, 
        ILogger<ReadEFRepository<TContext, TId, TAggregate>> loggerReader,
        ILogger<WriteEFRepository<TContext, TId, TAggregate>> loggerWriter)
    {
        _readRepository = new ReadEFRepository<TContext, TId, TAggregate> (unitOfWork, executor, loggerReader);
        _writeRepository = new WriteEFRepository<TContext, TId, TAggregate> (unitOfWork, loggerWriter);
    }

    #region Search

    #region Sync

    /// <summary>
    /// Retrieves all aggregates that match the specified specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <returns>A <see cref="Result{IEnumerable{TAggregate}}"/> with a collection of matching aggregates.</returns>
    public Result<IEnumerable<TAggregate>> List(ISpecification<TId, TAggregate> specification) => _readRepository.List(specification);

    /// <summary>
    /// Retrieves and projects aggregates that match the specified specification to a different type.
    /// </summary>
    /// <typeparam name="TProjected">The type to project the aggregates to.</typeparam>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="projection">An expression to project the filtered aggregates to <typeparamref name="TProjected"/>.</param>
    /// <returns>A <see cref="Result{IEnumerable{TProjected}}"/> with a collection of projected aggregates.</returns>
    public Result<IEnumerable<TProjected>> List<TProjected>(
        ISpecification<TId, TAggregate> specification,
        Expression<Func<TAggregate, TProjected>> projection) 
        => _readRepository.List(specification, projection);

    /// <summary>
    /// Finds the individual aggregate based on the SpecificationResultCardinality(First, FirstOrDefault, Single, SingleOrDefault) that matches the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <returns>A <see cref="Result{TAggregate}"/> with the first matching aggregate.</returns>
    public Result<TAggregate> Get(ISpecification<TId, TAggregate> specification) => _readRepository.Get(specification);

    /// <summary>
    /// Retrieves an aggregate by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate to retrieve.</param>
    /// <returns>A <see cref="Result{TAggregate}"/> with the retrieved aggregate.</returns>
    public Result<TAggregate> GetById(TId id) => _readRepository.GetById(id);

    /// <summary>
    /// Retrieves a scalar value based on the given specification.
    /// </summary>
    /// <typeparam name="T">The type of the scalar value.</typeparam>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <returns>A <see cref="Result{T}"/> with the retrieved scalar value.</returns>
    public Result<T> GetScalar<T>(ISpecification<TId, TAggregate> specification) where T : struct => _readRepository.GetScalar<T>(specification);

    /// <summary>
    /// Checks if any aggregate matches the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <returns>True if at least one aggregate matches; otherwise, false.</returns>
    public Result<bool> Any(ISpecification<TId, TAggregate> specification) => _readRepository.Any(specification);

    /// <summary>
    /// Counts the number of aggregates that match the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <returns>The total number of matching aggregates.</returns>
    public Result<int> Count(ISpecification<TId, TAggregate> specification) => _readRepository.Count(specification);

    /// <summary>
    /// Retrieves a list of dynamic results based on an analytics specification.
    /// </summary>
    /// <param name="specification">The analytics specification to execute.</param>
    /// <returns>A <see cref="Result{List{dynamic}}"/> with a list of dynamic results.</returns>
    public Result<IEnumerable<dynamic>> ListDynamic(ISpecification<TId, TAggregate> specification)
        => _readRepository.ListDynamic(specification);

    #endregion Sync

    #region Async

    /// <summary>
    /// Asynchronously retrieves aggregates that match the specified specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{IEnumerable{TAggregate}}"/> with a collection of matching aggregates.</returns>
    public Task<Result<IEnumerable<TAggregate>>> ListAsync(
        ISpecification<TId, TAggregate> specification, 
        CancellationToken cancellationToken = default) 
        => _readRepository.ListAsync(specification, cancellationToken);

    /// <summary>
    /// Asynchronously retrieves and projects aggregates that match the specified specification to a different type.
    /// </summary>
    /// <typeparam name="TProjected">The type to project the aggregates to.</typeparam>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="projection">An expression to project the filtered aggregates to <typeparamref name="TProjected"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{IEnumerable{TProjected}}"/> with a collection of projected aggregates.</returns>
    public Task<Result<IEnumerable<TProjected>>> ListAsync<TProjected>(
        ISpecification<TId, TAggregate> specification,
        Expression<Func<TAggregate, TProjected>> projection, 
        CancellationToken cancellationToken = default) 
        => _readRepository.ListAsync(specification, projection, cancellationToken);

    /// <summary>
    /// Asynchronously finds the aggregate that matches the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{TAggregate}"/> with the first matching aggregate.</returns>
    public Task<Result<TAggregate>> GetAsync(ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken = default) 
        => _readRepository.GetAsync(specification, cancellationToken);

    /// <summary>
    /// Asynchronously retrieves an aggregate by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate to retrieve.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{TAggregate}"/> with the retrieved aggregate.</returns>
    public Task<Result<TAggregate>> GetByIdAsync(TId id, CancellationToken cancellationToken = default) 
        => _readRepository.GetByIdAsync(id, cancellationToken);

    /// <summary>
    /// Asynchronously retrieves a scalar value based on the given specification.
    /// </summary>
    /// <typeparam name="T">The type of the scalar value.</typeparam>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with the retrieved scalar value.</returns>
    public Task<Result<T>> GetScalarAsync<T>(ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken = default) where T : struct => _readRepository.GetScalarAsync<T>(specification, cancellationToken);

    /// <summary>
    /// Asynchronously checks if any aggregate matches the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{bool}"/> indicating if a match was found.</returns>
    public Task<Result<bool>> AnyAsync(ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken = default) 
        => _readRepository.AnyAsync(specification, cancellationToken);
    
    /// <summary>
    /// Asynchronously counts the number of aggregates that match the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{int}"/> with the count of matching aggregates.</returns>
    public Task<Result<int>> CountAsync(ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken = default) 
        => _readRepository.CountAsync(specification, cancellationToken);

    /// <summary>
    /// Asynchronously retrieves a list of dynamic results based on an analytics specification.
    /// </summary>
    /// <param name="specification">The analytics specification to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{List{dynamic}}"/> with a list of dynamic results.</returns>
    public Task<Result<IEnumerable<dynamic>>> ListDynamicAsync(ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken = default)
        => _readRepository.ListDynamicAsync(specification, cancellationToken);

    #endregion Async

    #endregion

    #region CUD

    #region Sync

    /// <summary>
    /// Adds a new aggregate to the repository.
    /// </summary>
    /// <param name="aggregate">The aggregate to be added.</param>
    /// <returns>A <see cref="Result{TAggregate}"/> indicating the outcome.</returns>
    public Result<TAggregate> Add(TAggregate aggregate) => _writeRepository.Add(aggregate);

    /// <summary>
    /// Adds multiple new aggregates to the repository.
    /// </summary>
    /// <param name="aggregates">A collection of aggregates to be added.</param>
    /// <returns>A <see cref="Result{ICollection{TAggregate}}"/> indicating the outcome.</returns>
    public Result<ICollection<TAggregate>> AddRange(ICollection<TAggregate> aggregates) => _writeRepository.AddRange(aggregates);

    /// <summary>
    /// Updates an existing aggregate in the repository.
    /// </summary>
    /// <param name="aggregate">The aggregate to be updated.</param>
    /// <returns>A <see cref="Result{TAggregate}"/> indicating the outcome.</returns>
    public Result<TAggregate> Update(TAggregate aggregate) => _writeRepository.Update(aggregate);

    /// <summary>
    /// Removes aggregates from the repository based on a specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates to be removed.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome.</returns>
    public Result Remove(ISpecification<TId, TAggregate> specification) => _writeRepository.Remove(specification);

    /// <summary>
    /// Removes an aggregate from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate to be removed.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome.</returns>
    public Result RemoveById(TId id) => _writeRepository.RemoveById(id);

    /// <summary>
    /// Removes multiple aggregates from the repository based on a specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates to be removed.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome.</returns>
    public Result RemoveRange(ISpecification<TId, TAggregate> specification) => _writeRepository.RemoveRange(specification);

    #endregion Sync

    #region Async

    /// <summary>
    /// Asynchronously adds a new aggregate to the repository.
    /// </summary>
    /// <param name="aggregate">The aggregate to be added.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{TAggregate}"/> indicating the outcome.</returns>
    public Task<Result<TAggregate>> AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default) 
        => _writeRepository.AddAsync(aggregate, cancellationToken);

    /// <summary>
    /// Asynchronously adds multiple new aggregates to the repository.
    /// </summary>
    /// <param name="aggregates">A collection of aggregates to be added.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{ICollection{TAggregate}}"/> indicating the outcome.</returns>
    public Task<Result<ICollection<TAggregate>>> AddRangeAsync(ICollection<TAggregate> aggregates, CancellationToken cancellationToken = default) 
        => _writeRepository.AddRangeAsync(aggregates, cancellationToken);

    /// <summary>
    /// Asynchronously updates an existing aggregate in the repository.
    /// </summary>
    /// <param name="aggregate">The aggregate to be updated.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{TAggregate}"/> indicating the outcome.</returns>
    public Task<Result<TAggregate>> UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken = default) 
        => _writeRepository.UpdateAsync(aggregate, cancellationToken);

    /// <summary>
    /// Asynchronously removes aggregates from the repository based on a specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result"/> indicating the outcome.</returns>
    public Task<Result> RemoveAsync(ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken = default) 
        => _writeRepository.RemoveAsync(specification, cancellationToken);

    /// <summary>
    /// Asynchronously removes an aggregate from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result"/> indicating the outcome.</returns>
    public Task<Result> RemoveByIdAsync(TId id, CancellationToken cancellationToken = default) 
        => _writeRepository.RemoveByIdAsync(id, cancellationToken);

    /// <summary>
    /// Asynchronously removes multiple aggregates from the repository based on a specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result"/> indicating the outcome.</returns>
    public Task<Result> RemoveRangeAsync(ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken = default) 
        => _writeRepository.RemoveRangeAsync(specification, cancellationToken);

    #endregion Async

    #endregion
}