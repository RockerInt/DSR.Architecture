using Dsr.Architecture.Domain.Entities;
using Dsr.Architecture.Infrastructure.Persistence.Interfaces;
using MongoDB.Bson;
using System.Linq.Expressions;

namespace Dsr.Architecture.Infrastructure.Persistence.Mongo;

/// <summary>
/// Defines a generic repository interface for MongoDB.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IMongoRepository<TEntity> : IRepository<ObjectId, TEntity>
    where TEntity : MongoEntity
{
}