using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Result;
using Dsr.Architecture.Persistence.Abstractions;
using Dsr.Architecture.Utilities.TryCatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework;

/// <summary>
/// Repository implementation for read operations using Entity Framework Core.
/// This repository provides methods for retrieving entities from the database.
/// It uses a DbContext to perform database operations and includes error handling with logging.
/// The repository supports both synchronous and asynchronous methods for each operation, allowing for flexible usage in different scenarios.
/// The repository is designed to work with aggregate roots that implement the IAggregateRoot interface, ensuring that only valid entities are managed through the repository.
/// The repository includes methods for retrieving all entities, filtering entities by a specified expression, 
/// projecting entities to a different type, retrieving the first entity that matches a specified expression, 
/// and retrieving an entity by its unique identifier, providing a comprehensive set of read operations for managing entities in the database.
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TId"></typeparam>
/// <typeparam name="TAggregate"></typeparam>
public abstract class ReadEFRepository<TContext, TId, TAggregate> : IReadRepository<TId, TAggregate>
    where TContext : DbContext
    where TId : IEquatable<TId>, IComparable<TId>
    where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
{
    private readonly DbContext _context;
    private readonly IQueryable<TAggregate> _query;
    private readonly ILogger<ReadEFRepository<TContext, TId, TAggregate>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadEFRepository{TContext, TId, TAggregate}"/> class with the specified DbContext and logger.
    /// </summary>
    /// <param name="unitOfWork"></param>
    /// <param name="logger"></param>
    public ReadEFRepository(IUnitOfWork<TContext> unitOfWork, ILogger<ReadEFRepository<TContext, TId, TAggregate>> logger)
    {
        _context = unitOfWork.Context;
        _query = _context.Set<TAggregate>().AsNoTracking();
        _logger = logger;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadEFRepository{TContext, TId, TAggregate}"/> class with the specified transactional unit of work and logger.
    /// </summary>
    /// <param name="unitOfWork"></param>
    /// <param name="logger"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public ReadEFRepository(ITransactionalEFUnitOfWork unitOfWork, ILogger<ReadEFRepository<TContext, TId, TAggregate>> logger)
    {
        _context = unitOfWork.Accessor.DbContexts.FirstOrDefault(
            x => x is TContext) ?? throw new InvalidOperationException($"No DbContext of type {typeof(TContext).Name} found in the unit of work."
        );
        _query = _context.Set<TAggregate>().AsNoTracking();
        _logger = logger;
    }

    #region Search

    #region Sync

    /// <summary>
    /// Returns the entity set as an <see cref="IQueryable{TAggregate}"/>.
    /// </summary>
    /// <returns>An <see cref="IQueryable{TAggregate}"/> that can be used to query the entities.</returns>
    public IQueryable<TAggregate> AsQueryable() => _query;

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
        => await this.Try(async () => new Result<IEnumerable<TAggregate>>(await _query.ToListAsync(cancellationToken)))
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
        => await this.Try(async () => new Result<IEnumerable<TAggregate>>(await _query.Where(filterExpression).ToListAsync(cancellationToken)))
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
            await _query.Where(filterExpression).Select(projectionExpression).ToListAsync(cancellationToken)))
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
        => await this.Try(async () => new Result<TAggregate>(await _query.FirstOrDefaultAsync(filterExpression, cancellationToken)))
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
        => await this.Try(async () => new Result<TAggregate>(await _context.Set<TAggregate>().FindAsync([id], cancellationToken)))
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while retrieving the entity by ID.");
                return await Task.FromResult(Result<TAggregate>.Error(error.Message));
            })
            .Apply() ?? Result<TAggregate>.Error("An error occurred while retrieving the entity by ID.");

    #endregion

    #endregion
}