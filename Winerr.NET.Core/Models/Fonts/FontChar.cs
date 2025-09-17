using SixLabors.ImageSharp;

namespace Winerr.NET.Core.Models.Fonts
{
    public class FontChar
    {
        public int Id { get; set; }
        public Rectangle Source { get; set; }
        public Point Offset { get; set; }
        public int XAdvance { get; set; }
    }
}
