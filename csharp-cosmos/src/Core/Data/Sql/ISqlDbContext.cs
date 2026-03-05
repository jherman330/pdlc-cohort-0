using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Todo.Core.Common.Entities;

namespace Todo.Core.Data.Sql;

/// <summary>
/// Abstracts Azure SQL Database access using Entity Framework Core. Used by SQL repositories.
/// </summary>
public interface ISqlDbContext
{
    /// <summary>Returns the DbSet for entity type T (must inherit BaseEntity).</summary>
    DbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity;

    /// <summary>Persists pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Starts a database transaction.</summary>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
