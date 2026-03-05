using System.ComponentModel.DataAnnotations;

namespace Todo.Core.Configuration;

/// <summary>
/// Strongly-typed configuration for Azure SQL Database. Binds from AzureSql section or connection string env.
/// </summary>
public class AzureSqlSettings
{
    public const string SectionName = "AzureSql";

    /// <summary>Full connection string. When set, other properties are optional overrides.</summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Database name (used when building connection string from components).</summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>Command timeout in seconds.</summary>
    [Range(1, 3600)]
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>Enable retry on transient failures.</summary>
    public bool EnableRetryOnFailure { get; set; } = true;

    /// <summary>Max retry count when EnableRetryOnFailure is true.</summary>
    [Range(1, 10)]
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>Max connection pool size (0 = default).</summary>
    public int MaxPoolSize { get; set; }

    /// <summary>Connection timeout in seconds.</summary>
    public int ConnectionTimeout { get; set; } = 30;

    /// <summary>
    /// Returns the connection string to use. Prefers ConnectionString if non-empty; otherwise callers use components.
    /// </summary>
    public string GetConnectionString() => ConnectionString;
}
