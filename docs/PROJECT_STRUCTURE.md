# Project Structure

## Root

- `.github/workflows`
  - CI workflows for build/package and secret scan
- `.vscode`
  - shared local dev settings and tasks
- `README.md`
  - package-level quick reference and usage guide
- `docs/`
  - technical documentation set
- `scripts/`
  - helper scripts (smoke and automation)
- `brand/`
  - package branding assets
- `umbraco-marketplace.json`
  - marketplace metadata

## Source

- `src/AF.Umbraco.Redis.Connector`
  - package code and NuGet project metadata
- `src/Umbraco.Cms.15.x`
  - host compatibility target for Umbraco 15
- `src/Umbraco.Cms.16.x`
  - host compatibility target for Umbraco 16
- `src/Umbraco.Cms.17.x`
  - host compatibility target for Umbraco 17
- `src/Directory.Build.targets`
  - injects package `ProjectReference` into all hosts

## Package internals (`src/AF.Umbraco.Redis.Connector`)

- `Bootstrap/`
  - options model + startup validation hosted service
- `Composers/`
  - Umbraco DI/service registration orchestration
- `Middlewares/`
  - optional diagnostic endpoints
- `PackageMarker.cs`
  - marker type for assembly anchoring
