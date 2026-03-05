using Microsoft.Azure.Cosmos;

namespace Todo.Core.Data.Cosmos;

/// <summary>
/// Abstracts Cosmos DB client and container access for dependency injection and testability.
/// </summary>
public interface ICosmosDbContext
{
    /// <summary>Database name from configuration.</summary>
    string DatabaseName { get; }

    /// <summary>Gets the Cosmos container for entity type T. Container name is derived from type name by default.</summary>
    Container GetContainer<T>();
}
