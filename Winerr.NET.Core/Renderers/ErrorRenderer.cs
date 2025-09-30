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
            var am = AssetManager.Instance;


            var contentRenderer = new ContentAreaRenderer(
                text: config.Content,
                iconId: config.IconId,
                style: style,
                maxWidth: config.MaxWidth,
                wrapMode: TextWrapMode.Auto
            );
            var contentSize = contentRenderer.MeasureContentArea();

            var buttonAreaRenderer = new ButtonAreaRenderer();
            var buttonAreaSize = buttonAreaRenderer.MeasureButtonArea(config.Buttons, style, config.SortButtons);

            int finalContentWidth = Math.Max(contentSize.Width, buttonAreaSize.Width);
            if (config.MaxWidth.HasValue)
            {
                var leftBorder = am.GetStyleImage(style, AssetKeys.FrameParts.MiddleLeft) ?? throw new FileNotFoundException("middle_left sprite not found");
                var rightBorder = am.GetStyleImage(style, AssetKeys.FrameParts.MiddleRight) ?? throw new FileNotFoundException("middle_right sprite not found");
                int maxInnerWidth = config.MaxWidth.Value - leftBorder.Width - rightBorder.Width;
                if (maxInnerWidth < 0) maxInnerWidth = 0;
                finalContentWidth = Math.Min(finalContentWidth, maxInnerWidth);
            }

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
