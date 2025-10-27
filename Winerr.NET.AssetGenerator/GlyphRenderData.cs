using Winerr.NET.Core.Models.Fonts;

namespace Winerr.NET.AssetGenerator
{
    public class GlyphRenderData
    {
        public string Character { get; }
        public FontChar Metrics { get; }

        public GlyphRenderData(string character, FontChar metrics)
        {
            Character = character;
            Metrics = metrics;
        }
    }
}
