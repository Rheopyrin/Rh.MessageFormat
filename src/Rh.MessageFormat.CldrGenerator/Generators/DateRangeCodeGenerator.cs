using System.Text;
using System.Text.Json;

namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// Generates C# date range/interval format data classes from CLDR data.
/// </summary>
public class DateRangeCodeGenerator
{
    private readonly CldrConfig _config;

    public DateRangeCodeGenerator(CldrConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Generates date range data classes for all locales.
    /// </summary>
    public async Task GenerateAsync(string cldrRootDir, string outputDir, CancellationToken ct = default)
    {
        Console.WriteLine("Generating date range data classes...");

        // Ensure output directory exists
        Directory.CreateDirectory(outputDir);

        // Clean existing generated files
        foreach (var file in Directory.GetFiles(outputDir, "*.g.cs"))
        {
            File.Delete(file);
        }

        var mainPath = Path.Combine(cldrRootDir, _config.Paths.DatePatternsFolder);
        if (!Directory.Exists(mainPath))
        {
            Console.WriteLine($"  Warning: Date patterns folder not found at {mainPath}");
            return;
        }

        var generatedLocales = new List<(string Locale, string ClassName)>();

        foreach (var localeDir in GetSupportedLocaleDirectories(mainPath))
        {
            ct.ThrowIfCancellationRequested();

            var localeName = Path.GetFileName(localeDir);
            var normalizedLocale = LocaleFilter.Normalize(localeName);

            if (!LocaleFilter.IsSupported(normalizedLocale))
                continue;

            try
            {
                var data = await CollectDateRangeDataAsync(localeDir, ct);
                if (data == null || string.IsNullOrEmpty(data.FallbackPattern))
                    continue;

                var className = GetClassName(normalizedLocale);
                var code = GenerateDateRangeClass(normalizedLocale, className, data);

                var filePath = Path.Combine(outputDir, $"{className}.g.cs");
                await File.WriteAllTextAsync(filePath, code, ct);

                generatedLocales.Add((normalizedLocale, className));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Warning: Failed to generate date range class for '{normalizedLocale}': {ex.Message}");
            }
        }

        Console.WriteLine($"  Generated {generatedLocales.Count} date range classes.");

        // Generate the provider class
        await GenerateProviderAsync(outputDir, generatedLocales, ct);
    }

    private IEnumerable<string> GetSupportedLocaleDirectories(string basePath)
    {
        if (!Directory.Exists(basePath))
            return Enumerable.Empty<string>();

        return Directory.GetDirectories(basePath)
            .Where(d =>
            {
                var name = Path.GetFileName(d);
                var normalized = LocaleFilter.Normalize(name);
                return LocaleFilter.IsSupported(normalized);
            })
            .OrderBy(d => d);
    }

    private async Task<LocaleIntervalFormatData?> CollectDateRangeDataAsync(string localeDir, CancellationToken ct)
    {
        var gregorianPath = Path.Combine(localeDir, _config.Paths.DatePatternsFile);
        if (!File.Exists(gregorianPath))
            return null;

        var json = await File.ReadAllTextAsync(gregorianPath, ct);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("main", out var main))
            return null;

        foreach (var localeProperty in main.EnumerateObject())
        {
            if (localeProperty.Value.TryGetProperty("dates", out var dates) &&
                dates.TryGetProperty("calendars", out var calendars) &&
                calendars.TryGetProperty("gregorian", out var gregorian) &&
                gregorian.TryGetProperty("dateTimeFormats", out var dtFormats) &&
                dtFormats.TryGetProperty("intervalFormats", out var intervalFormats))
            {
                return ExtractIntervalFormats(intervalFormats);
            }
        }

        return null;
    }

    private static LocaleIntervalFormatData ExtractIntervalFormats(JsonElement element)
    {
        var data = new LocaleIntervalFormatData();

        // Get fallback pattern
        if (element.TryGetProperty("intervalFormatFallback", out var fallback))
        {
            data.FallbackPattern = fallback.GetString() ?? "{0} – {1}";
        }

        // Get skeleton patterns
        foreach (var skeletonProp in element.EnumerateObject())
        {
            if (skeletonProp.Name == "intervalFormatFallback")
                continue;

            var skeleton = skeletonProp.Name;
            var patterns = new Dictionary<char, string>();

            foreach (var diffProp in skeletonProp.Value.EnumerateObject())
            {
                // Key is the greatest difference field (y, M, d, H, m, etc.)
                if (diffProp.Name.Length == 1)
                {
                    var diffChar = diffProp.Name[0];
                    var pattern = diffProp.Value.GetString();
                    if (!string.IsNullOrEmpty(pattern))
                    {
                        patterns[diffChar] = pattern;
                    }
                }
            }

            if (patterns.Count > 0)
            {
                data.Skeletons[skeleton] = patterns;
            }
        }

        return data;
    }

    private string GenerateDateRangeClass(string locale, string className, LocaleIntervalFormatData data)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("// Generated from CLDR data. Do not modify manually.");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using Rh.MessageFormat.Abstractions.Models;");
        sb.AppendLine();
        sb.AppendLine("namespace Rh.MessageFormat.CldrData.DateRange.Generated;");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Date range/interval data for locale '{locale}'.");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"internal static class {className}");
        sb.AppendLine("{");
        sb.AppendLine($"    public static readonly string Locale = \"{locale}\";");
        sb.AppendLine();
        sb.AppendLine("    private static IntervalFormatData? _data;");
        sb.AppendLine();
        sb.AppendLine("    public static IntervalFormatData Data => _data ??= CreateData();");
        sb.AppendLine();
        sb.AppendLine("    private static IntervalFormatData CreateData()");
        sb.AppendLine("    {");

        if (data.Skeletons.Count > 0)
        {
            sb.AppendLine("        var skeletons = new Dictionary<string, IntervalPatterns>(StringComparer.Ordinal)");
            sb.AppendLine("        {");

            foreach (var (skeleton, patterns) in data.Skeletons.OrderBy(kvp => kvp.Key))
            {
                sb.AppendLine($"            {{ \"{skeleton}\", new IntervalPatterns(new Dictionary<char, string>()");
                sb.AppendLine("            {");
                foreach (var (diff, pattern) in patterns.OrderBy(kvp => kvp.Key))
                {
                    sb.AppendLine($"                {{ '{diff}', \"{EscapeString(pattern)}\" }},");
                }
                sb.AppendLine("            }) },");
            }

            sb.AppendLine("        };");
            sb.AppendLine();
            sb.AppendLine($"        return new IntervalFormatData(\"{EscapeString(data.FallbackPattern)}\", skeletons);");
        }
        else
        {
            sb.AppendLine($"        return new IntervalFormatData(\"{EscapeString(data.FallbackPattern)}\");");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private async Task GenerateProviderAsync(string outputDir, List<(string Locale, string ClassName)> locales, CancellationToken ct)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("// Generated from CLDR data. Do not modify manually.");
        sb.AppendLine($"// Generated: {DateTime.UtcNow:O}");
        sb.AppendLine($"// Locale count: {locales.Count}");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine("using Rh.MessageFormat.Abstractions.Models;");
        sb.AppendLine("using Rh.MessageFormat.CldrData.Services;");
        sb.AppendLine("using Rh.MessageFormat.CldrData.DateRange.Generated;");
        sb.AppendLine();
        sb.AppendLine("namespace Rh.MessageFormat.CldrData.DateRange;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Provides date range/interval data lookup and auto-registers with CldrDataProvider.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class DateRangeDataProvider");
        sb.AppendLine("{");
        sb.AppendLine("    private static readonly Dictionary<string, Func<IntervalFormatData>> _locales =");
        sb.AppendLine("        new(StringComparer.OrdinalIgnoreCase)");
        sb.AppendLine("    {");

        foreach (var (locale, className) in locales.OrderBy(l => l.Locale))
        {
            sb.AppendLine($"        {{ \"{locale}\", () => {className}.Data }},");
        }

        sb.AppendLine("    };");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Module initializer that auto-registers this provider with CldrDataProvider.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    [ModuleInitializer]");
        sb.AppendLine("    public static void Initialize()");
        sb.AppendLine("    {");
        sb.AppendLine("        CldrDataProvider.DateRangeDataProvider = GetDateRangeData;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets date range/interval data for a locale.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static IntervalFormatData? GetDateRangeData(string locale)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_locales.TryGetValue(locale, out var factory))");
        sb.AppendLine("            return factory();");
        sb.AppendLine();
        sb.AppendLine("        // Try base language fallback");
        sb.AppendLine("        var dashIndex = locale.IndexOf('-');");
        sb.AppendLine("        if (dashIndex > 0)");
        sb.AppendLine("        {");
        sb.AppendLine("            var baseLocale = locale.Substring(0, dashIndex);");
        sb.AppendLine("            if (_locales.TryGetValue(baseLocale, out factory))");
        sb.AppendLine("                return factory();");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        return null;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets all available date range locales.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static IReadOnlyCollection<string> AvailableLocales => _locales.Keys;");
        sb.AppendLine("}");

        var filePath = Path.Combine(outputDir, "DateRangeDataProvider.g.cs");
        await File.WriteAllTextAsync(filePath, sb.ToString(), ct);

        Console.WriteLine($"  Generated date range provider with {locales.Count} locales.");
    }

    public static string GetClassName(string locale)
    {
        var safe = locale.Replace('-', '_').Replace('.', '_');
        return $"DateRangeData_{safe}";
    }

    private static string EscapeString(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }
}
