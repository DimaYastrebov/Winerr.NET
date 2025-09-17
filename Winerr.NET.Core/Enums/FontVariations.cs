using SixLabors.ImageSharp;

namespace Winerr.NET.Core.Enums
{
    public static class FontVariations
    {
        public const string Gray = "Gray";
        public const string Black = "Black";
        public const string White = "White";

        public static readonly Dictionary<string, Color> ColorMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { Black, Color.Black },
            { Gray, Color.FromRgb(80, 80, 80) },
            { White, Color.White }
        };
    }
}
