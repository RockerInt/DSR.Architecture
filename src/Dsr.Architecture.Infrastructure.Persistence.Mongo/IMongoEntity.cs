using Dsr.Architecture.Domain.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Dsr.Architecture.Infrastructure.Persistence.Mongo;

/// <summary>
/// Represents a MongoDB entity with an ObjectId as the identifier.
/// </summary>
public interface IMongoEntity
    : IEntity<ObjectId>
{
    /// <summary>
    /// Gets or sets the ObjectId of the entity.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public new ObjectId Id { get; set; } 
}
