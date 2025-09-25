using System.Text;
using Winerr.NET.Core.Enums;
using Winerr.NET.Core.Models.Fonts;

namespace Winerr.NET.Core.Renderers
{
    public class TextWrapper
    {
        private readonly BitmapFont _font;

        public TextWrapper(BitmapFont font)
        {
            _font = font;
        }

        public List<string> Wrap(string text, int maxWidth, TextWrapMode wrapMode, TextTruncationMode truncationMode)
        {
            if (truncationMode == TextTruncationMode.SingleLine || truncationMode == TextTruncationMode.Ellipsis)
            {
                var lines = new List<string>();
                string singleLineText = text.Replace("\r", "").Replace("\n", " ");
                if (MeasureTextWidth(singleLineText) > maxWidth)
                {
                    if (truncationMode == TextTruncationMode.Ellipsis)
                    {
                        int ellipsisWidth = MeasureTextWidth("...");
                        string truncated = TruncateTextToWidth(singleLineText, maxWidth - ellipsisWidth);
                        lines.Add(truncated + "...");
                    }
                    else
                    {
                        lines.Add(TruncateTextToWidth(singleLineText, maxWidth));
                    }
                }
                else
                {
                    lines.Add(singleLineText);
                }
                return lines;
            }

            if (wrapMode == TextWrapMode.Symbol)
            {
                return WrapBySymbol(text, maxWidth);
            }
            return WrapByWord(text, maxWidth, wrapMode);
        }

        private List<string> WrapBySymbol(string text, int maxWidth)
        {
            var finalLines = new List<string>();
            var paragraphs = text.Replace("\r\n", "\n").Split('\n');

            foreach (var paragraph in paragraphs)
            {
                if (string.IsNullOrEmpty(paragraph))
                {
                    finalLines.Add("");
                    continue;
                }

                var currentLine = new StringBuilder();
                foreach (char c in paragraph)
                {
                    currentLine.Append(c);
                    if (MeasureTextWidth(currentLine.ToString()) > maxWidth && currentLine.Length > 1)
                    {
                        char lastChar = currentLine[currentLine.Length - 1];
                        currentLine.Length--;
                        finalLines.Add(currentLine.ToString());
                        currentLine.Clear();
                        currentLine.Append(lastChar);
                    }
                }

                if (currentLine.Length > 0)
                {
                    finalLines.Add(currentLine.ToString());
                }
            }
            return finalLines;
        }

        private List<string> WrapByWord(string text, int maxWidth, TextWrapMode wrapMode)
        {
            var finalLines = new List<string>();
            var paragraphs = text.Replace("\r\n", "\n").Split('\n');

            foreach (var paragraph in paragraphs)
            {
                if (string.IsNullOrEmpty(paragraph))
                {
                    finalLines.Add("");
                    continue;
                }

                var words = paragraph.Split(' ');
                var currentLine = new StringBuilder();

                foreach (var word in words)
                {
                    if (string.IsNullOrEmpty(word)) continue;
                    var wordWidth = MeasureTextWidth(word);

                    if (wrapMode == TextWrapMode.Auto && wordWidth > maxWidth)
                    {
                        if (currentLine.Length > 0)
                        {
                            finalLines.Add(currentLine.ToString());
                            currentLine.Clear();
                        }

                        var brokenParts = BreakLongWord(word, maxWidth);
                        for (int i = 0; i < brokenParts.Count - 1; i++)
                        {
                            finalLines.Add(brokenParts[i]);
                        }
                        currentLine.Append(brokenParts.Last());
                        continue;
                    }

                    var spaceWidth = currentLine.Length > 0 ? MeasureTextWidth(" ") : 0;
                    if (MeasureTextWidth(currentLine.ToString()) + spaceWidth + wordWidth > maxWidth)
                    {
                        finalLines.Add(currentLine.ToString());
                        currentLine.Clear();
                        currentLine.Append(word);
                    }
                    else
                    {
                        if (currentLine.Length > 0) currentLine.Append(" ");
                        currentLine.Append(word);
                    }
                }

                if (currentLine.Length > 0)
                {
                    finalLines.Add(currentLine.ToString());
                }
            }
            return finalLines;
        }

        private List<string> BreakLongWord(string word, int maxWidth)
        {
            var parts = new List<string>();
            var currentPart = new StringBuilder();

            foreach (char c in word)
            {
                currentPart.Append(c);
                if (MeasureTextWidth(currentPart.ToString()) > maxWidth && currentPart.Length > 1)
                {
                    char lastChar = currentPart[currentPart.Length - 1];
                    currentPart.Length--;

                    parts.Add(currentPart.ToString());
                    currentPart.Clear();
                    currentPart.Append(lastChar);
                }
            }

            if (currentPart.Length > 0)
            {
                parts.Add(currentPart.ToString());
            }

            return parts;
        }

        public string TruncateTextToWidth(string text, int maxWidth)
        {
            if (MeasureTextWidth(text) <= maxWidth) return text;
            var sb = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (MeasureTextWidth(sb.ToString() + c) > maxWidth) break;
                sb.Append(c);
            }
            return sb.ToString();
        }

        public int MeasureTextWidth(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            int totalWidth = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (TryGetCharacter(c, out var fontChar))
                {
                    if (fontChar != null)
                    {
                        totalWidth += fontChar.XAdvance;
                    }
                    if (i < text.Length - 1) totalWidth += _font.GetKerning(c, text[i + 1]);
                }
            }
            return totalWidth;
        }

        public bool TryGetCharacter(char c, out FontChar? fontChar)
        {
            fontChar = null;
            if (_font?.Characters == null) return false;

            if (_font.Characters.TryGetValue(c, out fontChar)) return true;
            if (_font.Characters.TryGetValue((char)65533, out fontChar)) return true;
            if (_font.Characters.TryGetValue('?', out fontChar)) return true;
            return false;
        }
    }
}
