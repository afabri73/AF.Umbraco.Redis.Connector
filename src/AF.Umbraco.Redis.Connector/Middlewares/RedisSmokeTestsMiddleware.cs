using AF.Umbraco.Redis.Connector.Bootstrap;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AF.Umbraco.Redis.Connector.Middlewares;

/// <summary>
/// Exposes opt-in diagnostics endpoints used to validate basic package and Redis behavior.
/// </summary>
/// <remarks>
/// This middleware is only registered when environment variable <c>AF_SMOKE_TESTS=1</c> is set.
/// Non-matching requests are delegated to the next middleware in the pipeline.
/// </remarks>
public sealed class RedisSmokeTestsMiddleware : IMiddleware
{
    private const string LogPrefix = "[AFURC]";

    private readonly IDistributedCache _distributedCache;
    private readonly IOptions<RedisConnectorOptions> _connectorOptions;
    private readonly ILogger<RedisSmokeTestsMiddleware> _logger;

    /// <summary>
    /// Creates a new smoke-test middleware instance.
    /// </summary>
    /// <param name="distributedCache">Distributed cache abstraction configured to use Redis.</param>
    /// <param name="connectorOptions">Package options used to build smoke-test key names.</param>
    /// <param name="logger">Logger used to report smoke endpoint failures.</param>
    public RedisSmokeTestsMiddleware(
        IDistributedCache distributedCache,
        IOptions<RedisConnectorOptions> connectorOptions,
        ILogger<RedisSmokeTestsMiddleware> logger)
    {
        _distributedCache = distributedCache;
        _connectorOptions = connectorOptions;
        _logger = logger;
    }

    /// <summary>
    /// Handles smoke endpoints and delegates all other HTTP requests.
    /// </summary>
    /// <param name="context">Current HTTP request context.</param>
    /// <param name="next">Delegate to continue pipeline execution for non-smoke requests.</param>
    /// <returns>A task that completes when request processing finishes.</returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (HttpMethods.IsGet(context.Request.Method) && context.Request.Path == "/smoke/health")
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"status\":\"ok\"}");
            return;
        }

        if (HttpMethods.IsGet(context.Request.Method) && context.Request.Path == "/smoke/redis/ping")
        {
            try
            {
                string cacheKey = $"{_connectorOptions.Value.InstanceName}smoke:{Guid.NewGuid():N}";

                await _distributedCache.SetStringAsync(
                    cacheKey,
                    "pong",
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                    });

                string? value = await _distributedCache.GetStringAsync(cacheKey);
                bool success = string.Equals(value, "pong", StringComparison.Ordinal);

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    $"{{\"status\":\"ok\",\"redis\":{success.ToString().ToLowerInvariant()},\"key\":\"{cacheKey}\"}}");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{LogPrefix} Redis smoke ping failed.", LogPrefix);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Redis smoke ping failed.");
                return;
            }
        }

        await next(context);
    }
}
