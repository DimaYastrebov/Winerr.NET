using System.Collections.Concurrent;
using System.Drawing.Imaging;
using System.Text;
using Winerr.NET.Core.Managers;
using Winerr.NET.Core.Models.Fonts;

using SD = System.Drawing;

namespace Winerr.NET.AssetGenerator
{
    internal class FontAssetPipeline
    {
        private readonly GenerationPreset _preset;
        private readonly string _baseOutputDir;

        public FontAssetPipeline(GenerationPreset preset, string baseOutputDir)
        {
            _preset = preset;
            _baseOutputDir = baseOutputDir;
        }

        public void Run()
        {
            Console.WriteLine($"\n--- Начинаю генерацию пресета: {_preset.OutputFolderName} ---");

            var outputDir = Path.Combine(_baseOutputDir, _preset.OutputFolderName);
            Directory.CreateDirectory(outputDir);

            Console.WriteLine("Загружаю .xml для получения списка символов...");
            string? fontXmlContent;
            using (var stream = AssetManager.Instance.GetResourceStream(_preset.SourceXmlResourceName))
            {
                if (stream == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ОШИБКА: Ресурс '{_preset.SourceXmlResourceName}' не найден. Пропускаю пресет.");
                    Console.ResetColor();
                    return;
                }
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    fontXmlContent = reader.ReadToEnd();
                }
            }

            var existingFont = BitmapFont.FromXml(fontXmlContent);
            var charactersToRender = existingFont.Characters.Keys.Distinct().ToList();
            if (!charactersToRender.Contains(" "))
            {
                charactersToRender.Add(" ");
            }

            Console.WriteLine($"Найдено {charactersToRender.Count} уникальных символов для рендеринга.");

            using var font = new SD.Font(_preset.FontName, _preset.FontSize, _preset.FontStyle);
            if (font is null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ОШИБКА: Не удалось создать шрифт. Пропускаю пресет.");
                Console.ResetColor();
                return;
            }
            Console.WriteLine($"Системный шрифт '{font.Name}' (Высота: {font.Height}px) создан.");

            var glyphRenderer = new GlyphRenderer(_preset);
            int baselinePx = glyphRenderer.GetBaselineInPixels(font);
            Console.WriteLine($"Точная базовая линия (tmAscent): {baselinePx}px от верха ячейки.");

            var allGlyphs = new ConcurrentBag<(string Character, FontChar Metrics, SD.Bitmap GlyphBitmap)>();

            Console.WriteLine("Начинаю рендеринг символов (многопоточно)...");
            IntPtr hFont = font.ToHfont();
            try
            {
                Parallel.ForEach(charactersToRender, c =>
                {
                    var result = glyphRenderer.ProcessGlyph(c, font, hFont);
                    allGlyphs.Add(result);
                });
            }
            finally
            {
                NativeMethods.DeleteObject(hFont);
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
            using var finalAtlas = new SD.Bitmap(finalAtlasWidth > 0 ? finalAtlasWidth : 1, finalAtlasHeight > 0 ? finalAtlasHeight : 1, PixelFormat.Format32bppArgb);
            using (var atlasG = SD.Graphics.FromImage(finalAtlas))
            {
                atlasG.Clear(SD.Color.Transparent);
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
    }
}
