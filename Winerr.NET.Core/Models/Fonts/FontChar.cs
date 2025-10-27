using SixLabors.ImageSharp;
using Winerr.NET.Core.Text;

namespace Winerr.NET.Core.Models.Fonts
{
    public class FontChar
    {
        public Symbol Id { get; set; }
        public Rectangle Source { get; set; }
        public Point Offset { get; set; }
        public int XAdvance { get; set; }
    }
}
