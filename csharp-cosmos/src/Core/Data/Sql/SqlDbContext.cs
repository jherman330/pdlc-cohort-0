using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Todo.Core.Common.Entities;

namespace Todo.Core.Data.Sql;

/// <summary>
/// Entity Framework Core DbContext for Azure SQL. Connection pooling and retry are configured in DI (AddDbContext).
/// Use Set{T}() for BaseEntity-derived types; ensure entities are registered in the model (e.g. via DbSet or OnModelCreating).
/// </summary>
public sealed class SqlDbContext : DbContext, ISqlDbContext
{
    public SqlDbContext(DbContextOptions<SqlDbContext> options)
        : base(options)
    {
    }

    /// <inheritdoc />
    public new DbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity => base.Set<TEntity>();

    /// <inheritdoc />
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => Database.BeginTransactionAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType) && entityType.ClrType != typeof(BaseEntity))
            {
                modelBuilder.Entity(entityType.ClrType, b =>
                {
                    b.HasKey(nameof(BaseEntity.Id));
                });
            }
        }
    }
}
