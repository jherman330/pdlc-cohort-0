namespace Todo.Core.Infrastructure.Entities;

public class LicenseUtilizationEntity
{
    public Guid UtilizationId { get; set; }
    public string AssetId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string LicenseType { get; set; } = string.Empty;
    public int UsedCount { get; set; }
    public int TotalCount { get; set; }
    public DateTime SnapshotDate { get; set; }
}
