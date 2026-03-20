using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Result;
using Dsr.Architecture.Domain.Specifications.Interfaces;
using Dsr.Architecture.Persistence.Abstractions;
using Dsr.Architecture.Utilities.TryCatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework;

/// <summary>
/// Repository implementation for write operations using Entity Framework Core.
/// This repository provides methods for adding, updating, and removing aggregates from the database.
/// It uses a DbContext to perform database operations and includes error handling with logging.
/// </summary>
/// <typeparam name="TContext">The type of the DbContext.</typeparam>
/// <typeparam name="TId">The type of the aggregate's unique identifier.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregate managed by this repository.</typeparam>
public class WriteEFRepository<TContext, TId, TAggregate> : IWriteRepository<TId, TAggregate>
    where TContext : DbContext
    where TId : IEquatable<TId>, IComparable<TId>
    where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
{
    private readonly DbContext _context;
    private readonly DbSet<TAggregate> _dbSet;
    private readonly ILogger<WriteEFRepository<TContext, TId, TAggregate>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WriteEFRepository{TContext, TId, TAggregate}"/> class with the specified unit of work and logger.
    /// </summary>
    /// <param name="unitOfWork">The unit of work containing the DbContext.</param>
    /// <param name="logger">The logger for this repository.</param>
    public WriteEFRepository(IUnitOfWork<TContext> unitOfWork, ILogger<WriteEFRepository<TContext, TId, TAggregate>> logger)
    {
        _context = unitOfWork.Context;
        _dbSet = _context.Set<TAggregate>();
        _logger = logger;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WriteEFRepository{TContext, TId, TAggregate}"/> class with the specified transactional unit of work and logger.
    /// </summary>
    /// <param name="unitOfWork">The transactional unit of work containing the DbContexts.</param>
    /// <param name="logger">The logger for this repository.</param>
    /// <exception cref="InvalidOperationException">Thrown if no DbContext of type <typeparamref name="TContext"/> is found in the unit of work.</exception>
    public WriteEFRepository(ITransactionalEFUnitOfWork unitOfWork, ILogger<WriteEFRepository<TContext, TId, TAggregate>> logger)
    {
        _context = unitOfWork.Accessor.DbContexts.FirstOrDefault(
            x => x is TContext) ?? throw new InvalidOperationException($"No DbContext of type {typeof(TContext).Name} found in the unit of work."
        );
        _dbSet = _context.Set<TAggregate>();
        _logger = logger;
    }


    #region CUD

    #region Sync

    /// <summary>
    /// Adds a new aggregate to the repository.
    /// </summary>
    /// <param name="entity">The aggregate to be added.</param>
    /// <returns>A <see cref="Result{TAggregate}"/> indicating the outcome.</returns>
    public Result<TAggregate> Add(TAggregate entity) => AddAsync(entity).GetAwaiter().GetResult();

    /// <summary>
    /// Adds multiple new aggregates to the repository.
    /// </summary>
    /// <param name="entities">A collection of aggregates to be added.</param>
    /// <returns>A <see cref="Result{ICollection{TAggregate}}"/> indicating the outcome.</returns>
    public Result<ICollection<TAggregate>> AddRange(ICollection<TAggregate> entities) => AddRangeAsync(entities).GetAwaiter().GetResult();

    /// <summary>
    /// Updates an existing aggregate in the repository.
    /// </summary>
    /// <param name="entity">The aggregate to be updated.</param>
    /// <returns>A <see cref="Result{TAggregate}"/> indicating the outcome.</returns>
    public Result<TAggregate> Update(TAggregate entity) => UpdateAsync(entity).GetAwaiter().GetResult();

    /// <summary>
    /// Removes an aggregate from the repository based on a specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregate to be removed.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome.</returns>
    public Result Remove(ISpecification<TId, TAggregate> specification) => RemoveAsync(specification).GetAwaiter().GetResult();

    /// <summary>
    /// Removes an aggregate from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate to be removed.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome.</returns>
    public Result RemoveById(TId id) => RemoveByIdAsync(id).GetAwaiter().GetResult();

    /// <summary>
    /// Removes multiple aggregates from the repository based on a specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates to be removed.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome.</returns>
    public Result RemoveRange(ISpecification<TId, TAggregate> specification) => RemoveRangeAsync(specification).GetAwaiter().GetResult();

    #endregion

    #region Async

    /// <summary>
    /// Asynchronously adds a new aggregate to the repository.
    /// </summary>
    /// <param name="entity">The aggregate to be added.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{TAggregate}"/> indicating the outcome.</returns>
    public async Task<Result<TAggregate>> AddAsync(TAggregate entity, CancellationToken cancellationToken = default)
        => await this.Try(async () =>
            {
                if (entity is null)
                    return Result<TAggregate>.Invalid(new Error() { Message = "Entity cannot be null." });
                await _dbSet.AddAsync(entity, cancellationToken);
                return Result<TAggregate>.Success(entity);
            })
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while adding the entity.");
                return await Task.FromResult(Result<TAggregate>.Error(error.Message));
            })
            .Apply() ?? Result<TAggregate>.Error("An error occurred while adding the entity.");

    /// <summary>
    /// Asynchronously adds multiple new aggregates to the repository.
    /// </summary>
    /// <param name="entities">A collection of aggregates to be added.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{ICollection{TAggregate}}"/> indicating the outcome.</returns>
    public async Task<Result<ICollection<TAggregate>>> AddRangeAsync(ICollection<TAggregate> entities, CancellationToken cancellationToken = default)
        => await this.Try(async () =>
            {
                if (entities is null || entities.Count == 0)
                    return Result<ICollection<TAggregate>>.Invalid(new Error() { Message = "Entities collection cannot be null or empty." });
                await _dbSet.AddRangeAsync(entities, cancellationToken);
                return Result<ICollection<TAggregate>>.Success(entities);
            })
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while adding the entities.");
                return await Task.FromResult(Result<ICollection<TAggregate>>.Error(error.Message));
            })
            .Apply() ?? Result<ICollection<TAggregate>>.Error("An error occurred while adding the entities.");

    /// <summary>
    /// Asynchronously updates an existing aggregate in the repository.
    /// </summary>
    /// <param name="entity">The aggregate to be updated.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{TAggregate}"/> indicating the outcome.</returns>
    public async Task<Result<TAggregate>> UpdateAsync(TAggregate entity, CancellationToken cancellationToken = default)
        => await this.Try(async () =>
            {
                if (entity is null)
                    return Result<TAggregate>.Invalid(new Error() { Message = "Entity cannot be null." });
                if (entity.Id is null)
                    return Result<TAggregate>.Invalid(new Error() { Message = "Entity ID cannot be null." });
                if (!await _dbSet.AnyAsync(e => e.Id!.Equals(entity.Id), cancellationToken))
                    return Result<TAggregate>.Invalid(new Error() { Message = "Entity not found in the database." });

                _context.Entry(entity).State = EntityState.Modified;
                return Result<TAggregate>.Success(entity);
            })
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while updating the entity.");
                return await Task.FromResult(Result<TAggregate>.Error(error.Message));
            })
            .Apply() ?? Result<TAggregate>.Error("An error occurred while updating the entity.");

    /// <summary>
    /// Asynchronously removes an aggregate from the repository based on a specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregate to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result"/> indicating the outcome.</returns>
    public async Task<Result> RemoveAsync(ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken = default)
        => await this.Try(async () =>
            {
                if (specification.Criteria is null)
                    return Result.Invalid(new Error() { Message = "The criteria of the specification cannot be null." });

                var entity = await _dbSet.FirstOrDefaultAsync(specification.Criteria, cancellationToken);
                if (entity != null)
                    _dbSet.Remove(entity);

                return Result.Success();
            })
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while removing the entity by specification.");
                return await Task.FromResult(Result.Error(error.Message));
            })
            .Apply() ?? Result.Error("An error occurred while removing the entity by specification.");

    /// <summary>
    /// Asynchronously removes an aggregate from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result"/> indicating the outcome.</returns>
    public async Task<Result> RemoveByIdAsync(TId id, CancellationToken cancellationToken = default)
        => await this.Try(async () =>
            {
                if (id is null)
                    return Result.Invalid(new Error() { Message = "ID cannot be null." });

                var entity = await _dbSet.FindAsync([id], cancellationToken);
                if (entity != null)
                    _dbSet.Remove(entity);

                return Result.Success();
            })
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while removing the entity by ID.");
                return await Task.FromResult(Result.Error(error.Message));
            })
            .Apply() ?? Result.Error("An error occurred while removing the entity by ID.");

    /// <summary>
    /// Asynchronously removes multiple aggregates from the repository based on a specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result"/> indicating the outcome.</returns>
    public async Task<Result> RemoveRangeAsync(ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken = default)
        => await this.Try(async () =>
            {
                if (specification.Criteria is null)
                    return Result.Invalid(new Error() { Message = "Filter expression cannot be null." });

                var entities = await _dbSet.Where(specification.Criteria).ToListAsync(cancellationToken);
                if (entities is not null && entities.Count != 0)
                    _dbSet.RemoveRange(entities);

                return Result.Success();
            })
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while removing entities by specification.");
                return await Task.FromResult(Result.Error(error.Message));
            })
            .Apply() ?? Result.Error("An error occurred while removing entities by specification.");

    #endregion

    #endregion
}