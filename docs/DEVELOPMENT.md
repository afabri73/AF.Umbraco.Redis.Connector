# Development Guide

## Prerequisites

- .NET SDK 10.x (compatible with `net9.0` and `net10.0` targets)
- Redis instance reachable by local hosts
- SQL Server reachable by local Umbraco hosts

## Project layout

- package source: `src/AF.Umbraco.Redis.Connector`
- host apps: `src/Umbraco.Cms.15.x`, `src/Umbraco.Cms.16.x`, `src/Umbraco.Cms.17.x`
- shared project reference injection: `src/Directory.Build.targets`

## Local restore/build workflow

```bash
dotnet restore src/AF.Umbraco.Redis.Connector.sln
dotnet build src/AF.Umbraco.Redis.Connector.sln -c Debug
```

## Package build and pack

```bash
dotnet build src/AF.Umbraco.Redis.Connector/AF.Umbraco.Redis.Connector.csproj -c Release
dotnet pack src/AF.Umbraco.Redis.Connector/AF.Umbraco.Redis.Connector.csproj -c Release --no-build
```

## Host sync verification strategy

Because host projects reference the package project through `Directory.Build.targets`, a host build should always copy the current connector DLL into each host output directory.

Suggested validation:

1. build solution
2. compare package DLL hash with each host output DLL hash for matching target framework
3. investigate mismatch as stale output/sync issue

## Smoke checks

Enable smoke endpoints before running host:

```bash
AF_SMOKE_TESTS=1 dotnet run --project src/Umbraco.Cms.17.x
```

Then verify:

- `/smoke/health`
- `/smoke/redis/ping`
