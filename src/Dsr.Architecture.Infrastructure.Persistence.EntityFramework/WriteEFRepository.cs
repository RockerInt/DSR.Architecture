using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Result;
using Dsr.Architecture.Persistence.Abstractions;
using Dsr.Architecture.Utilities.TryCatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework;

/// <summary>
/// Repository implementation for write operations using Entity Framework Core.
/// This repository provides methods for adding, updating, and removing entities from the database.
/// It uses a DbContext to perform database operations and includes error handling with logging.
/// The repository supports both synchronous and asynchronous methods for each operation, allowing for flexible usage in different scenarios.
/// The repository is designed to work with aggregate roots that implement the IAggregateRoot interface, ensuring that only valid entities are managed through the repository.
/// The repository also includes validation checks for null entities, null IDs, and existence of entities in the database before performing update and delete operations, providing robust error handling and feedback through the Result pattern.
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TId"></typeparam>
/// <typeparam name="TAggregate"></typeparam>
public abstract class WriteEFRepository<TContext, TId, TAggregate> : IWriteRepository<TId, TAggregate>
    where TContext : DbContext
    where TId : IEquatable<TId>, IComparable<TId>
    where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
{
    private readonly DbContext _context;
    private readonly DbSet<TAggregate> _dbSet;
    private readonly ILogger<WriteEFRepository<TContext, TId, TAggregate>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WriteEFRepository{TContext, TId, TAggregate}"/> class with the specified DbContext and logger.
    /// </summary>
    /// <param name="unitOfWork"></param>
    /// <param name="logger"></param>
    public WriteEFRepository(IUnitOfWork<TContext> unitOfWork, ILogger<WriteEFRepository<TContext, TId, TAggregate>> logger)
    {
        _context = unitOfWork.Context;
        _dbSet = _context.Set<TAggregate>();
        _logger = logger;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WriteEFRepository{TContext, TId, TAggregate}"/> class with the specified transactional unit of work and logger.
    /// </summary>
    /// <param name="unitOfWork"></param>
    /// <param name="logger"></param>
    /// <exception cref="InvalidOperationException"></exception>
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
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to be added.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome.</returns>
    public Result Add(TAggregate entity) => AddAsync(entity).GetAwaiter().GetResult();

    /// <summary>
    /// Adds multiple new entities to the repository.
    /// </summary>
    /// <param name="entities">A collection of entities to be added.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome.</returns>
    public Result AddRange(ICollection<TAggregate> entities) => AddRangeAsync(entities).GetAwaiter().GetResult();

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to be updated.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome.</returns>
    public Result Update(TAggregate entity) => UpdateAsync(entity).GetAwaiter().GetResult();

    /// <summary>
    /// Removes entities from the repository based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities to be removed.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome.</returns>
    public Result Remove(Expression<Func<TAggregate, bool>> filterExpression) => RemoveAsync(filterExpression).GetAwaiter().GetResult();

    /// <summary>
    /// Removes an entity from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to be removed.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome.</returns>
    public Result RemoveById(TId id) => RemoveByIdAsync(id).GetAwaiter().GetResult();

    /// <summary>
    /// Removes multiple entities from the repository based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities to be removed.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome.</returns>
    public Result RemoveRange(Expression<Func<TAggregate, bool>> filterExpression) => RemoveRangeAsync(filterExpression).GetAwaiter().GetResult();

    #endregion

    #region Async

    /// <summary>
    /// Asynchronously adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to be added.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result"/> indicating the outcome.</returns>
    public async Task<Result> AddAsync(TAggregate entity, CancellationToken cancellationToken = default)
        => await this.Try(async () =>
            {
                if (entity is null)
                    return Result.Invalid(new Error() { Message = "Entity cannot be null." });
                await _dbSet.AddAsync(entity, cancellationToken);
                return Result.Success();
            })
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while adding the entity.");
                return await Task.FromResult(Result.Error(error.Message));
            })
            .Apply() ?? Result.Error("An error occurred while adding the entity.");

    /// <summary>
    /// Asynchronously adds multiple new entities to the repository.
    /// </summary>
    /// <param name="entities">A collection of entities to be added.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result"/> indicating the outcome.</returns>
    public async Task<Result> AddRangeAsync(ICollection<TAggregate> entities, CancellationToken cancellationToken = default)
        => await this.Try(async () =>
            {
                if (entities is null || entities.Count == 0)
                    return Result.Invalid(new Error() { Message = "Entities collection cannot be null or empty." });
                await _dbSet.AddRangeAsync(entities, cancellationToken);
                return Result.Success();
            })
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while adding the entities.");
                return await Task.FromResult(Result.Error(error.Message));
            })
            .Apply() ?? Result.Error("An error occurred while adding the entities.");

    /// <summary>
    /// Asynchronously updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to be updated.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result"/> indicating the outcome.</returns>
    public async Task<Result> UpdateAsync(TAggregate entity, CancellationToken cancellationToken = default)
        => await this.Try(async () =>
            {
                if (entity is null)
                    return Result.Invalid(new Error() { Message = "Entity cannot be null." });
                if (entity.Id is null)
                    return Result.Invalid(new Error() { Message = "Entity ID cannot be null." });
                if (!await _dbSet.AnyAsync(e => e.Id!.Equals(entity.Id), cancellationToken))
                    return Result.Invalid(new Error() { Message = "Entity not found in the database." });

                _context.Entry(entity).State = EntityState.Modified;
                return Result.Success();
            })
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while updating the entity.");
                return await Task.FromResult(Result.Error(error.Message));
            })
            .Apply() ?? Result.Error("An error occurred while updating the entity.");

    /// <summary>
    /// Asynchronously removes entities from the repository based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result"/> indicating the outcome.</returns>
    public async Task<Result> RemoveAsync(Expression<Func<TAggregate, bool>> filterExpression, CancellationToken cancellationToken = default)
        => await this.Try(async () =>
            {
                if (filterExpression is null)
                    return Result.Invalid(new Error() { Message = "Filter expression cannot be null." });

                var entity = await _dbSet.FirstOrDefaultAsync(filterExpression, cancellationToken);
                if (entity != null)
                    _dbSet.Remove(entity);

                return Result.Success();
            })
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while removing the entity by filter expression.");
                return await Task.FromResult(Result.Error(error.Message));
            })
            .Apply() ?? Result.Error("An error occurred while removing the entity by filter expression.");

    /// <summary>
    /// Asynchronously removes an entity from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to be removed.</param>
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
    /// Asynchronously removes multiple entities from the repository based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result"/> indicating the outcome.</returns>
    public async Task<Result> RemoveRangeAsync(Expression<Func<TAggregate, bool>> filterExpression, CancellationToken cancellationToken = default)
        => await this.Try(async () =>
            {
                if (filterExpression is null)
                    return Result.Invalid(new Error() { Message = "Filter expression cannot be null." });

                var entities = await _dbSet.Where(filterExpression).ToListAsync(cancellationToken);
                if (entities is not null && entities.Count != 0)
                    _dbSet.RemoveRange(entities);

                return Result.Success();
            })
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while removing entities by filter expression.");
                return await Task.FromResult(Result.Error(error.Message));
            })
            .Apply() ?? Result.Error("An error occurred while removing entities by filter expression.");

    #endregion

    #endregion
}