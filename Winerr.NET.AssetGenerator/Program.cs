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
                }
            };

            var baseOutputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MsgBoxFont");
            Directory.CreateDirectory(baseOutputDir);

            Console.WriteLine("--- Генератор ассетов для Winerr.NET ---");
            Console.WriteLine($"Всего пресетов для генерации: {presetsToGenerate.Count}");

            foreach (var preset in presetsToGenerate)
            {
                var pipeline = new FontAssetPipeline(preset, baseOutputDir);
                pipeline.Run();
            }

            Console.WriteLine("\n--- ВСЕ ОПЕРАЦИИ ЗАВЕРШЕНЫ ---");
            Console.ReadKey();
        }
    }
}
