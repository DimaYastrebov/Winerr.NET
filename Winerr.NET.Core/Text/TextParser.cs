using System.Globalization;

namespace Winerr.NET.Core.Text
{
    public static class TextParser
    {
        public static List<Symbol> Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new List<Symbol>();
            }

            var symbols = new List<Symbol>();
            var enumerator = StringInfo.GetTextElementEnumerator(text);

            while (enumerator.MoveNext())
            {
                string grapheme = enumerator.GetTextElement();

                var codePoints = new List<int>();
                foreach (var rune in grapheme.EnumerateRunes())
                {
                    codePoints.Add(rune.Value);
                }

                if (codePoints.Count > 0)
                {
                    symbols.Add(new Symbol(codePoints));
                }
            }

            return symbols;
        }
    }
}
