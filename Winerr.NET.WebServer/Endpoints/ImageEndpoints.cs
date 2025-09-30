using SixLabors.ImageSharp;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SharpCompress.Common;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Writers;
using SharpCompress.Writers.Tar;
using SharpCompress.Writers.Zip;
using Winerr.NET.Core.Configs;
using Winerr.NET.Core.Enums;
using Winerr.NET.Core.Renderers;
using Winerr.NET.WebServer.Helpers;
using Winerr.NET.WebServer.Models;

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

        [JsonPropertyName("sort_buttons")]
        public bool? SortButtons { get; init; }
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

    public record ImageGenerationUsage : ApiUsage
    {
        public long GenerationTimeMs { get; init; }
        public int ImageWidth { get; init; }
        public int ImageHeight { get; init; }
        public long ImageSizeBytes { get; init; }

        public ImageGenerationUsage(long totalRequestTimeMs, long generationTimeMs, int imageWidth, int imageHeight, long imageSizeBytes)
            : base(totalRequestTimeMs)
        {
            GenerationTimeMs = generationTimeMs;
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
            ImageSizeBytes = imageSizeBytes;
        }
    }

    public record BatchMetadataEntry(
        [property: JsonPropertyName("file_name")] string FileName,
        [property: JsonPropertyName("generation_time_ms")] long GenerationTimeMs,
        [property: JsonPropertyName("source_request")] GenerateImageRequest SourceRequest
    );

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

                logger.LogInformation("Single image generated in {GenerationTime}ms (Total request: {TotalTime}ms)",
                    generationStopwatch.ElapsedMilliseconds, totalStopwatch.ElapsedMilliseconds);

                var usage = new ImageGenerationUsage(
                    totalRequestTimeMs: totalStopwatch.ElapsedMilliseconds,
                    generationTimeMs: generationStopwatch.ElapsedMilliseconds,
                    imageWidth: image.Width,
                    imageHeight: image.Height,
                    imageSizeBytes: memoryStream.Length
                );

                httpContext.Response.Headers.Append("X-Usage-Details", JsonSerializer.Serialize(usage));

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
                var totalStopwatch = Stopwatch.StartNew();

                if (batchRequest.Requests == null || !batchRequest.Requests.Any())
                {
                    var errorResponse = ApiResponseFactory.CreateError("bad_request", "The 'requests' array cannot be null or empty.", totalStopwatch);
                    return Results.BadRequest(errorResponse);
                }

                var archiveStream = new MemoryStream();
                var format = batchRequest.ArchiveFormat.ToLowerInvariant();
                var metadata = new List<BatchMetadataEntry>();

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

                            var generationStopwatch = Stopwatch.StartNew();
                            using var image = renderer.Generate(config);
                            generationStopwatch.Stop();

                            using var imageStream = new MemoryStream();
                            await image.SaveAsPngAsync(imageStream);
                            imageStream.Position = 0;

                            string entryFileName = $"{i}.png";
                            writer.Write(entryFileName, imageStream);

                            metadata.Add(new BatchMetadataEntry(entryFileName, generationStopwatch.ElapsedMilliseconds, request));
                        }

                        var metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                        using var metadataStream = new MemoryStream(Encoding.UTF8.GetBytes(metadataJson));
                        writer.Write("metadata.json", metadataStream);
                    }

                    archiveStream.Position = 0;
                    var mimeType = format == "zip" ? "application/zip" : "application/x-tar";
                    var archiveFileName = $"batch_{DateTime.UtcNow:yyyyMMddHHmmss}.{format}";

                    totalStopwatch.Stop();
                    logger.LogInformation("Batch of {Count} images generated in {TotalTime}ms",
                        batchRequest.Requests.Count, totalStopwatch.ElapsedMilliseconds);

                    return Results.File(archiveStream, mimeType, archiveFileName);
                }
                catch (ArgumentException ex)
                {
                    var errorResponse = ApiResponseFactory.CreateError("bad_request", ex.Message, totalStopwatch);
                    return Results.BadRequest(errorResponse);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during batch image generation.");
                    var errorResponse = ApiResponseFactory.CreateError("internal_server_error", "An unexpected error occurred during batch generation.", totalStopwatch);
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
                SortButtons = request.SortButtons ?? true,
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
