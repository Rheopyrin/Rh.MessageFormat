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

        // Generate spellout data if output directory is specified
        if (!string.IsNullOrEmpty(options.SpelloutOutputDirectory))
        {
            var spelloutOutputDirectory = Path.GetFullPath(options.SpelloutOutputDirectory);
            Directory.CreateDirectory(spelloutOutputDirectory);

            Console.WriteLine();
            Console.WriteLine($"Spellout output directory: {spelloutOutputDirectory}");

            var spelloutGenerator = new SpelloutCodeGenerator(config);
            await spelloutGenerator.GenerateAsync(cldrRoot, spelloutOutputDirectory);

            Console.WriteLine();
            Console.WriteLine($"Spellout classes generated successfully to {spelloutOutputDirectory}");
        }

        // Generate relative time data if output directory is specified
        if (!string.IsNullOrEmpty(options.RelativeTimeOutputDirectory))
        {
            var relativeTimeOutputDirectory = Path.GetFullPath(options.RelativeTimeOutputDirectory);
            Directory.CreateDirectory(relativeTimeOutputDirectory);

            Console.WriteLine();
            Console.WriteLine($"Relative time output directory: {relativeTimeOutputDirectory}");

            var relativeTimeGenerator = new RelativeTimeCodeGenerator(config);
            await relativeTimeGenerator.GenerateAsync(cldrRoot, relativeTimeOutputDirectory);

            Console.WriteLine();
            Console.WriteLine($"Relative time classes generated successfully to {relativeTimeOutputDirectory}");
        }

        // Generate list pattern data if output directory is specified
        if (!string.IsNullOrEmpty(options.ListsOutputDirectory))
        {
            var listsOutputDirectory = Path.GetFullPath(options.ListsOutputDirectory);
            Directory.CreateDirectory(listsOutputDirectory);

            Console.WriteLine();
            Console.WriteLine($"List patterns output directory: {listsOutputDirectory}");

            var listPatternGenerator = new ListPatternCodeGenerator(config);
            await listPatternGenerator.GenerateAsync(cldrRoot, listsOutputDirectory);

            Console.WriteLine();
            Console.WriteLine($"List pattern classes generated successfully to {listsOutputDirectory}");
        }

        // Generate date range data if output directory is specified
        if (!string.IsNullOrEmpty(options.DateRangeOutputDirectory))
        {
            var dateRangeOutputDirectory = Path.GetFullPath(options.DateRangeOutputDirectory);
            Directory.CreateDirectory(dateRangeOutputDirectory);

            Console.WriteLine();
            Console.WriteLine($"Date range output directory: {dateRangeOutputDirectory}");

            var dateRangeGenerator = new DateRangeCodeGenerator(config);
            await dateRangeGenerator.GenerateAsync(cldrRoot, dateRangeOutputDirectory);

            Console.WriteLine();
            Console.WriteLine($"Date range classes generated successfully to {dateRangeOutputDirectory}");
        }

        // Generate unit data if output directory is specified
        if (!string.IsNullOrEmpty(options.UnitsOutputDirectory))
        {
            var unitsOutputDirectory = Path.GetFullPath(options.UnitsOutputDirectory);
            Directory.CreateDirectory(unitsOutputDirectory);

            Console.WriteLine();
            Console.WriteLine($"Units output directory: {unitsOutputDirectory}");

            var unitGenerator = new UnitCodeGenerator(config);
            await unitGenerator.GenerateAsync(cldrRoot, unitsOutputDirectory);

            Console.WriteLine();
            Console.WriteLine($"Unit classes generated successfully to {unitsOutputDirectory}");
        }

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
