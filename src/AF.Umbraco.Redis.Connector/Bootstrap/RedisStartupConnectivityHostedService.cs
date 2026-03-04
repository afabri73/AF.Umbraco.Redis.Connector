using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace AF.Umbraco.Redis.Connector.Bootstrap;

/// <summary>
/// Validates Redis configuration and network/auth connectivity during application startup.
/// </summary>
/// <remarks>
/// This hosted service is intentionally fail-fast. If validation fails, startup is aborted and
/// Umbraco does not continue booting in an inconsistent runtime state.
/// </remarks>
internal sealed class RedisStartupConnectivityHostedService : IHostedService
{
    private const string LogPrefix = "[AFURC]";

    private readonly IConfiguration _configuration;
    private readonly IOptions<RedisConnectorOptions> _connectorOptions;
    private readonly ILogger<RedisStartupConnectivityHostedService> _logger;

    /// <summary>
    /// Creates a new startup connectivity validator for Redis.
    /// </summary>
    /// <param name="configuration">Application configuration used to resolve connection strings.</param>
    /// <param name="connectorOptions">Bound package options controlling validation behavior.</param>
    /// <param name="logger">Structured logger used for diagnostic and critical startup messages.</param>
    public RedisStartupConnectivityHostedService(
        IConfiguration configuration,
        IOptions<RedisConnectorOptions> connectorOptions,
        ILogger<RedisStartupConnectivityHostedService> logger)
    {
        _configuration = configuration;
        _connectorOptions = connectorOptions;
        _logger = logger;
    }

    /// <summary>
    /// Executes the Redis startup validation routine.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token propagated by the host startup pipeline.</param>
    /// <returns>A task that completes when validation succeeds or throws on failure.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the configured Redis connection string key cannot be resolved.
    /// </exception>
    /// <exception cref="RedisConnectionException">
    /// Thrown when Redis cannot be reached or authenticated.
    /// </exception>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        RedisConnectorOptions options = _connectorOptions.Value;

        if (!options.Enabled || !options.ValidateOnStartup)
        {
            _logger.LogInformation(
                "{LogPrefix} Redis startup validation is disabled. Enabled={Enabled}; ValidateOnStartup={ValidateOnStartup}.",
                LogPrefix,
                options.Enabled,
                options.ValidateOnStartup);
            return;
        }

        string connectionStringName = string.IsNullOrWhiteSpace(options.ConnectionStringName)
            ? "Redis"
            : options.ConnectionStringName;

        string? connectionString = _configuration.GetConnectionString(connectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            string message =
                $"{LogPrefix} Missing required connection string 'ConnectionStrings:{connectionStringName}'. Startup is blocked.";

            _logger.LogCritical(message);
            throw new InvalidOperationException(message);
        }

        try
        {
            ConfigurationOptions redisConfiguration = ConfigurationOptions.Parse(connectionString, true);
            redisConfiguration.AbortOnConnectFail = false;

            await using ConnectionMultiplexer connection = await ConnectionMultiplexer
                .ConnectAsync(redisConfiguration)
                .ConfigureAwait(false);

            TimeSpan ping = await connection.GetDatabase().PingAsync().ConfigureAwait(false);

            _logger.LogInformation(
                "{LogPrefix} Redis connectivity validation passed. Endpoint={Endpoint}; PingMs={PingMs}.",
                LogPrefix,
                string.Join(",", redisConfiguration.EndPoints.Select(endpoint => endpoint.ToString())),
                ping.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(
                ex,
                "{LogPrefix} Redis connectivity validation failed. Startup is blocked.",
                LogPrefix);
            throw;
        }
    }

    /// <summary>
    /// No-op stop hook required by <see cref="IHostedService"/>.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for host shutdown.</param>
    /// <returns>A completed task.</returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
