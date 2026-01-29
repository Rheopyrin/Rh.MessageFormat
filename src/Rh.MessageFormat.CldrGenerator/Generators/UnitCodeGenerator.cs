using System.Text;
using System.Text.Json;

namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// Generates C# unit data classes from CLDR data.
/// </summary>
public class UnitCodeGenerator
{
    private readonly CldrConfig _config;

    public UnitCodeGenerator(CldrConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Generates unit data classes for all locales.
    /// </summary>
    public async Task GenerateAsync(string cldrRootDir, string outputDir, CancellationToken ct = default)
    {
        Console.WriteLine("Generating unit data classes...");

        // Ensure output directory exists
        Directory.CreateDirectory(outputDir);

        // Clean existing generated files
        foreach (var file in Directory.GetFiles(outputDir, "*.g.cs"))
        {
            File.Delete(file);
        }

        var mainPath = Path.Combine(cldrRootDir, _config.Paths.UnitsFolder);
        if (!Directory.Exists(mainPath))
        {
            Console.WriteLine($"  Warning: Units folder not found at {mainPath}");
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
                var data = await CollectUnitDataAsync(localeDir, ct);
                if (data == null || data.Count == 0)
                    continue;

                var className = GetClassName(normalizedLocale);
                var code = GenerateUnitClass(normalizedLocale, className, data);

                var filePath = Path.Combine(outputDir, $"{className}.g.cs");
                await File.WriteAllTextAsync(filePath, code, ct);

                generatedLocales.Add((normalizedLocale, className));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Warning: Failed to generate unit class for '{normalizedLocale}': {ex.Message}");
            }
        }

        Console.WriteLine($"  Generated {generatedLocales.Count} unit classes.");

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

    private async Task<Dictionary<string, LocaleUnitData>?> CollectUnitDataAsync(string localeDir, CancellationToken ct)
    {
        var unitsPath = Path.Combine(localeDir, _config.Paths.UnitsFile);
        if (!File.Exists(unitsPath))
            return null;

        var json = await File.ReadAllTextAsync(unitsPath, ct);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("main", out var main))
            return null;

        var result = new Dictionary<string, LocaleUnitData>();

        foreach (var localeProperty in main.EnumerateObject())
        {
            if (localeProperty.Value.TryGetProperty("units", out var unitsObj))
            {
                ProcessUnitWidth(unitsObj, "long", result);
                ProcessUnitWidth(unitsObj, "short", result);
                ProcessUnitWidth(unitsObj, "narrow", result);
            }
        }

        return result;
    }

    private void ProcessUnitWidth(JsonElement unitsObj, string width, Dictionary<string, LocaleUnitData> result)
    {
        if (!unitsObj.TryGetProperty(width, out var widthObj))
            return;

        foreach (var unitProperty in widthObj.EnumerateObject())
        {
            var unitId = unitProperty.Name;
            // Skip compound and power units
            if (unitId.StartsWith("per") || unitId.StartsWith("times") || unitId.StartsWith("power") ||
                unitId == "coordinateUnit" || unitId == "durationUnit")
                continue;

            if (!result.TryGetValue(unitId, out var unitData))
            {
                unitData = new LocaleUnitData { Id = unitId };
                result[unitId] = unitData;
            }

            var unitElement = unitProperty.Value;

            if (unitElement.TryGetProperty("displayName", out var displayName))
                unitData.DisplayName ??= displayName.GetString();

            foreach (var count in new[] { "zero", "one", "two", "few", "many", "other" })
            {
                if (unitElement.TryGetProperty($"unitPattern-count-{count}", out var pattern))
                {
                    unitData.Patterns[$"{width}:{count}"] = pattern.GetString() ?? "";
                }
            }
        }
    }

    private string GenerateUnitClass(string locale, string className, Dictionary<string, LocaleUnitData> data)
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
        sb.AppendLine("namespace Rh.MessageFormat.CldrData.Units.Generated;");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Unit data for locale '{locale}'.");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"internal static class {className}");
        sb.AppendLine("{");
        sb.AppendLine($"    public static readonly string Locale = \"{locale}\";");
        sb.AppendLine();
        sb.AppendLine("    private static Dictionary<string, UnitData>? _data;");
        sb.AppendLine();
        sb.AppendLine("    public static Dictionary<string, UnitData> Data => _data ??= CreateData();");
        sb.AppendLine();
        sb.AppendLine("    private static Dictionary<string, UnitData> CreateData()");
        sb.AppendLine("    {");
        sb.AppendLine("        return new Dictionary<string, UnitData>(StringComparer.Ordinal)");
        sb.AppendLine("        {");

        foreach (var (unitId, unitData) in data.OrderBy(kvp => kvp.Key))
        {
            if (unitData.Patterns.Count == 0)
                continue;

            sb.AppendLine($"            {{ \"{unitId}\", new UnitData(");
            sb.AppendLine($"                \"{EscapeString(unitId)}\",");
            sb.AppendLine("                new Dictionary<string, string>(StringComparer.Ordinal)");
            sb.AppendLine("                {");

            foreach (var (key, pattern) in unitData.Patterns.OrderBy(kvp => kvp.Key))
            {
                sb.AppendLine($"                    {{ \"{key}\", \"{EscapeString(pattern)}\" }},");
            }

            sb.AppendLine("                }");
            sb.AppendLine("            ) },");
        }

        sb.AppendLine("        };");
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
        sb.AppendLine("using Rh.MessageFormat.CldrData.Units.Generated;");
        sb.AppendLine();
        sb.AppendLine("namespace Rh.MessageFormat.CldrData.Units;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Provides unit data lookup and auto-registers with CldrDataProvider.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class UnitDataProvider");
        sb.AppendLine("{");
        sb.AppendLine("    private static readonly Dictionary<string, Func<Dictionary<string, UnitData>>> _locales =");
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
        sb.AppendLine("        CldrDataProvider.UnitDataProvider = GetUnitData;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets unit data for a locale and unit ID.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static UnitData? GetUnitData(string locale, string unitId)");
        sb.AppendLine("    {");
        sb.AppendLine("        var data = GetLocaleData(locale);");
        sb.AppendLine("        if (data == null)");
        sb.AppendLine("            return null;");
        sb.AppendLine();
        sb.AppendLine("        if (data.TryGetValue(unitId, out var result))");
        sb.AppendLine("            return result;");
        sb.AppendLine();
        sb.AppendLine("        return null;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static Dictionary<string, UnitData>? GetLocaleData(string locale)");
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
        sb.AppendLine("    /// Gets all available unit locales.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static IReadOnlyCollection<string> AvailableLocales => _locales.Keys;");
        sb.AppendLine("}");

        var filePath = Path.Combine(outputDir, "UnitDataProvider.g.cs");
        await File.WriteAllTextAsync(filePath, sb.ToString(), ct);

        Console.WriteLine($"  Generated unit provider with {locales.Count} locales.");
    }

    public static string GetClassName(string locale)
    {
        var safe = locale.Replace('-', '_').Replace('.', '_');
        return $"UnitData_{safe}";
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
