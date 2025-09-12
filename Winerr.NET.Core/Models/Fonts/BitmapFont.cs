using System.Xml.Linq;
using SixLabors.ImageSharp;

namespace Winerr.NET.Core.Models.Fonts
{
    public class BitmapFont
    {
        public string Face { get; private set; }
        public int Size { get; private set; }
        public int LineHeight { get; private set; }
        public int Base { get; private set; }
        public int TextureWidth { get; private set; }
        public int TextureHeight { get; private set; }
        public XElement? Info { get; private set; }
        public XElement? Common { get; private set; }
        public XElement? KerningsNode { get; private set; }

        public Dictionary<int, FontChar> Characters { get; } = new();
        public Dictionary<int, Dictionary<int, int>> Kernings { get; } = new();

        private BitmapFont()
        {
            Face = string.Empty;
        }

        public int GetKerning(char first, char second)
        {
            if (Kernings.TryGetValue(first, out var kerningPair))
            {
                if (kerningPair.TryGetValue(second, out var amount))
                {
                    return amount;
                }
            }
            return 0;
        }

        public static BitmapFont FromXml(string xmlData)
        {
            var font = new BitmapFont();
            var doc = XDocument.Parse(xmlData);

            var info = doc.Descendants("info").FirstOrDefault();
            if (info == null) throw new InvalidDataException("Font file is missing <info> tag.");

            font.Info = info;
            font.Face = info.Attribute("face")?.Value ?? "";
            font.Size = int.Parse(info.Attribute("size")?.Value ?? "0");

            var common = doc.Descendants("common").FirstOrDefault();
            if (common == null) throw new InvalidDataException("Font file is missing <common> tag.");

            font.Common = common;
            font.LineHeight = int.Parse(common.Attribute("lineHeight")?.Value ?? "0");
            font.Base = int.Parse(common.Attribute("base")?.Value ?? "0");
            font.TextureWidth = int.Parse(common.Attribute("scaleW")?.Value ?? "0");
            font.TextureHeight = int.Parse(common.Attribute("scaleH")?.Value ?? "0");

            foreach (var charElement in doc.Descendants("char"))
            {
                var fontChar = new FontChar
                {
                    Id = int.Parse(charElement.Attribute("id")?.Value ?? "0"),
                    Source = new Rectangle(
                        int.Parse(charElement.Attribute("x")?.Value ?? "0"),
                        int.Parse(charElement.Attribute("y")?.Value ?? "0"),
                        int.Parse(charElement.Attribute("width")?.Value ?? "0"),
                        int.Parse(charElement.Attribute("height")?.Value ?? "0")
                        ),
                    Offset = new Point(
                        int.Parse(charElement.Attribute("xoffset")?.Value ?? "0"),
                        int.Parse(charElement.Attribute("yoffset")?.Value ?? "0")
                        ),
                    XAdvance = int.Parse(charElement.Attribute("xadvance")?.Value ?? "0")
                };
                font.Characters[fontChar.Id] = fontChar;
            }

            font.KerningsNode = doc.Descendants("kernings").FirstOrDefault();

            if (font.KerningsNode != null)
            {
                foreach (var kerningElement in font.KerningsNode.Descendants("kerning"))
                {
                    int first = int.Parse(kerningElement.Attribute("first")?.Value ?? "0");
                    int second = int.Parse(kerningElement.Attribute("second")?.Value ?? "0");
                    int amount = int.Parse(kerningElement.Attribute("amount")?.Value ?? "0");

                    if (!font.Kernings.ContainsKey(first))
                    {
                        font.Kernings[first] = new Dictionary<int, int>();
                    }
                    font.Kernings[first][second] = amount;
                }
            }

            return font;
        }
    }
}