using Dsr.Architecture.Domain.Aggregates;
using Dsr.Architecture.Domain.Result;
using Dsr.Architecture.Domain.Specifications;
using Dsr.Architecture.Domain.Specifications.Enums;
using Dsr.Architecture.Domain.Specifications.Interfaces;
using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries.Interfaces;
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
/// </summary>
/// <typeparam name="TContext">The type of the DbContext.</typeparam>
/// <typeparam name="TId">The type of the aggregate's unique identifier.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregate managed by this repository.</typeparam>
public class ReadEFRepository<TContext, TId, TAggregate> : IReadRepository<TId, TAggregate>
    where TContext : DbContext
    where TId : IEquatable<TId>, IComparable<TId>
    where TAggregate : AggregateRoot<TId>, IAggregateRoot<TId>
{
    private readonly DbContext _context;
    private readonly ICompiledSpecificationExecutor _executor;
    private readonly ILogger<ReadEFRepository<TContext, TId, TAggregate>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadEFRepository{TContext, TId, TAggregate}"/> class with the specified unit of work, executor and logger.
    /// </summary>
    /// <param name="unitOfWork">The unit of work containing the DbContext.</param>
    /// <param name="executor">The executor for compiled specifications.</param>
    /// <param name="logger">The logger for this repository.</param>
    public ReadEFRepository(IUnitOfWork<TContext> unitOfWork, ICompiledSpecificationExecutor executor, ILogger<ReadEFRepository<TContext, TId, TAggregate>> logger)
    {
        _context = unitOfWork.Context;
        _executor = executor;
        _logger = logger;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadEFRepository{TContext, TId, TAggregate}"/> class with the specified transactional unit of work, executor and logger.
    /// </summary>
    /// <param name="unitOfWork">The transactional unit of work containing the DbContexts.</param>
    /// <param name="executor">The executor for compiled specifications.</param>
    /// <param name="logger">The logger for this repository.</param>
    /// <exception cref="InvalidOperationException">Thrown if no DbContext of type <typeparamref name="TContext"/> is found in the unit of work.</exception>
    public ReadEFRepository(ITransactionalEFUnitOfWork unitOfWork, ICompiledSpecificationExecutor executor, ILogger<ReadEFRepository<TContext, TId, TAggregate>> logger)
    {
        _context = unitOfWork.Accessor.DbContexts.FirstOrDefault(
            x => x is TContext) ?? throw new InvalidOperationException($"No DbContext of type {typeof(TContext).Name} found in the unit of work."
        );
        _executor = executor;
        _logger = logger;
    }

    #region Search

    #region Sync

    /// <summary>
    /// Retrieves all aggregates that match the specified specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <returns>A <see cref="Result{IEnumerable{TAggregate}}"/> with a collection of matching aggregates.</returns>
    public Result<IEnumerable<TAggregate>> List(ISpecification<TId, TAggregate> specification) => ListAsync(specification).GetAwaiter().GetResult();

    /// <summary>
    /// Retrieves and projects aggregates that match the specified specification to a different type.
    /// </summary>
    /// <typeparam name="TProjected">The type to project the aggregates to.</typeparam>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="projection">An expression to project the filtered aggregates to <typeparamref name="TProjected"/>.</param>
    /// <returns>A <see cref="Result{IEnumerable{TProjected}}"/> with a collection of projected aggregates.</returns>
    public Result<IEnumerable<TProjected>> List<TProjected>(ISpecification<TId, TAggregate> specification, Expression<Func<TAggregate, TProjected>> projection) =>
        ListAsync(specification, projection).GetAwaiter().GetResult();

    /// <summary>
    /// Retrieves a list of dynamic results based on an analytics specification.
    /// </summary>
    /// <param name="specification">The analytics specification to execute.</param>
    /// <returns>A <see cref="Result{List{dynamic}}"/> with a list of dynamic results.</returns>
    public Result<IEnumerable<dynamic>> ListDynamic(ISpecification<TId, TAggregate> specification) => ListDynamicAsync(specification).GetAwaiter().GetResult();

    /// <summary>
    /// Finds the individual aggregate based on the SpecificationResultCardinality(First, FirstOrDefault, Single, SingleOrDefault) that matches the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <returns>A <see cref="Result{TAggregate}"/> with the matching aggregate.</returns>
    public Result<TAggregate> Get(ISpecification<TId, TAggregate> specification) => GetAsync(specification).GetAwaiter().GetResult();

    /// <summary>
    /// Retrieves an aggregate by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate to retrieve.</param>
    /// <returns>A <see cref="Result{TAggregate}"/> with the retrieved aggregate.</returns>
    public Result<TAggregate> GetById(TId id) => GetByIdAsync(id).GetAwaiter().GetResult();

    /// <summary>
    /// Retrieves a scalar value based on the given specification.
    /// </summary>
    /// <typeparam name="T">The type of the scalar value.</typeparam>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <returns>A <see cref="Result{T}"/> with the retrieved scalar value.</returns>
    public Result<T> GetScalar<T>(ISpecification<TId, TAggregate> specification) where T : struct => GetScalarAsync<T>(specification).GetAwaiter().GetResult();

    /// <summary>
    /// Checks if any aggregate matches the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <returns>A <see cref="Result{bool}"/> indicating if a match was found.</returns>
    public Result<bool> Any(ISpecification<TId, TAggregate> specification) => AnyAsync(specification).GetAwaiter().GetResult();

    /// <summary>
    /// Counts the number of aggregates that match the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <returns>A <see cref="Result{int}"/> with the count of matching aggregates.</returns>
    public Result<int> Count(ISpecification<TId, TAggregate> specification) => CountAsync(specification).GetAwaiter().GetResult();

    #endregion

    #region Async

    /// <summary>
    /// Asynchronously retrieves aggregates that match the specified specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{IEnumerable{TAggregate}}"/> with a collection of matching aggregates.</returns>
    public async Task<Result<IEnumerable<TAggregate>>> ListAsync(ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken = default)
        => await this.Try(async () => new Result<IEnumerable<TAggregate>>(await _executor.ExecuteAsync(_context, specification, cancellationToken)))
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while retrieving entities by specification.");
                return await Task.FromResult(Result<IEnumerable<TAggregate>>.Error(error.Message));
            })
            .Apply() ?? Result<IEnumerable<TAggregate>>.Error("An error occurred while retrieving entities by specification.");

    /// <summary>
    /// Asynchronously retrieves and projects aggregates that match the specified specification to a different type.
    /// </summary>
    /// <typeparam name="TProjected">The type to project the aggregates to.</typeparam>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="projection">An expression to project the filtered aggregates to <typeparamref name="TProjected"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{IEnumerable{TProjected}}"/> with a collection of projected aggregates.</returns>
    public async Task<Result<IEnumerable<TProjected>>> ListAsync<TProjected>(ISpecification<TId, TAggregate> specification, Expression<Func<TAggregate, TProjected>> projection, CancellationToken cancellationToken = default) 
        => await this.Try(async () => new Result<IEnumerable<TProjected>>(
            await _executor.ExecuteAsync(_context, specification, projection, cancellationToken)))
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while retrieving and projecting entities by specification.");
                return await Task.FromResult(Result<IEnumerable<TProjected>>.Error(error.Message));
            })
            .Apply() ?? Result<IEnumerable<TProjected>>.Error("An error occurred while retrieving and projecting entities by specification.");

    /// <summary>
    /// Asynchronously finds the aggregate that matches the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{TAggregate}"/> with the matching aggregate.</returns>
    public async Task<Result<TAggregate>> GetAsync(ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken = default)
        => await this.Try(async () => new Result<TAggregate>(await _executor.ExecuteSingleAsync(_context, specification, cancellationToken)))
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while retrieving the entity by specification.");
                return await Task.FromResult(Result<TAggregate>.Error(error.Message));
            })
            .Apply() ?? Result<TAggregate>.Error("An error occurred while retrieving the entity by specification.");

    /// <summary>
    /// Asynchronously retrieves an aggregate by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate to retrieve.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{TAggregate}"/> with the retrieved aggregate.</returns>
    public async Task<Result<TAggregate>> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        => await this.Try(async () => new Result<TAggregate>(await _context.Set<TAggregate>().FindAsync([id], cancellationToken)))
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while retrieving the entity by ID.");
                return await Task.FromResult(Result<TAggregate>.Error(error.Message));
            })
            .Apply() ?? Result<TAggregate>.Error("An error occurred while retrieving the entity by ID.");

    /// <summary>
    /// Asynchronously retrieves a scalar value based on the given specification.
    /// </summary>
    /// <typeparam name="T">The type of the scalar value.</typeparam>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with the retrieved scalar value.</returns>
    public async Task<Result<T>> GetScalarAsync<T>(ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken = default) where T : struct
        => await this.Try(async () => new Result<T>(await _executor.ExecuteScalarAsync<T, TId, TAggregate>(_context, specification, cancellationToken)))
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while retrieving the scalar value by specification.");
                return await Task.FromResult(Result<T>.Error(error.Message));
            })
            .Apply() ?? Result<T>.Error("An error occurred while retrieving the scalar value by specification.");

    /// <summary>
    /// Asynchronously checks if any aggregate matches the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{bool}"/> indicating if a match was found.</returns>
    public async Task<Result<bool>> AnyAsync(ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken = default)
        => await this.Try(async () =>
            {
                var query = _context.Set<TAggregate>().AsNoTracking();
                if (specification.Criteria != null)
                    query = query.Where(specification.Criteria);

                return new Result<bool>(await query.AnyAsync(cancellationToken));
            })
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while checking if any entity matches the specification.");
                return await Task.FromResult(Result<bool>.Error(error.Message));
            })
            .Apply() ?? Result<bool>.Error("An error occurred while checking if any entity matches the specification.");

    /// <summary>
    /// Asynchronously counts the number of aggregates that match the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the aggregates.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{int}"/> with the count of matching aggregates.</returns>
    public async Task<Result<int>> CountAsync(ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken = default)
        => await this.Try(async () =>
            {
                var query = _context.Set<TAggregate>().AsNoTracking();
                if (specification.Criteria != null)
                    query = query.Where(specification.Criteria);

                return new Result<int>(await query.CountAsync(cancellationToken));
            })
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while counting entities that match the specification.");
                return await Task.FromResult(Result<int>.Error(error.Message));
            })
            .Apply() ?? Result<int>.Error("An error occurred while counting entities that match the specification.");

    /// <summary>
    /// Asynchronously retrieves a list of dynamic results based on an analytics specification.
    /// </summary>
    /// <param name="specification">The analytics specification to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{List{dynamic}}"/> with a list of dynamic results.</returns>
    public async Task<Result<IEnumerable<dynamic>>> ListDynamicAsync(ISpecification<TId, TAggregate> specification, CancellationToken cancellationToken = default)
        => await this.Try(async () => new Result<IEnumerable<dynamic>>(await _executor.ExecuteDynamicAsync(_context, specification, cancellationToken)))
            .Catch(async (error) =>
            {
                _logger.LogError(error, "An error occurred while retrieving dynamic entities by specification.");
                return await Task.FromResult(Result<IEnumerable<dynamic>>.Error(error.Message));
            })
            .Apply() ?? Result<IEnumerable<dynamic>>.Error("An error occurred while retrieving dynamic entities by specification.");


    #endregion

    #endregion
}