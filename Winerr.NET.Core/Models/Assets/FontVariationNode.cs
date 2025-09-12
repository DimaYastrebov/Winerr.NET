namespace Winerr.NET.Core.Models.Assets
{
    public class FontVariationNode
    {
        public string Name { get; }
        public string? SpriteSheetPath { get; set; }
        public Dictionary<string, FontVariationNode> Children { get; } = new(StringComparer.OrdinalIgnoreCase);

        public FontVariationNode(string name)
        {
            Name = name;
        }
    }
}