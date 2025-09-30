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
                AssetKeys.FrameParts.TopLeft, AssetKeys.FrameParts.TopCenter, AssetKeys.FrameParts.TopRight,
                AssetKeys.FrameParts.MiddleLeft, AssetKeys.FrameParts.MiddleRight,
                AssetKeys.FrameParts.BottomLeft, AssetKeys.FrameParts.BottomCenter, AssetKeys.FrameParts.BottomRight
            };

            if (style.SystemInfo.IsCross)
            {
                requiredParts.AddRange(new[] { AssetKeys.FrameParts.Cross, AssetKeys.FrameParts.CrossDisabled });
            }

            var assets = AssetLoader.LoadRequiredImages(style, am.GetStyleImage, requiredParts.ToArray());

            var middleLeft = assets[AssetKeys.FrameParts.MiddleLeft];
            var middleRight = assets[AssetKeys.FrameParts.MiddleRight];
            var topCenter = assets[AssetKeys.FrameParts.TopCenter];
            var bottomCenter = assets[AssetKeys.FrameParts.BottomCenter];
            var topLeft = assets[AssetKeys.FrameParts.TopLeft];
            var topRight = assets[AssetKeys.FrameParts.TopRight];
            var bottomLeft = assets[AssetKeys.FrameParts.BottomLeft];
            var bottomRight = assets[AssetKeys.FrameParts.BottomRight];

            int contentWidth = contentResult.Image.Width;
            int contentHeight = contentResult.Image.Height;
            int buttonAreaHeight = (buttonResult != null && buttonResult.Image.Height > 1) ? buttonResult.Image.Height : 0;
            int totalInnerHeight = contentHeight + buttonAreaHeight;

            int totalWidth = middleLeft.Width + contentWidth + middleRight.Width;
            int totalHeight = topCenter.Height + totalInnerHeight + bottomCenter.Height;

            var frameImage = new Image<Rgba32>(Math.Max(1, totalWidth), Math.Max(1, totalHeight));

            frameImage.Mutate(ctx =>
            {
                ctx.DrawImage(topLeft, new Point(0, 0), 1f);
                ctx.DrawImage(topRight, new Point(totalWidth - topRight.Width, 0), 1f);
                ctx.DrawImage(bottomLeft, new Point(0, totalHeight - bottomLeft.Height), 1f);
                ctx.DrawImage(bottomRight, new Point(totalWidth - bottomRight.Width, totalHeight - bottomRight.Height), 1f);

                var topCenterMode = metrics.FramePartRenderModes.GetValueOrDefault(AssetKeys.FrameParts.TopCenter, FramePartRenderMode.Stretch);
                int topCenterWidth = totalWidth - topLeft.Width - topRight.Width;
                DrawPart(ctx, topCenter, topLeft.Width, 0, topCenterWidth, topCenter.Height, topCenterMode);

                var bottomCenterMode = metrics.FramePartRenderModes.GetValueOrDefault(AssetKeys.FrameParts.BottomCenter, FramePartRenderMode.Stretch);
                int bottomCenterWidth = totalWidth - bottomLeft.Width - bottomRight.Width;
                DrawPart(ctx, bottomCenter, bottomLeft.Width, totalHeight - bottomCenter.Height, bottomCenterWidth, bottomCenter.Height, bottomCenterMode);

                var middleLeftMode = metrics.FramePartRenderModes.GetValueOrDefault(AssetKeys.FrameParts.MiddleLeft, FramePartRenderMode.Stretch);
                var middleRightMode = metrics.FramePartRenderModes.GetValueOrDefault(AssetKeys.FrameParts.MiddleRight, FramePartRenderMode.Stretch);
                int sideHeight = totalInnerHeight;
                DrawPart(ctx, middleLeft, 0, topLeft.Height, middleLeft.Width, sideHeight - (topLeft.Height - topCenter.Height), middleLeftMode);
                DrawPart(ctx, middleRight, totalWidth - topRight.Width, topRight.Height, middleRight.Width, sideHeight - (topRight.Height - topCenter.Height), middleRightMode);

                Image<Rgba32>? crossImage = null;
                if (style.SystemInfo.IsCross)
                {
                    crossImage = isCrossActive ? assets[AssetKeys.FrameParts.Cross] : assets[AssetKeys.FrameParts.CrossDisabled];
                }

                if (!string.IsNullOrEmpty(title))
                {
                    int crossImageWidth = (crossImage != null) ? crossImage.Width : 0;
                    int maxTitleWidth = contentWidth - crossImageWidth - metrics.CrossPaddingLeft;

                    using var titleResult = _titleTextRenderer.Value.DrawText(
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
                    using (var resized = source.Clone(c => c.Resize(width, height)))
                    {
                        ctx.DrawImage(resized, new Point(x, y), 1f);
                    }
                    break;
            }
        }
    }
}
