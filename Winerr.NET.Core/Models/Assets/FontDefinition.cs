namespace Winerr.NET.Core.Models.Assets
{
    public class FontDefinition
    {
        public string Name { get; }
        public string? GlobalMetricsPath { get; set; }
        public Dictionary<string, FontSizeDefinition> Sizes { get; } = new(StringComparer.OrdinalIgnoreCase);

        public FontDefinition(string name)
        {
            Name = name;
        }
    }
}
