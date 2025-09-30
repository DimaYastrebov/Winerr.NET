
namespace Winerr.NET.Core.Enums
{
    public static class AssetKeys
    {
        public static class FrameParts
        {
            public const string TopLeft = "top_left";
            public const string TopCenter = "top_center";
            public const string TopRight = "top_right";
            public const string MiddleLeft = "middle_left";
            public const string MiddleCenter = "middle_center";
            public const string MiddleRight = "middle_right";
            public const string BottomLeft = "bottom_left";
            public const string BottomCenter = "bottom_center";
            public const string BottomRight = "bottom_right";
            public const string ButtonArea = "button_area";
            public const string Cross = "cross";
            public const string CrossDisabled = "cross_disabled";
        }

        public static class ButtonParts
        {
            public const string Left = "left";
            public const string Center = "center";
            public const string Right = "right";

            public static string DefaultLeft => $"{ButtonTypeNames.Default}_{Left}";
            public static string DefaultCenter => $"{ButtonTypeNames.Default}_{Center}";
            public static string DefaultRight => $"{ButtonTypeNames.Default}_{Right}";

            public static string RecommendedLeft => $"{ButtonTypeNames.Recommended}_{Left}";
            public static string RecommendedCenter => $"{ButtonTypeNames.Recommended}_{Center}";
            public static string RecommendedRight => $"{ButtonTypeNames.Recommended}_{Right}";

            public static string DisabledLeft => $"{ButtonTypeNames.Disabled}_{Left}";
            public static string DisabledCenter => $"{ButtonTypeNames.Disabled}_{Center}";
            public static string DisabledRight => $"{ButtonTypeNames.Disabled}_{Right}";
        }

        public static class ButtonTypeNames
        {
            public const string Default = "default";
            public const string Recommended = "recommended";
            public const string Disabled = "disabled";
        }
    }
}
