using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Winerr.NET.Core.Configs;
using Winerr.NET.Core.Enums;
using Winerr.NET.Core.Interfaces;
using Winerr.NET.Core.Managers;

namespace Winerr.NET.Core.Renderers
{
    public class ErrorRenderer : IRenderer
    {
        public Image<Rgba32> Generate(ErrorConfig config)
        {
            var style = config.SystemStyle;
            var metrics = style.Metrics;
            var am = AssetManager.Instance;

            var leftBorder = am.GetStyleImage(style, AssetKeys.FrameParts.MiddleLeft) ?? throw new FileNotFoundException("middle_left sprite not found");
            var rightBorder = am.GetStyleImage(style, AssetKeys.FrameParts.MiddleRight) ?? throw new FileNotFoundException("middle_right sprite not found");

            var contentRenderer = new ContentAreaRenderer(
                text: config.Content,
                iconId: config.IconId,
                style: style,
                maxWidth: null,
                wrapMode: TextWrapMode.Auto
            );
            var buttonAreaRenderer = new ButtonAreaRenderer();

            var naturalContentSize = contentRenderer.MeasureContentArea();
            var naturalButtonAreaSize = buttonAreaRenderer.MeasureButtonArea(config.Buttons, style, config.SortButtons);

            int minWidthForIcon = leftBorder.Width + metrics.IconPaddingLeft + metrics.ExpectedIconSize.Width + metrics.IconPaddingRight + rightBorder.Width;
            int minWidthForButtons = 0;
            if (naturalButtonAreaSize.Width > 0)
            {
                minWidthForButtons = naturalButtonAreaSize.Width + leftBorder.Width + rightBorder.Width;
            }

            int minWidth = Math.Max(minWidthForIcon, minWidthForButtons);

            int naturalContentWidth = Math.Max(naturalContentSize.Width, naturalButtonAreaSize.Width);
            int naturalTotalWidth = naturalContentWidth + leftBorder.Width + rightBorder.Width;

            int? limit = config.MaxWidth ?? metrics.DefaultMaxWidth;
            int widthWithLimit = limit.HasValue ? Math.Min(naturalTotalWidth, limit.Value) : naturalTotalWidth;
            int finalTotalWidth = Math.Max(widthWithLimit, minWidth);
            int finalContentWidth = Math.Max(0, finalTotalWidth - leftBorder.Width - rightBorder.Width);

            using var finalContentResult = contentRenderer.DrawContentArea(forcedWidth: finalContentWidth);

            using var finalButtonAreaResult = buttonAreaRenderer.DrawButtonArea(
                config.Buttons,
                style,
                config.ButtonAlignment,
                totalWidth: finalContentWidth,
                sort: config.SortButtons
            );

            var frameRenderer = new FrameRenderer(
                style: style,
                title: config.Title,
                contentResult: finalContentResult,
                buttonResult: finalButtonAreaResult,
                isCrossActive: config.IsCrossEnabled
            );

            using var frameResult = frameRenderer.DrawFrame();

            var leftBorderFinal = am.GetStyleImage(style, AssetKeys.FrameParts.MiddleLeft) ?? throw new FileNotFoundException("middle_left sprite not found");
            var topBorderFinal = am.GetStyleImage(style, AssetKeys.FrameParts.TopCenter) ?? throw new FileNotFoundException("top_center sprite not found");

            var finalImage = frameResult.Image.Clone();

            var contentPosition = new Point(leftBorderFinal.Width, topBorderFinal.Height);
            var buttonAreaPosition = new Point(leftBorderFinal.Width, topBorderFinal.Height + finalContentResult.Image.Height);

            finalImage.Mutate(ctx =>
            {
                ctx.DrawImage(finalContentResult.Image, contentPosition, 1f);
                if (finalButtonAreaResult.Image.Height > 1)
                {
                    ctx.DrawImage(finalButtonAreaResult.Image, buttonAreaPosition, 1f);
                }
            });

            return finalImage;
        }
    }
}
