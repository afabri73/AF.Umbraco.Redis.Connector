using AF.Umbraco.Redis.Connector.Bootstrap;
using AF.Umbraco.Redis.Connector.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace AF.Umbraco.Redis.Connector.Composers;

/// <summary>
/// Umbraco composer that wires all package services required for Redis integration.
/// </summary>
/// <remarks>
/// Registration responsibilities:
/// - Bind package options from current and legacy configuration sections.
/// - Configure <c>IDistributedCache</c> using StackExchange Redis.
/// - Persist ASP.NET Core Data Protection keys on Redis when configured.
/// - Register startup fail-fast validation and optional smoke middleware.
/// </remarks>
public sealed class RedisConnectorComposer : IComposer
{
    /// <summary>
    /// Registers package services and optional middleware into Umbraco's dependency container.
    /// </summary>
    /// <param name="builder">Umbraco builder used during host bootstrapping.</param>
    public void Compose(IUmbracoBuilder builder)
    {
        IConfigurationSection activeSection = ResolveActiveConfigurationSection(builder.Config);
        IConfigurationSection legacySection = builder.Config.GetSection(RedisConnectorOptions.LegacySectionName);

        RedisConnectorOptions packageOptions = new();
        if (legacySection.Exists())
        {
            legacySection.Bind(packageOptions);
        }

        if (activeSection.Exists())
        {
            activeSection.Bind(packageOptions);
        }

        builder.Services
            .AddOptions<RedisConnectorOptions>()
            .Configure(options =>
            {
                if (legacySection.Exists())
                {
                    legacySection.Bind(options);
                }

                if (activeSection.Exists())
                {
                    activeSection.Bind(options);
                }
            });

        if (!packageOptions.Enabled)
        {
            return;
        }

        string connectionStringName = string.IsNullOrWhiteSpace(packageOptions.ConnectionStringName)
            ? "Redis"
            : packageOptions.ConnectionStringName;

        string? redisConnectionString = builder.Config.GetConnectionString(connectionStringName);

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = packageOptions.InstanceName;
        });

        if (!string.IsNullOrWhiteSpace(packageOptions.DataProtectionKeysName)
            && !string.IsNullOrWhiteSpace(redisConnectionString))
        {
            // Share Data Protection key-ring across restarts/instances through Redis storage.
            Lazy<IConnectionMultiplexer> dataProtectionConnection = new(() =>
            {
                ConfigurationOptions configuration = ConfigurationOptions.Parse(redisConnectionString, true);
                configuration.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(configuration);
            });

            builder.Services
                .AddDataProtection()
                .SetApplicationName(packageOptions.InstanceName)
                .PersistKeysToStackExchangeRedis(
                    () => dataProtectionConnection.Value.GetDatabase(),
                    packageOptions.DataProtectionKeysName);
        }

        builder.Services.AddHostedService<RedisStartupConnectivityHostedService>();

        if (Environment.GetEnvironmentVariable("AF_SMOKE_TESTS") == "1")
        {
            builder.Services.TryAddTransient<RedisSmokeTestsMiddleware>();
            builder.Services.Configure<UmbracoPipelineOptions>(options =>
            {
                options.AddFilter(new UmbracoPipelineFilter(
                    "RedisSmokeTests",
                    prePipeline: app => app.UseMiddleware<RedisSmokeTestsMiddleware>()));
            });
        }
    }

    /// <summary>
    /// Resolves the effective configuration section, preferring the current section format.
    /// </summary>
    /// <param name="configuration">Application configuration root.</param>
    /// <returns>
    /// The current section <c>AF.Umbraco.Redis.Connector</c> when present;
    /// otherwise the legacy section <c>AF:Umbraco:Redis:Connector</c>.
    /// </returns>
    private static IConfigurationSection ResolveActiveConfigurationSection(IConfiguration configuration)
    {
        IConfigurationSection current = configuration.GetSection(RedisConnectorOptions.SectionName);
        if (current.Exists())
        {
            return current;
        }

        return configuration.GetSection(RedisConnectorOptions.LegacySectionName);
    }
}
