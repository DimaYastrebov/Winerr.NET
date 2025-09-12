using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Concurrent;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Text;
using Winerr.NET.Core.Managers;
using Winerr.NET.Core.Models.Fonts;

using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using Graphics = System.Drawing.Graphics;
using PointF = System.Drawing.PointF;
using Rectangle = System.Drawing.Rectangle;
using Bitmap = System.Drawing.Bitmap;

namespace Winerr.NET.AssetGenerator
{
    public class GenerationPreset
    {
        public string OutputFolderName { get; set; } = "Default";
        public string FontName { get; set; } = "Segoe UI";
        public int FontSize { get; set; } = 8;
        public FontStyle FontStyle { get; set; } = FontStyle.Regular;
        public bool UseClearType { get; set; } = true;
        public SixLabors.ImageSharp.Color BackgroundColor { get; set; } = SixLabors.ImageSharp.Color.Transparent;
        public SixLabors.ImageSharp.Color TextColor { get; set; } = SixLabors.ImageSharp.Color.Black;
        public bool ApplyContrastEnhancement { get; set; } = false;
        public float TextContrastMultiplier { get; set; } = 1.0f;
        public string SourceXmlResourceName { get; set; } = "Winerr.NET.Assets.Fonts.Segoe_UI.spritesheet.xml";
    }

    internal class Program
    {
        #region P/Invoke
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int DrawTextW(IntPtr hDC, string lpchText, int nCount, ref RECT lpRect, uint uFormat);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        public static extern bool GetTextExtentPoint32W(IntPtr hdc, string lpString, int cbString, out SIZE lpSize);

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
        #endregion


        [STAThread]
        static void Main(string[] args)
        {
            var presetsToGenerate = new List<GenerationPreset>
            {
                new()
                {
                    OutputFolderName = "SegoeUI_8pt_Black",
                    FontSize = 8,
                    TextColor = SixLabors.ImageSharp.Color.FromRgba(0, 0, 0, 255),
                    BackgroundColor = SixLabors.ImageSharp.Color.FromRgba(255, 255, 255, 255),
                    ApplyContrastEnhancement = false,
                    SourceXmlResourceName = "Winerr.NET.Assets.Fonts.Segoe_UI.spritesheet.xml"
                },
                new()
                {
                    OutputFolderName = "SegoeUI_8pt_Black_Button_Default",
                    FontSize = 8,
                    TextColor = SixLabors.ImageSharp.Color.FromRgba(0, 0, 0, 255),
                    BackgroundColor = SixLabors.ImageSharp.Color.FromRgba(221, 221, 221, 255),
                    ApplyContrastEnhancement = true,
                    TextContrastMultiplier = 1.2f,
                    SourceXmlResourceName = "Winerr.NET.Assets.Fonts.Segoe_UI.spritesheet.xml"
                },
                new()
                {
                    OutputFolderName = "SegoeUI_8pt_Black_Button_Recommended",
                    FontSize = 8,
                    TextColor = SixLabors.ImageSharp.Color.FromRgba(0, 0, 0, 255),
                    BackgroundColor = SixLabors.ImageSharp.Color.FromRgba(217, 223, 227, 255),
                    ApplyContrastEnhancement = true,
                    TextContrastMultiplier = 1.2f,
                    SourceXmlResourceName = "Winerr.NET.Assets.Fonts.Segoe_UI.spritesheet.xml"
                },
                new()
                {
                    OutputFolderName = "SegoeUI_8pt_Gray_Button_Disabled",
                    FontSize = 8,
                    TextColor = SixLabors.ImageSharp.Color.FromRgba(80, 80, 80, 255),
                    BackgroundColor = SixLabors.ImageSharp.Color.FromRgba(224, 224, 224, 255),
                    ApplyContrastEnhancement = true,
                    TextContrastMultiplier = 1.2f,
                    SourceXmlResourceName = "Winerr.NET.Assets.Fonts.Segoe_UI.spritesheet.xml"
                },
                new()
                {
                    OutputFolderName = "SegoeUI_9pt_Black_Win7Aero_title",
                    FontSize = 9,
                    TextColor = SixLabors.ImageSharp.Color.FromRgba(0, 0, 0, 255),
                    BackgroundColor = SixLabors.ImageSharp.Color.FromRgba(202, 218, 233, 255),
                    ApplyContrastEnhancement = true,
                    TextContrastMultiplier = 1.1f,
                    SourceXmlResourceName = "Winerr.NET.Assets.Fonts.Segoe_UI.spritesheet.xml"
                },
                new()
                {
                    OutputFolderName = "SegoeUI_9pt_Black_Win7Basic_title",
                    FontSize = 9,
                    TextColor = SixLabors.ImageSharp.Color.FromRgba(0, 0, 0, 255),
                    BackgroundColor = SixLabors.ImageSharp.Color.FromRgba(178, 203, 229, 255),
                    ApplyContrastEnhancement = true,
                    TextContrastMultiplier = 1.1f,
                    SourceXmlResourceName = "Winerr.NET.Assets.Fonts.Segoe_UI.spritesheet.xml"
                },
            };

            var baseOutputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MsgBoxFont");
            Directory.CreateDirectory(baseOutputDir);

            Console.WriteLine("--- Генератор ассетов для Winerr.NET ---");
            Console.WriteLine($"Всего пресетов для генерации: {presetsToGenerate.Count}");

            foreach (var preset in presetsToGenerate)
            {
                GenerateFontAssets(preset, baseOutputDir);
            }

            Console.WriteLine("\n--- ВСЕ ОПЕРАЦИИ ЗАВЕРШЕНЫ ---");
            Console.ReadKey();
        }

        static void GenerateFontAssets(GenerationPreset preset, string baseOutputDir)
        {
            Console.WriteLine($"\n--- Начинаю генерацию пресета: {preset.OutputFolderName} ---");

            var outputDir = Path.Combine(baseOutputDir, preset.OutputFolderName);
            Directory.CreateDirectory(outputDir);

            var bgPixel = preset.BackgroundColor.ToPixel<Rgba32>();
            var textPixel = preset.TextColor.ToPixel<Rgba32>();
            var backgroundColor = Color.FromArgb(bgPixel.A, bgPixel.R, bgPixel.G, bgPixel.B);
            var textColor = Color.FromArgb(textPixel.A, textPixel.R, textPixel.G, textPixel.B);

            Console.WriteLine("Загружаю .xml для получения списка символов...");
            string? fontXmlContent;
            using (var stream = AssetManager.Instance.GetResourceStream(preset.SourceXmlResourceName))
            {
                if (stream == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ОШИБКА: Ресурс '{preset.SourceXmlResourceName}' не найден. Пропускаю пресет.");
                    Console.ResetColor();
                    return;
                }
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    fontXmlContent = reader.ReadToEnd();
                }
            }

            var existingFont = BitmapFont.FromXml(fontXmlContent);
            var charactersToRender = existingFont.Characters.Keys.Select(id => (char)id).Distinct().ToList();
            charactersToRender.Add(' ');
            charactersToRender = charactersToRender.Distinct().ToList();

            Console.WriteLine($"Найдено {charactersToRender.Count} уникальных символов для рендеринга.");

            using var font = new Font(preset.FontName, preset.FontSize, preset.FontStyle);
            if (font is null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ОШИБКА: Не удалось создать шрифт. Пропускаю пресет.");
                Console.ResetColor();
                return;
            }
            Console.WriteLine($"Системный шрифт '{font.Name}' (Высота: {font.Height}px) создан.");

            int baselinePx = GetBaselineInPixels(font);
            Console.WriteLine($"Точная базовая линия (tmAscent): {baselinePx}px от верха ячейки.");

            var allGlyphs = new ConcurrentBag<(char Character, FontChar Metrics, Bitmap GlyphBitmap)>();

            Console.WriteLine("Начинаю рендеринг символов (многопоточно)...");
            IntPtr hFont = font.ToHfont();
            try
            {
                Parallel.ForEach(charactersToRender, c =>
                {
                    var result = ProcessGlyph(c, font, hFont, textColor, backgroundColor, preset.UseClearType,
                                              preset.ApplyContrastEnhancement, preset.TextContrastMultiplier);
                    allGlyphs.Add(result);
                });
            }
            finally
            {
                DeleteObject(hFont);
            }
            Console.WriteLine("Все метрики успешно измерены.");

            var processedGlyphs = allGlyphs.ToList();

            Console.WriteLine("Упаковываю текстурный атлас...");
            var packerGlyphs = processedGlyphs.Select(g => new GlyphRenderData(g.Character, g.Metrics)).ToList();
            var (finalAtlasWidth, finalAtlasHeight) = TexturePacker.Pack(packerGlyphs, 1);
            Console.WriteLine($"Размер атласа: {finalAtlasWidth}x{finalAtlasHeight}");

            Console.WriteLine("Генерирую .xml файл...");

            var xmlContent = XmlFontWriter.Generate(font.Name, (int)font.Size, font.Height, baselinePx, finalAtlasWidth, finalAtlasHeight, "spritesheet.png", packerGlyphs);

            var xmlPath = Path.Combine(outputDir, "spritesheet.xml");
            File.WriteAllText(xmlPath, xmlContent);
            Console.WriteLine($".xml файл сохранен: {xmlPath}");

            Console.WriteLine("Собираю финальный PNG атлас...");
            using var finalAtlas = new Bitmap(finalAtlasWidth > 0 ? finalAtlasWidth : 1, finalAtlasHeight > 0 ? finalAtlasHeight : 1, PixelFormat.Format32bppArgb);
            using (var atlasG = Graphics.FromImage(finalAtlas))
            {
                atlasG.Clear(Color.Transparent);
                foreach (var glyph in processedGlyphs)
                {
                    if (glyph.Metrics.Source.Width > 0 && glyph.Metrics.Source.Height > 0)
                    {
                        atlasG.DrawImageUnscaled(glyph.GlyphBitmap, glyph.Metrics.Source.X, glyph.Metrics.Source.Y);
                    }
                    glyph.GlyphBitmap.Dispose();
                }
            }

            var atlasPath = Path.Combine(outputDir, "spritesheet.png");
            finalAtlas.Save(atlasPath, ImageFormat.Png);
            Console.WriteLine($"Атлас сохранен: {atlasPath}");
        }

        private static int GetBaselineInPixels(Font font)
        {
            using var bmp = new Bitmap(1, 1);
            using var g = Graphics.FromImage(bmp);
            IntPtr hdc = g.GetHdc();
            IntPtr hFont = font.ToHfont();
            IntPtr hOldFont = SelectObject(hdc, hFont);

            try
            {
                if (GetTextMetrics(hdc, out TEXTMETRIC tm))
                {
                    return tm.tmAscent;
                }
            }
            finally
            {
                SelectObject(hdc, hOldFont);
                DeleteObject(hFont);
                g.ReleaseHdc(hdc);
            }

            return (int)Math.Round(font.SizeInPoints / 72.0 * 96.0 * 0.8);
        }

        private static (char Character, FontChar Metrics, Bitmap GlyphBitmap) ProcessGlyph(
            char c, Font font, IntPtr hFont, Color textColor, Color backgroundColor, bool useClearType,
            bool applyContrast, float contrastMultiplier)
        {
            var (textCanvas, textBounds, textMetrics) = RenderSingleGlyph(c, font, hFont, textColor, backgroundColor, useClearType,
                                                                          applyContrast: applyContrast, contrastMultiplier: contrastMultiplier);

            Bitmap finalGlyphBitmap = (textBounds.Width > 0 && textBounds.Height > 0)
                ? textCanvas.Clone(textBounds, textCanvas.PixelFormat)
                : new Bitmap(1, 1, textCanvas.PixelFormat);

            textCanvas.Dispose();
            textMetrics.Source = new SixLabors.ImageSharp.Rectangle(0, 0, textBounds.Width, textBounds.Height);
            return (c, textMetrics, finalGlyphBitmap);
        }

        private static (Bitmap Canvas, Rectangle Bounds, FontChar Metrics) RenderSingleGlyph(
            char character, Font font, IntPtr hFont, Color textColor, Color backgroundColor, bool useClearType,
            bool applyContrast, float contrastMultiplier)
        {
            using var gdiBitmap = new Bitmap(1, 1);
            using var g = Graphics.FromImage(gdiBitmap);
            IntPtr hdc = g.GetHdc();
            IntPtr hOldFont = SelectObject(hdc, hFont);
            var charString = character.ToString();
            GetTextExtentPoint32W(hdc, charString, charString.Length, out var size);
            SelectObject(hdc, hOldFont);
            g.ReleaseHdc(hdc);

            const int padding = 32;
            int tempWidth = size.cx + padding * 2;
            int tempHeight = font.Height + padding * 2;

            using var tempBitmap = new Bitmap(tempWidth, tempHeight, PixelFormat.Format32bppArgb);
            using var tempG = Graphics.FromImage(tempBitmap);

            if (useClearType)
            {
                tempG.Clear(backgroundColor);
                IntPtr tempHdc = tempG.GetHdc();
                IntPtr tempHOldFont = SelectObject(tempHdc, hFont);
                SetTextColor(tempHdc, ColorTranslator.ToWin32(textColor));
                SetBkColor(tempHdc, ColorTranslator.ToWin32(backgroundColor));
                RECT drawRect = new RECT { Left = padding, Top = padding, Right = tempWidth, Bottom = tempHeight };
                DrawTextW(tempHdc, charString, charString.Length, ref drawRect, DT_LEFT | DT_TOP | DT_NOCLIP | DT_SINGLELINE);
                SelectObject(tempHdc, tempHOldFont);
                tempG.ReleaseHdc(tempHdc);
            }
            else
            {
                tempG.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                tempG.Clear(backgroundColor);
                using var brush = new SolidBrush(textColor);
                tempG.DrawString(charString, font, brush, new PointF(padding, padding));
            }

            using Image<Rgba32> transparentImageSharp = ConvertToImageSharpAndMakeTransparent(tempBitmap, backgroundColor);

            if (applyContrast && contrastMultiplier > 1.0f)
            {
                transparentImageSharp.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        Span<Rgba32> row = accessor.GetRowSpan(y);
                        foreach (ref Rgba32 pixel in row)
                        {
                            pixel.A = (byte)Math.Min(255, pixel.A * contrastMultiplier);
                        }
                    }
                });
            }

            var bounds = FindBoundingBox(transparentImageSharp);

            var metrics = new FontChar
            {
                Id = character,
                Source = new SixLabors.ImageSharp.Rectangle(),
                Offset = new SixLabors.ImageSharp.Point(bounds.X - padding, bounds.Y - padding),
                XAdvance = size.cx
            };

            var finalCanvasBitmap = ConvertToSystemDrawingBitmap(transparentImageSharp);
            return (finalCanvasBitmap, bounds, metrics);
        }

        private static unsafe Image<Rgba32> ConvertToImageSharpAndMakeTransparent(
            Bitmap source,
            Color backgroundColor,
            int threshold = 30)
        {
            var image = new Image<Rgba32>(source.Width, source.Height);
            var rect = new Rectangle(0, 0, source.Width, source.Height);
            BitmapData sourceData = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < source.Height; y++)
                    {
                        byte* sourceRowPtr = (byte*)sourceData.Scan0 + (y * sourceData.Stride);
                        var destRow = accessor.GetRowSpan(y);

                        for (int x = 0; x < source.Width; x++)
                        {
                            byte b = sourceRowPtr[x * 4];
                            byte g = sourceRowPtr[x * 4 + 1];
                            byte r = sourceRowPtr[x * 4 + 2];

                            int distance = Math.Abs(r - backgroundColor.R) +
                                           Math.Abs(g - backgroundColor.G) +
                                           Math.Abs(b - backgroundColor.B);

                            if (distance < threshold)
                            {
                                destRow[x] = new Rgba32(0, 0, 0, 0);
                            }
                            else
                            {
                                byte newAlpha = (byte)Math.Min(255, distance * 1.5f);
                                var finalPixel = new Rgba32(r, g, b, newAlpha);
                                destRow[x] = finalPixel;
                            }
                        }
                    }
                });
            }
            finally
            {
                source.UnlockBits(sourceData);
            }
            return image;
        }

        private static unsafe Bitmap ConvertToSystemDrawingBitmap(Image<Rgba32> source)
        {
            var bmp = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            var rect = new Rectangle(0, 0, source.Width, source.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);

            try
            {
                source.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < source.Height; y++)
                    {
                        var sourceRow = accessor.GetRowSpan(y);
                        byte* destRowPtr = (byte*)bmpData.Scan0 + (y * bmpData.Stride);

                        for (int x = 0; x < source.Width; x++)
                        {
                            destRowPtr[x * 4 + 0] = sourceRow[x].B;
                            destRowPtr[x * 4 + 1] = sourceRow[x].G;
                            destRowPtr[x * 4 + 2] = sourceRow[x].R;
                            destRowPtr[x * 4 + 3] = sourceRow[x].A;
                        }
                    }
                });
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        private static Rectangle FindBoundingBox(Image<Rgba32> image)
        {
            int minX = image.Width, minY = image.Height, maxX = -1, maxY = -1;
            bool foundPixel = false;

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgba32> row = accessor.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                    {
                        if (row[x].A > 0)
                        {
                            if (x < minX) minX = x;
                            if (y < minY) minY = y;
                            if (x > maxX) maxX = x;
                            if (y > maxY) maxY = y;
                            foundPixel = true;
                        }
                    }
                }
            });

            return foundPixel ? new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1) : Rectangle.Empty;
        }
    }
}