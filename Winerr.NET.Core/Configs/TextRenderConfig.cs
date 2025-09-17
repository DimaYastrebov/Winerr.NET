using SixLabors.ImageSharp;

namespace Winerr.NET.Core.Configs
{
    public class TextRenderConfig
    {
        public bool DrawMnemonic { get; set; } = false;

        public TextRenderConfig(bool? drawMnemonic = false)
        {
            DrawMnemonic = drawMnemonic ?? false;
        }
    }
}
