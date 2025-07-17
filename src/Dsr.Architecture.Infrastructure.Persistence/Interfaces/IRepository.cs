using Dsr.Architecture.Domain.Entities;
using Dsr.Architecture.Domain.Interfaces;
using System.Linq.Expressions;

namespace Dsr.Architecture.Infrastructure.Persistence.Interfaces;

/// <summary>
/// Generic interface for persistence operations on entities.
/// Provides methods for search, insertion, update, and deletion.
/// </summary>
/// <typeparam name="TId">Type of the entity's unique identifier.</typeparam>
/// <typeparam name="TEntity">Type of the entity.</typeparam>
public interface IRepository<TId, TEntity> 
    where TId : IEquatable<TId>, IComparable<TId>
    where TEntity : Entity<TId>, IEntity<TId>
{
    #region Search

    /// <summary>
    /// Allows obtaining an IQueryable query over the entity.
    /// </summary>
    IQueryable<TEntity> AsQueryable();

    /// <summary>
    /// Filters entities according to an expression and returns the result.
    /// </summary>
    Task<Result<IEnumerable<TEntity>>> FilterBy(
        Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = new());

    /// <summary>
    /// Filters entities and projects the result to another type.
    /// </summary>
    Task<Result<IEnumerable<TProjected>>> FilterBy<TProjected>(
        Expression<Func<TEntity, bool>> filterExpression,
        Expression<Func<TEntity, TProjected>> projectionExpression, CancellationToken cancellationToken = new());

    /// <summary>
    /// Finds an entity that matches the given expression.
    /// </summary>
    Task<Result<TEntity>> FindOne(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = new());

    /// <summary>
    /// Finds an entity by its identifier.
    /// </summary>
    Task<Result<TEntity>> FindById(TId id, CancellationToken cancellationToken = new());

    #endregion Search

    #region CUD

    /// <summary>
    /// Inserts a new entity.
    /// </summary>
    Task<ResultSimple> InsertOne(TEntity entity, CancellationToken cancellationToken = new());

    /// <summary>
    /// Inserts multiple entities.
    /// </summary>
    Task<ResultSimple> InsertMany(ICollection<TEntity> entities, CancellationToken cancellationToken = new());

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    Task<ResultSimple> UpdateOne(TEntity entity, CancellationToken cancellationToken = new());

    /// <summary>
    /// Deletes an entity according to a filter expression.
    /// </summary>
    Task<ResultSimple> DeleteOne(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = new());

    /// <summary>
    /// Deletes an entity by its identifier.
    /// </summary>
    Task<ResultSimple> DeleteById(TId id, CancellationToken cancellationToken = new());

    /// <summary>
    /// Deletes multiple entities according to a filter expression.
    /// </summary>
    Task<ResultSimple> DeleteMany(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = new());

    #endregion CUD
}