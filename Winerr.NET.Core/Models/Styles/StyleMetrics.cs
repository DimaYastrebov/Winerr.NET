using SixLabors.ImageSharp;
using System.Collections;
using System.Reflection;
using Winerr.NET.Core.Enums;
using Winerr.NET.Core.Managers;
using Winerr.NET.Core.Models.Fonts;

namespace Winerr.NET.Core.Models.Styles
{
    public class StyleMetrics
    {
        private FontSet? _windowTitleFontSet;
        private FontSet? _textFontSet;
        private int? _minContentHeight;

        public string WindowTitleFontName { get; set; } = string.Empty;
        public string WindowTitleFontSizeKey { get; set; } = string.Empty;
        public string WindowTitleFontVariation { get; set; } = string.Empty;

        public string TextFontName { get; set; } = string.Empty;
        public string TextFontSizeKey { get; set; } = string.Empty;
        public string TextFontVariation { get; set; } = string.Empty;

        public string ButtonFontName { get; set; } = string.Empty;
        public string ButtonFontSizeKey { get; set; } = string.Empty;

        public int ButtonHeight { get; set; }
        public int MinButtonWidth { get; set; }
        public int ButtonSpacing { get; set; }
        public int ButtonsPaddingLeft { get; set; }
        public int ButtonsPaddingRight { get; set; }
        public Dictionary<string, FramePartRenderMode> FramePartRenderModes { get; set; } = new();
        public Dictionary<ButtonType, ButtonMetrics> ButtonTypeMetrics { get; set; } = new();
        public List<ButtonType> ButtonSortOrder { get; set; } = new();

        public Point CrossOffset { get; set; }
        public CrossAlignmentAnchor CrossAlignmentAnchor { get; set; }
        public Point WindowTitlePadding { get; set; }
        public int CrossPaddingLeft { get; set; }
        public int CrossPaddingRight { get; set; }
        public int CrossPaddingTop { get; set; }
        public int CrossPaddingBottom { get; set; }

        public Size ExpectedIconSize { get; set; }
        public int IconPaddingLeft { get; set; }
        public int IconPaddingTop { get; set; }
        public int IconPaddingRight { get; set; }
        public int TextPaddingRight { get; set; }
        public int TextPaddingTop { get; set; }
        public int TextPaddingBottom { get; set; }
        public float LineSpacing { get; set; }

        public ShadowConfig? Shadow { get; set; }

        public FontSet WindowTitleFontSet => _windowTitleFontSet ??= AssetManager.Instance.GetFontSet(WindowTitleFontName, WindowTitleFontSizeKey, WindowTitleFontVariation) ?? throw new InvalidOperationException($"Font '{WindowTitleFontName}' (size key: '{WindowTitleFontSizeKey}', variation: '{WindowTitleFontVariation}') not found.");
        public FontSet TextFontSet => _textFontSet ??= AssetManager.Instance.GetFontSet(TextFontName, TextFontSizeKey, TextFontVariation) ?? throw new InvalidOperationException($"Font '{TextFontName}' (size key: '{TextFontSizeKey}', variation: '{TextFontVariation}') not found.");
        public FontSet ButtonFontSet(string variation) => AssetManager.Instance.GetFontSet(ButtonFontName, ButtonFontSizeKey, variation) ?? throw new InvalidOperationException($"Font '{ButtonFontName}' (size key: '{ButtonFontSizeKey}', variation: '{variation}') not found.");

        public int MinContentHeight
        {
            get
            {
                if (_minContentHeight.HasValue)
                {
                    return _minContentHeight.Value;
                }

                int iconBottom = IconPaddingTop + ExpectedIconSize.Height;
                int minTextHeight = TextFontSet?.Metrics?.LineHeight ?? ExpectedIconSize.Height;
                int textBottom = TextPaddingTop + minTextHeight;

                _minContentHeight = Math.Max(iconBottom, textBottom) + TextPaddingBottom;

                return _minContentHeight.Value;
            }
        }

        public StyleMetrics()
        {
        }

        public StyleMetrics(StyleMetrics source)
        {
            var properties = typeof(StyleMetrics).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(source);
                if (value is IDictionary dictionary)
                {
                    var newDict = (IDictionary)Activator.CreateInstance(prop.PropertyType)!;
                    foreach (DictionaryEntry entry in dictionary)
                    {
                        newDict[entry.Key] = entry.Value;
                    }
                    prop.SetValue(this, newDict);
                }
                else if (value is IList list)
                {
                    var newList = (IList)Activator.CreateInstance(prop.PropertyType)!;
                    foreach (var item in list)
                    {
                        newList.Add(item);
                    }
                    prop.SetValue(this, newList);
                }
                else
                {
                    prop.SetValue(this, value);
                }
            }
        }
    }
}