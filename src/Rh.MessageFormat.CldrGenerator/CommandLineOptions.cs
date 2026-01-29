using CommandLine;

namespace Rh.MessageFormat.CldrGenerator;

public class CommandLineOptions
{
    [Option('r', "cldr-root", Required = true, HelpText = "Path to extracted CLDR archive folder (e.g., './cldr-temp/46.0.0/cldr-json-46.0.0').")]
    public string CldrRoot { get; set; } = string.Empty;

    [Option('o', "output", Required = true, HelpText = "Output directory for generated C# locale classes (e.g., './src/Rh.MessageFormat.CldrData/Generated').")]
    public string OutputDirectory { get; set; } = string.Empty;

    [Option('s', "spellout-output", Required = false, HelpText = "Output directory for generated spellout classes (e.g., './src/Rh.MessageFormat.CldrData.Spellout/Generated'). If not specified, spellout generation is skipped.")]
    public string? SpelloutOutputDirectory { get; set; }

    [Option('l', "locales", Required = false, HelpText = "Comma-separated list of locales to include (e.g., 'en-US, es-MX, de-DE'). If not specified, all .NET-supported locales are included.")]
    public string? Locales { get; set; }
}
