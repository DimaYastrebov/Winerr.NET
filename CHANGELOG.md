# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [[1.0.2]](https://github.com/DimaYastrebov/Winerr.NET/releases/tag/v1.0.2) - 2025-09-25
[Compare with v1.0.1](https://github.com/DimaYastrebov/Winerr.NET/compare/v1.0.1...v1.0.2)

### Fixed
- Fixed a critical issue in the WebUI that caused an infinite loop of API requests when updating component state, leading to excessive network traffic and potential browser crashes.

### Components
- `Winerr.NET.Core`: v0.11.3.428
- `Winerr.NET.WebServer`: v0.5.1.86
- `Winerr.NET.Cli`: v0.4.8.135
- `Winerr.NET.Assets`: v0.7.0.184

## [[1.0.1]](https://github.com/DimaYastrebov/Winerr.NET/releases/tag/v1.0.1) - 2025-09-25
[Compare with v1.0.0](https://github.com/DimaYastrebov/Winerr.NET/compare/v1.0.0...v1.0.1)

### Fixed
- Fixed numerous TypeScript and ESLint errors in the WebUI that were causing the production build (`npm run build`) to fail.
- Resolved a potential null reference warning during text measurement in the Core library.

### Components
- `Winerr.NET.Core`: v0.11.3.428
- `Winerr.NET.WebServer`: v0.5.1.86
- `Winerr.NET.Cli`: v0.4.8.135
- `Winerr.NET.Assets`: v0.7.0.184

## [[1.0.0]](https://github.com/DimaYastrebov/Winerr.NET/releases/tag/v1.0.0) - 2025-09-22
[Compare with v0.11.0](https://github.com/DimaYastrebov/Winerr.NET/compare/v0.11.0...v1.0.0)

### Added
- Implemented a full-featured WebUI for visual image creation, including single/batch modes, config import/export, and a button constructor with manual drag-n-drop sorting (available when auto-sort is disabled).
- Added a WebServer with an API to support the WebUI: image generation, fetching details on styles, icons, and assets.

### Changed
- The health check endpoint was moved from `/health` to `/v1/health` for API consistency.

### Components
- `Winerr.NET.Core`: v0.11.3.427
- `Winerr.NET.WebServer`: v0.5.1.86
- `Winerr.NET.Cli`: v0.4.8.135
- `Winerr.NET.Assets`: v0.7.0.184

## [[0.11.0]](https://github.com/DimaYastrebov/Winerr.NET/releases/tag/v0.11.0) - 2025-09-20
[Compare with v0.10.1](https://github.com/DimaYastrebov/Winerr.NET/compare/v0.10.1...v0.11.0)

### Added
- Added new endpoints for asset introspection:
    - `GET /v1/fonts`
    - `GET /v1/styles/{styleId}/assets`
    - `GET /v1/assets`

### Changed
- Refactored `AssetManager` to be provided via Dependency Injection in the WebServer.

### Components
- `Winerr.NET.Core`: v0.11.0
- `Winerr.NET.WebServer`: v0.5.0
- `Winerr.NET.Cli`: v0.4.8
- `Winerr.NET.Assets`: v0.7.0

## [[0.10.1]](https://github.com/DimaYastrebov/Winerr.NET/releases/tag/v0.10.1) - 2025-09-18
[Compare with v0.10.0](https://github.com/DimaYastrebov/Winerr.NET/compare/v0.10.0...v0.10.1)


## [[0.10.0]](https://github.com/DimaYastrebov/Winerr.NET/releases/tag/v0.10.0) - 2025-09-18
[Compare with v0.9.8.389](https://github.com/DimaYastrebov/Winerr.NET/compare/v0.9.8.389...v0.10.0)

### Added
- Implemented the Command-Line Interface (CLI) for generating images from the terminal.
- Added `list-styles` command to view all available styles.

## [[0.9.8.389]](https://github.com/DimaYastrebov/Winerr.NET/releases/tag/0.9.8.389) - 2025-09-12

### Added
- Initial public release of the `Winerr.NET.Core` rendering library and CLI.
