using System.Net;
using System.Text.Json;

namespace Winerr.NET.WebServer.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            var title = "An internal server error has occurred.";
            var detail = "Something went wrong on our end. Please try again later.";

            var baseException = exception.GetBaseException();
            if (exception is BadHttpRequestException || baseException is JsonException)
            {
                code = HttpStatusCode.BadRequest;
                title = "Invalid request body.";

                if (baseException.Message.Contains("The input does not contain any JSON tokens"))
                {
                    detail = "The request body is empty or contains only whitespace. A valid JSON object is required.";
                }
                else
                {
                    detail = $"The JSON in the request body is malformed. Please check the syntax. Error: {baseException.Message}";
                }
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            var result = JsonSerializer.Serialize(new { title, status = (int)code, detail });
            return context.Response.WriteAsync(result);
        }
    }
}