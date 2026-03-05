using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Todo.Core.Configuration;

namespace Todo.Core.Infrastructure;

/// <summary>
/// Ensures Cosmos DB database and core containers exist on startup (when enabled).
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Creates the Cosmos DB database and AssetInventory, LicenseAllocations, Events containers if they don't exist.
    /// Uses configured partition key /AssetID and container names from CosmosDbSettings.
    /// </summary>
    public static async Task EnsureDatabaseCreatedAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Todo.Core.Infrastructure.DatabaseInitializer");
        var client = app.Services.GetService<CosmosClient>();
        var settings = app.Services.GetService<IOptions<CosmosDbSettings>>()?.Value;

        if (client == null || settings == null || string.IsNullOrEmpty(settings.Endpoint) || string.IsNullOrEmpty(settings.DatabaseName))
        {
            logger.LogDebug("Cosmos DB not configured; skipping database initialization.");
            return;
        }

        try
        {
            var databaseResponse = await client.CreateDatabaseIfNotExistsAsync(settings.DatabaseName, cancellationToken: cancellationToken);
            var database = databaseResponse.Database;

            var containerNames = new[] { settings.ContainerNames.AssetInventory, settings.ContainerNames.LicenseAllocations, settings.ContainerNames.Events };
            foreach (var name in containerNames)
            {
                await database.CreateContainerIfNotExistsAsync(new ContainerProperties(name, "/AssetID"), cancellationToken: cancellationToken);
                logger.LogInformation("Cosmos container {ContainerName} ensured.", name);
            }

            logger.LogInformation("Cosmos DB database {DatabaseName} and containers initialized.", settings.DatabaseName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cosmos DB initialization failed for database {DatabaseName}.", settings.DatabaseName);
            throw;
        }
    }
}
