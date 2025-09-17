
namespace Winerr.NET.Core.Models.Assets
{
    public class FontSizeDefinition
    {
        public string SizeKey { get; }
        public string? OverrideMetricsPath { get; set; }
        public FontVariationNode VariationRoot { get; }

        public FontSizeDefinition(string sizeKey)
        {
            SizeKey = sizeKey;
            VariationRoot = new FontVariationNode("root");
        }
    }
}
