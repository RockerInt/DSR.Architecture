using Dsr.Architecture.Domain.Aggregates;

namespace Dsr.Architecture.Domain.Specifications;

/// <summary>
/// A specification that always evaluates to true.
/// Useful as a starting point for combining multiple specifications.
/// </summary>
/// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregate to be filtered.</typeparam>
public class TrueSpecification<TId, TAggregate>() : Specification<TId, TAggregate>(x => true)
    where TAggregate : IAggregateRoot<TId>
    where TId : IEquatable<TId>, IComparable<TId>
{
}