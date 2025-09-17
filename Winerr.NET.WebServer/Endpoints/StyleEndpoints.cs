using Winerr.NET.Core.Enums;
using Winerr.NET.Core.Managers;

namespace Winerr.NET.WebServer.Endpoints
{
    public record StyleDetailsDto
    {
        public string Id { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public int MaxIconId { get; init; }
        public StyleMetricsDto Metrics { get; init; } = new();
    }

    public record StyleMetricsDto
    {
        public SizeDto ExpectedIconSize { get; init; } = new(0, 0);
        public int MinButtonWidth { get; init; }
        public int ButtonHeight { get; init; }
        public int ButtonSpacing { get; init; }
        public IEnumerable<string> ButtonSortOrder { get; init; } = Enumerable.Empty<string>();
        public int ButtonsPaddingLeft { get; init; }
        public int ButtonsPaddingRight { get; init; }
        public float LineSpacing { get; init; }
    }

    public record SizeDto(int Width, int Height);


    public static class StyleEndpoints
    {
        public static IEndpointRouteBuilder MapStyleEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/v1/styles", () =>
            {
                var styles = SystemStyle.List()
                    .Where(s => s != null)
                    .Select(s => new { s.Id, s.DisplayName });

                return Results.Ok(styles);
            })
            .WithName("GetStyles")
            .WithDescription("Returns a list of all available system styles.")
            .WithOpenApi();

            app.MapGet("/v1/styles/{styleId}", (string styleId) =>
            {
                var style = SystemStyle.List()
                    .FirstOrDefault(s => s != null && s.Id.Equals(styleId, StringComparison.OrdinalIgnoreCase));

                if (style == null)
                {
                    return Results.NotFound(new { error = $"Style '{styleId}' not found." });
                }

                var metrics = style.Metrics;
                var styleName = style.Id.Split('_')[0];

                var am = AssetManager.Instance;
                var maxIconId = am.GetMaxIconIdForStyle(styleName);

                var response = new StyleDetailsDto
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
                        ButtonsPaddingLeft = metrics.ButtonsPaddingLeft,
                        ButtonsPaddingRight = metrics.ButtonsPaddingRight,
                        LineSpacing = metrics.LineSpacing
                    }
                };

                return Results.Ok(response);
            })
            .WithName("GetStyleDetails")
            .WithDescription("Returns detailed information about a specific system style.")
            .WithOpenApi();

            return app;
        }
    }
}
