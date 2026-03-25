using Microsoft.EntityFrameworkCore;
using Todo.Core.Infrastructure.Entities;

namespace Todo.Core.Infrastructure;

/// <summary>
/// EF Core DbContext for Azure SQL reference tables (Assets, AuditLog, LicenseUtilization).
/// Schema is created by infra/core/database/sql-schema.sql; WO-8 RLS by sql-rls.sql.
/// </summary>
public class ReferenceDataDbContext : DbContext
{
    public ReferenceDataDbContext(DbContextOptions<ReferenceDataDbContext> options)
        : base(options)
    {
    }

    public DbSet<AssetEntity> Assets => Set<AssetEntity>();
    public DbSet<AuditLogEntity> AuditLog => Set<AuditLogEntity>();
    public DbSet<LicenseUtilizationEntity> LicenseUtilization => Set<LicenseUtilizationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssetEntity>(e =>
        {
            e.ToTable("Assets");
            e.HasKey(x => x.AssetId);
            e.Property(x => x.TenantId).HasColumnName("TenantID");
        });
        modelBuilder.Entity<AuditLogEntity>(e =>
        {
            e.ToTable("AuditLog");
            e.HasKey(x => x.AuditId);
            e.Property(x => x.TenantId).HasColumnName("TenantID");
            e.HasIndex(x => x.Timestamp);
            e.HasIndex(x => x.AssetId);
        });
        modelBuilder.Entity<LicenseUtilizationEntity>(e =>
        {
            e.ToTable("LicenseUtilization");
            e.HasKey(x => x.UtilizationId);
            e.Property(x => x.TenantId).HasColumnName("TenantID");
            e.HasIndex(x => x.SnapshotDate);
            e.HasIndex(x => x.AssetId);
        });
    }
}
