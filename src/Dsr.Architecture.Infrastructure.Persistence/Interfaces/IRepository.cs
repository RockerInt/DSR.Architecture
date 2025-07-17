using Dsr.Architecture.Domain.Entities;
using Dsr.Architecture.Domain.Interfaces;
using System.Linq.Expressions;

namespace Dsr.Architecture.Infrastructure.Persistence.Interfaces;

/// <summary>
/// Defines a generic repository interface for common data access operations on entities.
/// Provides methods for searching, inserting, updating, and deleting entities.
/// </summary>
/// <typeparam name="TId">The type of the entity's unique identifier.</typeparam>
/// <typeparam name="TEntity">The type of the entity managed by this repository.</typeparam>
public interface IRepository<TId, TEntity> 
    where TId : IEquatable<TId>, IComparable<TId>
    where TEntity : Entity<TId>, IEntity<TId>
{
    #region Search

    /// <summary>
    /// Provides an <see cref="IQueryable{TEntity}"/> for building complex queries against the entity collection.
    /// </summary>
    /// <returns>An <see cref="IQueryable{TEntity}"/> instance.</returns>
    IQueryable<TEntity> AsQueryable();

    /// <summary>
    /// Asynchronously retrieves all entities from the repository.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with a collection of all entities.</returns>
    Task<Result<IEnumerable<TEntity>>> GetAll(CancellationToken cancellationToken = new());

    /// <summary>
    /// Asynchronously retrieves entities that match the specified filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with a collection of matching entities.</returns>
    Task<Result<IEnumerable<TEntity>>> GetBy(
        Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = new());

    /// <summary>
    /// Asynchronously retrieves and projects entities that match the specified filter expression to a different type.
    /// </summary>
    /// <typeparam name="TProjected">The type to project the entities to.</typeparam>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <param name="projectionExpression">An expression to project the filtered entities to <typeparamref name="TProjected"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with a collection of projected entities.</returns>
    Task<Result<IEnumerable<TProjected>>> GetBy<TProjected>(
        Expression<Func<TEntity, bool>> filterExpression,
        Expression<Func<TEntity, TProjected>> projectionExpression, CancellationToken cancellationToken = new());

    /// <summary>
    /// Asynchronously finds the first entity that matches the given filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with the first matching entity.</returns>
    Task<Result<TEntity>> First(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = new());

    /// <summary>
    /// Asynchronously retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with the retrieved entity.</returns>
    Task<Result<TEntity>> GetById(TId id, CancellationToken cancellationToken = new());

    #endregion Search

    #region CUD

    /// <summary>
    /// Asynchronously adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to be added.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    Task<ResultSimple> Add(TEntity entity, CancellationToken cancellationToken = new());

    /// <summary>
    /// Asynchronously adds multiple new entities to the repository.
    /// </summary>
    /// <param name="entities">A collection of entities to be added.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    Task<ResultSimple> AddMany(ICollection<TEntity> entities, CancellationToken cancellationToken = new());

    /// <summary>
    /// Asynchronously updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to be updated.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    Task<ResultSimple> Update(TEntity entity, CancellationToken cancellationToken = new());

    /// <summary>
    /// Asynchronously removes entities from the repository based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    Task<ResultSimple> Remove(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = new());

    /// <summary>
    /// Asynchronously removes an entity from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    Task<ResultSimple> RemoveById(TId id, CancellationToken cancellationToken = new());

    /// <summary>
    /// Asynchronously removes multiple entities from the repository based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the entities to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    Task<ResultSimple> RemoveMany(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = new());

    #endregion CUD
}