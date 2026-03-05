using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Todo.Core.Configuration;
using Todo.Core.Services;

namespace Todo.Core.Infrastructure;

/// <summary>
/// Service registration extensions for dependency injection configuration.
/// Follows the Backend blueprint feature-module pattern for Services, Repositories, Validators.
/// </summary>
public static class StartupExtensions
{
    /// <summary>
    /// Adds core infrastructure services: Cosmos DB client, Redis cache, idempotency service, and shared registrations.
    /// Call from the API host during startup.
    /// </summary>
    public static IServiceCollection AddCoreInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCosmosDb(configuration);
        services.AddAzureSql(configuration);
        services.AddRedisCache(configuration);
        services.AddIdempotencyService(configuration);
        return services;
    }

    /// <summary>
    /// Configures IDistributedCache with Redis when AZURE_REDIS_CONNECTION_STRING (or Redis:ConnectionString) is set;
    /// otherwise uses in-memory distributed cache so idempotency service can resolve.
    /// </summary>
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["AZURE_REDIS_CONNECTION_STRING"]
            ?? configuration["Redis:ConnectionString"];
        if (string.IsNullOrEmpty(connectionString))
        {
            services.AddDistributedMemoryCache();
            return services;
        }

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
        });
        return services;
    }

    /// <summary>
    /// Registers IdempotencyService and binds IdempotencyCache configuration.
    /// </summary>
    public static IServiceCollection AddIdempotencyService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IdempotencyOptions>(configuration.GetSection(IdempotencyOptions.SectionName));
        services.AddSingleton<IIdempotencyService, IdempotencyService>();
        return services;
    }

    /// <summary>
    /// Binds CosmosDbSettings from configuration (CosmosDb section and AZURE_COSMOS_* env vars),
    /// registers settings and the Cosmos DB client when endpoint is configured.
    /// </summary>
    public static IServiceCollection AddCosmosDb(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(CosmosDbSettings.SectionName);
        var settings = new CosmosDbSettings
        {
            Endpoint = section["Endpoint"] ?? configuration["AZURE_COSMOS_ENDPOINT"] ?? string.Empty,
            DatabaseName = section["DatabaseName"] ?? configuration["AZURE_COSMOS_DATABASE_NAME"] ?? string.Empty,
            ContainerNames = new CosmosDbContainerNames
            {
                AssetInventory = section["ContainerNames:AssetInventory"] ?? "AssetInventory",
                LicenseAllocations = section["ContainerNames:LicenseAllocations"] ?? "LicenseAllocations",
                Events = section["ContainerNames:Events"] ?? "Events"
            }
        };
        services.AddSingleton(settings);

        if (string.IsNullOrEmpty(settings.Endpoint))
        {
            return services;
        }

        var credential = new DefaultAzureCredential();
        var options = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            }
        };

        services.AddSingleton(_ => new CosmosClient(settings.Endpoint, credential, options));
        return services;
    }

    /// <summary>
    /// Registers ReferenceDataDbContext and AzureSqlSettings. Connection from AzureSql:ConnectionString or AZURE_SQL_CONNECTION_STRING.
    /// </summary>
    public static IServiceCollection AddAzureSql(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureSqlSettings>(configuration.GetSection(AzureSqlSettings.SectionName));

        var connectionString = configuration["AZURE_SQL_CONNECTION_STRING"]
            ?? configuration[$"{AzureSqlSettings.SectionName}:ConnectionString"];
        if (string.IsNullOrEmpty(connectionString))
        {
            return services;
        }

        var timeoutSeconds = configuration.GetValue($"{AzureSqlSettings.SectionName}:CommandTimeoutSeconds", 30);
        var enableRetry = configuration.GetValue($"{AzureSqlSettings.SectionName}:EnableRetryOnFailure", true);
        var maxRetryCount = configuration.GetValue($"{AzureSqlSettings.SectionName}:MaxRetryCount", 3);

        var builder = services.AddDbContext<ReferenceDataDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.CommandTimeout(timeoutSeconds);
                if (enableRetry)
                    sql.EnableRetryOnFailure(maxRetryCount);
            });
        });

        return services;
    }

    /// <summary>
    /// Placeholder for feature Services registration. Future features call e.g. services.AddAssetServices() from sibling extensions.
    /// </summary>
    public static IServiceCollection AddFeatureServices(this IServiceCollection services)
    {
        return services;
    }

    /// <summary>
    /// Placeholder for feature Repositories registration.
    /// </summary>
    public static IServiceCollection AddFeatureRepositories(this IServiceCollection services)
    {
        return services;
    }

    /// <summary>
    /// Placeholder for feature Validators registration.
    /// </summary>
    public static IServiceCollection AddFeatureValidators(this IServiceCollection services)
    {
        return services;
    }
}
