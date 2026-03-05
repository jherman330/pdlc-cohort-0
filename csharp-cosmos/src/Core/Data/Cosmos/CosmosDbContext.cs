using System.Collections.Concurrent;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Todo.Core.Configuration;

namespace Todo.Core.Data.Cosmos;

/// <summary>
/// Cosmos DB context: provides container access with caching and connection settings from configuration.
/// </summary>
public sealed class CosmosDbContext : ICosmosDbContext, IDisposable
{
    private readonly CosmosClient _client;
    private readonly CosmosDbSettings _settings;
    private readonly ConcurrentDictionary<string, Container> _containerCache = new();
    private Database? _database;

    public CosmosDbContext(CosmosClient client, IOptions<CosmosDbSettings> settings)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        DatabaseName = _settings.DatabaseName;
    }

    public string DatabaseName { get; }

    /// <inheritdoc />
    public Container GetContainer<T>()
    {
        var name = typeof(T).Name;
        return _containerCache.GetOrAdd(name, containerName =>
        {
            _database ??= _client.GetDatabase(_settings.DatabaseName);
            return _database.GetContainer(containerName);
        });
    }

    /// <summary>No-op; CosmosClient is owned by DI and not disposed here.</summary>
    public void Dispose() { }
}
