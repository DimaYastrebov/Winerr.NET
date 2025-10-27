using System.Text;

namespace Winerr.NET.Core.Text
{
    public readonly record struct Symbol : IEquatable<Symbol>
    {
        private readonly int[] _codePoints;

        public ReadOnlySpan<int> CodePoints => _codePoints;

        public Symbol(params int[] codePoints)
        {
            if (codePoints == null || codePoints.Length == 0)
            {
                throw new ArgumentException("Symbol must contain at least one code point.", nameof(codePoints));
            }
            _codePoints = (int[])codePoints.Clone();
        }

        public Symbol(IEnumerable<int> codePoints)
        {
            _codePoints = codePoints?.ToArray() ?? throw new ArgumentNullException(nameof(codePoints));
            if (_codePoints.Length == 0)
            {
                throw new ArgumentException("Symbol must contain at least one code point.", nameof(codePoints));
            }
        }

        public bool Equals(Symbol other)
        {
            return _codePoints.AsSpan().SequenceEqual(other._codePoints);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var cp in _codePoints)
            {
                hash.Add(cp);
            }
            return hash.ToHashCode();
        }

        public override string ToString()
        {
            if (_codePoints == null || _codePoints.Length == 0) return string.Empty;

            var sb = new StringBuilder();
            foreach (var codePoint in _codePoints)
            {
                sb.Append(char.ConvertFromUtf32(codePoint));
            }
            return sb.ToString();
        }
    }
}
