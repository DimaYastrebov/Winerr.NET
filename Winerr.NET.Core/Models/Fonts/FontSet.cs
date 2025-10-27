using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Winerr.NET.Core.Text;

namespace Winerr.NET.Core.Models.Fonts
{
    public class FontSet
    {
        public BitmapFont? Metrics { get; set; }
        public Dictionary<string, Image<Rgba32>> Variations { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Dictionary<Symbol, Image<Rgba32>>> PrecutGlyphs { get; } = new(StringComparer.OrdinalIgnoreCase);

        public FontSet(BitmapFont? metrics)
        {
            Metrics = metrics;
        }

        public Image<Rgba32>? GetVariation(string variationName)
        {
            Variations.TryGetValue(variationName, out var image);
            return image;
        }
    }
}
