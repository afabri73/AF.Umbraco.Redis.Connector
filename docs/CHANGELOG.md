# Changelog

## 1.1.1

- Changed `DataProtectionKeysName` default value to `AF.Umbraco.Redis.Connector:_DataProtectionKeys`.
- Updated runtime behavior documentation to reflect default-enabled Data Protection Redis key-ring persistence.
- Updated README and API/configuration documentation accordingly.

## 1.1.0

- Added support for `DataProtectionKeysName` option in package configuration.
- Added Redis-backed Data Protection key-ring persistence using `PersistKeysToStackExchangeRedis`.
- Added explicit support for current + legacy configuration section merge with precedence on current section.
- Hardened and expanded package XML documentation (English, detailed API-level comments).
- Updated technical documentation set and README to reflect runtime behavior and operational checks.

## 1.0.0

- Initial release scaffold for `AF.Umbraco.Redis.Connector`.
- Added Umbraco multi-host setup (`15.x`, `16.x`, `17.x`).
- Added Redis distributed cache auto-composition.
- Added startup fail-fast connectivity validation.
- Added optional smoke middleware endpoints.
