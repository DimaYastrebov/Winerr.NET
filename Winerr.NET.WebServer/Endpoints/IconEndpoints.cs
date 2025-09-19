using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Winerr.NET.Core.Enums;
using Winerr.NET.Core.Managers;
using Winerr.NET.Core.Renderers;
using Winerr.NET.Core.Services;
using Winerr.NET.WebServer.Helpers;
using Winerr.NET.WebServer.Models;

namespace Winerr.NET.WebServer.Endpoints
{
    public record IconApi;

    public record IconMapDto(
        [property: JsonPropertyName("spritesheet_url")] string SpritesheetUrl,
        [property: JsonPropertyName("icon_size")] SizeDto IconSize,
        [property: JsonPropertyName("map")] Dictionary<string, PointDto> Map
    );

    public static class IconEndpoints
    {
        public static IEndpointRouteBuilder MapIconEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/v1/styles/{styleId}/icons/spritesheet", (
                string styleId,
                ILogger<IconApi> logger,
                IMemoryCache cache) =>
            {
                var stopwatch = Stopwatch.StartNew();
                string cacheKey = $"spritesheet_{styleId}";

                if (cache.TryGetValue(cacheKey, out byte[]? cachedImage))
                {
                    logger.LogInformation("Returning spritesheet for {StyleId} from cache.", styleId);
                    return Results.File(cachedImage!, "image/png", $"spritesheet_{styleId}.png");
                }

                return EndpointHelpers.ExecuteWithStyleCheck(styleId, stopwatch, style =>
                {
                    try
                    {
                        logger.LogInformation("Generating new spritesheet for {StyleId}...", styleId);

                        var styleName = style.Id.Split('_')[0];
                        var icons = AssetManager.Instance.GetAllIconsForStyle(styleName);

                        if (!icons.Any())
                        {
                            var emptyImageStream = new MemoryStream();
                            new Image<Rgba32>(1, 1).SaveAsPng(emptyImageStream);
                            emptyImageStream.Position = 0;
                            return Results.File(emptyImageStream, "image/png");
                        }

                        var spritesheetService = new SpritesheetService();
                        using var result = spritesheetService.Generate(icons, style.Metrics.ExpectedIconSize);

                        using var memoryStream = new MemoryStream();
                        result.Spritesheet.SaveAsPng(memoryStream);
                        var imageBytes = memoryStream.ToArray();

                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                        cache.Set(cacheKey, imageBytes, cacheEntryOptions);

                        return Results.File(imageBytes, "image/png", $"spritesheet_{styleId}.png");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to generate spritesheet for style {StyleId}", styleId);
                        return Results.Problem("An internal server error occurred while generating the spritesheet.");
                    }
                });
            })
            .WithName("GetIconSpritesheet")
            .WithDescription("Returns a PNG spritesheet of all icons for a given style.")
            .WithOpenApi();


            app.MapGet("/v1/styles/{styleId}/icons/map", (
                string styleId,
                ILogger<IconApi> logger,
                IMemoryCache cache) =>
            {
                var stopwatch = Stopwatch.StartNew();
                string cacheKey = $"iconmap_{styleId}";

                if (cache.TryGetValue(cacheKey, out object? cachedMap))
                {
                    logger.LogInformation("Returning icon map for {StyleId} from cache.", styleId);
                    var cachedDto = (IconMapDto)cachedMap!;
                    var cachedResponse = ApiResponseFactory.CreateSuccess(cachedDto, "icon.map", stopwatch);
                    return Results.Ok(cachedResponse);
                }

                return EndpointHelpers.ExecuteWithStyleCheck(styleId, stopwatch, style =>
                {
                    try
                    {
                        logger.LogInformation("Generating new icon map for {StyleId}...", styleId);

                        var styleName = style.Id.Split('_')[0];
                        var icons = AssetManager.Instance.GetAllIconsForStyle(styleName);

                        var spritesheetService = new SpritesheetService();
                        using var result = spritesheetService.Generate(icons, style.Metrics.ExpectedIconSize);

                        var responseDto = new IconMapDto(
                            SpritesheetUrl: $"/v1/styles/{styleId}/icons/spritesheet",
                            IconSize: new SizeDto(result.IconSize.Width, result.IconSize.Height),
                            Map: result.IconMap.ToDictionary(
                                kvp => kvp.Key.ToString(),
                                kvp => new PointDto(kvp.Value.X, kvp.Value.Y)
                            )
                        );

                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                        cache.Set(cacheKey, responseDto, cacheEntryOptions);

                        var successResponse = ApiResponseFactory.CreateSuccess(responseDto, "icon.map", stopwatch);
                        return Results.Ok(successResponse);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to generate icon map for style {StyleId}", styleId);
                        var errorResponse = ApiResponseFactory.CreateError("internal_server_error", "An error occurred while generating the icon map.", stopwatch);
                        return Results.Problem(detail: errorResponse.Error?.Message, statusCode: 500);
                    }
                });
            })
            .WithName("GetIconMap")
            .WithDescription("Returns a JSON map of icon coordinates for a given style's spritesheet.")
            .WithOpenApi();

            app.MapGet("/v1/styles/{styleId}/icons/{iconId:int}", (
                string styleId,
                int iconId,
                ILogger<IconApi> logger) =>
            {
                var stopwatch = Stopwatch.StartNew();

                return EndpointHelpers.ExecuteWithStyleCheck(styleId, stopwatch, style =>
                {
                    try
                    {
                        var iconRenderer = new IconRenderer();
                        using var iconResult = iconRenderer.DrawIcon(iconId, style, ignoreMissing: false, shrinkMode: IconShrinkMode.None);

                        using var memoryStream = new MemoryStream();
                        iconResult.Image.SaveAsPng(memoryStream);
                        memoryStream.Position = 0;

                        return Results.File(memoryStream.ToArray(), "image/png", $"{styleId}_{iconId}.png");
                    }
                    catch (FileNotFoundException ex)
                    {
                        logger.LogWarning(ex, "Icon not found for style {StyleId}, iconId {IconId}", styleId, iconId);
                        var errorResponse = ApiResponseFactory.CreateError("icon_not_found", $"Icon with id '{iconId}' was not found for style '{styleId}'.", stopwatch);
                        return Results.NotFound(errorResponse);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to retrieve icon for style {StyleId}, iconId {IconId}", styleId, iconId);
                        var errorResponse = ApiResponseFactory.CreateError("internal_server_error", "An error occurred while retrieving the icon.", stopwatch);
                        return Results.Problem(detail: errorResponse.Error?.Message, statusCode: 500);
                    }
                });
            })
            .WithName("GetIconById")
            .WithDescription("Returns a single icon as a PNG image for a given style and icon ID.")
            .WithOpenApi();

            return app;
        }
    }
}
