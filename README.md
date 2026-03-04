# AF.Umbraco.Redis.Connector

Redis connector package for Umbraco 15/16/17 on .NET 9/10.

This package wires Redis into Umbraco hosts through auto-composition, without requiring manual `Program.cs` edits.
It provides:

- `IDistributedCache` backed by StackExchange Redis
- Startup fail-fast Redis connectivity validation
- Redis-backed Data Protection key-ring persistence (enabled by default, overridable)
- Optional smoke endpoints for runtime diagnostics (`AF_SMOKE_TESTS=1`)

## Compatibility

- Umbraco CMS: `15.x`, `16.x`, `17.x`
- .NET: `9.0`, `10.0`

## Dependencies

- `Umbraco.Cms.Web.Common`
- `Microsoft.Extensions.Caching.StackExchangeRedis`
- `Microsoft.AspNetCore.DataProtection.StackExchangeRedis`
- `StackExchange.Redis`

## Test Hosts

Local compatibility hosts are included in:

- `src/Umbraco.Cms.15.x`
- `src/Umbraco.Cms.16.x`
- `src/Umbraco.Cms.17.x`

Each host supports local overrides via `appsettings.Local.json`.

## Installation

```bash
dotnet add package AF.Umbraco.Redis.Connector
```

## Configuration

### Current package section (recommended)

- `AF.Umbraco.Redis.Connector`

### Legacy package section (supported fallback)

- `AF:Umbraco:Redis:Connector`

The current section takes precedence when both are present.

### Required connection string

- `ConnectionStrings:Redis` (or the key selected by `ConnectionStringName`)

### Recommended Redis ACL format

```json
"ConnectionStrings": {
  "Redis": "localhost:6379,user=admin,password=your-password,ssl=false,abortConnect=false"
}
```

### Package options

- `Enabled` (default `true`)
- `ConnectionStringName` (default `Redis`)
- `InstanceName` (default `AF.Umbraco.Redis.Connector:`)
- `DataProtectionKeysName` (default `AF.Umbraco.Redis.Connector:_DataProtectionKeys`)
- `ValidateOnStartup` (default `true`)

### Full example

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,user=admin,password=your-password,ssl=false,abortConnect=false"
  },
  "AF.Umbraco.Redis.Connector": {
    "Enabled": true,
    "ConnectionStringName": "Redis",
    "InstanceName": "AF.Umbraco.Redis.Connector:17:",
    "DataProtectionKeysName": "AF.Umbraco.Redis.Connector:17:_DataProtectionKeys",
    "ValidateOnStartup": true
  }
}
```

## Data Protection on Redis

When `DataProtectionKeysName` has a non-empty value (default is enabled), the package enables:

- `AddDataProtection()`
- `SetApplicationName(InstanceName)`
- `PersistKeysToStackExchangeRedis(..., DataProtectionKeysName)`

The key-ring is stored as a Redis list (array-like structure), where each item is an XML key entry.

## Startup Validation Behavior

If `ValidateOnStartup=true`, startup is blocked when:

- required Redis connection string is missing
- Redis is unreachable
- Redis authentication fails

This prevents running Umbraco with broken distributed-cache infrastructure.

## Smoke Endpoints (opt-in)

Enable:

```bash
AF_SMOKE_TESTS=1
```

Endpoints:

- `GET /smoke/health`
- `GET /smoke/redis/ping`

## Build and Pack

```bash
dotnet restore src/AF.Umbraco.Redis.Connector.sln
dotnet build src/AF.Umbraco.Redis.Connector.sln -c Debug
dotnet build src/AF.Umbraco.Redis.Connector/AF.Umbraco.Redis.Connector.csproj -c Release
dotnet pack src/AF.Umbraco.Redis.Connector/AF.Umbraco.Redis.Connector.csproj -c Release --no-build
```

## Documentation

- `docs/API_REFERENCE.md`
- `docs/ARCHITECTURE.md`
- `docs/CHANGELOG.md`
- `docs/CONFIGURATION.md`
- `docs/DEVELOPMENT.md`
- `docs/MAINTENANCE.md`
- `docs/PROJECT_STRUCTURE.md`
