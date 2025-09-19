using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics;
using Winerr.NET.Core.Managers;
using Winerr.NET.WebServer.Helpers;
using Winerr.NET.WebServer.Models;

namespace Winerr.NET.WebServer.Endpoints
{
    public static class AssetEndpoints
    {
        public static IEndpointRouteBuilder MapAssetEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/v1/assets", (string? path, AssetManager am) =>
            {
                var stopwatch = Stopwatch.StartNew();

                if (string.IsNullOrEmpty(path))
                {
                    var errorResponse = ApiResponseFactory.CreateError("bad_request", "Query parameter 'path' is required.", stopwatch);
                    return Results.BadRequest(errorResponse);
                }

                if (!path.StartsWith("Winerr.NET.Assets.Styles.", StringComparison.OrdinalIgnoreCase))
                {
                    var errorResponse = ApiResponseFactory.CreateError("access_denied", "Access to this resource is forbidden.", stopwatch);
                    return Results.Json(errorResponse, statusCode: StatusCodes.Status403Forbidden);
                }

                var stream = am.GetResourceStream(path);

                if (stream == null)
                {
                    var errorResponse = ApiResponseFactory.CreateError("asset_not_found", $"Asset with path '{path}' was not found.", stopwatch);
                    return Results.NotFound(errorResponse);
                }

                return Results.File(stream, "image/png", enableRangeProcessing: false);
            })
            .WithName("GetAssetByPath")
            .WithDescription("Returns a single asset file by its full resource path.")
            .WithOpenApi();

            return app;
        }
    }
}
