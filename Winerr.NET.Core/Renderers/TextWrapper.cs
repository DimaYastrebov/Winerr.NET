using System.Collections.Generic;
using Winerr.NET.Core.Enums;
using Winerr.NET.Core.Models.Fonts;
using Winerr.NET.Core.Text;

namespace Winerr.NET.Core.Renderers
{
    public class TextWrapper
    {
        private readonly BitmapFont _mainFont;
        private readonly BitmapFont? _emojiFont;
        private readonly Symbol _spaceSymbol;
        private readonly Symbol _replacementSymbol;
        private readonly Symbol _fallbackReplacementSymbol;
        private readonly List<Symbol> _ellipsisSymbols;

        public TextWrapper(BitmapFont mainFont, BitmapFont? emojiFont = null)
        {
            _mainFont = mainFont;
            _emojiFont = emojiFont;

            _spaceSymbol = new Symbol(' ');
            _replacementSymbol = new Symbol(0xFFFD);
            _fallbackReplacementSymbol = new Symbol('?');
            _ellipsisSymbols = TextParser.Parse("...");
        }

        public bool TryGetCharacter(Symbol symbol, out FontChar? fontChar, out bool isEmoji)
        {
            fontChar = null;
            isEmoji = false;

            if (_mainFont.Characters.TryGetValue(symbol, out fontChar))
            {
                isEmoji = false;
                return true;
            }

            if (_emojiFont?.Characters != null)
            {
                if (_emojiFont.Characters.TryGetValue(symbol, out fontChar))
                {
                    isEmoji = true;
                    return true;
                }
            }

            isEmoji = false;
            if (_mainFont.Characters.TryGetValue(_replacementSymbol, out fontChar))
            {
                return true;
            }
            if (_mainFont.Characters.TryGetValue(_fallbackReplacementSymbol, out fontChar))
            {
                return true;
            }

            return false;
        }

        public List<List<Symbol>> Wrap(List<Symbol> symbols, int maxWidth, TextWrapMode wrapMode, TextTruncationMode truncationMode)
        {
            if (truncationMode == TextTruncationMode.SingleLine || truncationMode == TextTruncationMode.Ellipsis)
            {
                var singleLineSymbols = symbols.Where(s => s.ToString() != "\n" && s.ToString() != "\r").ToList();

                if (MeasureSymbolsWidth(singleLineSymbols) > maxWidth)
                {
                    if (truncationMode == TextTruncationMode.Ellipsis)
                    {
                        int ellipsisWidth = MeasureSymbolsWidth(_ellipsisSymbols);
                        var truncated = TruncateSymbolsToWidth(singleLineSymbols, maxWidth - ellipsisWidth);
                        truncated.AddRange(_ellipsisSymbols);
                        return new List<List<Symbol>> { truncated };
                    }
                    else
                    {
                        return new List<List<Symbol>> { TruncateSymbolsToWidth(singleLineSymbols, maxWidth) };
                    }
                }
                else
                {
                    return new List<List<Symbol>> { singleLineSymbols };
                }
            }

            if (wrapMode == TextWrapMode.Symbol)
            {
                return WrapBySymbol(symbols, maxWidth);
            }
            return WrapByWord(symbols, maxWidth, wrapMode);
        }

        private List<List<Symbol>> WrapBySymbol(List<Symbol> symbols, int maxWidth)
        {
            var finalLines = new List<List<Symbol>>();
            var currentLine = new List<Symbol>();

            foreach (var symbol in symbols)
            {
                if (symbol.ToString() == "\n")
                {
                    finalLines.Add(currentLine);
                    currentLine = new List<Symbol>();
                    continue;
                }

                var tempLine = new List<Symbol>(currentLine) { symbol };
                if (MeasureSymbolsWidth(tempLine) > maxWidth && currentLine.Count > 0)
                {
                    finalLines.Add(currentLine);
                    currentLine = new List<Symbol> { symbol };
                }
                else
                {
                    currentLine.Add(symbol);
                }
            }

            if (currentLine.Count > 0)
            {
                finalLines.Add(currentLine);
            }

            return finalLines;
        }

        private List<List<Symbol>> WrapByWord(List<Symbol> symbols, int maxWidth, TextWrapMode wrapMode)
        {
            var finalLines = new List<List<Symbol>>();
            var paragraphs = SplitIntoParagraphs(symbols);

            foreach (var paragraph in paragraphs)
            {
                if (paragraph.Count == 0)
                {
                    finalLines.Add(new List<Symbol>());
                    continue;
                }

                var words = SplitIntoWords(paragraph);
                var currentLine = new List<Symbol>();

                foreach (var word in words)
                {
                    if (word.Count == 0) continue;
                    var wordWidth = MeasureSymbolsWidth(word);

                    if (wrapMode == TextWrapMode.Auto && wordWidth > maxWidth)
                    {
                        if (currentLine.Count > 0)
                        {
                            finalLines.Add(currentLine);
                            currentLine = new List<Symbol>();
                        }

                        var brokenParts = BreakLongWord(word, maxWidth);
                        for (int i = 0; i < brokenParts.Count - 1; i++)
                        {
                            finalLines.Add(brokenParts[i]);
                        }
                        currentLine.AddRange(brokenParts.Last());
                        continue;
                    }

                    var spaceWidth = currentLine.Count > 0 ? MeasureSymbolsWidth(new List<Symbol> { _spaceSymbol }) : 0;
                    if (MeasureSymbolsWidth(currentLine) + spaceWidth + wordWidth > maxWidth && currentLine.Count > 0)
                    {
                        finalLines.Add(currentLine);
                        currentLine = new List<Symbol>();
                        currentLine.AddRange(word);
                    }
                    else
                    {
                        if (currentLine.Count > 0) currentLine.Add(_spaceSymbol);
                        currentLine.AddRange(word);
                    }
                }

                if (currentLine.Count > 0)
                {
                    finalLines.Add(currentLine);
                }
            }
            return finalLines;
        }

        private List<List<Symbol>> SplitIntoParagraphs(List<Symbol> symbols)
        {
            var paragraphs = new List<List<Symbol>>();
            var currentParagraph = new List<Symbol>();
            var newlineSymbol = new Symbol('\n');

            foreach (var symbol in symbols)
            {
                if (symbol.Equals(newlineSymbol))
                {
                    paragraphs.Add(currentParagraph);
                    currentParagraph = new List<Symbol>();
                }
                else
                {
                    currentParagraph.Add(symbol);
                }
            }
            paragraphs.Add(currentParagraph);
            return paragraphs;
        }

        private List<List<Symbol>> SplitIntoWords(List<Symbol> symbols)
        {
            var words = new List<List<Symbol>>();
            var currentWord = new List<Symbol>();

            foreach (var symbol in symbols)
            {
                if (symbol.Equals(_spaceSymbol))
                {
                    if (currentWord.Count > 0)
                    {
                        words.Add(currentWord);
                        currentWord = new List<Symbol>();
                    }
                }
                else
                {
                    currentWord.Add(symbol);
                }
            }

            if (currentWord.Count > 0)
            {
                words.Add(currentWord);
            }

            return words;
        }

        private List<List<Symbol>> BreakLongWord(List<Symbol> word, int maxWidth)
        {
            var parts = new List<List<Symbol>>();
            var currentPart = new List<Symbol>();

            foreach (var symbol in word)
            {
                var tempPart = new List<Symbol>(currentPart) { symbol };
                if (MeasureSymbolsWidth(tempPart) > maxWidth && currentPart.Count > 0)
                {
                    parts.Add(currentPart);
                    currentPart = new List<Symbol> { symbol };
                }
                else
                {
                    currentPart.Add(symbol);
                }
            }

            if (currentPart.Count > 0)
            {
                parts.Add(currentPart);
            }

            return parts;
        }

        public List<Symbol> TruncateSymbolsToWidth(List<Symbol> symbols, int maxWidth)
        {
            if (MeasureSymbolsWidth(symbols) <= maxWidth) return symbols;

            var result = new List<Symbol>();
            foreach (var symbol in symbols)
            {
                var tempResult = new List<Symbol>(result) { symbol };
                if (MeasureSymbolsWidth(tempResult) > maxWidth) break;
                result.Add(symbol);
            }
            return result;
        }

        public int MeasureSymbolsWidth(List<Symbol> symbols)
        {
            if (symbols == null || symbols.Count == 0) return 0;

            int totalWidth = 0;
            Symbol? previousSymbol = null;
            bool wasPreviousEmoji = false;

            foreach (var symbol in symbols)
            {
                if (TryGetCharacter(symbol, out var fontChar, out bool isEmoji) && fontChar != null)
                {
                    int kerning = 0;
                    if (previousSymbol.HasValue && !isEmoji && !wasPreviousEmoji)
                    {
                        kerning = _mainFont.GetKerning(previousSymbol.Value, symbol);
                    }
                    totalWidth += kerning + fontChar.XAdvance;

                    previousSymbol = symbol;
                    wasPreviousEmoji = isEmoji;
                }
            }
            return totalWidth;
        }
    }
}
