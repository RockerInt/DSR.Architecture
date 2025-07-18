using Dsr.Architecture.Domain.Entities;
using Dsr.Architecture.Domain.Interfaces;
using Dsr.Architecture.Infrastructure.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dsr.Architecture.Infrastructure.Persistence.SqlServer;

/// <summary>
/// Generic repository for SQL Server using Entity Framework Core.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class SqlServerRepository<TId, TEntity> : IRepository<TId, TEntity>
    where TId : IEquatable<TId>, IComparable<TId>
    where TEntity : Entity<TId>, IEntity<TId>
{
    private readonly SqlServerDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerRepository{TId, TEntity}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public SqlServerRepository(SqlServerDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }


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
    {
        var result = await _dbSet.ToListAsync(cancellationToken);
        return new Result<IEnumerable<TEntity>>(result);
    }

    /// <summary>
    /// Asynchronously retrieves entities that match the specified filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with a collection of matching entities.</returns>
    public async Task<Result<IEnumerable<TEntity>>> GetByAsync(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet.Where(filterExpression).ToListAsync(cancellationToken);
        return new Result<IEnumerable<TEntity>>(result);
    }

    /// <summary>
    /// Asynchronously retrieves and projects entities that match the specified filter expression to a different type.
    /// </summary>
    /// <typeparam name="TProjected">The type to project the entities to.</typeparam>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <param name="projectionExpression">An expression to project the filtered entities to <typeparamref name="TProjected"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with a collection of projected entities.</returns>
    public async Task<Result<IEnumerable<TProjected>>> GetByAsync<TProjected>(Expression<Func<TEntity, bool>> filterExpression, Expression<Func<TEntity, TProjected>> projectionExpression, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet.Where(filterExpression).Select(projectionExpression).ToListAsync(cancellationToken);
        return new Result<IEnumerable<TProjected>>(result);
    }

    /// <summary>
    /// Asynchronously finds the first entity that matches the given filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with the first matching entity.</returns>
    public async Task<Result<TEntity>> FirstAsync(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet.FirstOrDefaultAsync(filterExpression, cancellationToken);
        return new Result<TEntity>(result);
    }

    /// <summary>
    /// Asynchronously retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with the retrieved entity.</returns>
    public async Task<Result<TEntity>> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet.FindAsync([id], cancellationToken);
        return new Result<TEntity>(result);
    }

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
    public ResultSimple AddMany(ICollection<TEntity> entities) => AddManyAsync(entities).GetAwaiter().GetResult();

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
    public ResultSimple RemoveMany(Expression<Func<TEntity, bool>> filterExpression) => RemoveManyAsync(filterExpression).GetAwaiter().GetResult();

    #endregion

    #region Async

    /// <summary>
    /// Asynchronously adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to be added.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    public async Task<ResultSimple> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return new ResultSimple();
    }

    /// <summary>
    /// Asynchronously adds multiple new entities to the repository.
    /// </summary>
    /// <param name="entities">A collection of entities to be added.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    public async Task<ResultSimple> AddManyAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return new ResultSimple();
    }

    /// <summary>
    /// Asynchronously updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to be updated.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    public async Task<ResultSimple> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
        return new ResultSimple();
    }

    /// <summary>
    /// Asynchronously removes entities from the repository based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    public async Task<ResultSimple> RemoveAsync(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(filterExpression, cancellationToken);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
        return new ResultSimple();
    }

    /// <summary>
    /// Asynchronously removes an entity from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    public async Task<ResultSimple> RemoveByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync([id], cancellationToken);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
        return new ResultSimple();
    }

    /// <summary>
    /// Asynchronously removes multiple entities from the repository based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    public async Task<ResultSimple> RemoveManyAsync(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet.Where(filterExpression).ToListAsync(cancellationToken);
        if (entities is not null && entities.Count != 0)
        {
            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync(cancellationToken);
        }
        return new ResultSimple();
    }

    #endregion

    #endregion
}