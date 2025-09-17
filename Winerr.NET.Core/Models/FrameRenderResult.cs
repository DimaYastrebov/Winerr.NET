using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Winerr.NET.Core.Models
{
    public class FrameRenderResult(Image<Rgba32> image, TimeSpan duration) : BaseRenderResult(image, duration)
    {
    }
}
