using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Winerr.NET.Core.Models
{
    public class TextRenderResult : BaseRenderResult
    {
        public Size Dimensions { get; }
        public int BaselineY { get; }
        public TimeSpan RenderDuration { get; }

        public TextRenderResult(Image<Rgba32> image, Size dimensions, int baselineY, TimeSpan totalDuration, TimeSpan renderDuration)
            : base(image, totalDuration)
        {
            Dimensions = dimensions;
            BaselineY = baselineY;
            RenderDuration = renderDuration;
        }
    }
}