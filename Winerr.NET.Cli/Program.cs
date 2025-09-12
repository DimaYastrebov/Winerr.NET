using CommandLine;
using CommandLine.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;using Winerr.NET.Core.Configs;
using Winerr.NET.Core.Enums;
using Winerr.NET.Core.Managers;
using Winerr.NET.Core.Renderers;

namespace Winerr.NET.Cli
{
    class Program
    {
        private record ButtonDto(string Text, string Type, bool? Mnemonic);
        static int Main(string[] args)
        {
            var parser = new Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<GenerateOptions, ListStylesOptions>(args);

            return parserResult.MapResult(
                (GenerateOptions opts) => RunGenerate(opts),
                (ListStylesOptions opts) => RunListStyles(opts),
                errors => HandleParseError(parserResult, errors)
            );
        }

        static int RunGenerate(GenerateOptions opts)
        {
            Image<Rgba32>? finalImage = null;
            try
            {
                var totalStopwatch = Stopwatch.StartNew();

                LogInfo("Loading assets...");
                AssetManager.Instance.LoadAssets();

                LogInfo("Creating error configuration...");
                var config = CreateErrorConfig(opts);

                LogInfo($"Starting generation for style '{config.SystemStyle.Id}'...");
                var renderer = new ErrorRenderer();
                finalImage = renderer.Generate(config);
                totalStopwatch.Stop();
                LogInfo($"Generation complete in {totalStopwatch.ElapsedMilliseconds}ms.");

                if (!string.IsNullOrEmpty(opts.OutputPath))
                {
                    finalImage.SaveAsPng(opts.OutputPath);
                    LogSuccess($"Image saved to: {opts.OutputPath}");
                }
                else
                {
                    using var outputStream = Console.OpenStandardOutput();
                    finalImage.SaveAsPng(outputStream);
                }

                return 0;
            }
            catch (ArgumentException ex)
            {
                LogError(ex.Message);
                return 1;
            }
            catch (FileNotFoundException ex)
            {
                LogError($"A required asset was not found: {ex.Message}");
                return 1;
            }
            catch (Exception ex)
            {
                LogError($"An unexpected fatal error occurred: {ex.GetType().Name}: {ex.Message}");
                return 1;
            }
            finally
            {
                finalImage?.Dispose();
            }
        }

        static int RunListStyles(ListStylesOptions opts)
        {
            try
            {
                Console.WriteLine("Available system styles:");
                var styles = SystemStyle.List()
                    .Where(s => s != null)
                    .OrderBy(s => s!.Id);

                foreach (var style in styles)
                {
                    Console.WriteLine($"- {style!.Id} ({style.DisplayName})");
                }
                return 0;
            }
            catch (Exception ex)
            {
                LogError($"Failed to list styles: {ex.Message}");
                return 1;
            }
        }

        static int HandleParseError<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            var helpText = HelpText.AutoBuild(result, h =>
            {
                var version = Assembly.GetExecutingAssembly()
                                      .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                                      .InformationalVersion;

                h.Heading = $"Winerr.NET.Cli {version}";
                h.Copyright = "Copyright (c) DimaYastrebov 2025";
                h.AdditionalNewLineAfterOption = false;

                if (errs.Any(e => e.Tag == ErrorType.NoVerbSelectedError))
                {
                    h.AddPreOptionsLine("Please specify a command (e.g., 'generate' or 'list-styles').");
                    h.AddPreOptionsLine("Use '--help' to see all commands.");
                }

                return h;
            }, e => e);

            Console.Error.WriteLine(helpText);
            return 1;
        }

        private static ErrorConfig CreateErrorConfig(GenerateOptions opts)
        {
            var style = FindStyle(opts.Style)
                ?? throw new ArgumentException($"Style '{opts.Style}' not found. Use 'list-styles' command to see available styles.");

            string processedContent = Regex.Replace(opts.Content, @"(?<!\\)\\n", "\n");
            processedContent = processedContent.Replace(@"\\n", @"\n");

            var config = new ErrorConfig
            {
                SystemStyle = style,
                Title = opts.Title ?? string.Empty,
                Content = processedContent,
                IconId = opts.IconId,
                Buttons = ParseButtons(opts.ButtonsJson),
                ButtonAlignment = opts.ButtonAlignment,
                IsCrossEnabled = opts.isCrossEnabled,
                MaxWidth = opts.MaxWidth
            };

            return config;
        }

        private static List<ButtonConfig> ParseButtons(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<ButtonConfig>();
            }

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var buttonDtos = JsonSerializer.Deserialize<List<ButtonDto>>(json, options);

                if (buttonDtos == null) return new List<ButtonConfig>();

                return buttonDtos.Select(dto => new ButtonConfig
                {
                    Text = dto.Text,
                    Type = MapButtonType(dto.Type),
                    TextConfig = new TextRenderConfig
                    {
                        DrawMnemonic = dto.Mnemonic ?? false
                    }
                }).ToList();
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Invalid JSON format for buttons: {ex.Message}");
            }
        }

        private static ButtonType MapButtonType(string typeName)
        {
            return typeName.ToLowerInvariant() switch
            {
                "default" => ButtonType.Default,
                "recommended" => ButtonType.Recommended,
                "disabled" => ButtonType.Disabled,
                _ => throw new ArgumentException($"Unknown button type '{typeName}'. Available types: Default, Recommended, Disabled.")
            };
        }

        private static SystemStyle? FindStyle(string styleId)
        {
            return SystemStyle.List()
                .FirstOrDefault(s => s != null && s.Id.Equals(styleId, StringComparison.OrdinalIgnoreCase));
        }

        private static void LogInfo(string message) => Console.Error.WriteLine($"[INFO] {message}");
        private static void LogSuccess(string message) => Console.Error.WriteLine($"[SUCCESS] {message}");
        private static void LogError(string message) => Console.Error.WriteLine($"[ERROR] {message}");
    }
}