namespace Winerr.NET.AssetGenerator
{
    public static class TexturePacker
    {
        public static (int Width, int Height) Pack(List<GlyphRenderData> glyphs, int padding)
        {
            var totalPixels = glyphs.Sum(g => g.Metrics.Source.Width * g.Metrics.Source.Height);
            if (totalPixels == 0)
            {
                totalPixels = glyphs.Count * 16 * 16;
            }
            var initialWidth = (int)Math.Ceiling(Math.Sqrt(totalPixels)) * 2;
            if (initialWidth == 0) initialWidth = 1024;

            int currentX = padding;
            int currentY = padding;
            int rowMaxHeight = 0;
            int finalWidth = 0;
            int finalHeight = 0;

            foreach (var glyph in glyphs)
            {
                if (currentX + glyph.Metrics.Source.Width + padding > initialWidth)
                {
                    currentY += rowMaxHeight + padding;
                    currentX = padding;
                    rowMaxHeight = 0;
                }

                glyph.Metrics.Source = new SixLabors.ImageSharp.Rectangle(
                    currentX,
                    currentY,
                    glyph.Metrics.Source.Width,
                    glyph.Metrics.Source.Height
                );

                currentX += glyph.Metrics.Source.Width + padding;

                if (glyph.Metrics.Source.Height > rowMaxHeight)
                {
                    rowMaxHeight = glyph.Metrics.Source.Height;
                }

                if (currentX > finalWidth)
                {
                    finalWidth = currentX;
                }
                finalHeight = currentY + rowMaxHeight + padding;
            }

            if (finalWidth == 0) finalWidth = 1;
            if (finalHeight == 0) finalHeight = 1;

            return (finalWidth, finalHeight);
        }
    }
}