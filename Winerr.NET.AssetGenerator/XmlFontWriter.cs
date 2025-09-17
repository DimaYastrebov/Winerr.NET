using System.Text;

namespace Winerr.NET.AssetGenerator
{
    public static class XmlFontWriter
    {
        public static string Generate(string fontName, int fontSize, int lineHeight, int baseHeight, int textureWidth, int textureHeight, string textureFileName, List<GlyphRenderData> glyphs)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<font>");

            sb.AppendLine($"  <info face=\"{fontName}\" size=\"{fontSize}\" bold=\"0\" italic=\"0\" charset=\"\" unicode=\"1\" stretchH=\"100\" smooth=\"0\" aa=\"0\" padding=\"0,0,0,0\" spacing=\"0,0\" outline=\"0\"/>");

            sb.AppendLine($"  <common lineHeight=\"{lineHeight}\" base=\"{baseHeight}\" scaleW=\"{textureWidth}\" scaleH=\"{textureHeight}\" pages=\"1\" packed=\"0\"/>");

            sb.AppendLine("  <pages>");
            sb.AppendLine($"    <page id=\"0\" file=\"{textureFileName}\"/>");
            sb.AppendLine("  </pages>");

            sb.AppendLine($"  <chars count=\"{glyphs.Count}\">");
            foreach (var glyph in glyphs.OrderBy(g => g.Metrics.Id))
            {
                var m = glyph.Metrics;

                var charAttributes = new StringBuilder();
                charAttributes.Append($"id=\"{m.Id}\" ");
                charAttributes.Append($"x=\"{m.Source.X}\" y=\"{m.Source.Y}\" ");
                charAttributes.Append($"width=\"{m.Source.Width}\" height=\"{m.Source.Height}\" ");
                charAttributes.Append($"xoffset=\"{m.Offset.X}\" yoffset=\"{m.Offset.Y}\" ");
                charAttributes.Append($"xadvance=\"{m.XAdvance}\" ");
                charAttributes.Append("page=\"0\" chnl=\"15\"");

                sb.AppendLine($"    <char {charAttributes}/>");
            }
            sb.AppendLine("  </chars>");

            sb.AppendLine("  <kernings count=\"0\"/>");
            sb.AppendLine("</font>");

            return sb.ToString();
        }
    }
}
