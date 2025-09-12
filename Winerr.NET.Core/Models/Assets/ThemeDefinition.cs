namespace Winerr.NET.Core.Models.Assets
{
    public class ThemeDefinition
    {
        public string Name { get; }
        public Dictionary<string, string> FramePartPaths { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> ButtonPaths { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<int, string> IconPaths { get; } = new();

        public ThemeDefinition(string name)
        {
            Name = name;
        }
    }
}