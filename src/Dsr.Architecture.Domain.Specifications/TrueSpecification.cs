using Dsr.Architecture.Domain.Aggregates;

namespace Dsr.Architecture.Domain.Specifications;

/// <summary>
/// A specification that initially returns true.
/// This specification can be combined with other specifications using the And or Or methods to create a more complex specification.
/// </summary>
/// <typeparam name="TId">The type of the identifier.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregate to be filtered.</typeparam>
public class TrueSpecification<TId, TAggregate>() : Specification<TId, TAggregate>(x => true)
    where TAggregate : IAggregateRoot<TId>
    where TId : IEquatable<TId>, IComparable<TId>
{
}