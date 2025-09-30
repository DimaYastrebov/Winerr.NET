using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using Winerr.NET.Core.Configs;
using Winerr.NET.Core.Enums;
using Winerr.NET.Core.Managers;
using Winerr.NET.Core.Models;

namespace Winerr.NET.Core.Renderers
{
    public class ButtonAreaRenderer
    {
        public ButtonAreaRenderer()
        { }

        public Size MeasureButtonArea(IEnumerable<ButtonConfig> buttons, SystemStyle style, bool sort = true)
        {
            var metrics = style.Metrics;
            var buttonRenderer = new ButtonRenderer();

            var validButtons = buttons.Where(b => b != null).ToList();

            if (!validButtons.Any())
            {
                return new Size(0, 0);
            }

            if (sort)
            {
                validButtons = validButtons
                    .OrderBy(b => style.Metrics.ButtonSortOrder.IndexOf(b.Type))
                    .ToList();
            }

            int buttonsBlockWidth = validButtons.Sum(b => buttonRenderer.MeasureButtonWidth(b, style)) +
                                    Math.Max(0, validButtons.Count - 1) * metrics.ButtonSpacing;

            int totalWidth = metrics.ButtonsPaddingLeft + buttonsBlockWidth + metrics.ButtonsPaddingRight;

            var am = AssetManager.Instance;
            var backgroundTile = am.GetStyleImage(style, AssetKeys.FrameParts.ButtonArea);
            int height = backgroundTile?.Height ?? metrics.ButtonHeight;

            return new Size(totalWidth, height);
        }

        public ButtonAreaRenderResult DrawButtonArea(
            IEnumerable<ButtonConfig> buttons,
            SystemStyle style,
            ButtonAlignment alignment,
            int? totalWidth = null, bool sort = true)
        {
            var stopwatch = Stopwatch.StartNew();
            var am = AssetManager.Instance;
            var metrics = style.Metrics;
            var buttonRenderer = new ButtonRenderer();

            var validButtons = buttons.Where(b => b != null).ToList();

            if (!validButtons.Any())
            {
                stopwatch.Stop();
                return new ButtonAreaRenderResult(new Image<Rgba32>(1, 1), stopwatch.Elapsed);
            }

            if (sort)
            {
                validButtons = validButtons
                    .OrderBy(b => style.Metrics.ButtonSortOrder.IndexOf(b.Type))
                    .ToList();
            }

            var renderedButtons = validButtons.Select(b => buttonRenderer.DrawButton(b, style)).ToList();
            int buttonsBlockWidth = renderedButtons.Sum(rb => rb.Image.Width) + Math.Max(0, renderedButtons.Count - 1) * metrics.ButtonSpacing;
            int finalWidth = totalWidth ?? (metrics.ButtonsPaddingLeft + buttonsBlockWidth + metrics.ButtonsPaddingRight);

            var backgroundTile = am.GetStyleImage(style, AssetKeys.FrameParts.ButtonArea);
            if (backgroundTile == null)
            {
                int maxHeightNoBg = renderedButtons.Any() ? renderedButtons.Max(rb => rb.Image.Height) : 1;
                var containerImageNoBg = new Image<Rgba32>(finalWidth, maxHeightNoBg);

                containerImageNoBg.Mutate(ctx =>
                {
                    int currentX = 0;
                    foreach (var buttonResult in renderedButtons)
                    {
                        if (currentX + buttonResult.Image.Width <= finalWidth)
                        {
                            ctx.DrawImage(buttonResult.Image, new Point(currentX, 0), 1f);
                            currentX += buttonResult.Image.Width + metrics.ButtonSpacing;
                        }
                        else
                        {
                            break;
                        }
                    }
                });

                renderedButtons.ForEach(b => b.Dispose());
                stopwatch.Stop();
                return new ButtonAreaRenderResult(containerImageNoBg, stopwatch.Elapsed);
            }

            int finalHeight = backgroundTile.Height;
            var finalImage = new Image<Rgba32>(finalWidth, finalHeight);

            finalImage.Mutate(ctx =>
            {
                for (int x = 0; x < finalWidth; x += backgroundTile.Width)
                {
                    ctx.DrawImage(backgroundTile, new Point(x, 0), 1f);
                }

                int buttonsStartX;
                if (totalWidth.HasValue)
                {
                    switch (alignment)
                    {
                        case ButtonAlignment.Left:
                            buttonsStartX = metrics.ButtonsPaddingLeft;
                            break;
                        case ButtonAlignment.Center:
                            buttonsStartX = (finalWidth - buttonsBlockWidth) / 2;
                            break;
                        case ButtonAlignment.Right:
                        default:
                            buttonsStartX = finalWidth - buttonsBlockWidth - metrics.ButtonsPaddingRight;
                            break;
                    }
                }
                else
                {
                    buttonsStartX = metrics.ButtonsPaddingLeft;
                }

                int currentX = buttonsStartX;
                int rightBoundary = finalWidth - metrics.ButtonsPaddingRight;
                foreach (var buttonResult in renderedButtons)
                {
                    if (currentX + buttonResult.Image.Width > rightBoundary && totalWidth.HasValue)
                    {
                        break;
                    }

                    int buttonY = (finalHeight - buttonResult.Image.Height) / 2;
                    ctx.DrawImage(buttonResult.Image, new Point(currentX, buttonY), 1f);
                    currentX += buttonResult.Image.Width + metrics.ButtonSpacing;
                }
            });

            renderedButtons.ForEach(b => b.Dispose());
            stopwatch.Stop();
            return new ButtonAreaRenderResult(finalImage, stopwatch.Elapsed);
        }
    }
}
