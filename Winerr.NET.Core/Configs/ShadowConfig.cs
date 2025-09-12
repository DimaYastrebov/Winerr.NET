using SixLabors.ImageSharp;

namespace Winerr.NET.Core.Models.Styles
{
    public class ShadowConfig
    {
        public Color Color { get; set; }
        public float Sigma { get; set; }
        public Point Offset { get; set; }
        public int ExpansionX { get; set; }
        public int ExpansionY { get; set; }

        public ShadowConfig()
        {
            Color = Color.Black;
            Sigma = 1.0f;
            Offset = Point.Empty;
            ExpansionX = 0;
            ExpansionY = 0;
        }
    }
}