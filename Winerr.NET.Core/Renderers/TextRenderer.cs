using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using Winerr.NET.Core.Enums;
using Winerr.NET.Core.Models;
using Winerr.NET.Core.Models.Fonts;
using Winerr.NET.Core.Models.Styles;

namespace Winerr.NET.Core.Renderers
{
    public class TextRenderer
    {
        private readonly FontSet _fontSet;
        private readonly GraphicsOptions _fastDrawOptions;
        private readonly TextWrapper? _textWrapper;

        public TextRenderer(FontSet fontSet)
        {
            _fontSet = fontSet;

            if (_fontSet.Metrics != null)
            {
                _textWrapper = new TextWrapper(_fontSet.Metrics);
            }

            _fastDrawOptions = new GraphicsOptions
            {
                Antialias = false,
                ColorBlendingMode = PixelColorBlendingMode.Normal,
                AlphaCompositionMode = PixelAlphaCompositionMode.SrcOver
            };
        }

        public TextRenderResult DrawText(
            string text,
            int? maxWidth = null,
            string variationName = "Black",
            TextTruncationMode truncationMode = TextTruncationMode.None,
            TextWrapMode wrapMode = TextWrapMode.Auto,
            bool drawMnemonic = false,
            float lineSpacing = 0f,
            ShadowConfig? shadowConfig = null)
        {
            var totalStopwatch = Stopwatch.StartNew();

            var emptyImage = new Image<Rgba32>(1, 1);
            var metrics = _fontSet.Metrics;

            if (string.IsNullOrEmpty(text) || metrics == null || _textWrapper == null)
            {
                totalStopwatch.Stop();
                return new TextRenderResult(emptyImage, Size.Empty, 0, totalStopwatch.Elapsed, TimeSpan.Zero);
            }

            var lines = _textWrapper.Wrap(text.Replace("&", ""), maxWidth ?? int.MaxValue, wrapMode, truncationMode);

            if (!lines.Any())
            {
                totalStopwatch.Stop();
                return new TextRenderResult(emptyImage, Size.Empty, 0, totalStopwatch.Elapsed, TimeSpan.Zero);
            }

            int canvasWidth = 0;
            if (maxWidth.HasValue)
            {
                canvasWidth = maxWidth.Value;
            }
            else
            {
                canvasWidth = lines.Max(line => _textWrapper.MeasureTextWidth(line));
            }

            int canvasHeight = lines.Count * metrics.LineHeight;
            if (canvasWidth <= 0) canvasWidth = 1;
            if (canvasHeight <= 0) canvasHeight = 1;

            var (textImage, renderDuration) = RenderPass(lines, canvasWidth, canvasHeight, variationName, drawMnemonic);

            var bounds = FindBoundingBox(textImage);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                totalStopwatch.Stop();
                return new TextRenderResult(emptyImage, Size.Empty, 0, totalStopwatch.Elapsed, renderDuration);
            }

            textImage.Mutate(ctx => ctx.Crop(bounds));

            int finalBaselineY = metrics.Base - bounds.Y;

            if (shadowConfig != null)
            {
                var shadowResult = RenderWithShadow(textImage, finalBaselineY, shadowConfig);
                totalStopwatch.Stop();
                return new TextRenderResult(shadowResult.Image, shadowResult.Image.Size, shadowResult.BaselineY, totalStopwatch.Elapsed, renderDuration);
            }

            totalStopwatch.Stop();
            return new TextRenderResult(textImage, textImage.Size, finalBaselineY, totalStopwatch.Elapsed, renderDuration);
        }

        private (Image<Rgba32> Image, int BaselineY) RenderWithShadow(Image<Rgba32> textImage, int baselineY, ShadowConfig shadowConfig)
        {
            int padding = (int)Math.Ceiling(shadowConfig.Sigma * 3);
            var canvasSize = new Size(textImage.Width + padding * 2, textImage.Height + padding * 2);
            var shadowMask = new Image<Rgba32>(canvasSize.Width, canvasSize.Height);

            var shadowRectangle = new RectangleF(
                padding - shadowConfig.ExpansionX,
                padding - shadowConfig.ExpansionY,
                textImage.Width + (shadowConfig.ExpansionX * 2),
                textImage.Height + (shadowConfig.ExpansionY * 2)
            );

            shadowMask.Mutate(ctx => ctx
                .Fill(Color.White, shadowRectangle)
                .GaussianBlur(shadowConfig.Sigma));

            var finalImage = new Image<Rgba32>(canvasSize.Width, canvasSize.Height);
            finalImage.Mutate(ctx => ctx.Fill(shadowConfig.Color));
            finalImage.Mutate(ctx => ctx.SetGraphicsOptions(new GraphicsOptions { AlphaCompositionMode = PixelAlphaCompositionMode.DestIn }).DrawImage(shadowMask, new Point(0, 0), 1f));

            var textPosition = new Point(padding + shadowConfig.Offset.X, padding + shadowConfig.Offset.Y);
            finalImage.Mutate(ctx => ctx.DrawImage(textImage, textPosition, _fastDrawOptions));
            textImage.Dispose();

            int finalBaselineY = baselineY + textPosition.Y;
            return (finalImage, finalBaselineY);
        }

        private (Image<Rgba32> Image, TimeSpan Duration) RenderPass(List<string> lines, int width, int height, string variationName, bool drawMnemonic)
        {
            var finalImage = new Image<Rgba32>(width, height);
            var metrics = _fontSet.Metrics!;
            var textWrapper = _textWrapper!;

            if (!_fontSet.PrecutGlyphs.TryGetValue(variationName, out var precutGlyphs))
            {
                precutGlyphs = _fontSet.PrecutGlyphs.FirstOrDefault().Value;
                if (precutGlyphs == null)
                {
                    return (finalImage, TimeSpan.Zero);
                }
            }

            var renderStopwatch = Stopwatch.StartNew();
            finalImage.Mutate(ctx =>
            {
                int drawLineY = 0;
                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];

                    if (drawMnemonic && !string.IsNullOrEmpty(line))
                    {
                        string baseVariationName = variationName.Split('.')[0];

                        if (FontVariations.ColorMap.TryGetValue(baseVariationName, out var lineColor))
                        {
                            if (textWrapper.TryGetCharacter(line[0], out var firstChar) && firstChar != null)
                            {
                                int lineY = drawLineY + metrics.LineHeight - 2;

                                var startPoint = new PointF(firstChar.Offset.X, lineY);
                                var endPoint = new PointF(firstChar.Offset.X + firstChar.XAdvance, lineY);

                                if (endPoint.X > startPoint.X)
                                {
                                    ctx.DrawLine(lineColor, 1, startPoint, endPoint);
                                }
                            }
                        }
                    }

                    int drawCursorX = 0;
                    for (int j = 0; j < line.Length; j++)
                    {
                        char c = line[j];

                        if (textWrapper.TryGetCharacter(c, out var fontChar) && fontChar != null)
                        {
                            if (precutGlyphs.TryGetValue(fontChar.Id, out var charSprite))
                            {
                                var location = new Point(
                                    (drawCursorX + fontChar.Offset.X),
                                    (drawLineY + fontChar.Offset.Y)
                                );
                                ctx.DrawImage(charSprite, location, _fastDrawOptions);
                            }

                            drawCursorX += fontChar.XAdvance;
                            if (j < line.Length - 1)
                            {
                                drawCursorX += metrics.GetKerning(c, line[j + 1]);
                            }
                        }
                        else
                        {
                            if (textWrapper.TryGetCharacter(' ', out var spaceChar) && spaceChar != null)
                            {
                                drawCursorX += spaceChar.XAdvance;
                            }
                        }
                    }
                    drawLineY += metrics.LineHeight;
                }
            });
            renderStopwatch.Stop();
            return (finalImage, renderStopwatch.Elapsed);
        }

        private Rectangle FindBoundingBox(Image<Rgba32> image)
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

            return foundPixel ? new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1) : Rectangle.Empty;
        }
    }
}
