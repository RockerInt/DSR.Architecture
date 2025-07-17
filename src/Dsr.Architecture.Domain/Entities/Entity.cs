using Dsr.Architecture.Domain.Interfaces;

namespace Dsr.Architecture.Domain.Entities;

/// <summary>
/// Abstract base class for an entity with a dynamic identifier.
/// </summary>
/// <remarks>
/// Constructor for the Entity class.
/// </remarks>
/// <param name="id">Dynamic identifier for the entity.</param>
public abstract class Entity(dynamic? id) : IEntity
{
    /// <summary>
    /// Dynamic identifier for the entity.
    /// </summary>
    public dynamic? Id { get; set; } = id;

    /// <summary>
    /// Indicates whether the entity is enabled.
    /// </summary>
    public bool Enable { get; set; } = true;

    /// <summary>
    /// Date and time when the entity was created.
    /// </summary>
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Abstract base class for an entity with a strongly-typed identifier.
/// </summary>
/// <typeparam name="TId">Type of the identifier.</typeparam>
public abstract class Entity<TId> : Entity, IEntity<TId>
    where TId : IEquatable<TId>, IComparable<TId>
{
    /// <summary>
    /// Constructor for the Entity class with a strongly-typed identifier.
    /// </summary>
    /// <param name="id">Strongly-typed identifier for the entity.</param>
    public Entity(TId? id)
        : base(id)
        => Id = id;

    /// <summary>
    /// Strongly-typed identifier for the entity.
    /// </summary>
    public new TId? Id
    {
        get => (TId?)base.Id;
        set => base.Id = value;
    }
}

