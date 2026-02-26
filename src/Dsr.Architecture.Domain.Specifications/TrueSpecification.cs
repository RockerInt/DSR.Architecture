namespace Dsr.Architecture.Domain.Specifications;

/// <summary>
/// A specification that initially returns true.
/// This specification can be combined with other specifications using the And or Or methods to create a more complex specification.
/// </summary>
/// <typeparam name="T">The type of the entity to be filtered.</typeparam>
public class TrueSpecification<T>() : Specification<T>(x => true)
{
}