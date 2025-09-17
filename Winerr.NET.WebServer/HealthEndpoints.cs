namespace Winerr.NET.WebServer.Endpoints
{
    public static class HealthEndpoints
    {
        public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
                .WithName("HealthCheck")
                .WithDescription("Returns a 200 OK status if the server is running.")
                .WithOpenApi();

            return app;
        }
    }
}
