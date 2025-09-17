using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp;
using Winerr.NET.Core.Enums;
using Winerr.NET.Core.Managers;
using Winerr.NET.Core.Services;

namespace Winerr.NET.WebServer.Endpoints
{
    public record IconApi;

    public static class IconEndpoints
    {
        public static IEndpointRouteBuilder MapIconEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/v1/styles/{styleId}/icons/spritesheet", (
                string styleId,
                ILogger<IconApi> logger,
                IMemoryCache cache) =>
            {
                string cacheKey = $"spritesheet_{styleId}";

                if (cache.TryGetValue(cacheKey, out byte[]? cachedImage))
                {
                    logger.LogInformation("Returning spritesheet for {StyleId} from cache.", styleId);
                    return Results.File(cachedImage!, "image/png", $"spritesheet_{styleId}.png");
                }

                try
                {
                    logger.LogInformation("Generating new spritesheet for {StyleId}...", styleId);
                    var style = FindStyle(styleId);
                    if (style == null)
                    {
                        return Results.NotFound(new { error = $"Style '{styleId}' not found." });
                    }

                    var styleName = style.Id.Split('_')[0];
                    var icons = AssetManager.Instance.GetAllIconsForStyle(styleName);

                    if (!icons.Any())
                    {
                        var emptyImageStream = new MemoryStream();
                        new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(1, 1).SaveAsPng(emptyImageStream);
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
            })
            .WithName("GetIconSpritesheet")
            .WithDescription("Returns a PNG spritesheet of all icons for a given style.")
            .WithOpenApi();


            app.MapGet("/v1/styles/{styleId}/icons/map", (
                string styleId,
                ILogger<IconApi> logger,
                IMemoryCache cache) =>
            {
                string cacheKey = $"iconmap_{styleId}";

                if (cache.TryGetValue(cacheKey, out object? cachedMap))
                {
                    logger.LogInformation("Returning icon map for {StyleId} from cache.", styleId);
                    return Results.Ok(cachedMap);
                }

                try
                {
                    logger.LogInformation("Generating new icon map for {StyleId}...", styleId);
                    var style = FindStyle(styleId);
                    if (style == null)
                    {
                        return Results.NotFound(new { error = $"Style '{styleId}' not found." });
                    }

                    var styleName = style.Id.Split('_')[0];
                    var icons = AssetManager.Instance.GetAllIconsForStyle(styleName);

                    var spritesheetService = new SpritesheetService();
                    using var result = spritesheetService.Generate(icons, style.Metrics.ExpectedIconSize);

                    var response = new
                    {
                        spritesheetUrl = $"/v1/styles/{styleId}/icons/spritesheet",
                        iconSize = new { width = result.IconSize.Width, height = result.IconSize.Height },
                        map = result.IconMap.ToDictionary(kvp => kvp.Key.ToString(), kvp => new { x = kvp.Value.X, y = kvp.Value.Y })
                    };

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                    cache.Set(cacheKey, response, cacheEntryOptions);

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to generate icon map for style {StyleId}", styleId);
                    return Results.Problem("An internal server error occurred while generating the icon map.");
                }
            })
            .WithName("GetIconMap")
            .WithDescription("Returns a JSON map of icon coordinates for a given style's spritesheet.")
            .WithOpenApi();

            return app;
        }

        private static SystemStyle? FindStyle(string styleId)
        {
            return SystemStyle.List()
                .FirstOrDefault(s => s != null && s.Id.Equals(styleId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
