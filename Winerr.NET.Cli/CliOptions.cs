using CommandLine;
using Winerr.NET.Core.Enums;

namespace Winerr.NET.Cli
{
    [Verb("generate", isDefault: true, HelpText = "Generate an error window image.")]
    public class GenerateOptions
    {
        [Option('s', "style", Required = true, HelpText = "System style ID (e.g., 'Win7_Aero', 'Win7_Basic').")]
        public string Style { get; set; } = string.Empty;

        [Option('t', "title", Required = false, HelpText = "The text for the window title bar.")]
        public string? Title { get; set; }

        [Option('c', "content", Required = true, HelpText = "The main content text of the error window.")]
        public string Content { get; set; } = string.Empty;

        [Option('i', "icon", Required = true, HelpText = "The ID (number) of the icon to display.")]
        public int IconId { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output file path (.png). If not specified, the raw PNG data will be written to standard output.")]
        public string? OutputPath { get; set; }

        [Option('w', "width", Required = false, HelpText = "Maximum total width of the content area in pixels.")]
        public int? MaxWidth { get; set; }

        [Option("buttons", Required = false, Default = "[]", HelpText = "A JSON string representing the buttons. Example: '[{\"Text\":\"OK\",\"Type\":\"Recommended\",\"Mnemonic\":true}]'")]
        public string ButtonsJson { get; set; } = "[]";

        [Option("align", Required = false, Default = ButtonAlignment.Right, HelpText = "Button alignment: Left, Center, Right.")]
        public ButtonAlignment ButtonAlignment { get; set; }

        [Option("cross", Required = false, Default = true, HelpText = "Set to true to display an active cross, false for a disabled one.")]
        public bool isCrossEnabled { get; set; } = true;
    }

    [Verb("list-styles", HelpText = "List all available system style IDs.")]
    public class ListStylesOptions
    { }
}