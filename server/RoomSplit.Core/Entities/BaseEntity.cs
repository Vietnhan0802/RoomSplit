namespace RoomSplit.Core.Entities;

public interface IEntity
{
    Guid Id { get; set; }
}

public abstract class BaseEntity : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
