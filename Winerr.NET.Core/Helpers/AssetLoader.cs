using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Winerr.NET.Core.Enums;

namespace Winerr.NET.Core.Helpers
{
    internal static class AssetLoader
    {
        public static Dictionary<string, Image<Rgba32>> LoadRequiredImages(
            SystemStyle style,
            Func<SystemStyle, string, Image<Rgba32>?> loaderFunc,
            params string[] assetNames)
        {
            var loadedAssets = new Dictionary<string, Image<Rgba32>>(StringComparer.OrdinalIgnoreCase);
            var missingParts = new List<string>();

            foreach (var name in assetNames)
            {
                var image = loaderFunc(style, name);
                if (image != null)
                {
                    loadedAssets[name] = image;
                }
                else
                {
                    missingParts.Add(name);
                }
            }

            if (missingParts.Any())
            {
                string expected = string.Join(", ", assetNames);
                string missing = string.Join(", ", missingParts);

                throw new FileNotFoundException(
                    $"Failed to load required assets for style '{style.Id}'.\n" +
                    $"Expected parts: [{expected}]\n" +
                    $"Missing parts: [{missing}]"
                );
            }

            return loadedAssets;
        }
    }
}
