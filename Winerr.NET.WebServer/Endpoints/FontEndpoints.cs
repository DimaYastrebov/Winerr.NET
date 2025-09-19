using System.Diagnostics;
using System.Text.Json.Serialization;
using Winerr.NET.Core.Managers;
using Winerr.NET.WebServer.Helpers;

namespace Winerr.NET.WebServer.Endpoints
{
    public record FontInfoResponseDto(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("sizes")] Dictionary<string, FontSizeInfoResponseDto> Sizes
    );

    public record FontSizeInfoResponseDto(
        [property: JsonPropertyName("variations")] List<string> Variations
    );

    public static class FontEndpoints
    {
        public static IEndpointRouteBuilder MapFontEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/v1/fonts", (AssetManager am) =>
            {
                var stopwatch = Stopwatch.StartNew();

                var fontInfoCore = am.GetFontInfo();

                var responseData = fontInfoCore.Select(f => new FontInfoResponseDto(
                    f.Name,
                    f.Sizes.ToDictionary(
                        s => s.Key,
                        s => new FontSizeInfoResponseDto(s.Value.Variations)
                    )
                )).ToList();

                var response = ApiResponseFactory.CreateSuccess(responseData, "font.list", stopwatch);
                return Results.Ok(response);
            })
            .WithName("GetFonts")
            .WithDescription("Returns a list of all available fonts, their sizes, and variations.")
            .WithOpenApi();

            return app;
        }
    }
}
