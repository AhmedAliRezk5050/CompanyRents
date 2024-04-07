namespace Core.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}