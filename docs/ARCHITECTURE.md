# Architecture

## Design goals

The package is intentionally focused on infrastructure integration, with predictable startup behavior:

- minimal host-code footprint (no explicit `Program.cs` package registration)
- explicit and deterministic Redis dependency checks
- backward-compatible configuration model migration
- operational visibility through opt-in smoke endpoints

## Module layout

- `Composers/RedisConnectorComposer.cs`
  - service registration and feature orchestration
- `Bootstrap/RedisConnectorOptions.cs`
  - strongly typed options contract
- `Bootstrap/RedisStartupConnectivityHostedService.cs`
  - fail-fast startup validation
- `Middlewares/RedisSmokeTestsMiddleware.cs`
  - runtime smoke diagnostics
- `PackageMarker.cs`
  - assembly anchor for tests/discovery

## Configuration flow

1. composer resolves current config section
2. composer applies legacy fallback merge
3. options are registered in DI
4. runtime reads options from `IOptions<RedisConnectorOptions>`

Precedence rule:

- current section `AF.Umbraco.Redis.Connector` overrides legacy values

## Runtime flow

1. Umbraco bootstraps and executes composers
2. Redis connector registers distributed cache and hosted service
3. startup hosted service validates Redis connectivity
4. optional Data Protection key-ring persistence is attached
5. optional smoke middleware is attached only when explicitly enabled

## Failure modes and expected behavior

- missing connection string -> startup blocked
- Redis network unreachable -> startup blocked
- Redis auth failure (`NOAUTH`/ACL mismatch) -> startup blocked
- smoke middleware disabled -> no diagnostic endpoints exposed

## Data Protection considerations

When enabled, Data Protection keys are shared via Redis list storage.

Default behavior:

- key-ring persistence is enabled by default through `DataProtectionKeysName = AF.Umbraco.Redis.Connector:_DataProtectionKeys`
- it can be explicitly disabled by setting `DataProtectionKeysName` to an empty value

Operational implications:

- improved key sharing across restarts and multiple app instances
- key-ring may be stored unencrypted at rest unless an XML encryptor is configured externally
- key naming should be isolated per environment/application
