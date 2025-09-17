using SixLabors.ImageSharp;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Winerr.NET.Core.Configs;
using Winerr.NET.Core.Enums;
using Winerr.NET.Core.Renderers;

namespace Winerr.NET.WebServer.Endpoints
{
    public record ImageApi;

    public record GenerateImageRequest
    {
        [Required(ErrorMessage = "StyleId is a required field.")]
        [JsonPropertyName("styleId")]
        public string StyleId { get; init; } = string.Empty;

        [JsonPropertyName("title")]
        public string? Title { get; init; }

        [Required(ErrorMessage = "Content is a required field.")]
        [JsonPropertyName("content")]
        public string Content { get; init; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "IconId must be a non-negative number.")]
        [JsonPropertyName("iconId")]
        public int IconId { get; init; }

        [JsonPropertyName("buttons")]
        public List<ButtonRequest>? Buttons { get; init; }

        [JsonPropertyName("maxWidth")]
        public int? MaxWidth { get; init; }

        [JsonPropertyName("buttonAlignment")]
        public string? ButtonAlignment { get; init; }

        [JsonPropertyName("isCrossEnabled")]
        public bool? IsCrossEnabled { get; init; }
    }

    public record ButtonRequest(string Text, string Type, bool? Mnemonic);

    public static class ImageEndpoints
    {
        public static IEndpointRouteBuilder MapImageEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/v1/images/generate", (
                GenerateImageRequest request,
                ILogger<ImageApi> logger,
                HttpContext httpContext) =>
            {
                try
                {
                    var style = SystemStyle.List()
                        .FirstOrDefault(s => s != null && s.Id.Equals(request.StyleId, StringComparison.OrdinalIgnoreCase));

                    if (style == null)
                    {
                        return Results.BadRequest(new { error = $"Style '{request.StyleId}' not found." });
                    }

                    var config = new ErrorConfig
                    {
                        SystemStyle = style,
                        Title = request.Title ?? string.Empty,
                        Content = request.Content,
                        IconId = request.IconId,
                        MaxWidth = request.MaxWidth,
                        IsCrossEnabled = request.IsCrossEnabled ?? true,
                        ButtonAlignment = Enum.TryParse<ButtonAlignment>(request.ButtonAlignment, true, out var align) ? align : ButtonAlignment.Right,
                        Buttons = request.Buttons?.Select(b => new ButtonConfig
                        {
                            Text = b.Text,
                            Type = b.Type.ToLowerInvariant() switch
                            {
                                "recommended" => ButtonType.Recommended,
                                "disabled" => ButtonType.Disabled,
                                _ => ButtonType.Default
                            },
                            TextConfig = new TextRenderConfig { DrawMnemonic = b.Mnemonic ?? false }
                        }).ToList() ?? new List<ButtonConfig>()
                    };

                    var renderer = new ErrorRenderer();
                    using var image = renderer.Generate(config);

                    using var memoryStream = new MemoryStream();
                    image.SaveAsPng(memoryStream);
                    memoryStream.Position = 0;

                    return Results.File(memoryStream.ToArray(), "image/png", "error.png");
                }
                catch (Exception ex)
                {
                    var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    logger.LogError(ex, "Error generating image for request from {IP}", ipAddress);

                    return Results.Problem(
                        detail: "An internal server error occurred while generating the image. Check server logs for details.",
                        title: "Image Generation Failed",
                        statusCode: 500
                    );
                }
            })
            .WithName("GenerateImage")
            .WithDescription("Generates an error window image based on the provided configuration.")
            .WithOpenApi();

            return app;
        }
    }
}
