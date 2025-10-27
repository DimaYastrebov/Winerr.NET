using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using Winerr.NET.Core.Models.Fonts;

using SD = System.Drawing;
using SixLaborsPoint = SixLabors.ImageSharp.Point;
using SixLaborsRectangle = SixLabors.ImageSharp.Rectangle;

namespace Winerr.NET.AssetGenerator
{
    internal class GlyphRenderer
    {
        private readonly GenerationPreset _preset;
        private readonly SD.Color _textColor;
        private readonly SD.Color _backgroundColor;

        public GlyphRenderer(GenerationPreset preset)
        {
            _preset = preset;

            var bgPixel = _preset.BackgroundColor.ToPixel<Rgba32>();
            var textPixel = _preset.TextColor.ToPixel<Rgba32>();
            _backgroundColor = SD.Color.FromArgb(bgPixel.A, bgPixel.R, bgPixel.G, bgPixel.B);
            _textColor = SD.Color.FromArgb(textPixel.A, textPixel.R, textPixel.G, textPixel.B);
        }

        public (string Character, FontChar Metrics, SD.Bitmap GlyphBitmap) ProcessGlyph(string character, SD.Font font, IntPtr hFont)
        {
            var (textCanvas, textBounds, textMetrics) = RenderSingleGlyph(character, font, hFont);

            SD.Bitmap finalGlyphBitmap = (textBounds.Width > 0 && textBounds.Height > 0)
                ? textCanvas.Clone(textBounds, textCanvas.PixelFormat)
                : new SD.Bitmap(1, 1, textCanvas.PixelFormat);

            textCanvas.Dispose();
            textMetrics.Source = new SixLaborsRectangle(0, 0, textBounds.Width, textBounds.Height);
            return (character, textMetrics, finalGlyphBitmap);
        }

        public int GetBaselineInPixels(SD.Font font)
        {
            using var bmp = new SD.Bitmap(1, 1);
            using var g = SD.Graphics.FromImage(bmp);
            IntPtr hdc = g.GetHdc();
            IntPtr hFont = font.ToHfont();
            IntPtr hOldFont = NativeMethods.SelectObject(hdc, hFont);

            try
            {
                if (NativeMethods.GetTextMetrics(hdc, out NativeMethods.TEXTMETRIC tm))
                {
                    return tm.tmAscent;
                }
            }
            finally
            {
                NativeMethods.SelectObject(hdc, hOldFont);
                NativeMethods.DeleteObject(hFont);
                g.ReleaseHdc(hdc);
            }

            return (int)Math.Round(font.SizeInPoints / 72.0 * 96.0 * 0.8);
        }

        private (SD.Bitmap Canvas, SD.Rectangle Bounds, FontChar Metrics) RenderSingleGlyph(string character, SD.Font font, IntPtr hFont)
        {
            using var gdiBitmap = new SD.Bitmap(1, 1);
            using var g = SD.Graphics.FromImage(gdiBitmap);
            IntPtr hdc = g.GetHdc();
            IntPtr hOldFont = NativeMethods.SelectObject(hdc, hFont);
            NativeMethods.GetTextExtentPoint32W(hdc, character, character.Length, out var size);
            NativeMethods.SelectObject(hdc, hOldFont);
            g.ReleaseHdc(hdc);

            const int padding = 32;
            int tempWidth = size.cx + padding * 2;
            int tempHeight = font.Height + padding * 2;

            using var tempBitmap = new SD.Bitmap(tempWidth, tempHeight, PixelFormat.Format32bppArgb);
            using var tempG = SD.Graphics.FromImage(tempBitmap);

            if (_preset.UseClearType)
            {
                tempG.Clear(_backgroundColor);
                IntPtr tempHdc = tempG.GetHdc();
                IntPtr tempHOldFont = NativeMethods.SelectObject(tempHdc, hFont);
                NativeMethods.SetTextColor(tempHdc, SD.ColorTranslator.ToWin32(_textColor));
                NativeMethods.SetBkColor(tempHdc, SD.ColorTranslator.ToWin32(_backgroundColor));
                var drawRect = new NativeMethods.RECT { Left = padding, Top = padding, Right = tempWidth, Bottom = tempHeight };
                NativeMethods.DrawTextW(tempHdc, character, character.Length, ref drawRect, NativeMethods.DT_LEFT | NativeMethods.DT_TOP | NativeMethods.DT_NOCLIP | NativeMethods.DT_SINGLELINE);
                NativeMethods.SelectObject(tempHdc, tempHOldFont);
                tempG.ReleaseHdc(tempHdc);
            }
            else
            {
                tempG.TextRenderingHint = TextRenderingHint.AntiAlias;
                tempG.Clear(_backgroundColor);
                using var brush = new SD.SolidBrush(_textColor);
                tempG.DrawString(character, font, brush, new SD.PointF(padding, padding));
            }

            using Image<Rgba32> transparentImageSharp = ConvertToImageSharpAndMakeTransparent(tempBitmap, _backgroundColor);

            if (_preset.ApplyContrastEnhancement && _preset.TextContrastMultiplier > 1.0f)
            {
                transparentImageSharp.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        Span<Rgba32> row = accessor.GetRowSpan(y);
                        foreach (ref Rgba32 pixel in row)
                        {
                            pixel.A = (byte)Math.Min(255, pixel.A * _preset.TextContrastMultiplier);
                        }
                    }
                });
            }

            var bounds = FindBoundingBox(transparentImageSharp);

            var metrics = new FontChar
            {
                Id = character,
                Source = new SixLaborsRectangle(),
                Offset = new SixLaborsPoint(bounds.X - padding, bounds.Y - padding),
                XAdvance = size.cx
            };

            var finalCanvasBitmap = ConvertToSystemDrawingBitmap(transparentImageSharp);
            return (finalCanvasBitmap, bounds, metrics);
        }

        private unsafe Image<Rgba32> ConvertToImageSharpAndMakeTransparent(SD.Bitmap source, SD.Color backgroundColor, int threshold = 30)
        {
            var image = new Image<Rgba32>(source.Width, source.Height);
            var rect = new SD.Rectangle(0, 0, source.Width, source.Height);
            BitmapData sourceData = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < source.Height; y++)
                    {
                        byte* sourceRowPtr = (byte*)sourceData.Scan0 + (y * sourceData.Stride);
                        var destRow = accessor.GetRowSpan(y);

                        for (int x = 0; x < source.Width; x++)
                        {
                            byte b = sourceRowPtr[x * 4];
                            byte g = sourceRowPtr[x * 4 + 1];
                            byte r = sourceRowPtr[x * 4 + 2];
                            byte a = sourceRowPtr[x * 4 + 3];

                            if (_preset.UseClearType)
                            {
                                int distance = Math.Abs(r - backgroundColor.R) +
                                               Math.Abs(g - backgroundColor.G) +
                                               Math.Abs(b - backgroundColor.B);

                                if (distance < threshold)
                                {
                                    destRow[x] = new Rgba32(0, 0, 0, 0);
                                }
                                else
                                {
                                    byte newAlpha = (byte)Math.Min(255, distance * 1.5f);
                                    var finalPixel = new Rgba32(r, g, b, newAlpha);
                                    destRow[x] = finalPixel;
                                }
                            }
                            else
                            {
                                destRow[x] = new Rgba32(r, g, b, a);
                            }
                        }
                    }
                });
            }
            finally
            {
                source.UnlockBits(sourceData);
            }
            return image;
        }

        private unsafe SD.Bitmap ConvertToSystemDrawingBitmap(Image<Rgba32> source)
        {
            var bmp = new SD.Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            var rect = new SD.Rectangle(0, 0, source.Width, source.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);

            try
            {
                source.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < source.Height; y++)
                    {
                        var sourceRow = accessor.GetRowSpan(y);
                        byte* destRowPtr = (byte*)bmpData.Scan0 + (y * bmpData.Stride);

                        for (int x = 0; x < source.Width; x++)
                        {
                            destRowPtr[x * 4 + 0] = sourceRow[x].B;
                            destRowPtr[x * 4 + 1] = sourceRow[x].G;
                            destRowPtr[x * 4 + 2] = sourceRow[x].R;
                            destRowPtr[x * 4 + 3] = sourceRow[x].A;
                        }
                    }
                });
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        private SD.Rectangle FindBoundingBox(Image<Rgba32> image)
        {
            int minX = image.Width, minY = image.Height, maxX = -1, maxY = -1;
            bool foundPixel = false;

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgba32> row = accessor.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                    {
                        if (row[x].A > 0)
                        {
                            if (x < minX) minX = x;
                            if (y < minY) minY = y;
                            if (x > maxX) maxX = x;
                            if (y > maxY) maxY = y;
                            foundPixel = true;
                        }
                    }
                }
            });

            return foundPixel ? new SD.Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1) : SD.Rectangle.Empty;
        }
    }
}
