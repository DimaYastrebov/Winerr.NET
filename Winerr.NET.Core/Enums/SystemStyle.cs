using SixLabors.ImageSharp;
using System.Reflection;
using Winerr.NET.Core.Models.Styles;

namespace Winerr.NET.Core.Enums
{
    public class SystemStyle
    {
        public static readonly SystemStyle Windows7Aero = new("Win7_Aero", "Windows 7 (Aero)",
            new SystemInfo(buttonCount: 3, isCross: true),
            new StyleMetrics
            {
                WindowTitleFontName = "Segoe_UI",
                WindowTitleFontSizeKey = "9pt",
                WindowTitleFontVariation = "Black.Win7Aero_title",

                TextFontName = "Segoe_UI",
                TextFontSizeKey = "8pt",
                TextFontVariation = "Black",

                ButtonFontName = "Segoe_UI",
                ButtonFontSizeKey = "8pt",

                Shadow = new ShadowConfig
                {
                    Color = Color.FromRgba(255, 255, 255, 200),
                    Sigma = 6f,
                    Offset = new Point(0, 1),
                    ExpansionX = 2,
                    ExpansionY = 3,

                },

                ButtonHeight = 21,
                MinButtonWidth = 66,
                ButtonSpacing = 8,
                ButtonsPaddingLeft = 11,
                ButtonsPaddingRight = 11,
                ButtonTypeMetrics = new Dictionary<ButtonType, ButtonMetrics>
        {
            {
                ButtonType.Default, new ButtonMetrics
                {
                    FontVariation = $"{FontVariations.Black}.Win7_Button.Default",
                    HorizontalPadding = 7,
                    VerticalTextOffset = 1
                }
            },
            {
                ButtonType.Recommended, new ButtonMetrics
                {
                    FontVariation = $"{FontVariations.Black}.Win7_Button.Recommended",
                    HorizontalPadding = 7,
                    VerticalTextOffset = 1
                }
            },
            {
                ButtonType.Disabled, new ButtonMetrics
                {
                    FontVariation = $"{FontVariations.Gray}.Win7_Button.Disabled",
                    HorizontalPadding = 7,
                    VerticalTextOffset = 1
                }
            }
        },
                ButtonSortOrder = new List<ButtonType>
                {
                    ButtonType.Default,
                    ButtonType.Recommended,
                    ButtonType.Disabled
                },

                CrossOffset = new Point(-36, -6),
                CrossAlignmentAnchor = CrossAlignmentAnchor.TopRight,
                CrossPaddingLeft = 6,
                CrossPaddingRight = 0,
                CrossPaddingTop = 0,
                CrossPaddingBottom = 0,

                WindowTitlePadding = new Point(3, 6),

                ExpectedIconSize = new Size(32, 32),
                IconPaddingLeft = 10,
                IconPaddingTop = 10,
                IconPaddingRight = 10,
                TextPaddingRight = 9,
                TextPaddingTop = 16,
                TextPaddingBottom = 16
            });

        public static readonly SystemStyle Windows7Architecture = new("Win7_Architecture", "Windows 7 (Architecture)",
            new SystemInfo(buttonCount: 3, isCross: true),
            new StyleMetrics(Windows7Aero.Metrics)
            {
                CrossOffset = new Point(-36, -12),
            },
            Windows7Aero);

        public static readonly SystemStyle Windows7Landscapes = new("Win7_Landscapes", "Windows 7 (Landscapes)",
            new SystemInfo(buttonCount: 3, isCross: true),
            new StyleMetrics(Windows7Architecture.Metrics),
            Windows7Architecture);

        public static readonly SystemStyle Windows7Nature = new("Win7_Nature", "Windows 7 (Nature)",
            new SystemInfo(buttonCount: 3, isCross: true),
            new StyleMetrics(Windows7Architecture.Metrics),
            Windows7Architecture);

        public static readonly SystemStyle Windows7Scenes = new("Win7_Scenes", "Windows 7 (Scenes)",
            new SystemInfo(buttonCount: 3, isCross: true),
            new StyleMetrics(Windows7Architecture.Metrics),
            Windows7Architecture);

        public static readonly SystemStyle Windows7Ruby = new("Win7_Ruby", "Windows 7 (Ruby)",
            new SystemInfo(buttonCount: 3, isCross: true),
            new StyleMetrics(Windows7Architecture.Metrics),
            Windows7Architecture);

        public static readonly SystemStyle Windows7Gold = new("Win7_Gold", "Windows 7 (Gold)",
            new SystemInfo(buttonCount: 3, isCross: true),
            new StyleMetrics(Windows7Architecture.Metrics),
            Windows7Architecture);

        public static readonly SystemStyle Windows7Onyx = new("Win7_Onyx", "Windows 7 (Onyx)",
            new SystemInfo(buttonCount: 3, isCross: true),
            new StyleMetrics(Windows7Architecture.Metrics),
            Windows7Architecture);

        public static readonly SystemStyle Windows7Emerald = new("Win7_Emerald", "Windows 7 (Emerald)",
            new SystemInfo(buttonCount: 3, isCross: true),
            new StyleMetrics(Windows7Architecture.Metrics),
            Windows7Architecture);

        public static readonly SystemStyle Windows7Sea = new("Win7_Sea", "Windows 7 (Sea)",
            new SystemInfo(buttonCount: 3, isCross: true),
            new StyleMetrics(Windows7Architecture.Metrics),
            Windows7Architecture);

        public static readonly SystemStyle Windows7Basic = new("Win7_Basic", "Windows 7 (Basic)",
            new SystemInfo(buttonCount: 3, isCross: true),
            new StyleMetrics(Windows7Aero.Metrics)
            {
                WindowTitlePadding = new Point(12, 4),
                CrossOffset = new Point(-22, 4),
                WindowTitleFontName = "Segoe_UI",
                WindowTitleFontSizeKey = "9pt",
                WindowTitleFontVariation = "Black.Win7Basic_title",
                Shadow = null
            },
            Windows7Aero);

        public string DisplayName { get; }
        public string Id { get; }
        public SystemInfo SystemInfo { get; set; }
        public StyleMetrics Metrics { get; }
        public SystemStyle? ResourceAliasFor { get; }

        private SystemStyle(string id, string displayName, SystemInfo systemInfo, StyleMetrics metrics, SystemStyle? resourceAliasFor = null)
        {
            Id = id;
            DisplayName = displayName;
            SystemInfo = systemInfo;
            Metrics = metrics;
            ResourceAliasFor = resourceAliasFor;
        }

        public static IEnumerable<SystemStyle?> List()
        {
            return typeof(SystemStyle).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(SystemStyle))
                .Select(f => (SystemStyle?)f.GetValue(null));
        }
    }
}