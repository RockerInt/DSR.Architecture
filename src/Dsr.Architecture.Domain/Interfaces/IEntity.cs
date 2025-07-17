namespace Dsr.Architecture.Domain.Interfaces;

public interface IEntity
{
    public dynamic? Id { get; set; }
    public bool Enable { get; set; }
    public DateTime CreateDate { get; set; }
}

public interface IEntity<TId> : IEntity
{
    public new TId? Id { get; set; }
}