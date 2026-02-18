namespace Dsr.Architecture.Domain.Entities;

/// <summary>
/// Defines a generic entity with a dynamic identifier.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets or sets the dynamic identifier of the entity.
    /// </summary>
    public dynamic? Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity is enabled.
    /// </summary>
    public bool Enable { get; set; }

    /// <summary>
    /// Gets or sets the creation date and time of the entity.
    /// </summary>
    public DateTime CreateDate { get; set; }
}

/// <summary>
/// Defines a generic entity with a strongly-typed identifier.
/// </summary>
/// <typeparam name="TId">The type of the identifier.</typeparam>
public interface IEntity<TId> : IEntity
{
    /// <summary>
    /// Gets or sets the strongly-typed identifier of the entity.
    /// </summary>
    public new TId? Id { get; set; }
}
