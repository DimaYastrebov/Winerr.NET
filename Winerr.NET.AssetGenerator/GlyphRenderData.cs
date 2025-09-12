using Winerr.NET.Core.Models.Fonts;

namespace Winerr.NET.AssetGenerator
{
    public class GlyphRenderData
    {
        public char Character { get; }
        public FontChar Metrics { get; }

        public GlyphRenderData(char character, FontChar metrics)
        {
            Character = character;
            Metrics = metrics;
        }
    }
}