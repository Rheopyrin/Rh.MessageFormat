using System.Text;
using Rh.MessageFormat.CldrGenerator.Templates;

namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// Main orchestrator for generating C# locale classes from CLDR data using T4 templates.
/// </summary>
public class LocaleCodeGenerator
{
    private readonly CldrConfig _config;

    public LocaleCodeGenerator(CldrConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Generates all locale C# classes and the provider registry.
    /// </summary>
    /// <param name="cldrRootDir">Path to CLDR data root folder (e.g., cldr-json folder containing cldr-core, cldr-numbers-full, etc.).</param>
    /// <param name="outputDir">Output directory for generated files.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task GenerateAsync(string cldrRootDir, string outputDir, CancellationToken ct = default)
    {
        Console.WriteLine("Collecting CLDR data from all locales...");

        // Collect all locale data
        var collector = new LocaleDataCollector(_config, cldrRootDir);
        var locales = await collector.CollectAllAsync(ct);

        Console.WriteLine($"Collected data for {locales.Count} locales.");

        // Ensure output directory exists
        Directory.CreateDirectory(outputDir);

        // Get templates directory
        var templatesDir = Path.Combine(AppContext.BaseDirectory, "Templates");
        if (!Directory.Exists(templatesDir))
        {
            // Fallback for development - look relative to the project
            templatesDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Templates");
        }

        Console.WriteLine($"Using templates from: {templatesDir}");

        // Initialize template engine
        var templateEngine = new TemplateEngine(templatesDir);

        // Generate class for each locale that has unique data
        var generatedClasses = 0;
        var reusedClasses = 0;
        var totalCount = locales.Count;
        var currentCount = 0;

        foreach (var (locale, data) in locales.OrderBy(kvp => kvp.Key))
        {
            ct.ThrowIfCancellationRequested();
            currentCount++;

            // Skip locales that reuse another class
            if (data.UseClassFrom != null)
            {
                reusedClasses++;
                continue;
            }

            try
            {
                var generator = new LocaleClassGenerator(data);
                var templateData = generator.PrepareTemplateData();
                var code = await templateEngine.GenerateLocaleClassAsync(templateData);

                var className = LocaleClassGenerator.GetClassName(locale);
                var fileName = $"{className}.g.cs";
                var filePath = Path.Combine(outputDir, fileName);

                await File.WriteAllTextAsync(filePath, code, ct);
                generatedClasses++;

                if (currentCount % 50 == 0 || currentCount == totalCount)
                {
                    Console.WriteLine($"  Processed {currentCount}/{totalCount} locales...");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Warning: Failed to generate class for locale '{locale}': {ex.Message}");
            }
        }

        Console.WriteLine($"Generated {generatedClasses} unique locale classes ({reusedClasses} locales reuse existing classes).");

        // Generate the provider registry with all locales (including those reusing classes)
        await GenerateProviderRegistryAsync(templateEngine, outputDir, locales, ct);

        Console.WriteLine($"Generation complete. Output directory: {outputDir}");
    }

    /// <summary>
    /// Generates the CldrDataProvider.g.cs file using template.
    /// </summary>
    private async Task GenerateProviderRegistryAsync(
        TemplateEngine templateEngine,
        string outputDir,
        Dictionary<string, LocaleData> locales,
        CancellationToken ct)
    {
        Console.WriteLine("Generating provider registry...");

        // Prepare locale entries - map each locale to the appropriate class
        var sb = new StringBuilder();
        foreach (var (locale, data) in locales.OrderBy(kvp => kvp.Key))
        {
            // Use the class from another locale if specified, otherwise use own class
            var classLocale = data.UseClassFrom ?? locale;
            var className = LocaleClassGenerator.GetClassName(classLocale);
            sb.AppendLine($"        {{ \"{locale}\", () => {className}.Instance }},");
        }

        var templateData = new ProviderRegistryTemplateData
        {
            GeneratedDate = DateTime.UtcNow.ToString("O"),
            LocaleCount = locales.Count,
            LocaleEntries = sb.ToString().TrimEnd()
        };

        var code = await templateEngine.GenerateProviderRegistryAsync(templateData);

        var filePath = Path.Combine(outputDir, "CldrDataProvider.g.cs");
        await File.WriteAllTextAsync(filePath, code, ct);

        Console.WriteLine($"  Generated provider registry with {locales.Count} locales.");
    }
}
