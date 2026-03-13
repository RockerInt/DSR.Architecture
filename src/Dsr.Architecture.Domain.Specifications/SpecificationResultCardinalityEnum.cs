namespace Dsr.Architecture.Domain.Specifications;

/// <summary>
/// Defines the expected cardinality of a specification result.
/// </summary>
public enum SpecificationResultCardinality
{
    /// <summary>
    /// The result is expected to be a list of entities.
    /// </summary>
    List,

    /// <summary>
    /// The result is expected to be the first entity found, or an exception if none are found.
    /// </summary>
    First,

    /// <summary>
    /// The result is expected to be the first entity found, or null if none are found.
    /// </summary>
    FirstOrDefault,

    /// <summary>
    /// The result is expected to be a single entity, or an exception if none or more than one are found.
    /// </summary>
    Single,

    /// <summary>
    /// The result is expected to be a single entity, or null if none are found; an exception is thrown if more than one are found.
    /// </summary>
    SingleOrDefault
}