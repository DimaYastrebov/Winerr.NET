using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using Winerr.NET.Core.Enums;
using Winerr.NET.Core.Helpers;
using Winerr.NET.Core.Managers;
using Winerr.NET.Core.Models;

namespace Winerr.NET.Core.Renderers
{
    public class ContentAreaRenderer(string text, int iconId, SystemStyle style, int? maxWidth = null, TextTruncationMode truncationMode = TextTruncationMode.None, TextWrapMode wrapMode = TextWrapMode.Symbol)
    {
        private readonly Lazy<IconRenderer> _iconRenderer = new(() => new IconRenderer());
        private readonly Lazy<TextRenderer> _textRenderer = new(() =>
            new TextRenderer(
                style.Metrics.TextFontSet ??
                throw new InvalidOperationException($"Content font '{style.Metrics.TextFontSet}' not found for style '{style.DisplayName}'.")
            )
        );

        public Size MeasureContentArea()
        {
            var metrics = style.Metrics;
            var fontSet = style.Metrics.TextFontSet;

            if (fontSet?.Metrics == null)
            {
                throw new InvalidOperationException($"Font metrics not found for style '{style.DisplayName}'.");
            }

            var iconSize = metrics.ExpectedIconSize;

            int? maxTextWidth = null;
            if (maxWidth.HasValue)
            {
                maxTextWidth = maxWidth.Value - metrics.IconPaddingLeft - iconSize.Width - metrics.IconPaddingRight - metrics.TextPaddingRight;
            }

            var textWrapper = new TextWrapper(fontSet.Metrics);
            var lines = textWrapper.Wrap(text, maxTextWidth ?? int.MaxValue, wrapMode, truncationMode);

            int textWidth = lines.Any() ? lines.Max(line => textWrapper.MeasureTextWidth(line)) : 0;
            int textHeight = lines.Count * fontSet.Metrics.LineHeight;

            int naturalContentWidth = metrics.IconPaddingLeft + iconSize.Width + metrics.IconPaddingRight + textWidth + metrics.TextPaddingRight;

            int iconBottom = metrics.IconPaddingTop + iconSize.Height;
            int textBottom = metrics.TextPaddingTop + textHeight;
            int requiredContentHeight = Math.Max(iconBottom, textBottom) + metrics.TextPaddingBottom;
            int finalHeight = Math.Max(requiredContentHeight, metrics.MinContentHeight);

            return new Size(naturalContentWidth, finalHeight);
        }

        public ContentAreaRenderResult DrawContentArea(int? forcedWidth = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var am = AssetManager.Instance;
            var metrics = style.Metrics;

            using var iconResult = _iconRenderer.Value.DrawIcon(iconId, style, ignoreMissing: true, shrinkMode: IconShrinkMode.Exact);
            Size iconSize = iconResult.Image.Size;

            int iconPaddingLeft = metrics.IconPaddingLeft;
            int iconPaddingTop = metrics.IconPaddingTop;
            int iconPaddingRight = metrics.IconPaddingRight;
            int textPaddingRight = metrics.TextPaddingRight;
            int textPaddingTop = metrics.TextPaddingTop;
            int textPaddingBottom = metrics.TextPaddingBottom;

            int? maxTextWidth = null;
            if (maxWidth.HasValue)
            {
                maxTextWidth = maxWidth.Value - iconPaddingLeft - iconSize.Width - iconPaddingRight - textPaddingRight;
            }
            else if (forcedWidth.HasValue)
            {
                maxTextWidth = forcedWidth.Value - iconPaddingLeft - iconSize.Width - iconPaddingRight - textPaddingRight;
            }

            using var textResult = _textRenderer.Value.DrawText(
                text: text,
                maxWidth: maxTextWidth,
                variationName: metrics.TextFontVariation,
                truncationMode: truncationMode,
                wrapMode: wrapMode,
                lineSpacing: metrics.LineSpacing
            );
            Size textSize = textResult.Dimensions;
            int naturalContentWidth = iconPaddingLeft + iconSize.Width + iconPaddingRight + textSize.Width + textPaddingRight;
            int finalWidth = forcedWidth ?? naturalContentWidth;

            int iconBottom = iconPaddingTop + iconSize.Height;
            int textBottom = textPaddingTop + textSize.Height;
            int requiredContentHeight = Math.Max(iconBottom, textBottom) + textPaddingBottom;
            int finalHeight = Math.Max(requiredContentHeight, metrics.MinContentHeight);

            var finalImage = new Image<Rgba32>(finalWidth, finalHeight);

            var assets = AssetLoader.LoadRequiredImages(style, am.GetStyleImage, AssetKeys.FrameParts.MiddleCenter);
            var backgroundTile = assets[AssetKeys.FrameParts.MiddleCenter];
            finalImage.Mutate(ctx =>
            {
                for (int y = 0; y < finalHeight; y += backgroundTile.Height)
                {
                    for (int x = 0; x < finalWidth; x += backgroundTile.Width)
                    {
                        ctx.DrawImage(backgroundTile, new Point(x, y), 1f);
                    }
                }
            });
            finalImage.Mutate(ctx =>
            {
                int iconX = iconPaddingLeft;
                int iconY = iconPaddingTop;
                ctx.DrawImage(iconResult.Image, new Point(iconX, iconY), 1f);

                int textX = iconPaddingLeft + iconSize.Width + iconPaddingRight;
                int textY = textPaddingTop;
                ctx.DrawImage(textResult.Image, new Point(textX, textY), 1f);
            });

            stopwatch.Stop();
            return new ContentAreaRenderResult(finalImage, stopwatch.Elapsed);
        }
    }
}
