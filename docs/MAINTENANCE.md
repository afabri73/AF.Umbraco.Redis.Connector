# Maintenance

## Dependency governance

- regularly check for deprecated packages:
  - `dotnet list <project> package --deprecated`
- track vulnerability advisories from restore/build output
- keep Umbraco references aligned with supported 15/16/17 range

## Configuration governance

- keep current section `AF.Umbraco.Redis.Connector` as source of truth
- keep legacy section support until explicit deprecation/removal plan
- enforce explicit Redis ACL credentials in non-trivial environments

## Runtime governance

- keep `ValidateOnStartup=true` in production-like environments
- use environment-specific `InstanceName` and `DataProtectionKeysName`
- monitor logs for `[AFURC]` startup validation messages

## Documentation governance

After each functional change:

1. update XMLDoc in C# source (detailed, maintainability-oriented)
2. update `docs/*` technical documentation
3. update root `README.md`
4. validate examples against current code and host configuration
