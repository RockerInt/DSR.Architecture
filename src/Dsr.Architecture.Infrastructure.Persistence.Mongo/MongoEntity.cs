using Dsr.Architecture.Domain.Entities;
using Dsr.Architecture.Domain.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Dsr.Architecture.Infrastructure.Persistence.Mongo;

/// <summary>
/// Represents a MongoDB entity with an ObjectId as the identifier.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MongoEntity"/> class.
/// </remarks>
/// <param name="id">The ObjectId of the entity. If null, a new ObjectId is generated.</param>
public abstract class MongoEntity(ObjectId? id = null)
    : Entity<ObjectId>(id ?? GetNewId()), IMongoEntity, IEntity<ObjectId>
{
    /// <summary>
    /// Gets or sets the ObjectId of the entity.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public new ObjectId Id { get; set; } = id ?? GetNewId();

    /// <summary>
    /// Gets or sets the date when the entity was created.
    /// </summary>
    public new DateTime CreateDate { get; set; } = (id ?? GetNewId()).CreationTime;

    /// <summary>
    /// Generates a new ObjectId.
    /// </summary>
    /// <returns>A new ObjectId.</returns>
    private static ObjectId GetNewId() => ObjectId.GenerateNewId(DateTime.Now);
}
