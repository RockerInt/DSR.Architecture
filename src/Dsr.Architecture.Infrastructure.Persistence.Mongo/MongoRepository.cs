using Dsr.Architecture.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Dsr.Architecture.Infrastructure.Persistence.Mongo;

/// <summary>
/// Clase abstracta del modelo repositorio para la comunicación con MongoDB
/// </summary>
/// <typeparam name="TDocument">Tipo de documento manejado por el repositorio</typeparam>
public abstract class MongoRepository<TDocument> : IMongoRepository<TDocument>
    where TDocument : MongoEntity
{
    /// <summary>
    /// Colección de MongoDB para el tipo de documento especificado
    /// </summary>
    private readonly IMongoCollection<TDocument> _collection;

    /// <summary>
    /// Constructor del repositorio MongoDB
    /// </summary>
    /// <param name="settings">Configuración para la conexión a la base de datos MongoDB</param>
    public MongoRepository(IPersistenceSettings settings)
    {
        var database = new MongoClient(settings.ConnectionString).GetDatabase(settings.DatabaseName);
        _collection = database.GetCollection<TDocument>(typeof(TDocument).GetCollectionName());
    }

    #region Search

    /// <summary>
    /// Devuelve una consulta sobre la colección MongoDB
    /// </summary>
    /// <returns>Un IQueryable de documentos</returns>
    public virtual IQueryable<TDocument> AsQueryable() => _collection.AsQueryable();

    /// <summary>
    /// Filtra los documentos de la colección según la expresión de filtro proporcionada
    /// </summary>
    /// <param name="filterExpression">Expresión para filtrar los documentos</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Una tarea que resulta en un objeto Result conteniendo una enumeración de documentos</returns>
    public virtual async Task<Result<IEnumerable<TDocument>>> FilterBy(
        Expression<Func<TDocument, bool>> filterExpression, CancellationToken cancellationToken = new())
        => await Task.Run(() => new Result<IEnumerable<TDocument>>(_collection.Find(filterExpression).ToEnumerable()), cancellationToken);

    /// <summary>
    /// Filtra y proyecta los documentos de la colección según las expresiones proporcionadas
    /// </summary>
    /// <typeparam name="TProjected">Tipo de la proyección</typeparam>
    /// <param name="filterExpression">Expresión para filtrar los documentos</param>
    /// <param name="projectionExpression">Expresión para proyectar los documentos</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Una tarea que resulta en un objeto Result conteniendo una enumeración de los documentos proyectados</returns>
    public virtual async Task<Result<IEnumerable<TProjected>>> FilterBy<TProjected>(
        Expression<Func<TDocument, bool>> filterExpression,
        Expression<Func<TDocument, TProjected>> projectionExpression, CancellationToken cancellationToken = new())
        => await Task.Run(
            () => new Result<IEnumerable<TProjected>>(
                _collection.Find(filterExpression).Project(projectionExpression).ToEnumerable()
                ),
            cancellationToken
        );

    /// <summary>
    /// Encuentra un documento que cumple con la expresión de filtro proporcionada
    /// </summary>
    /// <param name="filterExpression">Expresión para filtrar los documentos</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Una tarea que resulta en un objeto Result conteniendo el documento encontrado</returns>
    public virtual async Task<Result<TDocument>> FindOne(Expression<Func<TDocument, bool>> filterExpression, CancellationToken cancellationToken = new())
        => new Result<TDocument>(await _collection.Find(filterExpression).FirstOrDefaultAsync(cancellationToken));

    /// <summary>
    /// Encuentra un documento por su ID
    /// </summary>
    /// <param name="id">ID del documento</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Una tarea que resulta en un objeto Result conteniendo el documento encontrado</returns>
    public virtual async Task<Result<TDocument>> FindById(ObjectId id, CancellationToken cancellationToken = new())
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
        return new Result<TDocument>(await _collection.Find(filter).SingleOrDefaultAsync(cancellationToken));
    }

    #endregion Search

    #region CUD

    /// <summary>
    /// Inserta un documento en la colección
    /// </summary>
    /// <param name="document">Documento a insertar</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Una tarea que resulta en un objeto ResultSimple indicando el resultado de la operación</returns>
    public virtual async Task<ResultSimple> InsertOne(TDocument document, CancellationToken cancellationToken = new())
    {
        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        return new ResultSimple();
    }

    /// <summary>
    /// Inserta múltiples documentos en la colección
    /// </summary>
    /// <param name="documents">Colección de documentos a insertar</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Una tarea que resulta en un objeto ResultSimple indicando el resultado de la operación</returns>
    public virtual async Task<ResultSimple> InsertMany(ICollection<TDocument> documents, CancellationToken cancellationToken = new())
    {
        await _collection.InsertManyAsync(documents, cancellationToken: cancellationToken);
        return new ResultSimple();
    }

    /// <summary>
    /// Actualiza un documento en la colección
    /// </summary>
    /// <param name="document">Documento a actualizar</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Una tarea que resulta en un objeto ResultSimple indicando el resultado de la operación</returns>
    public virtual async Task<ResultSimple> UpdateOne(TDocument document, CancellationToken cancellationToken = new())
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
        await _collection.FindOneAndReplaceAsync(filter, document, cancellationToken: cancellationToken);
        return new ResultSimple();
    }

    /// <summary>
    /// Elimina un documento de la colección basado en una expresión de filtro
    /// </summary>
    /// <param name="filterExpression">Expresión para filtrar los documentos</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Una tarea que resulta en un objeto ResultSimple indicando el resultado de la operación</returns>
    public virtual async Task<ResultSimple> DeleteOne(Expression<Func<TDocument, bool>> filterExpression, CancellationToken cancellationToken = new())
    {
        await _collection.FindOneAndDeleteAsync(filterExpression, cancellationToken: cancellationToken);
        return new ResultSimple();
    }

    /// <summary>
    /// Elimina un documento de la colección por su ID
    /// </summary>
    /// <param name="id">ID del documento</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Una tarea que resulta en un objeto ResultSimple indicando el resultado de la operación</returns>
    public virtual async Task<ResultSimple> DeleteById(ObjectId id, CancellationToken cancellationToken = new())
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
        await _collection.FindOneAndDeleteAsync(filter, cancellationToken: cancellationToken);
        return new ResultSimple();
    }

    /// <summary>
    /// Elimina múltiples documentos de la colección basado en una expresión de filtro
    /// </summary>
    /// <param name="filterExpression">Expresión para filtrar los documentos</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Una tarea que resulta en un objeto ResultSimple indicando el resultado de la operación</returns>
    public virtual async Task<ResultSimple> DeleteMany(Expression<Func<TDocument, bool>> filterExpression, CancellationToken cancellationToken = new())
    {
        await _collection.FindOneAndDeleteAsync(filterExpression, cancellationToken: cancellationToken);
        return new ResultSimple();
    }

    #endregion CUD
}