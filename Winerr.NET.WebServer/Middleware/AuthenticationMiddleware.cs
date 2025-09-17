using Microsoft.Extensions.Primitives;
using Winerr.NET.WebServer.Config;

namespace Winerr.NET.WebServer.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ServerConfig _config;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(RequestDelegate next, ServerConfig config, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _config = config;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_config.Authentication.Enabled)
            {
                await _next(context);
                return;
            }

            if (!TryExtractApiKey(context, out var providedApiKey))
            {
                _logger.LogWarning("Blocked request from {IP}. Reason: Missing API Key.", context.Connection.RemoteIpAddress);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("API Key is missing.");
                return;
            }

            if (!IsApiKeyValid(providedApiKey))
            {
                _logger.LogWarning("Blocked request from {IP}. Reason: Invalid API Key.", context.Connection.RemoteIpAddress);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid API Key.");
                return;
            }

            await _next(context);
        }

        private bool TryExtractApiKey(HttpContext context, out string? apiKey)
        {
            apiKey = null;

            if (context.Request.Query.TryGetValue("api_key", out StringValues queryKey))
            {
                apiKey = queryKey.FirstOrDefault();
                return !string.IsNullOrEmpty(apiKey);
            }

            if (context.Request.Headers.TryGetValue("Authorization", out StringValues headerAuth))
            {
                var authHeader = headerAuth.FirstOrDefault();
                if (authHeader != null && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    apiKey = authHeader.Substring("Bearer ".Length).Trim();
                    return !string.IsNullOrEmpty(apiKey);
                }
            }

            return false;
        }

        private bool IsApiKeyValid(string? providedApiKey)
        {
            if (string.IsNullOrEmpty(providedApiKey) || string.IsNullOrEmpty(_config.Authentication.ApiKey))
            {
                return false;
            }

            return string.Equals(providedApiKey, _config.Authentication.ApiKey, StringComparison.Ordinal);
        }
    }
}