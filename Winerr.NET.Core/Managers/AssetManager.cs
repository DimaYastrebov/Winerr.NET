using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using Winerr.NET.Core.Enums;
using Winerr.NET.Core.Models.Assets;
using Winerr.NET.Core.Models.Fonts;

namespace Winerr.NET.Core.Managers
{
    public record FontInfoDto(string Name, Dictionary<string, FontSizeInfoDto> Sizes);
    public record FontSizeInfoDto(List<string> Variations);

    public sealed class AssetManager : IDisposable
    {
        private static readonly Lazy<AssetManager> _lazyInstance = new(() => new AssetManager());
        public static AssetManager Instance => _lazyInstance.Value;

        private readonly Dictionary<string, StyleDefinition> _styles = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, FontDefinition> _fonts = new(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, Image<Rgba32>> _imageCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, BitmapFont> _fontMetricsCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, FontSet> _fontSetCache = new(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, (SevenZipArchive Archive, Stream Stream)> _openArchives = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Dictionary<int, IArchiveEntry>> _iconIndex = new(StringComparer.OrdinalIgnoreCase);
        private const string IconArchiveName = "Archive.7z";

        private Assembly? _assetsAssembly;
        private bool _isLoaded = false;
        private bool _disposed = false;

        private AssetManager() { }

        public void LoadAssets()
        {
            if (_isLoaded) return;

            try
            {
                _assetsAssembly = Assembly.Load("Winerr.NET.Assets");
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException("Critical error: Winerr.NET.Assets.dll not found.");
            }

            var allResourceNames = _assetsAssembly.GetManifestResourceNames();
            ParseAndBuildAssetTree(allResourceNames);
            IndexIconArchives(allResourceNames);

            _isLoaded = true;
        }

        public FontSet? GetFontSet(string fontName, string sizeKey, string variationName)
        {
            string cacheKey = $"{fontName}_{sizeKey}_{variationName}";
            if (_fontSetCache.TryGetValue(cacheKey, out var cachedFontSet))
            {
                return cachedFontSet;
            }

            if (!_fonts.TryGetValue(fontName, out var fontDef) || !fontDef.Sizes.TryGetValue(sizeKey, out var sizeDef))
            {
                return null;
            }

            var variationParts = variationName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            string? metricsPath = FindMetricsPathForVariation(sizeDef.VariationRoot, variationParts);

            if (string.IsNullOrEmpty(metricsPath))
            {
                return null;
            }

            var metrics = LoadMetricsFromResource(metricsPath);
            if (metrics == null)
            {
                return null;
            }

            var newFontSet = new FontSet(metrics);
            LoadFontVariationsRecursive(sizeDef.VariationRoot, "", newFontSet);

            _fontSetCache[cacheKey] = newFontSet;
            return newFontSet;
        }

        public Image<Rgba32>? GetIcon(SystemStyle style, int iconId)
        {
            var (styleName, themeName) = ParseSystemStyleId(style.Id);
            string cacheKey = $"icon_{styleName}_{iconId}";

            if (_imageCache.TryGetValue(cacheKey, out var cachedImage))
            {
                return cachedImage;
            }

            if (_iconIndex.TryGetValue(styleName, out var iconDict) && iconDict.TryGetValue(iconId, out var iconEntry))
            {
                using var entryStream = iconEntry.OpenEntryStream();
                using var memoryStream = new MemoryStream();
                entryStream.CopyTo(memoryStream);
                memoryStream.Position = 0;

                var image = Image.Load<Rgba32>(memoryStream);
                _imageCache[cacheKey] = image;
                return image;
            }

            var resourcePath = FindResourcePath(
                style,
                iconId,
                (theme, key) => theme.IconPaths.TryGetValue(key, out var path) ? path : null,
                (styleDef, key) => styleDef.GlobalIconPaths.TryGetValue(key, out var path) ? path : null
            );

            return resourcePath != null ? LoadImageFromResource(resourcePath) : null;
        }

        public Image<Rgba32>? GetStyleImage(SystemStyle style, string partName)
        {
            var resourcePath = FindResourcePath(
                style,
                partName,
                (theme, key) => theme.FramePartPaths.TryGetValue(key, out var path) ? path : null,
                (styleDef, key) => null
            );

            return resourcePath != null ? LoadImageFromResource(resourcePath) : null;
        }

        public Image<Rgba32>? GetButtonImage(SystemStyle style, string partName)
        {
            var resourcePath = FindResourcePath(
                style,
                partName,
                (theme, key) => theme.ButtonPaths.TryGetValue(key, out var path) ? path : null,
                (styleDef, key) => styleDef.GlobalButtonPaths.TryGetValue(key, out var path) ? path : null
            );

            return resourcePath != null ? LoadImageFromResource(resourcePath) : null;
        }

        public Dictionary<int, Image<Rgba32>> GetAllIconsForStyle(string styleName)
        {
            if (!_iconIndex.TryGetValue(styleName, out var iconDict))
            {
                return new Dictionary<int, Image<Rgba32>>();
            }

            var result = new Dictionary<int, Image<Rgba32>>();
            foreach (var kvp in iconDict)
            {
                var iconId = kvp.Key;
                var iconEntry = kvp.Value;

                string cacheKey = $"icon_{styleName}_{iconId}";

                if (_imageCache.TryGetValue(cacheKey, out var cachedImage))
                {
                    result[iconId] = cachedImage;
                }
                else
                {
                    using var entryStream = iconEntry.OpenEntryStream();
                    using var memoryStream = new MemoryStream();
                    entryStream.CopyTo(memoryStream);
                    memoryStream.Position = 0;

                    var image = Image.Load<Rgba32>(memoryStream);
                    _imageCache[cacheKey] = image;
                    result[iconId] = image;
                }
            }

            return result;
        }

        public Stream? GetResourceStream(string fullResourceName)
        {
            if (_assetsAssembly == null)
            {
                LoadAssets();
                if (_assetsAssembly == null) return null;
            }
            return _assetsAssembly.GetManifestResourceStream(fullResourceName);
        }

        private void IndexIconArchives(string[] resourceNames)
        {
            if (_assetsAssembly == null) return;

            var iconArchives = resourceNames
                .Where(name => name.EndsWith(IconArchiveName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var resourcePath in iconArchives)
            {
                var parts = resourcePath.Split('.');
                if (parts.Length < 5) continue;

                string styleName = parts[4];

                var stream = _assetsAssembly.GetManifestResourceStream(resourcePath);
                if (stream == null) continue;

                var archive = SevenZipArchive.Open(stream);
                _openArchives[styleName] = (archive, stream);

                var iconDict = new Dictionary<int, IArchiveEntry>();
                foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                {
                    if (int.TryParse(Path.GetFileNameWithoutExtension(entry.Key), out int iconId))
                    {
                        iconDict[iconId] = entry;
                    }
                }
                _iconIndex[styleName] = iconDict;
            }
        }

        public int GetMaxIconIdForStyle(string styleName)
        {
            if (_iconIndex.TryGetValue(styleName, out var iconDict))
            {
                return iconDict.Keys.Any() ? iconDict.Keys.Max() : -1;
            }
            return -1;
        }

        private void ParseAndBuildAssetTree(string[] resourceNames)
        {
            const string prefix = "Winerr.NET.Assets.";
            foreach (var name in resourceNames.OrderBy(n => n.Length))
            {
                if (!name.StartsWith(prefix)) continue;

                if (name.EndsWith(IconArchiveName, StringComparison.OrdinalIgnoreCase)) continue;

                var path = name.Substring(prefix.Length);
                var parts = path.Split('.');

                if (parts.Length < 3) continue;

                if (parts[0].Equals("Styles", StringComparison.OrdinalIgnoreCase))
                {
                    ParseStyleResource(parts.Skip(1).ToArray(), name);
                }
                else if (parts[0].Equals("Fonts", StringComparison.OrdinalIgnoreCase))
                {
                    ParseFontResource(parts.Skip(1).ToArray(), name);
                }
            }
        }

        private void ParseStyleResource(string[] parts, string fullPath)
        {
            if (parts.Length < 3) return;

            string styleName = parts[0];
            var styleDef = GetOrCreateStyle(styleName);
            string resourceType = parts[1];

            string fileName = string.Join(".", parts.Take(parts.Length - 1).TakeLast(parts.Length - 2));

            switch (resourceType.ToLower())
            {
                case "icons" when parts.Length == 4 && int.TryParse(parts[2], out int iconId):
                    styleDef.GlobalIconPaths[iconId] = fullPath;
                    break;

                case "buttons" when parts.Length == 4:
                    styleDef.GlobalButtonPaths[parts[2]] = fullPath;
                    break;

                case "themes" when parts.Length >= 5:
                    string themeName = parts[2];
                    var themeDef = GetOrCreateTheme(styleDef, themeName);

                    if (parts.Length == 5)
                    {
                        themeDef.FramePartPaths[parts[3]] = fullPath;
                    }
                    else if (parts.Length == 6)
                    {
                        string subfolder = parts[3];
                        string subfolderFileName = parts[4];

                        if (subfolder.Equals("Icons", StringComparison.OrdinalIgnoreCase) && int.TryParse(subfolderFileName, out int themeIconId))
                        {
                            themeDef.IconPaths[themeIconId] = fullPath;
                        }
                        else if (subfolder.Equals("Buttons", StringComparison.OrdinalIgnoreCase))
                        {
                            themeDef.ButtonPaths[subfolderFileName] = fullPath;
                        }
                    }
                    break;
            }
        }

        private void ParseFontResource(string[] parts, string fullPath)
        {
            if (parts.Length < 2) return;

            var cleanParts = parts.Select(p =>
                (p.Length > 1 && p[0] == '_' && char.IsDigit(p[1])) ? p.Substring(1) : p
            ).ToList();

            string fontName = cleanParts[0];
            var fontDef = GetOrCreateFont(fontName);

            if (cleanParts.Count < 4) return;

            string sizeKey = cleanParts[1];
            var sizeDef = GetOrCreateFontSize(fontDef, sizeKey);

            var variationPathParts = cleanParts.Skip(2).Take(cleanParts.Count - 4).ToList();

            FontVariationNode currentNode = sizeDef.VariationRoot;
            foreach (var part in variationPathParts)
            {
                if (!currentNode.Children.TryGetValue(part, out var childNode))
                {
                    childNode = new FontVariationNode(part);
                    currentNode.Children[part] = childNode;
                }
                currentNode = childNode;
            }

            string fileName = string.Join(".", cleanParts.TakeLast(2));

            if (!currentNode.Children.TryGetValue(fileName, out var fileNode))
            {
                fileNode = new FontVariationNode(fileName);
                currentNode.Children[fileNode.Name] = fileNode;
            }
            fileNode.SpriteSheetPath = fullPath;
        }

        private string? FindResourcePath<TKey>(
            SystemStyle style,
            TKey key,
            Func<ThemeDefinition, TKey, string?> themePathSelector,
            Func<StyleDefinition, TKey, string?> globalPathSelector)
        {
            var currentStyle = style;

            while (currentStyle != null)
            {
                var (styleName, themeName) = ParseSystemStyleId(currentStyle.Id);

                if (_styles.TryGetValue(styleName, out var styleDef))
                {
                    if (styleDef.Themes.TryGetValue(themeName, out var themeDef))
                    {
                        var path = themePathSelector(themeDef, key);
                        if (path != null) return path;
                    }
                    if (styleDef.Themes.TryGetValue("default", out var defaultThemeDef))
                    {
                        var path = themePathSelector(defaultThemeDef, key);
                        if (path != null) return path;
                    }
                    var globalPath = globalPathSelector(styleDef, key);
                    if (globalPath != null) return globalPath;
                }
                currentStyle = currentStyle.ResourceAliasFor;
            }

            return null;
        }

        private (string styleName, string themeName) ParseSystemStyleId(string id)
        {
            var parts = id.Split('_', 2);
            return parts.Length >= 2 ? (parts[0], parts[1]) : (parts[0], "default");
        }

        private string? FindMetricsPathForVariation(FontVariationNode rootNode, string[] variationPathParts)
        {
            for (int i = variationPathParts.Length; i >= 0; i--)
            {
                var currentPathParts = variationPathParts.Take(i);
                FontVariationNode currentNode = rootNode;
                bool pathExists = true;

                foreach (var part in currentPathParts)
                {
                    if (currentNode.Children.TryGetValue(part, out var nextNode))
                    {
                        currentNode = nextNode;
                    }
                    else
                    {
                        pathExists = false;
                        break;
                    }
                }

                if (!pathExists) continue;

                if (currentNode.Children.TryGetValue("spritesheet.xml", out var metricsNode) && !string.IsNullOrEmpty(metricsNode.SpriteSheetPath))
                {
                    return metricsNode.SpriteSheetPath;
                }
            }

            if (rootNode.Children.TryGetValue("spritesheet.xml", out var rootMetricsNode) && !string.IsNullOrEmpty(rootMetricsNode.SpriteSheetPath))
            {
                return rootMetricsNode.SpriteSheetPath;
            }

            return null;
        }

        private void LoadFontVariationsRecursive(FontVariationNode node, string currentPath, FontSet fontSet)
        {
            if (fontSet.Metrics == null) return;

            var pngNode = node.Children
                .FirstOrDefault(kvp => kvp.Key.Equals("spritesheet.png", StringComparison.OrdinalIgnoreCase))
                .Value;

            if (pngNode != null && !string.IsNullOrEmpty(pngNode.SpriteSheetPath))
            {
                var variationImage = LoadImageFromResource(pngNode.SpriteSheetPath);
                if (variationImage != null)
                {
                    fontSet.Variations[currentPath] = variationImage;
                    var precutCache = new Dictionary<int, Image<Rgba32>>();

                    foreach (var fontChar in fontSet.Metrics.Characters.Values)
                    {
                        if (fontChar.Source.Width > 0 && fontChar.Source.Height > 0)
                        {
                            var sourceRect = new Rectangle(0, 0, variationImage.Width, variationImage.Height);
                            if (sourceRect.Contains(fontChar.Source))
                            {
                                precutCache[fontChar.Id] = variationImage.Clone(ctx => ctx.Crop(fontChar.Source));
                            }
                        }
                    }
                    fontSet.PrecutGlyphs[currentPath] = precutCache;
                }
            }

            foreach (var child in node.Children.Values.Where(n => !n.Name.StartsWith("spritesheet.")))
            {
                string newPath = string.IsNullOrEmpty(currentPath) ? child.Name : $"{currentPath}.{child.Name}";
                LoadFontVariationsRecursive(child, newPath, fontSet);
            }
        }

        private StyleDefinition GetOrCreateStyle(string name)
        {
            if (!_styles.TryGetValue(name, out var style))
            {
                style = new StyleDefinition(name);
                _styles[name] = style;
            }
            return style;
        }

        private ThemeDefinition GetOrCreateTheme(StyleDefinition styleDef, string name)
        {
            if (!styleDef.Themes.TryGetValue(name, out var theme))
            {
                theme = new ThemeDefinition(name);
                styleDef.Themes[name] = theme;
            }
            return theme;
        }

        private FontDefinition GetOrCreateFont(string name)
        {
            if (!_fonts.TryGetValue(name, out var font))
            {
                font = new FontDefinition(name);
                _fonts[name] = font;
            }
            return font;
        }

        private FontSizeDefinition GetOrCreateFontSize(FontDefinition fontDef, string sizeKey)
        {
            if (!fontDef.Sizes.TryGetValue(sizeKey, out var sizeDef))
            {
                sizeDef = new FontSizeDefinition(sizeKey);
                fontDef.Sizes[sizeKey] = sizeDef;
            }
            return sizeDef;
        }

        private Image<Rgba32>? LoadImageFromResource(string resourcePath)
        {
            if (_imageCache.TryGetValue(resourcePath, out var cachedImage))
            {
                return cachedImage;
            }

            if (_assetsAssembly == null) return null;

            using var stream = _assetsAssembly.GetManifestResourceStream(resourcePath);
            if (stream == null) return null;

            var image = Image.Load<Rgba32>(stream);
            _imageCache[resourcePath] = image;
            return image;
        }

        private BitmapFont? LoadMetricsFromResource(string resourcePath)
        {
            if (_fontMetricsCache.TryGetValue(resourcePath, out var cachedMetrics))
            {
                return cachedMetrics;
            }

            if (_assetsAssembly == null) return null;

            using var stream = _assetsAssembly.GetManifestResourceStream(resourcePath);
            if (stream == null) return null;

            using var reader = new StreamReader(stream, Encoding.UTF8);
            var xmlContent = reader.ReadToEnd();
            var metrics = BitmapFont.FromXml(xmlContent);
            _fontMetricsCache[resourcePath] = metrics;
            return metrics;
        }

        public Dictionary<string, Dictionary<string, string>> GetStyleAssetPaths(SystemStyle style)
        {
            var assetPaths = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            if (!_styles.TryGetValue(style.Id.Split('_')[0], out var styleDef))
            {
                return assetPaths;
            }

            var frameParts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var buttons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var btn in styleDef.GlobalButtonPaths)
            {
                buttons[btn.Key] = btn.Value;
            }

            if (styleDef.Themes.TryGetValue(style.Id.Split('_').Length > 1 ? style.Id.Split('_')[1] : "default", out var themeDef))
            {
                foreach (var part in themeDef.FramePartPaths)
                {
                    frameParts[part.Key] = part.Value;
                }
                foreach (var btn in themeDef.ButtonPaths)
                {
                    buttons[btn.Key] = btn.Value;
                }
            }

            var buttonAreaPath = FindResourcePath(style, "button_area", (theme, key) => theme.FramePartPaths.GetValueOrDefault(key), (s, k) => null);
            if (buttonAreaPath != null)
            {
                frameParts["button_area"] = buttonAreaPath;
            }

            var middleCenterPath = FindResourcePath(style, "middle_center", (theme, key) => theme.FramePartPaths.GetValueOrDefault(key), (s, k) => null);
            if (middleCenterPath != null)
            {
                frameParts["middle_center"] = middleCenterPath;
            }


            assetPaths["frame_parts"] = frameParts;
            assetPaths["buttons"] = buttons;

            return assetPaths;
        }

        public List<FontInfoDto> GetFontInfo()
        {
            var fontInfoList = new List<FontInfoDto>();

            foreach (var fontDef in _fonts.Values)
            {
                var sizesDict = new Dictionary<string, FontSizeInfoDto>();
                foreach (var sizeDef in fontDef.Sizes.Values)
                {
                    var variations = new List<string>();
                    CollectVariationPaths(sizeDef.VariationRoot, "", variations);
                    sizesDict[sizeDef.SizeKey] = new FontSizeInfoDto(variations.Distinct().OrderBy(v => v).ToList());
                }
                fontInfoList.Add(new FontInfoDto(fontDef.Name, sizesDict));
            }

            return fontInfoList;
        }

        private void CollectVariationPaths(FontVariationNode node, string currentPath, List<string> paths)
        {
            if (node.Children.Any(c => c.Key.Equals("spritesheet.png", StringComparison.OrdinalIgnoreCase)))
            {
                var cleanPath = currentPath.StartsWith("root.") ? currentPath.Substring(5) : currentPath;
                if (!string.IsNullOrEmpty(cleanPath))
                {
                    paths.Add(cleanPath);
                }
            }

            foreach (var child in node.Children.Values)
            {
                if (child.Name.StartsWith("spritesheet.")) continue;

                string newPath = string.IsNullOrEmpty(currentPath) ? child.Name : $"{currentPath}.{child.Name}";
                CollectVariationPaths(child, newPath, paths);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            foreach (var (archive, stream) in _openArchives.Values)
            {
                archive.Dispose();
                stream.Dispose();
            }

            _openArchives.Clear();
            _iconIndex.Clear();

            foreach (var image in _imageCache.Values)
            {
                image.Dispose();
            }
            _imageCache.Clear();

            foreach (var fontSet in _fontSetCache.Values)
            {
                if (fontSet != null)
                {
                    foreach (var variationImage in fontSet.Variations.Values)
                    {
                        variationImage?.Dispose();
                    }
                }

                if (fontSet?.PrecutGlyphs != null)
                {
                    foreach (var precutDict in fontSet.PrecutGlyphs.Values)
                    {
                        foreach (var glyphImage in precutDict.Values)
                        {
                            glyphImage?.Dispose();
                        }
                    }
                }
            }
            _fontSetCache.Clear();

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        ~AssetManager()
        {
            Dispose();
        }
    }
}
