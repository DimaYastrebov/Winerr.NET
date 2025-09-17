using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Winerr.NET.Core.Models
{
    public class SpritesheetResult : IDisposable
    {
        public Image<Rgba32> Spritesheet { get; }
        public Dictionary<int, Point> IconMap { get; }
        public Size IconSize { get; }

        public SpritesheetResult(Image<Rgba32> spritesheet, Dictionary<int, Point> iconMap, Size iconSize)
        {
            Spritesheet = spritesheet;
            IconMap = iconMap;
            IconSize = iconSize;
        }

        public void Dispose()
        {
            Spritesheet?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
