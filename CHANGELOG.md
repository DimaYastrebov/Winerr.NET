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
