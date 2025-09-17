using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Winerr.NET.Core.Models;

namespace Winerr.NET.Core.Services
{
    public class SpritesheetService
    {
        public SpritesheetService()
        {
        }

        public SpritesheetResult Generate(Dictionary<int, Image<Rgba32>> icons, Size expectedIconSize)
        {
            if (!icons.Any() || expectedIconSize.Width <= 0 || expectedIconSize.Height <= 0)
            {
                return new SpritesheetResult(new Image<Rgba32>(1, 1), new Dictionary<int, Point>(), Size.Empty);
            }

            var sortedIcons = icons.OrderBy(kvp => kvp.Key).ToList();

            int iconsPerRow = (int)Math.Ceiling(Math.Sqrt(sortedIcons.Count));
            if (iconsPerRow == 0) iconsPerRow = 1;

            int spritesheetWidth = iconsPerRow * expectedIconSize.Width;
            int numRows = (int)Math.Ceiling((double)sortedIcons.Count / iconsPerRow);
            int spritesheetHeight = numRows * expectedIconSize.Height;

            if (spritesheetWidth == 0) spritesheetWidth = 1;
            if (spritesheetHeight == 0) spritesheetHeight = 1;

            var spritesheet = new Image<Rgba32>(spritesheetWidth, spritesheetHeight);
            var iconMap = new Dictionary<int, Point>();

            int currentX = 0;
            int currentY = 0;

            foreach (var iconPair in sortedIcons)
            {
                var iconId = iconPair.Key;
                var iconImage = iconPair.Value;

                if (iconImage.Width != expectedIconSize.Width || iconImage.Height != expectedIconSize.Height)
                {
                    iconImage.Mutate(ctx => ctx.Resize(expectedIconSize));
                }

                var drawPosition = new Point(currentX, currentY);
                spritesheet.Mutate(ctx => ctx.DrawImage(iconImage, drawPosition, 1f));

                iconMap[iconId] = drawPosition;

                currentX += expectedIconSize.Width;
                if (currentX >= spritesheetWidth)
                {
                    currentX = 0;
                    currentY += expectedIconSize.Height;
                }
            }

            return new SpritesheetResult(spritesheet, iconMap, expectedIconSize);
        }
    }
}
