using System.Diagnostics;
using Winerr.NET.WebServer.Models;

namespace Winerr.NET.WebServer.Helpers
{
    public static class ApiResponseFactory
    {
        public static ApiResponse<T> CreateSuccess<T>(T data, string objectType, Stopwatch stopwatch)
        {
            stopwatch.Stop();
            return new ApiResponse<T>(
                Id: $"resp_{Guid.NewGuid():N}",
                Object: objectType,
                CreatedAt: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Status: "completed",
                Data: data,
                Error: null,
                Usage: new ApiUsage(TotalRequestTimeMs: stopwatch.ElapsedMilliseconds)
            );
        }
        public static ApiResponse<object> CreateError(string errorCode, string errorMessage, Stopwatch stopwatch)
        {
            stopwatch.Stop();
            return new ApiResponse<object>(
                Id: $"resp_{Guid.NewGuid():N}",
                Object: "error",
                CreatedAt: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Status: "failed",
                Data: null,
                Error: new ApiError(Code: errorCode, Message: errorMessage),
                Usage: new ApiUsage(TotalRequestTimeMs: stopwatch.ElapsedMilliseconds)
            );
        }
    }
}
