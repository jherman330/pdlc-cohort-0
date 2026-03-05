namespace Todo.Core.Configuration;

/// <summary>
/// Strongly-typed configuration for Cosmos DB. Binds from CosmosDb section or AZURE_COSMOS_* env vars.
/// </summary>
public class CosmosDbSettings
{
    public const string SectionName = "CosmosDb";

    /// <summary>Cosmos DB account endpoint (e.g. from AZURE_COSMOS_ENDPOINT).</summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>Database name (e.g. from AZURE_COSMOS_DATABASE_NAME).</summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>Container names for core collections.</summary>
    public CosmosDbContainerNames ContainerNames { get; set; } = new();

    /// <summary>Default TTL in seconds; -1 or null for no TTL.</summary>
    public int? DefaultTtlSeconds { get; set; }
}

/// <summary>
/// Container name mapping for Cosmos DB collections.
/// </summary>
public class CosmosDbContainerNames
{
    public string AssetInventory { get; set; } = "AssetInventory";
    public string LicenseAllocations { get; set; } = "LicenseAllocations";
    public string Events { get; set; } = "Events";
}
