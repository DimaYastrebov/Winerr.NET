using System.Text.Json.Serialization;

namespace Winerr.NET.WebServer.Models
{
    public record SizeDto(
        [property: JsonPropertyName("width")] int Width,
        [property: JsonPropertyName("height")] int Height
    );
    public record PointDto(
        [property: JsonPropertyName("x")] int X,
        [property: JsonPropertyName("y")] int Y
    );
}
