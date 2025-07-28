using Dsr.Architecture.Domain.Entities;
using Dsr.Architecture.Domain.Interfaces;
using Dsr.Architecture.Infrastructure.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dsr.Architecture.Infrastructure.Persistence.SqlServer;

/// <summary>
/// Represents a repository for managing entities in a SQL Server database.
/// </summary>
/// <typeparam name="TId">The type of the unique identifier for the entity.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="SqlServerRepository{TId, TEntity}"/> class.
/// </remarks>
/// <param name="context">The <see cref="DbContext"/> to be used by the repository.</param>
public abstract class SqlServerRepository<TId, TEntity>(DbContext context) : IRepository<TId, TEntity>
    where TId : IEquatable<TId>, IComparable<TId>
    where TEntity : Entity<TId>, IEntity<TId>
{
    private readonly DbContext _context = context;
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    #region Search

    #region Sync

    /// <summary>
    /// Returns the entity set as an <see cref="IQueryable{TEntity}"/>.
    /// </summary>
    /// <returns>An <see cref="IQueryable{TEntity}"/> that can be used to query the entities.</returns>
    public IQueryable<TEntity> AsQueryable() => _dbSet.AsQueryable();

    /// <summary>
    /// Retrieves all entities from the repository.
    /// </summary>
    /// <returns>A <see cref="Result{T}"/> with a collection of all entities.</returns>
    public Result<IEnumerable<TEntity>> GetAll() => GetAllAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Retrieves entities that match the specified filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <returns>A <see cref="Result{T}"/> with a collection of matching entities.</returns>
    public Result<IEnumerable<TEntity>> GetBy(Expression<Func<TEntity, bool>> filterExpression) => GetByAsync(filterExpression).GetAwaiter().GetResult();

    /// <summary>
    /// Retrieves and projects entities that match the specified filter expression to a different type.
    /// </summary>
    /// <typeparam name="TProjected">The type to project the entities to.</typeparam>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <param name="projectionExpression">An expression to project the filtered entities to <typeparamref name="TProjected"/>.</param>
    /// <returns>A <see cref="Result{T}"/> with a collection of projected entities.</returns>
    public Result<IEnumerable<TProjected>> GetBy<TProjected>(Expression<Func<TEntity, bool>> filterExpression, Expression<Func<TEntity, TProjected>> projectionExpression) =>
        GetByAsync(filterExpression, projectionExpression).GetAwaiter().GetResult();

    /// <summary>
    /// Finds the first entity that matches the given filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <returns>A <see cref="Result{T}"/> with the first matching entity.</returns>
    public Result<TEntity> First(Expression<Func<TEntity, bool>> filterExpression) => FirstAsync(filterExpression).GetAwaiter().GetResult();

    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <returns>A <see cref="Result{T}"/> with the retrieved entity.</returns>
    public Result<TEntity> GetById(TId id) => GetByIdAsync(id).GetAwaiter().GetResult();

    #endregion

    #region Async

    /// <summary>
    /// Asynchronously retrieves all entities from the repository.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with a collection of all entities.</returns>
    public async Task<Result<IEnumerable<TEntity>>> GetAllAsync(CancellationToken cancellationToken = default)
        => await this.Try(async () => new Result<IEnumerable<TEntity>>(await _dbSet.ToListAsync(cancellationToken)))
            .Catch(async (error) => await Task.FromResult(new Result<IEnumerable<TEntity>>(null, 1, error.Message)))
            .Apply() ?? new Result<IEnumerable<TEntity>>(null, 1, "An error occurred while retrieving all entities.");

    /// <summary>
    /// Asynchronously retrieves entities that match the specified filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with a collection of matching entities.</returns>
    public async Task<Result<IEnumerable<TEntity>>> GetByAsync(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = default)
        => await this.Try(async () => new Result<IEnumerable<TEntity>>(await _dbSet.Where(filterExpression).ToListAsync(cancellationToken)))
            .Catch(async (error) => await Task.FromResult(new Result<IEnumerable<TEntity>>(null, 1, error.Message)))
            .Apply() ?? new Result<IEnumerable<TEntity>>(null, 1, "An error occurred while retrieving entities by filter expression.");

    /// <summary>
    /// Asynchronously retrieves and projects entities that match the specified filter expression to a different type.
    /// </summary>
    /// <typeparam name="TProjected">The type to project the entities to.</typeparam>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <param name="projectionExpression">An expression to project the filtered entities to <typeparamref name="TProjected"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with a collection of projected entities.</returns>
    public async Task<Result<IEnumerable<TProjected>>> GetByAsync<TProjected>(Expression<Func<TEntity, bool>> filterExpression, Expression<Func<TEntity, TProjected>> projectionExpression, CancellationToken cancellationToken = default)
        => await this.Try(async () => new Result<IEnumerable<TProjected>>(
            await _dbSet.Where(filterExpression).Select(projectionExpression).ToListAsync(cancellationToken)))
            .Catch(async (error) => await Task.FromResult(new Result<IEnumerable<TProjected>>(null, 1, error.Message)))
            .Apply() ?? new Result<IEnumerable<TProjected>>(null, 1, "An error occurred while retrieving and projecting entities by filter expression.");

    /// <summary>
    /// Asynchronously finds the first entity that matches the given filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with the first matching entity.</returns>
    public async Task<Result<TEntity>> FirstAsync(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = default)
        => await this.Try(async () => new Result<TEntity>(await _dbSet.FirstOrDefaultAsync(filterExpression, cancellationToken)))
            .Catch(async (error) => await Task.FromResult(new Result<TEntity>(null, 1, error.Message)))
            .Apply() ?? new Result<TEntity>(null, 1, "An error occurred while retrieving the first entity by filter expression.");

    /// <summary>
    /// Asynchronously retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with the retrieved entity.</returns>
    public async Task<Result<TEntity>> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        => await this.Try(async () => new Result<TEntity>(await _dbSet.FindAsync([id], cancellationToken)))
            .Catch(async (error) => await Task.FromResult(new Result<TEntity>(null, 1, error.Message)))
            .Apply() ?? new Result<TEntity>(null, 1, "An error occurred while retrieving the entity by ID.");

    #endregion

    #endregion

    #region CUD

    #region Sync

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to be added.</param>
    /// <returns>A <see cref="ResultSimple"/> indicating the outcome.</returns>
    public ResultSimple Add(TEntity entity) => AddAsync(entity).GetAwaiter().GetResult();

    /// <summary>
    /// Adds multiple new entities to the repository.
    /// </summary>
    /// <param name="entities">A collection of entities to be added.</param>
    /// <returns>A <see cref="ResultSimple"/> indicating the outcome.</returns>
    public ResultSimple AddRange(ICollection<TEntity> entities) => AddRangeAsync(entities).GetAwaiter().GetResult();

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to be updated.</param>
    /// <returns>A <see cref="ResultSimple"/> indicating the outcome.</returns>
    public ResultSimple Update(TEntity entity) => UpdateAsync(entity).GetAwaiter().GetResult();

    /// <summary>
    /// Removes entities from the repository based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities to be removed.</param>
    /// <returns>A <see cref="ResultSimple"/> indicating the outcome.</returns>
    public ResultSimple Remove(Expression<Func<TEntity, bool>> filterExpression) => RemoveAsync(filterExpression).GetAwaiter().GetResult();

    /// <summary>
    /// Removes an entity from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to be removed.</param>
    /// <returns>A <see cref="ResultSimple"/> indicating the outcome.</returns>
    public ResultSimple RemoveById(TId id) => RemoveByIdAsync(id).GetAwaiter().GetResult();

    /// <summary>
    /// Removes multiple entities from the repository based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities to be removed.</param>
    /// <returns>A <see cref="ResultSimple"/> indicating the outcome.</returns>
    public ResultSimple RemoveRange(Expression<Func<TEntity, bool>> filterExpression) => RemoveRangeAsync(filterExpression).GetAwaiter().GetResult();

    #endregion

    #region Async

    /// <summary>
    /// Asynchronously adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to be added.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    public async Task<ResultSimple> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await this.Try(async () => {
                    if (entity is null)
                        return new ResultSimple(1, "Entity cannot be null.");
                    await _dbSet.AddAsync(entity, cancellationToken);
                    return new ResultSimple();
                })
                .Catch(async (error) => await Task.FromResult(new ResultSimple(1, error.Message)))
                .Apply() ?? new ResultSimple(1, "An error occurred while adding the entity.");

    /// <summary>
    /// Asynchronously adds multiple new entities to the repository.
    /// </summary>
    /// <param name="entities">A collection of entities to be added.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    public async Task<ResultSimple> AddRangeAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = default)
        => await this.Try(async () => {
                if (entities is null || entities.Count == 0)
                    return new ResultSimple(1, "Entities collection cannot be null or empty.");
                await _dbSet.AddRangeAsync(entities, cancellationToken);
                return new ResultSimple();
            })
            .Catch(async (error) => await Task.FromResult(new ResultSimple(1, error.Message)))
            .Apply() ?? new ResultSimple(1, "An error occurred while adding the entities.");

    /// <summary>
    /// Asynchronously updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to be updated.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    public async Task<ResultSimple> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await this.Try(async () => {
                if (entity is null)
                    return new ResultSimple(1, "Entity cannot be null.");
                if (entity.Id is null)
                    return new ResultSimple(1, "Entity ID cannot be null.");
                if (!await _dbSet.AnyAsync(e => e.Id!.Equals(entity.Id), cancellationToken))
                    return new ResultSimple(1, "Entity not found in the database.");

                _context.Entry(entity).State = EntityState.Modified;
                return new ResultSimple();
            })
            .Catch(async (error) => await Task.FromResult(new ResultSimple(1, error.Message)))
            .Apply() ?? new ResultSimple(1, "An error occurred while updating the entity.");

    /// <summary>
    /// Asynchronously removes entities from the repository based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    public async Task<ResultSimple> RemoveAsync(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = default)
        => await this.Try(async () => {
                if (filterExpression is null)
                    return new ResultSimple(1, "Filter expression cannot be null.");

                var entity = await _dbSet.FirstOrDefaultAsync(filterExpression, cancellationToken);
                if (entity != null)
                    _dbSet.Remove(entity);
                    
                return new ResultSimple();
            })
            .Catch(async (error) => await Task.FromResult(new ResultSimple(1, error.Message)))
            .Apply() ?? new ResultSimple(1, "An error occurred while removing the entity by filter expression.");

    /// <summary>
    /// Asynchronously removes an entity from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    public async Task<ResultSimple> RemoveByIdAsync(TId id, CancellationToken cancellationToken = default)
        => await this.Try(async () => {
                if (id is null)
                    return new ResultSimple(1, "ID cannot be null.");

                var entity = await _dbSet.FindAsync([id], cancellationToken);
                if (entity != null)
                    _dbSet.Remove(entity);
                    
                return new ResultSimple();
            })
            .Catch(async (error) => await Task.FromResult(new ResultSimple(1, error.Message)))
            .Apply() ?? new ResultSimple(1, "An error occurred while removing the entity by ID.");

    /// <summary>
    /// Asynchronously removes multiple entities from the repository based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    public async Task<ResultSimple> RemoveRangeAsync(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = default)
        => await this.Try(async () => {
                if (filterExpression is null)
                    return new ResultSimple(1, "Filter expression cannot be null.");

                var entities = await _dbSet.Where(filterExpression).ToListAsync(cancellationToken);
                if (entities is not null && entities.Count != 0)
                    _dbSet.RemoveRange(entities);

                return new ResultSimple();
            })
            .Catch(async (error) => await Task.FromResult(new ResultSimple(1, error.Message)))
            .Apply() ?? new ResultSimple(1, "An error occurred while removing entities by filter expression.");

    #endregion

    #endregion
}