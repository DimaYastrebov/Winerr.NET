using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Winerr.NET.WebServer.Models
{
    public record ApiResponse<T>(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("object")] string Object,
        [property: JsonPropertyName("created_at")] long CreatedAt,
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("data")] T? Data,
        [property: JsonPropertyName("error")] ApiError? Error,
        [property: JsonPropertyName("usage")] ApiUsage? Usage
    );

    public record ApiError(
        [property: JsonPropertyName("code")] string Code,
        [property: JsonPropertyName("message")] string Message
    );

    public record ApiUsage(
        [property: JsonPropertyName("total_request_time_ms")] long TotalRequestTimeMs
    );
}
