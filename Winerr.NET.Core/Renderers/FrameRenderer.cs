using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using Winerr.NET.Core.Enums;
using Winerr.NET.Core.Helpers;
using Winerr.NET.Core.Managers;
using Winerr.NET.Core.Models;

namespace Winerr.NET.Core.Renderers
{
    public class FrameRenderer(
        SystemStyle style,
        string title,
        ContentAreaRenderResult contentResult,
        ButtonAreaRenderResult? buttonResult,
        bool isCrossActive = true)
    {
        private readonly Lazy<TextRenderer> _titleTextRenderer = new(() => new TextRenderer(style.Metrics.WindowTitleFontSet));

        public FrameRenderResult DrawFrame()
        {
            var stopwatch = Stopwatch.StartNew();
            var metrics = style.Metrics;
            var am = AssetManager.Instance;

            var requiredParts = new List<string>
            {
                "top_left", "top_center", "top_right",
                "middle_left", "middle_right",
                "bottom_left", "bottom_center", "bottom_right"
            };

            if (style.SystemInfo.IsCross)
            {
                requiredParts.AddRange(new[] { "cross", "cross_disabled" });
            }

            var assets = AssetLoader.LoadRequiredImages(style, am.GetStyleImage, requiredParts.ToArray());

            var middleLeft = assets["middle_left"];
            var middleRight = assets["middle_right"];
            var topCenter = assets["top_center"];
            var bottomCenter = assets["bottom_center"];
            var topLeft = assets["top_left"];
            var topRight = assets["top_right"];
            var bottomLeft = assets["bottom_left"];
            var bottomRight = assets["bottom_right"];

            int contentWidth = contentResult.Image.Width;
            int contentHeight = contentResult.Image.Height;
            int buttonAreaHeight = (buttonResult != null && buttonResult.Image.Height > 1) ? buttonResult.Image.Height : 0;
            int totalInnerHeight = contentHeight + buttonAreaHeight;

            int totalWidth = middleLeft.Width + contentWidth + middleRight.Width;
            int totalHeight = topCenter.Height + totalInnerHeight + bottomCenter.Height;

            var frameImage = new Image<Rgba32>(totalWidth, totalHeight);

            frameImage.Mutate(ctx =>
            {
                ctx.DrawImage(topLeft, new Point(0, 0), 1f);
                ctx.DrawImage(topRight, new Point(totalWidth - topRight.Width, 0), 1f);
                ctx.DrawImage(bottomLeft, new Point(0, totalHeight - bottomLeft.Height), 1f);
                ctx.DrawImage(bottomRight, new Point(totalWidth - bottomRight.Width, totalHeight - bottomRight.Height), 1f);

                var topCenterMode = metrics.FramePartRenderModes.GetValueOrDefault("top_center", FramePartRenderMode.Stretch);
                int topCenterWidth = totalWidth - topLeft.Width - topRight.Width;
                DrawPart(ctx, topCenter, topLeft.Width, 0, topCenterWidth, topCenter.Height, topCenterMode);

                var bottomCenterMode = metrics.FramePartRenderModes.GetValueOrDefault("bottom_center", FramePartRenderMode.Stretch);
                int bottomCenterWidth = totalWidth - bottomLeft.Width - bottomRight.Width;
                DrawPart(ctx, bottomCenter, bottomLeft.Width, totalHeight - bottomCenter.Height, bottomCenterWidth, bottomCenter.Height, bottomCenterMode);

                var middleLeftMode = metrics.FramePartRenderModes.GetValueOrDefault("middle_left", FramePartRenderMode.Stretch);
                var middleRightMode = metrics.FramePartRenderModes.GetValueOrDefault("middle_right", FramePartRenderMode.Stretch);
                int sideHeight = totalInnerHeight;
                DrawPart(ctx, middleLeft, 0, topLeft.Height, middleLeft.Width, sideHeight - (topLeft.Height - topCenter.Height), middleLeftMode);
                DrawPart(ctx, middleRight, totalWidth - topRight.Width, topRight.Height, middleRight.Width, sideHeight - (topRight.Height - topCenter.Height), middleRightMode);

                Image<Rgba32>? crossImage = null;
                if (style.SystemInfo.IsCross)
                {
                    crossImage = isCrossActive ? assets["cross"] : assets["cross_disabled"];
                }

                if (!string.IsNullOrEmpty(title))
                {
                    int crossImageWidth = (crossImage != null) ? crossImage.Width : 0;
                    int maxTitleWidth = contentWidth - crossImageWidth - metrics.CrossPaddingLeft;

                    var titleResult = _titleTextRenderer.Value.DrawText(
                        title,
                        maxWidth: maxTitleWidth,
                        variationName: metrics.WindowTitleFontVariation,
                        truncationMode: TextTruncationMode.Ellipsis,
                        lineSpacing: metrics.LineSpacing,
                        shadowConfig: metrics.Shadow
                    );

                    var titleY = (topCenter.Height - titleResult.Dimensions.Height) / 2 + metrics.WindowTitlePadding.Y;
                    var titlePosition = new Point(metrics.WindowTitlePadding.X, titleY);
                    ctx.DrawImage(titleResult.Image, titlePosition, 1f);
                }

                if (crossImage != null)
                {
                    Point anchorPosition;
                    Size anchorSize;

                    switch (metrics.CrossAlignmentAnchor)
                    {
                        case CrossAlignmentAnchor.TopLeft:
                            anchorPosition = new Point(0, 0);
                            anchorSize = topLeft.Size;
                            break;
                        case CrossAlignmentAnchor.TopCenter:
                            anchorPosition = new Point(topLeft.Width, 0);
                            anchorSize = new Size(totalWidth - topLeft.Width - topRight.Width, topCenter.Height);
                            break;
                        case CrossAlignmentAnchor.TopRight:
                        default:
                            anchorPosition = new Point(totalWidth - topRight.Width, 0);
                            anchorSize = topRight.Size;
                            break;
                    }

                    int centeredX = anchorPosition.X + (anchorSize.Width - crossImage.Width) / 2;
                    int centeredY = anchorPosition.Y + (anchorSize.Height - crossImage.Height) / 2;
                    var finalCrossPosition = new Point(centeredX + metrics.CrossOffset.X, centeredY + metrics.CrossOffset.Y);

                    ctx.DrawImage(crossImage, finalCrossPosition, 1f);
                }
            });

            stopwatch.Stop();
            return new FrameRenderResult(frameImage, stopwatch.Elapsed);
        }

        private void DrawPart(IImageProcessingContext ctx, Image<Rgba32> source, int x, int y, int width, int height, FramePartRenderMode mode)
        {
            if (width <= 0 || height <= 0) return;
            if (source.Width == width && source.Height == height)
            {
                ctx.DrawImage(source, new Point(x, y), 1f);
                return;
            }

            switch (mode)
            {
                case FramePartRenderMode.Tile:
                    var brush = new ImageBrush(source);

                    ctx.Fill(brush, new RectangleF(x, y, width, height));
                    break;

                case FramePartRenderMode.Stretch:
                default:
                    var resized = source.Clone(c => c.Resize(width, height));
                    ctx.DrawImage(resized, new Point(x, y), 1f);
                    break;
            }
        }
    }
}