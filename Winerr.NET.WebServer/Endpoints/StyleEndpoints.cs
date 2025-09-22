using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Winerr.NET.Core.Enums;
using Winerr.NET.Core.Managers;
using Winerr.NET.WebServer.Helpers;
using Winerr.NET.WebServer.Models;

namespace Winerr.NET.WebServer.Endpoints
{
    public record StyleShortDto(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("display_name")] string DisplayName
    );

    public record StyleDetailsDto
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = string.Empty;

        [JsonPropertyName("display_name")]
        public string DisplayName { get; init; } = string.Empty;

        [JsonPropertyName("max_icon_id")]
        public int MaxIconId { get; init; }

        [JsonPropertyName("metrics")]
        public StyleMetricsDto Metrics { get; init; } = new();
    }

    public record StyleMetricsDto
    {
        [JsonPropertyName("expected_icon_size")]
        public SizeDto ExpectedIconSize { get; init; } = new(0, 0);

        [JsonPropertyName("min_button_width")]
        public int MinButtonWidth { get; init; }

        [JsonPropertyName("button_height")]
        public int ButtonHeight { get; init; }

        [JsonPropertyName("button_spacing")]
        public int ButtonSpacing { get; init; }

        [JsonPropertyName("button_sort_order")]
        public IEnumerable<string> ButtonSortOrder { get; init; } = Enumerable.Empty<string>();

        [JsonPropertyName("supported_button_types")]
        public IEnumerable<string> SupportedButtonTypes { get; init; } = Enumerable.Empty<string>();

        [JsonPropertyName("buttons_padding_left")]
        public int ButtonsPaddingLeft { get; init; }

        [JsonPropertyName("buttons_padding_right")]
        public int ButtonsPaddingRight { get; init; }

        [JsonPropertyName("line_spacing")]
        public float LineSpacing { get; init; }
    }

    public record StyleAssetsDto(
        [property: JsonPropertyName("frame_parts")] Dictionary<string, string> FrameParts,
        [property: JsonPropertyName("buttons")] Dictionary<string, string> Buttons
    );

    public static class StyleEndpoints
    {
        public static IEndpointRouteBuilder MapStyleEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/v1/styles", () =>
            {
                var stopwatch = Stopwatch.StartNew();

                var styles = SystemStyle.List()
                    .Where(s => s != null)
                    .Select(s => new StyleShortDto(s!.Id, s.DisplayName));

                var response = ApiResponseFactory.CreateSuccess(styles, "style.list", stopwatch);
                return Results.Ok(response);
            })
            .WithName("GetStyles")
            .WithDescription("Returns a list of all available system styles.")
            .WithOpenApi();

            app.MapGet("/v1/styles/{styleId}", (string styleId, AssetManager am) =>
            {
                var stopwatch = Stopwatch.StartNew();

                return EndpointHelpers.ExecuteWithStyleCheck(styleId, stopwatch, style =>
                {
                    var metrics = style.Metrics;
                    var styleName = style.Id.Split('_')[0];
                    var maxIconId = am.GetMaxIconIdForStyle(styleName);

                    var detailsDto = new StyleDetailsDto
                    {
                        Id = style.Id,
                        DisplayName = style.DisplayName,
                        MaxIconId = maxIconId,
                        Metrics = new StyleMetricsDto
                        {
                            ExpectedIconSize = new SizeDto(metrics.ExpectedIconSize.Width, metrics.ExpectedIconSize.Height),
                            MinButtonWidth = metrics.MinButtonWidth,
                            ButtonHeight = metrics.ButtonHeight,
                            ButtonSpacing = metrics.ButtonSpacing,
                            ButtonSortOrder = metrics.ButtonSortOrder.Select(bt => bt.DisplayName),
                            SupportedButtonTypes = metrics.ButtonTypeMetrics.Keys.Select(bt => bt.DisplayName),
                            ButtonsPaddingLeft = metrics.ButtonsPaddingLeft,
                            ButtonsPaddingRight = metrics.ButtonsPaddingRight,
                            LineSpacing = metrics.LineSpacing
                        }
                    };

                    var successResponse = ApiResponseFactory.CreateSuccess(detailsDto, "style.details", stopwatch);
                    return Results.Ok(successResponse);
                });
            })
            .WithName("GetStyleDetails")
            .WithDescription("Returns detailed information about a specific system style.")
            .WithOpenApi();

            app.MapGet("/v1/styles/{styleId}/assets", (string styleId, AssetManager am) =>
            {
                var stopwatch = Stopwatch.StartNew();

                return EndpointHelpers.ExecuteWithStyleCheck(styleId, stopwatch, style =>
                {
                    var assetPaths = am.GetStyleAssetPaths(style);

                    Func<string, string> toUrl = (resourcePath) =>
                    {
                        var uri = "/v1/assets";
                        var queryParams = new Dictionary<string, string> { { "path", resourcePath } };
                        return QueryHelpers.AddQueryString(uri, queryParams!);
                    };

                    var responseDto = new StyleAssetsDto(
                        FrameParts: assetPaths.GetValueOrDefault("frame_parts", new())
                                              .ToDictionary(kvp => kvp.Key, kvp => toUrl(kvp.Value)),
                        Buttons: assetPaths.GetValueOrDefault("buttons", new())
                                           .ToDictionary(kvp => kvp.Key, kvp => toUrl(kvp.Value))
                    );

                    var successResponse = ApiResponseFactory.CreateSuccess(responseDto, "style.assets", stopwatch);
                    return Results.Ok(successResponse);
                });
            })
            .WithName("GetStyleAssets")
            .WithDescription("Returns a map of asset names to their respective URLs for a given style.")
            .WithOpenApi();

            return app;
        }
    }
}
