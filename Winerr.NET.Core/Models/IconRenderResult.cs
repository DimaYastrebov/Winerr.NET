using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Winerr.NET.Core.Models
{
    public class IconRenderResult : BaseRenderResult
    {
        public int SourceIconId { get; }

        public IconRenderResult(Image<Rgba32> image, int sourceIconId, TimeSpan duration)
            : base(image, duration)
        {
            SourceIconId = sourceIconId;
        }
    }
}