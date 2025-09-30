# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [[1.1.1]](https://github.com/DimaYastrebov/Winerr.NET/releases/tag/v1.1.1) - 2025-09-30
[Compare with v1.1.0](https://github.com/DimaYastrebov/Winerr.NET/compare/v1.1.0...v1.1.1)

### Added
- **WebUI:** Added a link button to the GitHub repository in the preview panel for easy access to the source code.

### Fixed
- **Core:** Fixed a critical `ArgumentOutOfRangeException` crash that occurred when generating an image with a very small `maxWidth`. The issue was caused by the calculated content width becoming negative or zero, which is invalid for the image constructor. ([#3](https://github.com/DimaYastrebov/Winerr.NET/issues/3))

### Components
- `Winerr.NET.Core`: v0.12.1.450
- `Winerr.NET.WebServer`: v0.5.2.93
- `Winerr.NET.Cli`: v0.4.8.135
- `Winerr.NET.Assets`: v0.7.0.184
-
## [[1.1.0]](https://github.com/DimaYastrebov/Winerr.NET/releases/tag/v1.1.0) - 2025-09-30
[Compare with v1.0.2](https://github.com/DimaYastrebov/Winerr.NET/compare/v1.0.2...v1.1.0)

### Added
- **WebUI:** Implemented drag-and-drop functionality for reordering error instances in batch mode.
- **WebUI:** Added instance numbering in the batch mode UI for better navigation.
- **WebUI:** The generation time is now displayed in the preview panel after a successful single image generation.
- **WebServer:** Batch-generated archives now include a `metadata.json` file containing generation time and source configuration for each image.

### Changed
- **Core:** Refactored the rendering pipeline to a two-phase process (measure then draw), eliminating redundant rendering operations and improving performance.
- **Core:** Replaced hardcoded asset name strings with a centralized `AssetKeys` static class to improve code maintainability and prevent typos.
- **WebServer:** Image files within batch archives are now named sequentially (e.g., `0.png`, `1.png`) instead of `image_0.png`.

### Fixed
- **WebUI:** Fixed a bug where dragging an error instance in batch mode would cause the UI layout to shift due to horizontal overflow.
- **WebUI/WebServer:** Corrected inconsistent naming in the `X-Usage-Details` API header (`GenerationTimeMs` was incorrectly using snake_case initially), ensuring proper **PascalCase** deserialization on the frontend.

### Components
- `Winerr.NET.Core`: v0.12.0.446
- `Winerr.NET.WebServer`: v0.5.2.93
- `Winerr.NET.Cli`: v0.4.8.135
- `Winerr.NET.Assets`: v0.7.0.184

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

- Added `Winerr.NET.WebServer` ([WebAPI](https://github.com/DimaYastrebov/Winerr.NET/issues/1)), a web server for working with Winerr.NET over the network, without using the terminal.

## [[0.9.8.389]](https://github.com/DimaYastrebov/Winerr.NET/releases/tag/0.9.8.389) - 2025-09-12

### Added
- Initial public release of the `Winerr.NET.Core` rendering library and CLI.
