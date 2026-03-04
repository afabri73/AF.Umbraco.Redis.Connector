# Configuration

## Overview

The package supports two configuration section formats:

- current: `AF.Umbraco.Redis.Connector`
- legacy: `AF:Umbraco:Redis:Connector`

If both are present, the current section wins on overlapping values.

## Required settings

### Connection string

At minimum, define one Redis connection string under `ConnectionStrings`.

Default key expected by package:

- `ConnectionStrings:Redis`

Example:

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,user=admin,password=your-password,ssl=false,abortConnect=false"
  }
}
```

## Package options

### Enabled

- Type: `bool`
- Default: `true`
- Effect: enables/disables all package registrations

### ConnectionStringName

- Type: `string`
- Default: `Redis`
- Effect: selects the connection string key name under `ConnectionStrings`

### InstanceName

- Type: `string`
- Default: `AF.Umbraco.Redis.Connector:`
- Effect: distributed-cache key prefix and Data Protection application name

### DataProtectionKeysName

- Type: `string`
- Default: `AF.Umbraco.Redis.Connector:_DataProtectionKeys`
- Effect: enables Data Protection key persistence on Redis when non-empty (enabled by default)

### ValidateOnStartup

- Type: `bool`
- Default: `true`
- Effect: blocks app startup if Redis validation fails

## Complete sample

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,user=admin,password=your-password,ssl=false,abortConnect=false"
  },
  "AF.Umbraco.Redis.Connector": {
    "Enabled": true,
    "ConnectionStringName": "Redis",
    "InstanceName": "AF.Umbraco.Redis.Connector:15:",
    "DataProtectionKeysName": "AF.Umbraco.Redis.Connector:15:_DataProtectionKeys",
    "ValidateOnStartup": true
  }
}
```

## ACL notes for Redis 6+

When Redis ACL is enabled and `default` user is off, include explicit `user=...` in connection string.

Symptoms of wrong credentials:

- `NOAUTH Authentication required`
- startup fail-fast exceptions from the package hosted service
