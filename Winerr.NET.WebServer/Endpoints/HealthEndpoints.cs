using System.Diagnostics;
using Winerr.NET.Core.Managers;
using Winerr.NET.WebServer.Helpers;

namespace Winerr.NET.WebServer.Endpoints
{
    public static class HealthEndpoints
    {
        public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/health", (AssetManager am) =>
            {
                var stopwatch = Stopwatch.StartNew();

                var healthStatus = new { status = "Healthy" };

                var response = ApiResponseFactory.CreateSuccess(healthStatus, "health.check", stopwatch);

                return Results.Ok(response);
            })
            .WithName("HealthCheck")
            .WithDescription("Returns a 200 OK status if the server is running.")
            .WithOpenApi();

            return app;
        }
    }
}
