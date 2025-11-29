namespace E_commerce.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } // Unique identifier for the entity (Primary Key)
    public DateTime CreatedAt { get; protected set; } // Timestamp when entity was created (UTC)
    public DateTime? UpdatedAt { get; protected set; } // Timestamp when entity was last updated (nullable, UTC)
    public bool IsDeleted { get; protected set; } // Soft delete flag - true if entity is deleted

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
