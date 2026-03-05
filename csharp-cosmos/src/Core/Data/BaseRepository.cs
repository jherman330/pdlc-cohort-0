using Microsoft.Extensions.Logging;
using Todo.Core.Common.Entities;
using Todo.Core.Exceptions;

namespace Todo.Core.Data;

/// <summary>
/// Shared repository logic: timestamp handling, logging, and exception mapping. Specific storage is in derived classes.
/// </summary>
public abstract class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected ILogger<BaseRepository<T>> Logger { get; }

    protected BaseRepository(ILogger<BaseRepository<T>> logger)
    {
        Logger = logger;
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ValidationException(
                new Dictionary<string, string[]> { { "id", new[] { "Id is required." } } });
        }

        Logger.LogDebug("GetById {Id} for {EntityName}", id, typeof(T).Name);
        try
        {
            return await GetByIdCoreAsync(id, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not ApplicationExceptionBase)
        {
            MapAndThrow(ex, "GetById", id);
            return default;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("GetAll for {EntityName}", typeof(T).Name);
        try
        {
            return await GetAllCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not ApplicationExceptionBase)
        {
            MapAndThrow(ex, "GetAll");
            return Array.Empty<T>();
        }
    }

    /// <inheritdoc />
    public async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        SetCreateTimestamps(entity);
        Logger.LogDebug("Create for {EntityName} Id={Id}", typeof(T).Name, entity.Id);
        try
        {
            return await CreateCoreAsync(entity, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not ApplicationExceptionBase)
        {
            MapAndThrow(ex, "Create", entity.Id);
            return entity;
        }
    }

    /// <inheritdoc />
    public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        entity.TrackUpdate();
        Logger.LogDebug("Update for {EntityName} Id={Id}", typeof(T).Name, entity.Id);
        try
        {
            return await UpdateCoreAsync(entity, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not ApplicationExceptionBase)
        {
            MapAndThrow(ex, "Update", entity.Id);
            return entity;
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ValidationException(
                new Dictionary<string, string[]> { { "id", new[] { "Id is required." } } });
        }

        Logger.LogDebug("Delete {Id} for {EntityName}", id, typeof(T).Name);
        try
        {
            await DeleteCoreAsync(id, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not ApplicationExceptionBase)
        {
            MapAndThrow(ex, "Delete", id);
        }
    }

    /// <inheritdoc />
    public abstract IQueryable<T> Query();

    protected abstract Task<T?> GetByIdCoreAsync(string id, CancellationToken cancellationToken);
    protected abstract Task<IReadOnlyList<T>> GetAllCoreAsync(CancellationToken cancellationToken);
    protected abstract Task<T> CreateCoreAsync(T entity, CancellationToken cancellationToken);
    protected abstract Task<T> UpdateCoreAsync(T entity, CancellationToken cancellationToken);
    protected abstract Task DeleteCoreAsync(string id, CancellationToken cancellationToken);

    /// <summary>Override to map storage-specific exceptions to ApplicationExceptionBase.</summary>
    protected abstract void MapAndThrow(Exception ex, string operation, string? id = null);

    private static void SetCreateTimestamps(T entity)
    {
        var now = DateTime.UtcNow;
        entity.CreatedAt = now;
        entity.UpdatedAt = now;
    }
}
