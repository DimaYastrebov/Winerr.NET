## [0.11.0] - 2025-09-19

### Added

-   Added new endpoints for asset introspection:
    -   `GET /v1/fonts`
    -   `GET /v1/styles/{styleId}/assets`
    -   `GET /v1/assets`

### Changed

-   Refactored `AssetManager` to be provided via Dependency Injection in the WebServer.

### Components

-   `Winerr.NET.Core`: v0.11.0
-   `Winerr.NET.WebServer`: v0.5.0
-   `Winerr.NET.Cli`: v0.4.8
-   `Winerr.NET.Assets`: v0.7.0

## [1.0.0] - 2025-09-22

### Added

-   Implemented a full-featured WebUI for visual image creation, including single/batch modes, config import/export, and a button constructor with manual drag-n-drop sorting (available when auto-sort is disabled).
-   Added a WebServer with an API to support the WebUI: image generation, fetching details on styles, icons, and assets.

### Changed

-   The health check endpoint was moved from `/health` to `/v1/health` for API consistency.

### Components

-   `Winerr.NET.Core`: v0.11.3.427
-   `Winerr.NET.WebServer`: v0.5.1.86
-   `Winerr.NET.Cli`: v0.4.8.135
-   `Winerr.NET.Assets`: v0.7.0.184

## [1.0.1] - 2025-09-25

### Fixed

-   Fixed numerous TypeScript and ESLint errors in the WebUI that were causing the production build (`npm run build`) to fail.
-   Resolved a potential null reference warning during text measurement in the Core library.

### Components

-   `Winerr.NET.Core`: v0.11.3.428
-   `Winerr.NET.WebServer`: v0.5.1.86
-   `Winerr.NET.Cli`: v0.4.8.135
-   `Winerr.NET.Assets`: v0.7.0.184

## [1.0.2] - 2025-09-25

### Fixed

-   Fixed a critical issue in the WebUI that caused an infinite loop of API requests when updating component state, leading to excessive network traffic and potential browser crashes.

### Components

-   `Winerr.NET.Core`: v0.11.3.428
-   `Winerr.NET.WebServer`: v0.5.1.86
-   `Winerr.NET.Cli`: v0.4.8.135
-   `Winerr.NET.Assets`: v0.7.0.184
