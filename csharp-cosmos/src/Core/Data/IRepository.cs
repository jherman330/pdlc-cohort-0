using Todo.Core.Common.Entities;

namespace Todo.Core.Data;

/// <summary>
/// Generic repository contract for CRUD and query operations. Implemented by Cosmos and SQL repositories.
/// </summary>
/// <typeparam name="T">Entity type; must inherit from BaseEntity.</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>Gets an entity by id, or null if not found.</summary>
    Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Gets all entities. Use Query() for filtered/paged results.</summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates an entity and returns the persisted instance with timestamps set.</summary>
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing entity. Returns the updated instance.</summary>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Deletes an entity by id. Throws if not found.</summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Returns a queryable for advanced filtering and paging. Implementation-specific.</summary>
    IQueryable<T> Query();
}
