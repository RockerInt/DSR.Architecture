using Dsr.Architecture.Domain.Aggregates;

namespace Dsr.Architecture.Persistence.Abstractions;

/// <summary>
/// Defines a generic repository interface for common data access operations on entities.
/// Provides methods for searching, inserting, updating, and deleting entities.
/// This interface extends both IReadRepository and IWriteRepository, representing
/// a full-featured repository (useful for simple CRUD or before splitting into CQRS).
/// </summary>
/// <typeparam name="TId">The type of the aggregate's unique identifier.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregate managed by this repository.</typeparam>
public interface IRepository<TId, TAggregate> : IReadRepository<TId, TAggregate>, IWriteRepository<TId, TAggregate>
    where TId : IEquatable<TId>, IComparable<TId>
    where TAggregate : IAggregateRoot<TId>
{
}