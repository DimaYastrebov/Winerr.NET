using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Winerr.NET.Core.Configs;

namespace Winerr.NET.Core.Models
{
    public class ButtonRenderResult : BaseRenderResult
    {
        public ButtonConfig SourceConfig { get; }

        public ButtonRenderResult(Image<Rgba32> image, ButtonConfig sourceConfig, TimeSpan duration)
            : base(image, duration)
        {
            SourceConfig = sourceConfig;
        }
    }
}
