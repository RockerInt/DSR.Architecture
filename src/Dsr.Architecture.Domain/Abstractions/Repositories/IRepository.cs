using Dsr.Architecture.Domain.Entities;
using Dsr.Architecture.Domain.Result;
using System.Linq.Expressions;

namespace Dsr.Architecture.Domain.Abstractions.Repositories;

/// <summary>
/// Defines a generic repository interface for common data access operations on entities.
/// Provides methods for searching, inserting, updating, and deleting entities.
/// </summary>
/// <typeparam name="TId">The type of the entity's unique identifier.</typeparam>
/// <typeparam name="TEntity">The type of the entity managed by this repository.</typeparam>
public interface IRepository<TId, TEntity>
    where TId : IEquatable<TId>, IComparable<TId>
    where TEntity : IEntity<TId>
{
    #region Search

    #region Sync

    /// <summary>
    /// Provides an <see cref="IQueryable{TEntity}"/> for building complex queries against the entity collection.
    /// </summary>
    /// <returns>An <see cref="IQueryable{TEntity}"/> instance.</returns>
    IQueryable<TEntity> AsQueryable();

    /// <summary>
    /// Retrieves all entities from the repository.
    /// </summary>
    /// <returns>A <see cref="Result{T}"/> with a collection of all entities.</returns>
    Result<IEnumerable<TEntity>> GetAll();

    /// <summary>
    /// Retrieves entities that match the specified filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <returns>A <see cref="Result{T}"/> with a collection of matching entities.</returns>
    Result<IEnumerable<TEntity>> GetBy(
        Expression<Func<TEntity, bool>> filterExpression);

    /// <summary>
    /// Retrieves and projects entities that match the specified filter expression to a different type.
    /// </summary>
    /// <typeparam name="TProjected">The type to project the entities to.</typeparam>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <param name="projectionExpression">An expression to project the filtered entities to <typeparamref name="TProjected"/>.</param>
    /// <returns>A <see cref="Result{T}"/> with a collection of projected entities.</returns>
    Result<IEnumerable<TProjected>> GetBy<TProjected>(
        Expression<Func<TEntity, bool>> filterExpression,
        Expression<Func<TEntity, TProjected>> projectionExpression);

    /// <summary>
    /// Finds the first entity that matches the given filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <returns>A <see cref="Result{T}"/> with the first matching entity.</returns>
    Result<TEntity> First(Expression<Func<TEntity, bool>> filterExpression);

    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <returns>A <see cref="Result{T}"/> with the retrieved entity.</returns>
    Result<TEntity> GetById(TId id);

    #endregion Sync

    #region Async

    /// <summary>
    /// Asynchronously retrieves all entities from the repository.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with a collection of all entities.</returns>
    Task<Result<IEnumerable<TEntity>>> GetAllAsync(CancellationToken cancellationToken = new());

    /// <summary>
    /// Asynchronously retrieves entities that match the specified filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with a collection of matching entities.</returns>
    Task<Result<IEnumerable<TEntity>>> GetByAsync(
        Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = new());

    /// <summary>
    /// Asynchronously retrieves and projects entities that match the specified filter expression to a different type.
    /// </summary>
    /// <typeparam name="TProjected">The type to project the entities to.</typeparam>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <param name="projectionExpression">An expression to project the filtered entities to <typeparamref name="TProjected"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with a collection of projected entities.</returns>
    Task<Result<IEnumerable<TProjected>>> GetByAsync<TProjected>(
        Expression<Func<TEntity, bool>> filterExpression,
        Expression<Func<TEntity, TProjected>> projectionExpression, CancellationToken cancellationToken = new());

    /// <summary>
    /// Asynchronously finds the first entity that matches the given filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with the first matching entity.</returns>
    Task<Result<TEntity>> FirstAsync(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = new());

    /// <summary>
    /// Asynchronously retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with the retrieved entity.</returns>
    Task<Result<TEntity>> GetByIdAsync(TId id, CancellationToken cancellationToken = new());

    #endregion Async

    #endregion Search

    #region CUD

    #region Sync

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to be added.</param>
    /// <returns>A <see cref="Result.Result"/> indicating the outcome.</returns>
    Result.Result Add(TEntity entity);

    /// <summary>
    /// Adds multiple new entities to the repository.
    /// </summary>
    /// <param name="entities">A collection of entities to be added.</param>
    /// <returns>A <see cref="Result.Result"/> indicating the outcome.</returns>
    Result.Result AddRange(ICollection<TEntity> entities);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to be updated.</param>
    /// <returns>A <see cref="Result.Result"/> indicating the outcome.</returns>
    Result.Result Update(TEntity entity);

    /// <summary>
    /// Removes entities from the repository based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities to be removed.</param>
    /// <returns>A <see cref="Result.Result"/> indicating the outcome.</returns>
    Result.Result Remove(Expression<Func<TEntity, bool>> filterExpression);

    /// <summary>
    /// Removes an entity from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to be removed.</param>
    /// <returns>A <see cref="Result.Result"/> indicating the outcome.</returns>
    Result.Result RemoveById(TId id);

    /// <summary>
    /// Removes multiple entities from the repository based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities to be removed.</param>
    /// <returns>A <see cref="Result.Result"/> indicating the outcome.</returns>
    Result.Result RemoveRange(Expression<Func<TEntity, bool>> filterExpression);

    #endregion Sync
 
    #region Async

    /// <summary>
    /// Asynchronously adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to be added.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result.Result"/> indicating the outcome.</returns>
    Task<Result.Result> AddAsync(TEntity entity, CancellationToken cancellationToken = new());

    /// <summary>
    /// Asynchronously adds multiple new entities to the repository.
    /// </summary>
    /// <param name="entities">A collection of entities to be added.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result.Result"/> indicating the outcome.</returns>
    Task<Result.Result> AddRangeAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = new());

    /// <summary>
    /// Asynchronously updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to be updated.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result.Result"/> indicating the outcome.</returns>
    Task<Result.Result> UpdateAsync(TEntity entity, CancellationToken cancellationToken = new());

    /// <summary>
    /// Asynchronously removes entities from the repository based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result.Result"/> indicating the outcome.</returns>
    Task<Result.Result> RemoveAsync(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = new());

    /// <summary>
    /// Asynchronously removes an entity from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result.Result"/> indicating the outcome.</returns>
    Task<Result.Result> RemoveByIdAsync(TId id, CancellationToken cancellationToken = new());

    /// <summary>
    /// Asynchronously removes multiple entities from the repository based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result.Result"/> indicating the outcome.</returns>
    Task<Result.Result> RemoveRangeAsync(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = new());

    #endregion Async
 
    #endregion CUD
}