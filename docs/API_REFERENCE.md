# API Reference

## Package configuration model

Section (current):

- `AF.Umbraco.Redis.Connector`

Section (legacy fallback):

- `AF:Umbraco:Redis:Connector`

Options:

- `Enabled` (`bool`, default `true`)
- `ConnectionStringName` (`string`, default `Redis`)
- `InstanceName` (`string`, default `AF.Umbraco.Redis.Connector:`)
- `DataProtectionKeysName` (`string`, default `AF.Umbraco.Redis.Connector:_DataProtectionKeys`)
- `ValidateOnStartup` (`bool`, default `true`)

## Runtime wiring

`RedisConnectorComposer` registers:

- `IDistributedCache` via `AddStackExchangeRedisCache`
- `RedisStartupConnectivityHostedService`
- optional Data Protection Redis persistence
- optional smoke middleware when `AF_SMOKE_TESTS=1`

## Startup connectivity validation

`RedisStartupConnectivityHostedService` behavior:

1. reads package options
2. resolves `ConnectionStrings:<ConnectionStringName>`
3. parses `ConfigurationOptions`
4. opens a `ConnectionMultiplexer`
5. executes `PING`
6. logs success or throws blocking exception

## Data Protection key-ring persistence

If `DataProtectionKeysName` is non-empty (default is set) and Redis connection string is available, the package configures:

- `AddDataProtection()`
- `SetApplicationName(InstanceName)`
- `PersistKeysToStackExchangeRedis(() => db, DataProtectionKeysName)`

Storage shape on Redis:

- Key type: `list`
- Key name: value of `DataProtectionKeysName`
- Payload items: XML key entries

## Smoke endpoints

Enabled only when environment variable `AF_SMOKE_TESTS=1` is present.

- `GET /smoke/health`
  - returns: `{ "status": "ok" }`
- `GET /smoke/redis/ping`
  - writes and reads a temporary cache key using `IDistributedCache`
  - returns status + key used for test
