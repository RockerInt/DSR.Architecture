using Dsr.Architecture.Domain.Aggregates;

namespace Dsr.Architecture.Domain.Abstractions.Repositories;

/// <summary>
/// Defines a repository interface for managing aggregate roots.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TId"></typeparam>
public interface IAggregateRepository<T, TId> : IRepository<TId, T>
    where T : AggregateRoot<TId>
    where TId : IEquatable<TId>, IComparable<TId>
{
    
}