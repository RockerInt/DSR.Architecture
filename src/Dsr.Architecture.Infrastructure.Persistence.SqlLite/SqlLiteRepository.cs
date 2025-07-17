using Dsr.Architecture.Domain.Entities;
using Dsr.Architecture.Domain.Interfaces;
using Dsr.Architecture.Infrastructure.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dsr.Architecture.Infrastructure.Persistence.SqlLite;

/// <summary>
/// Generic repository for SQL Server using Entity Framework Core.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class SqlLiteRepository<TId, TEntity> : IRepository<TId, TEntity>
    where TId : IEquatable<TId>, IComparable<TId>
    where TEntity : Entity<TId>, IEntity<TId>
{
    private readonly SqlLiteDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlLiteRepository{TId, TEntity}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public SqlLiteRepository(SqlLiteDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }

    /// <summary>
    /// Returns the entity set as an <see cref="IQueryable{TEntity}"/>.
    /// </summary>
    /// <returns>An <see cref="IQueryable{TEntity}"/> that can be used to query the entities.</returns>
    public IQueryable<TEntity> AsQueryable() => _dbSet.AsQueryable();

    /// <summary>
    /// Filters entities based on a predicate.
    /// </summary>
    /// <param name="filterExpression">The filter expression.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the collection of filtered entities.</returns>
    public async Task<Result<IEnumerable<TEntity>>> FilterBy(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet.Where(filterExpression).ToListAsync(cancellationToken);
        return new Result<IEnumerable<TEntity>>(result);
    }

    /// <summary>
    /// Filters and projects entities based on predicates.
    /// </summary>
    /// <typeparam name="TProjected">The type of the projected entity.</typeparam>
    /// <param name="filterExpression">The filter expression.</param>
    /// <param name="projectionExpression">The projection expression.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the collection of projected entities.</returns>
    public async Task<Result<IEnumerable<TProjected>>> FilterBy<TProjected>(Expression<Func<TEntity, bool>> filterExpression, Expression<Func<TEntity, TProjected>> projectionExpression, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet.Where(filterExpression).Select(projectionExpression).ToListAsync(cancellationToken);
        return new Result<IEnumerable<TProjected>>(result);
    }

    /// <summary>
    /// Finds a single entity based on a predicate.
    /// </summary>
    /// <param name="filterExpression">The filter expression.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the found entity or null.</returns>
    public async Task<Result<TEntity>> FindOne(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet.FirstOrDefaultAsync(filterExpression, cancellationToken);
        return new Result<TEntity>(result);
    }

    /// <summary>
    /// Finds an entity by its identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the found entity or null.</returns>
    public async Task<Result<TEntity>> FindById(TId id, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet.FindAsync([id], cancellationToken);
        return new Result<TEntity>(result);
    }

    /// <summary>
    /// Inserts a single entity into the database.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating the success of the operation.</returns>
    public async Task<ResultSimple> InsertOne(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return new ResultSimple();
    }

    /// <summary>
    /// Inserts a collection of entities into the database.
    /// </summary>
    /// <param name="entities">The collection of entities to insert.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating the success of the operation.</returns>
    public async Task<ResultSimple> InsertMany(ICollection<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return new ResultSimple();
    }

    /// <summary>
    /// Updates a single entity in the database.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating the success of the operation.</returns>
    public async Task<ResultSimple> UpdateOne(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
        return new ResultSimple();
    }

    /// <summary>
    /// Deletes a single entity based on a predicate.
    /// </summary>
    /// <param name="filterExpression">The filter expression.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating the success of the operation.</returns>
    public async Task<ResultSimple> DeleteOne(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = default)
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
    /// Deletes an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating the success of the operation.</returns>
    public async Task<ResultSimple> DeleteById(TId id, CancellationToken cancellationToken = default)
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
    /// Deletes multiple entities based on a predicate.
    /// </summary>
    /// <param name="filterExpression">The filter expression.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating the success of the operation.</returns>
    public async Task<ResultSimple> DeleteMany(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet.Where(filterExpression).ToListAsync(cancellationToken);
        if (entities is not null && entities.Count != 0)
        {
            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync(cancellationToken);
        }
        return new ResultSimple();
    }
}