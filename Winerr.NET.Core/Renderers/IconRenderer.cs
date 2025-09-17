using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using Winerr.NET.Core.Enums;
using Winerr.NET.Core.Managers;
using Winerr.NET.Core.Models;

namespace Winerr.NET.Core.Renderers
{
    public class IconRenderer
    {
        public IconRenderer()
        {
        }
        public IconRenderResult DrawIcon(int iconId, SystemStyle style, bool ignoreMissing = false, IconShrinkMode shrinkMode = IconShrinkMode.None)
        {
            var stopwatch = Stopwatch.StartNew();
            var am = AssetManager.Instance;

            var iconImage = am.GetIcon(style, iconId);

            if (iconImage == null)
            {
                if (ignoreMissing)
                {
                    var expectedSize = style.Metrics.ExpectedIconSize;
                    var emptyImage = new Image<Rgba32>(expectedSize.Width > 0 ? expectedSize.Width : 1, expectedSize.Height > 0 ? expectedSize.Height : 1);
                    stopwatch.Stop();
                    return new IconRenderResult(emptyImage, -1, stopwatch.Elapsed);
                }
                else
                {
                    throw new FileNotFoundException($"Icon with ID '{iconId}' was not found for style '{style.Id}' or its aliases.");
                }
            }

            var finalIconImage = iconImage.Clone();
            var expectedIconSize = style.Metrics.ExpectedIconSize;

            if (shrinkMode != IconShrinkMode.None)
            {
                bool needsResize = false;

                if (shrinkMode == IconShrinkMode.ToFit)
                {
                    if (finalIconImage.Width > expectedIconSize.Width || finalIconImage.Height > expectedIconSize.Height)
                    {
                        needsResize = true;
                    }
                }
                else if (shrinkMode == IconShrinkMode.Exact)
                {
                    if (finalIconImage.Width != expectedIconSize.Width || finalIconImage.Height != expectedIconSize.Height)
                    {
                        needsResize = true;
                    }
                }

                if (needsResize)
                {
                    finalIconImage.Mutate(ctx => ctx.Resize(expectedIconSize));
                }
            }

            stopwatch.Stop();
            return new IconRenderResult(finalIconImage, iconId, stopwatch.Elapsed);
        }
    }
}
