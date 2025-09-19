using SixLabors.ImageSharp;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Winerr.NET.Core.Configs;
using Winerr.NET.Core.Enums;
using Winerr.NET.Core.Renderers;
using Winerr.NET.WebServer.Helpers;
using Winerr.NET.WebServer.Models;
using SharpCompress.Writers;
using SharpCompress.Common;
using SharpCompress.Writers.Zip;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Writers.Tar;

namespace Winerr.NET.WebServer.Endpoints
{
    public record ImageApi;

    public record GenerateImageRequest
    {
        [Required(ErrorMessage = "StyleId is a required field.")]
        [JsonPropertyName("style_id")]
        public string StyleId { get; init; } = string.Empty;

        [JsonPropertyName("title")]
        public string? Title { get; init; }

        [Required(ErrorMessage = "Content is a required field.")]
        [JsonPropertyName("content")]
        public string Content { get; init; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "IconId must be a non-negative number.")]
        [JsonPropertyName("icon_id")]
        public int IconId { get; init; }

        [JsonPropertyName("buttons")]
        public List<ButtonRequest>? Buttons { get; init; }

        [JsonPropertyName("max_width")]
        public int? MaxWidth { get; init; }

        [JsonPropertyName("button_alignment")]
        public string? ButtonAlignment { get; init; }

        [JsonPropertyName("is_cross_enabled")]
        public bool? IsCrossEnabled { get; init; }
    }

    public record ButtonRequest(
        [property: JsonPropertyName("text")] string Text,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("mnemonic")] bool? Mnemonic
    );

    public record GenerateImageBatchRequest
    {
        [JsonPropertyName("archive_format")]
        public string ArchiveFormat { get; init; } = "zip";

        [JsonPropertyName("compression_level")]
        public int? CompressionLevel { get; init; } = 6;

        [Required]
        [JsonPropertyName("requests")]
        public List<GenerateImageRequest> Requests { get; init; } = new();
    }
    public record ImageGenerationUsage(long TotalRequestTimeMs, long GenerationTimeMs, int ImageWidth, int ImageHeight, long ImageSizeBytes)
        : ApiUsage(TotalRequestTimeMs);

    public static class ImageEndpoints
    {
        public static IEndpointRouteBuilder MapImageEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/v1/images/generate", async (
                GenerateImageRequest request,
                ILogger<ImageApi> logger,
                HttpContext httpContext) =>
            {
                var totalStopwatch = Stopwatch.StartNew();
                var config = CreateErrorConfigFromRequest(request);

                if (config == null)
                {
                    var errorResponse = ApiResponseFactory.CreateError("style_not_found", $"Style '{request.StyleId}' not found.", totalStopwatch);
                    return Results.BadRequest(errorResponse);
                }

                var renderer = new ErrorRenderer();

                var generationStopwatch = Stopwatch.StartNew();
                using var image = renderer.Generate(config);
                generationStopwatch.Stop();

                using var memoryStream = new MemoryStream();
                await image.SaveAsPngAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                totalStopwatch.Stop();

                var usage = new ImageGenerationUsage(
                    TotalRequestTimeMs: totalStopwatch.ElapsedMilliseconds,
                    GenerationTimeMs: generationStopwatch.ElapsedMilliseconds,
                    ImageWidth: image.Width,
                    ImageHeight: image.Height,
                    ImageSizeBytes: memoryStream.Length
                );

                httpContext.Response.Headers.Append("X-Usage-Details", System.Text.Json.JsonSerializer.Serialize(usage));

                return Results.File(imageBytes, "image/png", "error.png");
            })
            .WithName("GenerateImage")
            .WithDescription("Generates an error window image based on the provided configuration.")
            .WithOpenApi();

            app.MapPost("/v1/images/generate/batch", async (
                GenerateImageBatchRequest batchRequest,
                ILogger<ImageApi> logger,
                HttpContext httpContext) =>
            {
                var stopwatch = Stopwatch.StartNew();

                if (batchRequest.Requests == null || !batchRequest.Requests.Any())
                {
                    var errorResponse = ApiResponseFactory.CreateError("bad_request", "The 'requests' array cannot be null or empty.", stopwatch);
                    return Results.BadRequest(errorResponse);
                }

                var archiveStream = new MemoryStream();
                var format = batchRequest.ArchiveFormat.ToLowerInvariant();

                try
                {
                    using (var writer = CreateWriter(archiveStream, format, batchRequest.CompressionLevel))
                    {
                        for (int i = 0; i < batchRequest.Requests.Count; i++)
                        {
                            var request = batchRequest.Requests[i];
                            var config = CreateErrorConfigFromRequest(request);
                            if (config == null) continue;

                            var renderer = new ErrorRenderer();
                            using var image = renderer.Generate(config);
                            using var imageStream = new MemoryStream();
                            await image.SaveAsPngAsync(imageStream);
                            imageStream.Position = 0;

                            writer.Write($"image_{i}.png", imageStream);
                        }
                    }

                    archiveStream.Position = 0;
                    var mimeType = format == "zip" ? "application/zip" : "application/x-tar";
                    var fileName = $"batch_{DateTime.UtcNow:yyyyMMddHHmmss}.{format}";

                    return Results.File(archiveStream, mimeType, fileName);
                }
                catch (ArgumentException ex)
                {
                    var errorResponse = ApiResponseFactory.CreateError("bad_request", ex.Message, stopwatch);
                    return Results.BadRequest(errorResponse);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during batch image generation.");
                    var errorResponse = ApiResponseFactory.CreateError("internal_server_error", "An unexpected error occurred during batch generation.", stopwatch);
                    return Results.Problem(errorResponse.Error?.Message);
                }
            })
            .WithName("GenerateImageBatch")
            .WithDescription("Generates multiple error window images and returns them in a single archive file (zip or tar).")
            .WithOpenApi();

            return app;
        }

        private static IWriter CreateWriter(Stream stream, string format, int? level)
        {
            switch (format)
            {
                case "zip":
                    var zipCompression = MapCompressionLevel(level);
                    var zipOptions = new ZipWriterOptions(CompressionType.Deflate) { DeflateCompressionLevel = zipCompression };
                    return new ZipWriter(stream, zipOptions);
                case "tar":
                    var tarOptions = new TarWriterOptions(CompressionType.None, finalizeArchiveOnClose: true);
                    return new TarWriter(stream, tarOptions);
                default:
                    throw new ArgumentException($"Unsupported archive format '{format}'. Supported formats: zip, tar.");
            }
        }

        private static CompressionLevel MapCompressionLevel(int? level)
        {
            return level switch
            {
                0 => CompressionLevel.None,
                <= 2 => CompressionLevel.BestSpeed,
                >= 7 => CompressionLevel.BestCompression,
                _ => CompressionLevel.Default,
            };
        }

        private static ErrorConfig? CreateErrorConfigFromRequest(GenerateImageRequest request)
        {
            var style = SystemStyle.List().FirstOrDefault(s => s != null && s.Id.Equals(request.StyleId, StringComparison.OrdinalIgnoreCase));
            if (style == null) return null;

            return new ErrorConfig
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
        }
    }
}
