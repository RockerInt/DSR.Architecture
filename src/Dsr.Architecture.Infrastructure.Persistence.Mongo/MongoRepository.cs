using Dsr.Architecture.Domain.Entities;
using Dsr.Architecture.Infrastructure.Persistence.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Dsr.Architecture.Infrastructure.Persistence.Mongo;

/// <summary>
/// Abstract class for the repository model for communication with MongoDB
/// </summary>
/// <typeparam name="TDocument">Type of document handled by the repository</typeparam>
public abstract class MongoRepository<TDocument> : IRepository<ObjectId, TDocument>
    where TDocument : MongoEntity
{
    /// <summary>
    /// MongoDB collection for the specified document type
    /// </summary>
    private readonly IMongoCollection<TDocument> _collection;

    /// <summary>
    /// MongoDB repository constructor
    /// </summary>
    /// <param name="settings">Configuration for connecting to the MongoDB database</param>
    public MongoRepository(IPersistenceSettings settings)
    {
        var database = new MongoClient(settings.ConnectionString).GetDatabase(settings.DatabaseName);
        _collection = database.GetCollection<TDocument>(typeof(TDocument).GetCollectionName());
    }

    #region Search

    #region Sync

    /// <summary>
    /// Returns a query on the MongoDB collection
    /// </summary>
    /// <returns>An IQueryable of documents</returns>
    public virtual IQueryable<TDocument> AsQueryable() => _collection.AsQueryable();

    /// <summary>
    /// Retrieves all documents from the collection.
    /// </summary>
    /// <returns>A <see cref="Result{T}"/> with a collection of all documents.</returns>
    public Result<IEnumerable<TDocument>> GetAll() => GetAllAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Retrieves documents that match the specified filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the documents.</param>
    /// <returns>A <see cref="Result{T}"/> with a collection of matching documents.</returns>
    public Result<IEnumerable<TDocument>> GetBy(Expression<Func<TDocument, bool>> filterExpression) => GetByAsync(filterExpression).GetAwaiter().GetResult();

    /// <summary>
    /// Retrieves and projects documents that match the specified filter expression to a different type.
    /// </summary>
    /// <typeparam name="TProjected">The type to project the documents to.</typeparam>
    /// <param name="filterExpression">An expression to filter the documents.</param>
    /// <param name="projectionExpression">An expression to project the filtered documents to <typeparamref name="TProjected"/>.</param>
    /// <returns>A <see cref="Result{T}"/> with a collection of projected documents.</returns>
    public Result<IEnumerable<TProjected>> GetBy<TProjected>(Expression<Func<TDocument, bool>> filterExpression,
            Expression<Func<TDocument, TProjected>> projectionExpression) => GetByAsync(filterExpression, projectionExpression).GetAwaiter().GetResult();

    /// <summary>
    /// Finds the first document that matches the given filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the documents.</param>
    /// <returns>A <see cref="Result{T}"/> with the first matching document.</returns>
    public Result<TDocument> First(Expression<Func<TDocument, bool>> filterExpression) => FirstAsync(filterExpression).GetAwaiter().GetResult();

    /// <summary>
    /// Retrieves a document by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the document to retrieve.</param>
    /// <returns>A <see cref="Result{T}"/> with the retrieved document.</returns>
    public Result<TDocument> GetById(ObjectId id) => GetByIdAsync(id).GetAwaiter().GetResult();

    #endregion

    #region Async

    /// <summary>
    /// Asynchronously retrieves all documents from the collection.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with a collection of all documents.</returns>
    public async Task<Result<IEnumerable<TDocument>>> GetAllAsync(CancellationToken cancellationToken = new()) => new Result<IEnumerable<TDocument>>(await _collection.Find(_ => true).ToListAsync(cancellationToken));

    /// <summary>
    /// Asynchronously retrieves documents that match the specified filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the documents.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with a collection of matching documents.</returns>
    public virtual async Task<Result<IEnumerable<TDocument>>> GetByAsync(
        Expression<Func<TDocument, bool>> filterExpression, CancellationToken cancellationToken = new())
        => new Result<IEnumerable<TDocument>>(await _collection.Find(filterExpression).ToListAsync(cancellationToken));

    /// <summary>
    /// Asynchronously retrieves and projects documents that match the specified filter expression to a different type.
    /// </summary>
    /// <typeparam name="TProjected">The type to project the documents to.</typeparam>
    /// <param name="filterExpression">An expression to filter the documents.</param>
    /// <param name="projectionExpression">An expression to project the filtered documents to <typeparamref name="TProjected"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with a collection of projected documents.</returns>
    public virtual async Task<Result<IEnumerable<TProjected>>> GetByAsync<TProjected>(
        Expression<Func<TDocument, bool>> filterExpression,
        Expression<Func<TDocument, TProjected>> projectionExpression, CancellationToken cancellationToken = new())
        => new Result<IEnumerable<TProjected>>(await _collection.Find(filterExpression).Project(projectionExpression).ToListAsync(cancellationToken));

    /// <summary>
    /// Asynchronously finds the first document that matches the given filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the documents.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with the first matching document.</returns>
    public virtual async Task<Result<TDocument>> FirstAsync(Expression<Func<TDocument, bool>> filterExpression, CancellationToken cancellationToken = new())
        => new Result<TDocument>(await _collection.Find(filterExpression).FirstOrDefaultAsync(cancellationToken));

    /// <summary>
    /// Asynchronously retrieves a document by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the document to retrieve.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="Result{T}"/> with the retrieved document.</returns>
    public virtual async Task<Result<TDocument>> GetByIdAsync(ObjectId id, CancellationToken cancellationToken = new())
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
        return new Result<TDocument>(await _collection.Find(filter).SingleOrDefaultAsync(cancellationToken));
    }

    #endregion

    #endregion

    #region CUD

    #region Sync

    /// <summary>
    /// Adds a new document to the collection.
    /// </summary>
    /// <param name="entity">The document to be added.</param>
    /// <returns>A <see cref="ResultSimple"/> indicating the outcome.</returns>
    public ResultSimple Add(TDocument entity) => AddAsync(entity).GetAwaiter().GetResult();

    /// <summary>
    /// Adds multiple new documents to the collection.
    /// </summary>
    /// <param name="entities">A collection of documents to be added.</param>
    /// <returns>A <see cref="ResultSimple"/> indicating the outcome.</returns>
    public ResultSimple AddRange(ICollection<TDocument> entities) => AddRangeAsync(entities).GetAwaiter().GetResult();

    /// <summary>
    /// Updates an existing document in the collection.
    /// </summary>
    /// <param name="entity">The document to be updated.</param>
    /// <returns>A <see cref="ResultSimple"/> indicating the outcome.</returns>
    public ResultSimple Update(TDocument entity) => UpdateAsync(entity).GetAwaiter().GetResult();

    /// <summary>
    /// Removes documents from the collection based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the documents to be removed.</param>
    /// <returns>A <see cref="ResultSimple"/> indicating the outcome.</returns>
    public ResultSimple Remove(Expression<Func<TDocument, bool>> filterExpression) => RemoveAsync(filterExpression).GetAwaiter().GetResult();

    /// <summary>
    /// Removes a document from the collection by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the document to be removed.</param>
    /// <returns>A <see cref="ResultSimple"/> indicating the outcome.</returns>
    public ResultSimple RemoveById(ObjectId id) => RemoveByIdAsync(id).GetAwaiter().GetResult();

    /// <summary>
    /// Removes multiple documents from the collection based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the documents to be removed.</param>
    /// <returns>A <see cref="ResultSimple"/> indicating the outcome.</returns>
    public ResultSimple RemoveRange(Expression<Func<TDocument, bool>> filterExpression) => RemoveRangeAsync(filterExpression).GetAwaiter().GetResult();

    #endregion

    #region Async

    /// <summary>
    /// Asynchronously adds a new document to the collection.
    /// </summary>
    /// <param name="document">The document to be added.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    public virtual async Task<ResultSimple> AddAsync(TDocument document, CancellationToken cancellationToken = new())
    {
        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        return new ResultSimple();
    }

    /// <summary>
    /// Asynchronously adds multiple new documents to the collection.
    /// </summary>
    /// <param name="documents">A collection of documents to be added.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    public virtual async Task<ResultSimple> AddRangeAsync(ICollection<TDocument> documents, CancellationToken cancellationToken = new())
    {
        await _collection.InsertRangeAsync(documents, cancellationToken: cancellationToken);
        return new ResultSimple();
    }

    /// <summary>
    /// Asynchronously updates an existing document in the collection.
    /// </summary>
    /// <param name="document">The document to be updated.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    public virtual async Task<ResultSimple> UpdateAsync(TDocument document, CancellationToken cancellationToken = new())
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
        await _collection.FindOneAndReplaceAsync(filter, document, cancellationToken: cancellationToken);
        return new ResultSimple();
    }

    /// <summary>
    /// Asynchronously removes documents from the collection based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the documents to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    public virtual async Task<ResultSimple> RemoveAsync(Expression<Func<TDocument, bool>> filterExpression, CancellationToken cancellationToken = new())
    {
        await _collection.FindOneAndDeleteAsync(filterExpression, cancellationToken: cancellationToken);
        return new ResultSimple();
    }

    /// <summary>
    /// Asynchronously removes a document from the collection by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the document to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    public virtual async Task<ResultSimple> RemoveByIdAsync(ObjectId id, CancellationToken cancellationToken = new())
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
        await _collection.FindOneAndDeleteAsync(filter, cancellationToken: cancellationToken);
        return new ResultSimple();
    }

    /// <summary>
    /// Asynchronously removes multiple documents from the collection based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">An expression to filter the documents to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a <see cref="ResultSimple"/> indicating the outcome.</returns>
    public virtual async Task<ResultSimple> RemoveRangeAsync(Expression<Func<TDocument, bool>> filterExpression, CancellationToken cancellationToken = new())
    {
        await _collection.DeleteRangeAsync(filterExpression, cancellationToken: cancellationToken);
        return new ResultSimple();
    }

    #endregion

    #endregion
}
