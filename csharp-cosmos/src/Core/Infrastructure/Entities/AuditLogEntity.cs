namespace Todo.Core.Infrastructure.Entities;

public class AuditLogEntity
{
    public Guid AuditId { get; set; }
    public string AssetId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
}
