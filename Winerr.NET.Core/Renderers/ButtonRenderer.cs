using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using Winerr.NET.Core.Configs;
using Winerr.NET.Core.Enums;
using Winerr.NET.Core.Helpers;
using Winerr.NET.Core.Managers;
using Winerr.NET.Core.Models;
using Winerr.NET.Core.Models.Styles;
using Winerr.NET.Core.Text;

namespace Winerr.NET.Core.Renderers
{
    public class ButtonRenderer
    {
        public ButtonRenderer()
        { }

        public int MeasureButtonWidth(ButtonConfig config, SystemStyle style)
        {
            var metrics = style.Metrics;

            if (!metrics.ButtonTypeMetrics.TryGetValue(config.Type, out var buttonMetrics))
            {
                buttonMetrics = new ButtonMetrics { FontVariation = FontVariations.Black, HorizontalPadding = 0, VerticalTextOffset = 0 };
            }

            var fontSet = metrics.ButtonFontSet(buttonMetrics.FontVariation);
            if (fontSet.Metrics == null)
            {
                throw new InvalidOperationException("Font metrics not loaded for button font set.");
            }

            var textWrapper = new TextWrapper(fontSet.Metrics, metrics.ButtonEmojiFontSet?.Metrics);
            var processedSymbols = TextParser.Parse(config.Text);
            int textWidth = textWrapper.MeasureSymbolsWidth(processedSymbols);

            var contentWidth = textWidth + buttonMetrics.HorizontalPadding * 2;
            return Math.Max(contentWidth, metrics.MinButtonWidth);
        }

        public ButtonRenderResult DrawButton(ButtonConfig config, SystemStyle style)
        {
            var stopwatch = Stopwatch.StartNew();
            var metrics = style.Metrics;

            if (!metrics.ButtonTypeMetrics.TryGetValue(config.Type, out var buttonMetrics))
            {
                buttonMetrics = new ButtonMetrics { FontVariation = FontVariations.Black, HorizontalPadding = 0, VerticalTextOffset = 0 };
            }

            var fontSet = metrics.ButtonFontSet(buttonMetrics.FontVariation);
            if (fontSet.Metrics == null)
            {
                throw new InvalidOperationException("Font metrics not loaded for button font set.");
            }

            var textRenderer = new TextRenderer(fontSet, metrics.ButtonEmojiFontSet);
            using var textResult = textRenderer.DrawText(
                config.Text,
                null,
                mainVariationName: buttonMetrics.FontVariation,
                emojiVariationName: metrics.ButtonEmojiFontVariation,
                drawMnemonic: config.TextConfig?.DrawMnemonic ?? false,
                truncationMode: TextTruncationMode.Ellipsis
            );

            if (config.Type == ButtonType.Disabled)
            {
                textResult.Image.Mutate(ctx => ctx.Grayscale());
            }

            var finalWidth = MeasureButtonWidth(config, style);
            var buttonImage = new Image<Rgba32>(finalWidth, metrics.ButtonHeight);

            var am = AssetManager.Instance;

            string leftKey, centerKey, rightKey;

            if (config.Type == ButtonType.Recommended)
            {
                leftKey = AssetKeys.ButtonParts.RecommendedLeft;
                centerKey = AssetKeys.ButtonParts.RecommendedCenter;
                rightKey = AssetKeys.ButtonParts.RecommendedRight;
            }
            else if (config.Type == ButtonType.Disabled)
            {
                leftKey = AssetKeys.ButtonParts.DisabledLeft;
                centerKey = AssetKeys.ButtonParts.DisabledCenter;
                rightKey = AssetKeys.ButtonParts.DisabledRight;
            }
            else
            {
                leftKey = AssetKeys.ButtonParts.DefaultLeft;
                centerKey = AssetKeys.ButtonParts.DefaultCenter;
                rightKey = AssetKeys.ButtonParts.DefaultRight;
            }

            var buttonAssets = AssetLoader.LoadRequiredImages(
                style,
                am.GetButtonImage,
                leftKey,
                centerKey,
                rightKey
            );

            var left = buttonAssets[leftKey];
            var center = buttonAssets[centerKey];
            var right = buttonAssets[rightKey];

            buttonImage.Mutate(ctx =>
            {
                ctx.DrawImage(left, new Point(0, 0), 1f);

                int centerWidth = finalWidth - left.Width - right.Width;
                if (centerWidth > 0)
                {
                    using var centerResized = center.Clone(c => c.Resize(centerWidth, metrics.ButtonHeight));
                    ctx.DrawImage(centerResized, new Point(left.Width, 0), 1f);
                }

                ctx.DrawImage(right, new Point(finalWidth - right.Width, 0), 1f);

                int textX = (finalWidth - textResult.Dimensions.Width) / 2;

                int idealLineTopY = (metrics.ButtonHeight - fontSet.Metrics.LineHeight) / 2;
                int targetBaselineY = idealLineTopY + fontSet.Metrics.Base;
                int textY = targetBaselineY - textResult.BaselineY + buttonMetrics.VerticalTextOffset;

                ctx.DrawImage(textResult.Image, new Point(textX, textY), 1f);
            });

            stopwatch.Stop();
            return new ButtonRenderResult(buttonImage, config, stopwatch.Elapsed);
        }
    }
}
