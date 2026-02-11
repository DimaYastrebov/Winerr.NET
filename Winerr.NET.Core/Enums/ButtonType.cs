namespace Winerr.NET.Core.Enums
{
    public class ButtonType
    {
        public static readonly ButtonType Default = new(0, "Default");
        public static readonly ButtonType Recommended = new(1, "Recommended");
        public static readonly ButtonType Disabled = new(2, "Disabled");

        public int Id { get; set; }
        public string DisplayName { get; set; }

        private ButtonType(int id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }

        public static ButtonType FromString(string typeName)
        {
            return typeName?.ToLowerInvariant() switch
            {
                "recommended" => Recommended,
                "disabled" => Disabled,
                _ => Default
            };
        }
    }
}
