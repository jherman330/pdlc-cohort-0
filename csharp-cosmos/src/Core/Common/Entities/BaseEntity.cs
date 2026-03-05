namespace Todo.Core.Common.Entities;

/// <summary>
/// Base entity with common fields for all data models: id, timestamps, and schema version.
/// Establishes entity conventions across Cosmos and SQL repositories.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>UTC creation time.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC last update time.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Schema version for migrations and compatibility.</summary>
    public int SchemaVersion { get; set; } = 1;

    /// <summary>
    /// Initializes timestamps and default schema version. Call from derived constructors.
    /// </summary>
    protected BaseEntity()
    {
        var now = DateTime.UtcNow;
        CreatedAt = now;
        UpdatedAt = now;
    }

    /// <summary>
    /// Call before persisting an update to set UpdatedAt. Override to update other fields.
    /// </summary>
    public virtual void TrackUpdate()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
