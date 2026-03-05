using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Result;
using Dsr.Architecture.Persistence.Abstractions;
using Dsr.Architecture.Utilities.TryCatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework;

/// <summary>
/// Repository implementation for managing aggregates using Entity Framework Core.
/// This repository provides methods for querying and modifying aggregate entities in the database.
/// It uses a DbContext to perform database operations and includes error handling with logging.
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TId"></typeparam>
/// <typeparam name="TAggregate"></typeparam>
public abstract class EFRepository<TContext, TId, TAggregate> : IRepository<TId, TAggregate>, IReadRepository<TId, TAggregate>, IWriteRepository<TId, TAggregate>
    where TContext : DbContext
    where TId : IEquatable<TId>, IComparable<TId>
    where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
{
    private readonly DbContext _context;
    private readonly DbSet<TAggregate> _dbSet;
    private readonly IQueryable<TAggregate> _dbSetReadOnly;
    private readonly ILogger<EFRepository<TContext, TId, TAggregate>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EFRepository{TContext, TId, TAggregate}"/> class with the specified DbContext and logger.
    /// </summary>
    /// <param name="unitOfWork"></param>
    /// <param name="logger"></param>
    public EFRepository(IUnitOfWork<TContext> unitOfWork, ILogger<EFRepository<TContext, TId, TAggregate>> logger)
    {
        _context = unitOfWork.Context;
        _dbSet = unitOfWork.Context.Set<TAggregate>();
        _dbSetReadOnly = unitOfWork.Context.Set<TAggregate>().AsNoTracking();
        _logger = logger;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EFRepository{TContext, TId, TAggregate}"/> class with the specified transactional unit of work and logger.
    /// </summary>
    /// <param name="unitOfWork"></param>
    /// <param name="logger"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public EFRepository(ITransactionalEFUnitOfWork unitOfWork, ILogger<EFRepository<TContext, TId, TAggregate>> logger)
    {
        _context = unitOfWork.Accessor.DbContexts.FirstOrDefault(
            x => x is TContext) ?? throw new InvalidOperationException($"No DbContext of type {typeof(TContext).Name} found in the unit of work."
        );
        _dbSet = _context.Set<TAggregate>();
        _dbSetReadOnly = _context.Set<TAggregate>().AsNoTracking();
        _logger = logger;
    }

    #region Search

    #region Sync

    /// <summary>
    /// Returns the entity set as an <see cref="IQueryable{TAggregate}"/>.
    /// </summary>
    /// <returns>An <see cref="IQueryable{TAggregate}"/> that can be used to query the entities.</returns>
    public IQueryable<TAggregate> AsQueryable() => _dbSetReadOnly;

    /// <summary>
    /// Retrieves all entities from the repository.
    /// </summary>
    /// <returns>A <see cref="Result{T}"/> with a collection of all entities.</returns>
    public Result<IEnumerable<TAggregate>> GetAll() => GetAllAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Retrieves entities that match the specified filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <returns>A <see cref="Result{T}"/> with a collection of matching entities.</returns>
    public Result<IEnumerable<TAggregate>> GetBy(Expression<Func<TAggregate, bool>> filterExpression) => GetByAsync(filterExpression).GetAwaiter().GetResult();

    /// <summary>
    /// Retrieves and projects entities that match the specified filter expression to a different type.
    /// </summary>
    /// <typeparam name="TProjected">The type to project the entities to.</typeparam>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <param name="projectionExpression">An expression to project the filtered entities to <typeparamref name="TProjected"/>.</param>
    /// <returns>A <see cref="Result{T}"/> with a collection of projected entities.</returns>
    public Result<IEnumerable<TProjected>> GetBy<TProjected>(Expression<Func<TAggregate, bool>> filterExpression, Expression<Func<TAggregate, TProjected>> projectionExpression) =>
        GetByAsync(filterExpression, projectionExpression).GetAwaiter().GetResult();

    /// <summary>
    /// Finds the first entity that matches the given filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <returns>A <see cref="Result{T}"/> with the first matching entity.</returns>
    public Result<TAggregate> First(Expression<Func<TAggregate, bool>> filterExpression) => FirstAsync(filterExpression).GetAwaiter().GetResult();

    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <returns>A <see cref="Result{T}"/> with the retrieved entity.</returns>
    public Result<TAggregate> GetById(TId id) => GetByIdAsync(id).GetAwaiter().GetResult();

    #endregion

    #region Async

    /// <summary>
    /// Asynchronously retrieves all entities from the repository.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with a collection of all entities.</returns>
    public async Task<Result<IEnumerable<TAggregate>>> GetAllAsync(CancellationToken cancellationToken = default)
        => await this.Try(async () => new Result<IEnumerable<TAggregate>>(await _dbSetReadOnly.ToListAsync(cancellationToken)))
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while retrieving all entities.");
                return await Task.FromResult(Result<IEnumerable<TAggregate>>.Error(error.Message));
            })
            .Apply() ?? Result<IEnumerable<TAggregate>>.Error("An error occurred while retrieving all entities.");

    /// <summary>
    /// Asynchronously retrieves entities that match the specified filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with a collection of matching entities.</returns>
    public async Task<Result<IEnumerable<TAggregate>>> GetByAsync(Expression<Func<TAggregate, bool>> filterExpression, CancellationToken cancellationToken = default)
        => await this.Try(async () => new Result<IEnumerable<TAggregate>>(await _dbSetReadOnly.Where(filterExpression).ToListAsync(cancellationToken)))
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while retrieving entities by filter expression.");
                return await Task.FromResult(Result<IEnumerable<TAggregate>>.Error(error.Message));
            })
            .Apply() ?? Result<IEnumerable<TAggregate>>.Error("An error occurred while retrieving entities by filter expression.");

    /// <summary>
    /// Asynchronously retrieves and projects entities that match the specified filter expression to a different type.
    /// </summary>
    /// <typeparam name="TProjected">The type to project the entities to.</typeparam>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <param name="projectionExpression">An expression to project the filtered entities to <typeparamref name="TProjected"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with a collection of projected entities.</returns>
    public async Task<Result<IEnumerable<TProjected>>> GetByAsync<TProjected>(Expression<Func<TAggregate, bool>> filterExpression, Expression<Func<TAggregate, TProjected>> projectionExpression, CancellationToken cancellationToken = default)
        => await this.Try(async () => new Result<IEnumerable<TProjected>>(
            await _dbSetReadOnly.Where(filterExpression).Select(projectionExpression).ToListAsync(cancellationToken)))
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while retrieving and projecting entities by filter expression.");
                return await Task.FromResult(Result<IEnumerable<TProjected>>.Error(error.Message));
            })
            .Apply() ?? Result<IEnumerable<TProjected>>.Error("An error occurred while retrieving and projecting entities by filter expression.");

    /// <summary>
    /// Asynchronously finds the first entity that matches the given filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with the first matching entity.</returns>
    public async Task<Result<TAggregate>> FirstAsync(Expression<Func<TAggregate, bool>> filterExpression, CancellationToken cancellationToken = default)
        => await this.Try(async () => new Result<TAggregate>(await _dbSetReadOnly.FirstOrDefaultAsync(filterExpression, cancellationToken)))
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while retrieving the first entity by filter expression.");
                return await Task.FromResult(Result<TAggregate>.Error(error.Message));
            })
            .Apply() ?? Result<TAggregate>.Error("An error occurred while retrieving the first entity by filter expression.");

    /// <summary>
    /// Asynchronously retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with the retrieved entity.</returns>
    public async Task<Result<TAggregate>> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        => await this.Try(async () => new Result<TAggregate>(await _dbSet.FindAsync([id], cancellationToken)))
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while retrieving the entity by ID.");
                return await Task.FromResult(Result<TAggregate>.Error(error.Message));
            })
            .Apply() ?? Result<TAggregate>.Error("An error occurred while retrieving the entity by ID.");

    #endregion

    #endregion

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