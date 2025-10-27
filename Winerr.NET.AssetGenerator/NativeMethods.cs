using System.Runtime.InteropServices;

namespace Winerr.NET.AssetGenerator
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int DrawTextW(IntPtr hDC, string lpchText, int nCount, ref RECT lpRect, uint uFormat);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        public static extern bool GetTextExtentPoint32W(IntPtr hdc, string lpString, int cbString, out SIZE lpSize);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        public static extern uint GetGlyphOutlineW(IntPtr hdc, uint uChar, uint uFormat, out GLYPHMETRICS lpgm, uint cbBuffer, IntPtr lpvBuffer, ref MAT2 lpmat2);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern uint SetTextColor(IntPtr hdc, int color);

        [DllImport("gdi32.dll")]
        public static extern uint SetBkColor(IntPtr hdc, int color);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        public static extern bool GetTextMetrics(IntPtr hdc, out TEXTMETRIC lptm);

        [StructLayout(LayoutKind.Sequential)]
        public struct FIXED
        {
            public short fract;
            public short value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MAT2
        {
            public FIXED eM11;
            public FIXED eM12;
            public FIXED eM21;
            public FIXED eM22;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GLYPHMETRICS
        {
            public uint gmBlackBoxX;
            public uint gmBlackBoxY;
            public POINT gmptGlyphOrigin;
            public short gmCellIncX;
            public short gmCellIncY;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct TEXTMETRIC
        {
            public int tmHeight;
            public int tmAscent;
            public int tmDescent;
            public int tmInternalLeading;
            public int tmExternalLeading;
            public int tmAveCharWidth;
            public int tmMaxCharWidth;
            public int tmWeight;
            public int tmOverhang;
            public int tmDigitizedAspectX;
            public int tmDigitizedAspectY;
            public char tmFirstChar;
            public char tmLastChar;
            public char tmDefaultChar;
            public char tmBreakChar;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }

        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE { public int cx; public int cy; }

        public const uint DT_TOP = 0x00000000;
        public const uint DT_LEFT = 0x00000000;
        public const uint DT_NOCLIP = 0x00000100;
        public const uint DT_SINGLELINE = 0x00000020;

        public const uint GGO_METRICS = 0;
        public const uint GGO_COLOR = 0x0100;
    }
}
