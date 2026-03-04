namespace AF.Umbraco.Redis.Connector.Bootstrap;

/// <summary>
/// Defines all runtime options for <c>AF.Umbraco.Redis.Connector</c>.
/// </summary>
/// <remarks>
/// The options in this type are loaded from configuration and consumed by the composer,
/// the startup connectivity validation hosted service, and the optional smoke middleware.
/// </remarks>
public sealed class RedisConnectorOptions
{
    /// <summary>
    /// Current configuration section path used by the package.
    /// </summary>
    /// <remarks>
    /// Example key: <c>AF.Umbraco.Redis.Connector:Enabled</c>.
    /// </remarks>
    public const string SectionName = "AF.Umbraco.Redis.Connector";

    /// <summary>
    /// Legacy configuration section path preserved for backward compatibility.
    /// </summary>
    /// <remarks>
    /// Example key: <c>AF:Umbraco:Redis:Connector:Enabled</c>.
    /// </remarks>
    public const string LegacySectionName = "AF:Umbraco:Redis:Connector";

    /// <summary>
    /// Enables or disables the package behavior entirely.
    /// </summary>
    /// <remarks>
    /// When set to <see langword="false"/>, the composer exits early and does not register:
    /// Redis distributed cache, Data Protection persistence on Redis, startup validation,
    /// or the optional smoke middleware.
    /// </remarks>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Name of the connection string under <c>ConnectionStrings</c>.
    /// </summary>
    /// <remarks>
    /// Default value <c>Redis</c> maps to <c>ConnectionStrings:Redis</c>.
    /// </remarks>
    public string ConnectionStringName { get; set; } = "Redis";

    /// <summary>
    /// Prefix applied to distributed cache keys managed by this package.
    /// </summary>
    /// <remarks>
    /// Use a distinct prefix per application/environment to avoid key collisions
    /// when multiple apps share the same Redis instance.
    /// </remarks>
    public string InstanceName { get; set; } = "AF.Umbraco.Redis.Connector:";

    /// <summary>
    /// Redis key name used to persist the ASP.NET Core Data Protection key-ring.
    /// </summary>
    /// <remarks>
    /// Default value is <c>AF.Umbraco.Redis.Connector:_DataProtectionKeys</c>.
    /// When this value is not empty, Data Protection keys are stored in Redis through
    /// <c>PersistKeysToStackExchangeRedis</c>. The provider stores the key-ring as a Redis list.
    /// </remarks>
    public string DataProtectionKeysName { get; set; } = "AF.Umbraco.Redis.Connector:_DataProtectionKeys";

    /// <summary>
    /// Enables startup fail-fast Redis connectivity validation.
    /// </summary>
    /// <remarks>
    /// When enabled, missing connection string or Redis connection/authentication failures
    /// stop application startup to avoid partially working environments.
    /// </remarks>
    public bool ValidateOnStartup { get; set; } = true;
}
