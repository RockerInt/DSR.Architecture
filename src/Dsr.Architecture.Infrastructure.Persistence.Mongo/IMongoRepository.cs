using Dsr.Architecture.Domain.Entities;
using Dsr.Architecture.Infrastructure.Persistence.Interfaces;
using MongoDB.Bson;
using System.Linq.Expressions;

namespace Dsr.Architecture.Infrastructure.Persistence.Mongo;

public interface IMongoRepository<TEntity> : IRepository<ObjectId, TEntity>
    where TEntity : MongoEntity
{
}