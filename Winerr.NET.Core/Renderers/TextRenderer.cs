using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Winerr.NET.Core.Enums;
using Winerr.NET.Core.Models;
using Winerr.NET.Core.Models.Fonts;
using Winerr.NET.Core.Models.Styles;
using Winerr.NET.Core.Text;

namespace Winerr.NET.Core.Renderers
{
    public class TextRenderer
    {
        private readonly FontSet _mainFontSet;
        private readonly FontSet? _emojiFontSet;
        private readonly GraphicsOptions _fastDrawOptions;
        private readonly TextWrapper? _textWrapper;

        public TextRenderer(FontSet mainFontSet, FontSet? emojiFontSet = null)
        {
            _mainFontSet = mainFontSet;
            _emojiFontSet = emojiFontSet;

            if (_mainFontSet.Metrics != null)
            {
                _textWrapper = new TextWrapper(_mainFontSet.Metrics, _emojiFontSet?.Metrics);
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
    string mainVariationName = "Black",
    string? emojiVariationName = null,
    TextTruncationMode truncationMode = TextTruncationMode.None,
    TextWrapMode wrapMode = TextWrapMode.Auto,
    bool drawMnemonic = false,
    float lineSpacing = 0f,
    ShadowConfig? shadowConfig = null)
        {
            var totalStopwatch = Stopwatch.StartNew();
            var emptyImage = new Image<Rgba32>(1, 1);
            var metrics = _mainFontSet.Metrics;

            if (string.IsNullOrEmpty(text) || metrics == null || _textWrapper == null)
            {
                totalStopwatch.Stop();
                return new TextRenderResult(emptyImage, Size.Empty, 0, totalStopwatch.Elapsed, TimeSpan.Zero);
            }

            var processedSymbols = TextParser.Parse(text.Replace("&", ""));
            var lines = _textWrapper.Wrap(processedSymbols, maxWidth ?? int.MaxValue, wrapMode, truncationMode);

            if (!lines.Any() || lines.All(l => l.Count == 0))
            {
                totalStopwatch.Stop();
                return new TextRenderResult(emptyImage, Size.Empty, 0, totalStopwatch.Elapsed, TimeSpan.Zero);
            }

            int canvasWidth = maxWidth.HasValue ? maxWidth.Value : lines.Max(line => _textWrapper.MeasureSymbolsWidth(line));
            int canvasHeight = lines.Count * metrics.LineHeight;
            if (canvasWidth <= 0) canvasWidth = 1;
            if (canvasHeight <= 0) canvasHeight = 1;

            var (textImage, renderDuration) = RenderPass(lines, canvasWidth, canvasHeight, mainVariationName, emojiVariationName, drawMnemonic);

            var bounds = FindBoundingBox(textImage);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                totalStopwatch.Stop();
                return new TextRenderResult(emptyImage, Size.Empty, 0, totalStopwatch.Elapsed, renderDuration);
            }

            var originalBounds = bounds;
            textImage.Mutate(ctx => ctx.Crop(bounds));
            int finalBaselineY = metrics.Base - bounds.Y;

            Image<Rgba32> finalImageToStabilize = textImage;

            if (shadowConfig != null)
            {
                var shadowResult = RenderWithShadow(textImage, finalBaselineY, shadowConfig);
                finalImageToStabilize = shadowResult.Image;
                finalBaselineY = shadowResult.BaselineY;
            }

            // --- НАЧАЛО БЛОКА СТАБИЛИЗАЦИИ (v2, с фиксом тени) ---
            int padding = 0;
            if (shadowConfig != null)
            {
                // Рассчитываем паддинг, который добавляет тень
                padding = (int)System.Math.Ceiling(shadowConfig.Sigma * 3);
            }

            // Создаем финальный холст с предсказуемой высотой + место для тени
            int stableHeight = (lines.Count * metrics.LineHeight) + (padding * 2);
            if (stableHeight <= 0) stableHeight = 1;

            // Ширина холста должна быть достаточной, чтобы вместить результат с тенью
            int stableWidth = Math.Max(canvasWidth, finalImageToStabilize.Width);
            if (stableWidth <= 0) stableWidth = 1;

            var stableCanvas = new Image<Rgba32>(stableWidth, stableHeight);

            // Вычисляем, куда поместить наше изображение.
            // Целевая базовая линия на стабильном холсте = metrics.Base + padding (чтобы было место для тени сверху)
            // Текущая базовая линия в нашем (возможно, с тенью) изображении = finalBaselineY
            int drawY = (metrics.Base + padding) - finalBaselineY;
            int drawX = originalBounds.X;

            stableCanvas.Mutate(ctx => ctx.DrawImage(finalImageToStabilize, new Point(drawX, drawY), _fastDrawOptions));

            if (finalImageToStabilize != stableCanvas)
            {
                finalImageToStabilize.Dispose();
            }
            // --- КОНЕЦ БЛОКА СТАБИЛИЗАЦИИ ---

            totalStopwatch.Stop();
            // Возвращаем СТАБИЛЬНЫЙ холст и СТАБИЛЬНУЮ базовую линию (с учетом паддинга)
            int stableBaselineY = metrics.Base + padding;
            return new TextRenderResult(stableCanvas, stableCanvas.Size, stableBaselineY, totalStopwatch.Elapsed, renderDuration);
        }

        private (Image<Rgba32> Image, int BaselineY) RenderWithShadow(Image<Rgba32> textImage, int baselineY, ShadowConfig shadowConfig)
        {
            int padding = (int)System.Math.Ceiling(shadowConfig.Sigma * 3);
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

        private (Image<Rgba32> Image, TimeSpan Duration) RenderPass(List<List<Symbol>> lines, int width, int height, string mainVariationName, string? emojiVariationName, bool drawMnemonic)
        {
            var finalImage = new Image<Rgba32>(width, height);
            var mainMetrics = _mainFontSet.Metrics!;
            var textWrapper = _textWrapper!;

            if (!_mainFontSet.PrecutGlyphs.TryGetValue(mainVariationName, out var mainPrecutGlyphs))
            {
                mainPrecutGlyphs = _mainFontSet.PrecutGlyphs.FirstOrDefault().Value;
            }

            if (mainPrecutGlyphs == null)
            {
                return (finalImage, TimeSpan.Zero);
            }

            Dictionary<Symbol, Image<Rgba32>>? emojiPrecutGlyphs = null;
            if (_emojiFontSet != null && !string.IsNullOrEmpty(emojiVariationName))
            {
                if (!_emojiFontSet.PrecutGlyphs.TryGetValue(emojiVariationName, out emojiPrecutGlyphs))
                {
                    emojiPrecutGlyphs = _emojiFontSet.PrecutGlyphs.FirstOrDefault().Value;
                }
            }

            var renderStopwatch = Stopwatch.StartNew();
            finalImage.Mutate(ctx =>
            {
                int drawLineY = 0;
                foreach (var line in lines)
                {
                    int drawCursorX = 0;
                    Symbol? previousSymbol = null;
                    bool wasPreviousEmoji = false;

                    foreach (var symbol in line)
                    {
                        if (textWrapper.TryGetCharacter(symbol, out var fontChar, out bool isEmoji) && fontChar != null)
                        {
                            int kerning = 0;
                            if (previousSymbol.HasValue && !isEmoji && !wasPreviousEmoji)
                            {
                                kerning = mainMetrics.GetKerning(previousSymbol.Value, symbol);
                            }
                            drawCursorX += kerning;

                            var precutGlyphs = isEmoji ? emojiPrecutGlyphs : mainPrecutGlyphs;
                            if (precutGlyphs != null)
                            {
                                if (precutGlyphs.TryGetValue(fontChar.Id, out var charSprite))
                                {
                                    var location = new Point((drawCursorX + fontChar.Offset.X), (drawLineY + fontChar.Offset.Y));
                                    ctx.DrawImage(charSprite, location, _fastDrawOptions);
                                }
                            }
                            drawCursorX += fontChar.XAdvance;

                            previousSymbol = symbol;
                            wasPreviousEmoji = isEmoji;
                        }
                        else
                        {
                            if (textWrapper.TryGetCharacter(new Symbol(' '), out var spaceChar, out _) && spaceChar != null)
                            {
                                drawCursorX += spaceChar.XAdvance;
                            }
                        }
                    }
                    drawLineY += mainMetrics.LineHeight;
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
                    System.Span<Rgba32> row = accessor.GetRowSpan(y);
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
