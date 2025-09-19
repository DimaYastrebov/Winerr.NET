using System.Diagnostics;
using Winerr.NET.Core.Enums;

namespace Winerr.NET.WebServer.Helpers
{
    public static class EndpointHelpers
    {
        public static IResult ExecuteWithStyleCheck(string styleId, Stopwatch stopwatch, Func<SystemStyle, IResult> onSuccess)
        {
            var style = SystemStyle.List()
                .FirstOrDefault(s => s != null && s.Id.Equals(styleId, StringComparison.OrdinalIgnoreCase));

            if (style == null)
            {
                var errorResponse = ApiResponseFactory.CreateError("style_not_found", $"Style with id '{styleId}' was not found.", stopwatch);
                return Results.NotFound(errorResponse);
            }

            return onSuccess(style);
        }
    }
}
