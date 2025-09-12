using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Winerr.NET.Core.Models
{
    public abstract class BaseRenderResult : IDisposable
    {
        public Image<Rgba32> Image { get; }
        public TimeSpan Duration { get; }

        protected BaseRenderResult(Image<Rgba32> image, TimeSpan duration)
        {
            Image = image;
            Duration = duration;
        }

        public void Dispose()
        {
            Image?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}