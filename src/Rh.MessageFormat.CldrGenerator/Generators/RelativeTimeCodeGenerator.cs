using System.Text;
using System.Text.Json;

namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// Generates C# relative time data classes from CLDR data.
/// </summary>
public class RelativeTimeCodeGenerator
{
    private readonly CldrConfig _config;

    /// <summary>
    /// Fields that support relative time formatting.
    /// </summary>
    private static readonly string[] RelativeTimeFields =
    {
        "year", "quarter", "month", "week", "day", "hour", "minute", "second",
        "sun", "mon", "tue", "wed", "thu", "fri", "sat"
    };

    /// <summary>
    /// Widths supported for relative time formatting.
    /// </summary>
    private static readonly string[] RelativeTimeWidths = { "long", "short", "narrow" };

    public RelativeTimeCodeGenerator(CldrConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Generates relative time data classes for all locales.
    /// </summary>
    public async Task GenerateAsync(string cldrRootDir, string outputDir, CancellationToken ct = default)
    {
        Console.WriteLine("Generating relative time data classes...");

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
                var data = await CollectRelativeTimeDataAsync(localeDir, ct);
                if (data == null || data.Count == 0)
                    continue;

                var className = GetClassName(normalizedLocale);
                var code = GenerateRelativeTimeClass(normalizedLocale, className, data);

                var filePath = Path.Combine(outputDir, $"{className}.g.cs");
                await File.WriteAllTextAsync(filePath, code, ct);

                generatedLocales.Add((normalizedLocale, className));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Warning: Failed to generate relative time class for '{normalizedLocale}': {ex.Message}");
            }
        }

        Console.WriteLine($"  Generated {generatedLocales.Count} relative time classes.");

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

    private async Task<Dictionary<string, LocaleRelativeTimeData>?> CollectRelativeTimeDataAsync(string localeDir, CancellationToken ct)
    {
        var dateFieldsPath = Path.Combine(localeDir, _config.Paths.DateFieldsFile);
        if (!File.Exists(dateFieldsPath))
            return null;

        var json = await File.ReadAllTextAsync(dateFieldsPath, ct);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("main", out var main))
            return null;

        var result = new Dictionary<string, LocaleRelativeTimeData>();

        foreach (var localeProperty in main.EnumerateObject())
        {
            if (localeProperty.Value.TryGetProperty("dates", out var dates) &&
                dates.TryGetProperty("fields", out var fields))
            {
                ProcessRelativeTimeFields(fields, result);
            }
        }

        return result;
    }

    private void ProcessRelativeTimeFields(JsonElement fields, Dictionary<string, LocaleRelativeTimeData> result)
    {
        foreach (var baseField in RelativeTimeFields)
        {
            foreach (var width in RelativeTimeWidths)
            {
                var fieldName = width == "long" ? baseField : $"{baseField}-{width}";

                if (!fields.TryGetProperty(fieldName, out var fieldElement))
                    continue;

                var key = $"{baseField}:{width}";
                var data = new LocaleRelativeTimeData
                {
                    Field = baseField,
                    Width = width,
                    DisplayName = GetStringProperty(fieldElement, "displayName")
                };

                // Collect relative types (-1, 0, 1)
                foreach (var offset in new[] { -1, 0, 1 })
                {
                    var propName = $"relative-type-{offset}";
                    var value = GetStringProperty(fieldElement, propName);
                    if (value != null)
                    {
                        data.RelativeTypes[offset.ToString()] = value;
                    }
                }

                // Collect future patterns
                if (fieldElement.TryGetProperty("relativeTime-type-future", out var futureObj))
                {
                    foreach (var count in new[] { "zero", "one", "two", "few", "many", "other" })
                    {
                        var pattern = GetStringProperty(futureObj, $"relativeTimePattern-count-{count}");
                        if (pattern != null)
                        {
                            data.FuturePatterns[count] = pattern;
                        }
                    }
                }

                // Collect past patterns
                if (fieldElement.TryGetProperty("relativeTime-type-past", out var pastObj))
                {
                    foreach (var count in new[] { "zero", "one", "two", "few", "many", "other" })
                    {
                        var pattern = GetStringProperty(pastObj, $"relativeTimePattern-count-{count}");
                        if (pattern != null)
                        {
                            data.PastPatterns[count] = pattern;
                        }
                    }
                }

                if (data.RelativeTypes.Count > 0 || data.FuturePatterns.Count > 0 || data.PastPatterns.Count > 0)
                {
                    result[key] = data;
                }
            }
        }
    }

    private static string? GetStringProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;
    }

    private string GenerateRelativeTimeClass(string locale, string className, Dictionary<string, LocaleRelativeTimeData> data)
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
        sb.AppendLine("namespace Rh.MessageFormat.CldrData.RelativeTime.Generated;");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Relative time data for locale '{locale}'.");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"internal static class {className}");
        sb.AppendLine("{");
        sb.AppendLine($"    public static readonly string Locale = \"{locale}\";");
        sb.AppendLine();
        sb.AppendLine("    private static Dictionary<string, RelativeTimeData>? _data;");
        sb.AppendLine();
        sb.AppendLine("    public static Dictionary<string, RelativeTimeData> Data => _data ??= CreateData();");
        sb.AppendLine();
        sb.AppendLine("    private static Dictionary<string, RelativeTimeData> CreateData()");
        sb.AppendLine("    {");
        sb.AppendLine("        return new Dictionary<string, RelativeTimeData>(StringComparer.Ordinal)");
        sb.AppendLine("        {");

        foreach (var (key, rtData) in data.OrderBy(kvp => kvp.Key))
        {
            sb.AppendLine($"            {{ \"{key}\", new RelativeTimeData(");
            sb.AppendLine($"                \"{EscapeString(rtData.Field)}\",");
            sb.AppendLine($"                {(rtData.DisplayName != null ? $"\"{EscapeString(rtData.DisplayName)}\"" : "null")},");

            // RelativeTypes
            if (rtData.RelativeTypes.Count > 0)
            {
                sb.AppendLine("                new Dictionary<string, string>(StringComparer.Ordinal)");
                sb.AppendLine("                {");
                foreach (var (offset, value) in rtData.RelativeTypes.OrderBy(kvp => kvp.Key))
                {
                    sb.AppendLine($"                    {{ \"{offset}\", \"{EscapeString(value)}\" }},");
                }
                sb.AppendLine("                },");
            }
            else
            {
                sb.AppendLine("                null,");
            }

            // FuturePatterns
            if (rtData.FuturePatterns.Count > 0)
            {
                sb.AppendLine("                new Dictionary<string, string>(StringComparer.Ordinal)");
                sb.AppendLine("                {");
                foreach (var (count, pattern) in rtData.FuturePatterns.OrderBy(kvp => kvp.Key))
                {
                    sb.AppendLine($"                    {{ \"{count}\", \"{EscapeString(pattern)}\" }},");
                }
                sb.AppendLine("                },");
            }
            else
            {
                sb.AppendLine("                null,");
            }

            // PastPatterns
            if (rtData.PastPatterns.Count > 0)
            {
                sb.AppendLine("                new Dictionary<string, string>(StringComparer.Ordinal)");
                sb.AppendLine("                {");
                foreach (var (count, pattern) in rtData.PastPatterns.OrderBy(kvp => kvp.Key))
                {
                    sb.AppendLine($"                    {{ \"{count}\", \"{EscapeString(pattern)}\" }},");
                }
                sb.AppendLine("                }");
            }
            else
            {
                sb.AppendLine("                null");
            }

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
        sb.AppendLine("using Rh.MessageFormat.CldrData.RelativeTime.Generated;");
        sb.AppendLine();
        sb.AppendLine("namespace Rh.MessageFormat.CldrData.RelativeTime;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Provides relative time data lookup and auto-registers with CldrDataProvider.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class RelativeTimeDataProvider");
        sb.AppendLine("{");
        sb.AppendLine("    private static readonly Dictionary<string, Func<Dictionary<string, RelativeTimeData>>> _locales =");
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
        sb.AppendLine("        CldrDataProvider.RelativeTimeDataProvider = GetRelativeTimeData;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets relative time data for a locale, field, and width.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static RelativeTimeData? GetRelativeTimeData(string locale, string field, string width)");
        sb.AppendLine("    {");
        sb.AppendLine("        var data = GetLocaleData(locale);");
        sb.AppendLine("        if (data == null)");
        sb.AppendLine("            return null;");
        sb.AppendLine();
        sb.AppendLine("        var key = $\"{field}:{width}\";");
        sb.AppendLine("        if (data.TryGetValue(key, out var result))");
        sb.AppendLine("            return result;");
        sb.AppendLine();
        sb.AppendLine("        // Fallback to long width");
        sb.AppendLine("        if (width != \"long\")");
        sb.AppendLine("        {");
        sb.AppendLine("            key = $\"{field}:long\";");
        sb.AppendLine("            if (data.TryGetValue(key, out result))");
        sb.AppendLine("                return result;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        return null;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static Dictionary<string, RelativeTimeData>? GetLocaleData(string locale)");
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
        sb.AppendLine("    /// Gets all available relative time locales.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static IReadOnlyCollection<string> AvailableLocales => _locales.Keys;");
        sb.AppendLine("}");

        var filePath = Path.Combine(outputDir, "RelativeTimeDataProvider.g.cs");
        await File.WriteAllTextAsync(filePath, sb.ToString(), ct);

        Console.WriteLine($"  Generated relative time provider with {locales.Count} locales.");
    }

    public static string GetClassName(string locale)
    {
        var safe = locale.Replace('-', '_').Replace('.', '_');
        return $"RelativeTimeData_{safe}";
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
