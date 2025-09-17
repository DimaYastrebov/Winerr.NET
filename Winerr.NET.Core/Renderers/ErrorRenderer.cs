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

            using var initialContentResult = contentRenderer.DrawContentArea(forcedWidth: null);

            var buttonAreaRenderer = new ButtonAreaRenderer();

            using var initialButtonAreaResult = buttonAreaRenderer.DrawButtonArea(
                config.Buttons,
                style,
                config.ButtonAlignment,
                totalWidth: null
            );

            int finalContentWidth = Math.Max(initialContentResult.Image.Width, initialButtonAreaResult.Image.Width);

            var finalContentResult = initialContentResult;
            var finalButtonAreaResult = initialButtonAreaResult;

            if (initialContentResult.Image.Width < finalContentWidth)
            {
                finalContentResult = contentRenderer.DrawContentArea(forcedWidth: finalContentWidth);
            }
            else if (initialButtonAreaResult.Image.Width < finalContentWidth)
            {
                finalButtonAreaResult = buttonAreaRenderer.DrawButtonArea(
                    config.Buttons,
                    style,
                    config.ButtonAlignment,
                    totalWidth: finalContentWidth
                );
            }

            var frameRenderer = new FrameRenderer(
                style: style,
                title: config.Title,
                contentResult: finalContentResult,
                buttonResult: finalButtonAreaResult,
                isCrossActive: config.IsCrossEnabled
            );

            using var frameResult = frameRenderer.DrawFrame();

            var leftBorder = am.GetStyleImage(style, "middle_left") ?? throw new FileNotFoundException("middle_left sprite not found");
            var topBorder = am.GetStyleImage(style, "top_center") ?? throw new FileNotFoundException("top_center sprite not found");

            var finalImage = frameResult.Image.Clone();

            var contentPosition = new Point(leftBorder.Width, topBorder.Height);
            var buttonAreaPosition = new Point(leftBorder.Width, topBorder.Height + finalContentResult.Image.Height);

            finalImage.Mutate(ctx =>
            {
                ctx.DrawImage(finalContentResult.Image, contentPosition, 1f);
                ctx.DrawImage(finalButtonAreaResult.Image, buttonAreaPosition, 1f);
            });

            if (!ReferenceEquals(initialContentResult, finalContentResult))
            {
                finalContentResult.Dispose();
            }

            if (!ReferenceEquals(initialButtonAreaResult, finalButtonAreaResult))
            {
                finalButtonAreaResult.Dispose();
            }

            return finalImage;
        }
    }
}
