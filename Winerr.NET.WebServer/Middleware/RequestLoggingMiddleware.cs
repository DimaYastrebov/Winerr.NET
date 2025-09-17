using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Winerr.NET.WebServer.Config;

namespace Winerr.NET.WebServer.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly ServerConfig _config;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger, ServerConfig config)
        {
            _next = next;
            _logger = logger;
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            if (_config.IpFilter.Enabled)
            {
                bool isBlocked = _config.IpFilter.Mode switch
                {
                    IpFilterMode.Blacklist => _config.IpFilter.IpList.Contains(ipAddress),
                    IpFilterMode.Whitelist => !_config.IpFilter.IpList.Contains(ipAddress),
                    _ => false
                };

                if (isBlocked)
                {
                    stopwatch.Stop();
                    _logger.LogWarning("Blocked request from {IP} due to IP filter ({Mode}) -> {StatusCode} ({Elapsed}ms)",
                        ipAddress, _config.IpFilter.Mode, _config.IpFilter.BlockResponseCode, stopwatch.ElapsedMilliseconds);

                    context.Response.StatusCode = _config.IpFilter.BlockResponseCode;
                    await context.Response.WriteAsync(_config.IpFilter.BlockResponseMessage);
                    return;
                }
            }

            var path = context.Request.Path;
            var method = context.Request.Method;

            await _next(context);

            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            string logMessageTemplate;

            if (method == HttpMethods.Post || method == HttpMethods.Put)
            {
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                var bodyAsString = await reader.ReadToEndAsync();
                context.Request.Body.Seek(0, SeekOrigin.Begin);

                string formattedBody = FormatJsonBody(bodyAsString);

                logMessageTemplate = "Request from {IP} -> {Method} {Path} -> {StatusCode} ({Elapsed}ms)\nBody:\n{Body}";
                _logger.LogInformation(logMessageTemplate, ipAddress, method, path, statusCode, elapsedMs, formattedBody);
            }
            else
            {
                logMessageTemplate = "Request from {IP} -> {Method} {Path} -> {StatusCode} ({Elapsed}ms)";
                _logger.LogInformation(logMessageTemplate, ipAddress, method, path, statusCode, elapsedMs);
            }
        }

        private string FormatJsonBody(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return "<empty body>";
            }

            try
            {
                using var jsonDoc = JsonDocument.Parse(body);
                return JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (JsonException)
            {
                return body.Replace("\r\n", " ").Replace("\n", " ");
            }
        }
    }
}
