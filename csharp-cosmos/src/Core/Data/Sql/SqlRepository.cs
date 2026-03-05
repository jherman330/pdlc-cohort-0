using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Todo.Core.Common.Entities;
using Todo.Core.Exceptions;

namespace Todo.Core.Data.Sql;

/// <summary>
/// Azure SQL implementation of IRepository using Entity Framework Core.
/// </summary>
public sealed class SqlRepository<T> : BaseRepository<T> where T : BaseEntity
{
    private readonly ISqlDbContext _context;

    public SqlRepository(ISqlDbContext context, ILogger<SqlRepository<T>> logger)
        : base(logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private DbSet<T> DbSet => _context.Set<T>();

    protected override async Task<T?> GetByIdCoreAsync(string id, CancellationToken cancellationToken)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
    }

    protected override async Task<IReadOnlyList<T>> GetAllCoreAsync(CancellationToken cancellationToken)
    {
        return await DbSet.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    protected override async Task<T> CreateCoreAsync(T entity, CancellationToken cancellationToken)
    {
        await DbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity;
    }

    protected override async Task<T> UpdateCoreAsync(T entity, CancellationToken cancellationToken)
    {
        DbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity;
    }

    protected override async Task DeleteCoreAsync(string id, CancellationToken cancellationToken)
    {
        var existing = await DbSet.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (existing == null)
            throw new EntityNotFoundException(typeof(T).Name, id);
        DbSet.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override IQueryable<T> Query() => DbSet.AsQueryable();

    protected override void MapAndThrow(Exception ex, string operation, string? id = null)
    {
        if (ex is EntityNotFoundException)
            throw ex;
        if (ex is DbUpdateException dbEx)
            throw new DataAccessException(operation, id, dbEx.InnerException?.Message ?? dbEx.Message, System.Net.HttpStatusCode.BadRequest, dbEx);
        throw new DataAccessException(operation, id, ex.Message, System.Net.HttpStatusCode.InternalServerError, ex);
    }
}
