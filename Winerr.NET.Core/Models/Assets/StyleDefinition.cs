namespace Winerr.NET.Core.Models.Assets
{
    public class StyleDefinition
    {
        public string Name { get; }
        public Dictionary<int, string> GlobalIconPaths { get; } = new();
        public Dictionary<string, string> GlobalButtonPaths { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, ThemeDefinition> Themes { get; } = new(StringComparer.OrdinalIgnoreCase);

        public StyleDefinition(string name)
        {
            Name = name;
        }
    }
}