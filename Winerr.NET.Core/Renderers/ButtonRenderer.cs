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

namespace Winerr.NET.Core.Renderers
{
    public class ButtonRenderer
    {
        public ButtonRenderer()
        { }

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

            var textRenderer = new TextRenderer(fontSet);
            var textResult = textRenderer.DrawText(
                config.Text,
                null,
                buttonMetrics.FontVariation,
                drawMnemonic: config.TextConfig?.DrawMnemonic ?? false,
                truncationMode: TextTruncationMode.Ellipsis
            );

            var contentWidth = textResult.Dimensions.Width + buttonMetrics.HorizontalPadding * 2;
            var finalWidth = Math.Max(contentWidth, metrics.MinButtonWidth);
            var buttonImage = new Image<Rgba32>(finalWidth, metrics.ButtonHeight);

            var am = AssetManager.Instance;
            string buttonTypeName = config.Type.DisplayName.ToLower();
            var buttonAssets = AssetLoader.LoadRequiredImages(
                style,
                am.GetButtonImage,
                $"{buttonTypeName}_left",
                $"{buttonTypeName}_center",
                $"{buttonTypeName}_right"
            );

            var left = buttonAssets[$"{buttonTypeName}_left"];
            var center = buttonAssets[$"{buttonTypeName}_center"];
            var right = buttonAssets[$"{buttonTypeName}_right"];

            buttonImage.Mutate(ctx =>
            {
                ctx.DrawImage(left, new Point(0, 0), 1f);

                int centerWidth = finalWidth - left.Width - right.Width;
                if (centerWidth > 0)
                {
                    var centerResized = center.Clone(c => c.Resize(centerWidth, metrics.ButtonHeight));
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