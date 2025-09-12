using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Winerr.NET.Core.Models
{
    public class ButtonAreaRenderResult : BaseRenderResult
    {
        public ButtonAreaRenderResult(Image<Rgba32> image, TimeSpan duration)
            : base(image, duration)
        {
        }
    }
}