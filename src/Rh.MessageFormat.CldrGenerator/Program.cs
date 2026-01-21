using CommandLine;
using Microsoft.Extensions.Configuration;
using Rh.MessageFormat.CldrGenerator;
using Rh.MessageFormat.CldrGenerator.Generators;

return await Parser.Default.ParseArguments<CommandLineOptions>(args)
    .MapResult(
        async options => await RunAsync(options),
        _ => Task.FromResult(1));

static async Task<int> RunAsync(CommandLineOptions options)
{
    try
    {
        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var config = configuration.GetSection("Cldr").Get<CldrConfig>()
            ?? throw new InvalidOperationException("Failed to load CLDR configuration from appsettings.json");

        // Validate CLDR root directory exists
        var cldrRoot = Path.GetFullPath(options.CldrRoot);
        if (!Directory.Exists(cldrRoot))
        {
            Console.Error.WriteLine($"Error: CLDR root directory does not exist: {cldrRoot}");
            return 1;
        }

        // Resolve output directory
        var outputDirectory = Path.GetFullPath(options.OutputDirectory);
        Directory.CreateDirectory(outputDirectory);

        Console.WriteLine($"CLDR root directory: {cldrRoot}");
        Console.WriteLine($"Output directory: {outputDirectory}");

        // Initialize locale filter
        var localeErrors = LocaleFilter.Initialize(options.Locales);
        if (localeErrors.Count > 0)
        {
            Console.Error.WriteLine("Locale validation errors:");
            foreach (var error in localeErrors)
            {
                Console.Error.WriteLine($"  - {error}");
            }
            return 1;
        }

        if (LocaleFilter.HasUserFilter)
        {
            var filteredLocales = LocaleFilter.GetUserFilteredLocales()!;
            Console.WriteLine($"Locale filter: {string.Join(", ", filteredLocales.OrderBy(l => l))}");
        }

        // Clean up existing generated files before generation
        CleanupGeneratedFiles(outputDirectory);

        // Report locale filtering
        if (LocaleFilter.HasUserFilter)
        {
            Console.WriteLine($"Filtering to {LocaleFilter.Count} user-specified locales");
        }
        else
        {
            Console.WriteLine($"Including all {LocaleFilter.Count} .NET-supported locales");
        }
        Console.WriteLine();

        // Generate C# locale classes
        var codeGenerator = new LocaleCodeGenerator(config);
        await codeGenerator.GenerateAsync(cldrRoot, outputDirectory);

        Console.WriteLine();
        Console.WriteLine($"C# locale classes generated successfully to {outputDirectory}");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        Console.Error.WriteLine(ex.StackTrace);
        return 1;
    }
}

/// <summary>
/// Removes all *.g.cs files from the output directory before generation.
/// </summary>
static void CleanupGeneratedFiles(string outputDirectory)
{
    if (!Directory.Exists(outputDirectory))
    {
        return;
    }

    var generatedFiles = Directory.GetFiles(outputDirectory, "*.g.cs", SearchOption.TopDirectoryOnly);

    if (generatedFiles.Length > 0)
    {
        Console.WriteLine($"Cleaning up {generatedFiles.Length} existing generated files...");

        foreach (var file in generatedFiles)
        {
            File.Delete(file);
        }

        Console.WriteLine($"Removed {generatedFiles.Length} *.g.cs files.");
    }
}
