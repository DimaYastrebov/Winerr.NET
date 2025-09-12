using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Winerr.NET.Core.Models
{
    public class ContentAreaRenderResult : BaseRenderResult
    {
        public ContentAreaRenderResult(Image<Rgba32> image, TimeSpan duration)
            : base(image, duration)
        {
        }
    }
}