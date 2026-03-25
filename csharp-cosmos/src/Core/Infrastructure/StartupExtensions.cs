using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Todo.Core.Configuration;
using Todo.Core.Data.Cosmos;
using Todo.Core.Data.Sql;
using Todo.Core.Security;
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
        services.Configure<PiiEncryptionSettings>(configuration.GetSection(PiiEncryptionSettings.SectionName));
        var piiKey = configuration[$"{PiiEncryptionSettings.SectionName}:EncryptionKeyBase64"];
        if (!string.IsNullOrWhiteSpace(piiKey))
            services.AddSingleton<IPiiFieldProtector, PiiFieldProtector>();

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
    /// registers settings, Cosmos DB client when endpoint is configured, and ICosmosDbContext.
    /// </summary>
    public static IServiceCollection AddCosmosDb(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(CosmosDbSettings.SectionName);
        services.Configure<CosmosDbSettings>(options =>
        {
            options.Endpoint = section["Endpoint"] ?? configuration["AZURE_COSMOS_ENDPOINT"] ?? string.Empty;
            options.DatabaseName = section["DatabaseName"] ?? configuration["AZURE_COSMOS_DATABASE_NAME"] ?? string.Empty;
            options.Key = section["Key"] ?? configuration["AZURE_COSMOS_KEY"] ?? string.Empty;
            options.MaxConnectionLimit = section.GetValue(nameof(CosmosDbSettings.MaxConnectionLimit), 50);
            options.RequestTimeout = section.GetValue(nameof(CosmosDbSettings.RequestTimeout), 60);
            options.ContainerNames = new CosmosDbContainerNames
            {
                AssetInventory = section["ContainerNames:AssetInventory"] ?? "AssetInventory",
                LicenseAllocations = section["ContainerNames:LicenseAllocations"] ?? "LicenseAllocations",
                Events = section["ContainerNames:Events"] ?? "Events"
            };
            options.EnableCertificatePinning = section.GetValue(nameof(CosmosDbSettings.EnableCertificatePinning), false);
            options.PinnedCertificateThumbprints = section.GetSection(nameof(CosmosDbSettings.PinnedCertificateThumbprints)).Get<string[]>()
                ?? Array.Empty<string>();
        });

        var endpoint = section["Endpoint"] ?? configuration["AZURE_COSMOS_ENDPOINT"] ?? string.Empty;
        if (!string.IsNullOrEmpty(endpoint))
        {
            services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CosmosDbSettings>>().Value;
                var options = new CosmosClientOptions
                {
                    SerializerOptions = new CosmosSerializationOptions
                    {
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    },
                    RequestTimeout = TimeSpan.FromSeconds(settings.RequestTimeout),
                    GatewayModeMaxConnectionLimit = settings.MaxConnectionLimit
                };

                if (settings.EnableCertificatePinning && settings.PinnedCertificateThumbprints is { Length: > 0 })
                {
                    var allowed = new HashSet<string>(settings.PinnedCertificateThumbprints, StringComparer.OrdinalIgnoreCase);
                    options.HttpClientFactory = () =>
                    {
                        var handler = new SocketsHttpHandler();
                        handler.SslOptions.RemoteCertificateValidationCallback = ( _, cert, _, _ ) =>
                        {
                            if (cert == null)
                                return false;
                            var thumb = cert.GetCertHashString();
                            return allowed.Contains(thumb);
                        };
                        return new HttpClient(handler);
                    };
                }

                if (!string.IsNullOrEmpty(settings.Key))
                    return new CosmosClient(settings.Endpoint, settings.Key, options);
                var credential = new DefaultAzureCredential();
                return new CosmosClient(settings.Endpoint, credential, options);
            });

            services.AddScoped<ICosmosDbContext>(sp =>
                new CosmosDbContext(
                    sp.GetRequiredService<CosmosClient>(),
                    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CosmosDbSettings>>()));
        }

        return services;
    }

    /// <summary>
    /// Registers ReferenceDataDbContext, SqlDbContext, and AzureSqlSettings. Connection from AzureSql:ConnectionString or AZURE_SQL_CONNECTION_STRING.
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

        var section = configuration.GetSection(AzureSqlSettings.SectionName);
        var timeoutSeconds = section.GetValue(nameof(AzureSqlSettings.CommandTimeoutSeconds), 30);
        var enableRetry = section.GetValue(nameof(AzureSqlSettings.EnableRetryOnFailure), true);
        var maxRetryCount = section.GetValue(nameof(AzureSqlSettings.MaxRetryCount), 3);
        var maxPoolSize = section.GetValue(nameof(AzureSqlSettings.MaxPoolSize), 0);
        var connectionTimeout = section.GetValue(nameof(AzureSqlSettings.ConnectionTimeout), 30);

        void SqlConfig(Microsoft.EntityFrameworkCore.Infrastructure.SqlServerDbContextOptionsBuilder sql)
        {
            sql.CommandTimeout(timeoutSeconds);
            if (enableRetry) sql.EnableRetryOnFailure(maxRetryCount);
        }

        var connBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString)
        {
            Encrypt = true,
            TrustServerCertificate = false,
        };
        if (maxPoolSize > 0) connBuilder.MaxPoolSize = maxPoolSize;
        if (connectionTimeout > 0) connBuilder.ConnectTimeout = connectionTimeout;
        var finalConnectionString = connBuilder.ConnectionString;

        services.AddSingleton<SqlSessionContextInterceptor>();
        services.AddDbContext<ReferenceDataDbContext>((sp, options) =>
        {
            options.UseSqlServer(finalConnectionString, SqlConfig)
                .AddInterceptors(sp.GetRequiredService<SqlSessionContextInterceptor>());
        });

        services.AddDbContext<SqlDbContext>((sp, options) =>
        {
            options.UseSqlServer(finalConnectionString, SqlConfig)
                .AddInterceptors(sp.GetRequiredService<SqlSessionContextInterceptor>());
        });

        services.AddScoped<ISqlDbContext>(sp => sp.GetRequiredService<SqlDbContext>());

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
    /// Placeholder for feature Repositories registration. Feature modules register IRepository{T} implementations here.
    /// ICosmosDbContext, CosmosDbContext, ISqlDbContext, and SqlDbContext are registered in AddCosmosDb and AddAzureSql.
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
