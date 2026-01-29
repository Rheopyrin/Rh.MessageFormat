using System.Text;
using System.Text.Json;

namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// Generates C# spellout data classes from CLDR RBNF data.
/// </summary>
public class SpelloutCodeGenerator
{
    private readonly CldrConfig _config;

    public SpelloutCodeGenerator(CldrConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Generates spellout data classes for all locales with RBNF data.
    /// </summary>
    public async Task GenerateAsync(string cldrRootDir, string outputDir, CancellationToken ct = default)
    {
        Console.WriteLine("Generating spellout data classes...");

        // Ensure output directory exists
        Directory.CreateDirectory(outputDir);

        // Clean existing generated files
        foreach (var file in Directory.GetFiles(outputDir, "*.g.cs"))
        {
            File.Delete(file);
        }

        // Find RBNF folder
        var rbnfFolder = Path.Combine(cldrRootDir, _config.Paths.RbnfFolder);
        if (!Directory.Exists(rbnfFolder))
        {
            Console.WriteLine($"  Warning: RBNF folder not found at {rbnfFolder}");
            return;
        }

        // Get all RBNF JSON files
        var jsonFiles = Directory.GetFiles(rbnfFolder, "*.json");
        Console.WriteLine($"  Found {jsonFiles.Length} RBNF files.");

        var generatedLocales = new List<(string Locale, string ClassName)>();

        foreach (var jsonFile in jsonFiles.OrderBy(f => f))
        {
            ct.ThrowIfCancellationRequested();

            var locale = Path.GetFileNameWithoutExtension(jsonFile);
            var normalizedLocale = locale.Replace('_', '-');

            // Check if this locale should be included based on the filter
            // For RBNF, we need to be more lenient - include if the base language is supported
            if (!ShouldIncludeLocale(normalizedLocale))
            {
                continue;
            }

            try
            {
                var spelloutData = await ParseRbnfFileAsync(jsonFile, ct);
                if (spelloutData == null || spelloutData.RuleSets.Count == 0)
                {
                    continue;
                }

                var className = GetClassName(locale);
                var code = GenerateSpelloutClass(locale, className, spelloutData);

                var filePath = Path.Combine(outputDir, $"{className}.g.cs");
                await File.WriteAllTextAsync(filePath, code, ct);

                generatedLocales.Add((locale, className));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Warning: Failed to generate spellout class for '{locale}': {ex.Message}");
            }
        }

        Console.WriteLine($"  Generated {generatedLocales.Count} spellout classes.");

        // Generate the provider partial class
        await GenerateProviderPartialAsync(outputDir, generatedLocales, ct);
    }

    private bool ShouldIncludeLocale(string locale)
    {
        // If no filter is set, include all
        if (!LocaleFilter.HasUserFilter)
        {
            return true;
        }

        // Check exact match
        if (LocaleFilter.IsSupported(locale))
        {
            return true;
        }

        // Check if base language matches any filtered locale
        var dashIndex = locale.IndexOf('-');
        if (dashIndex > 0)
        {
            var baseLanguage = locale.Substring(0, dashIndex);
            if (LocaleFilter.IsSupported(baseLanguage))
            {
                return true;
            }
        }
        else
        {
            // This is a base language - check if any regional variant is in the filter
            var filteredLocales = LocaleFilter.GetUserFilteredLocales();
            if (filteredLocales != null)
            {
                return filteredLocales.Any(l => l.StartsWith(locale + "-", StringComparison.OrdinalIgnoreCase) ||
                                                l.Equals(locale, StringComparison.OrdinalIgnoreCase));
            }
        }

        return false;
    }

    private async Task<SpelloutFileData?> ParseRbnfFileAsync(string jsonFile, CancellationToken ct)
    {
        var json = await File.ReadAllTextAsync(jsonFile, ct);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("rbnf", out var rbnfRoot))
            return null;

        if (!rbnfRoot.TryGetProperty("rbnf", out var rbnfData))
            return null;

        if (!rbnfData.TryGetProperty("SpelloutRules", out var spelloutRules))
            return null;

        var result = new SpelloutFileData();

        foreach (var property in spelloutRules.EnumerateObject())
        {
            // Skip metadata properties
            if (property.Name.StartsWith("_"))
                continue;

            var ruleSetName = property.Name;
            var rules = ParseRuleSet(property.Value);

            if (rules.Count > 0)
            {
                result.RuleSets[ruleSetName] = rules;
            }
        }

        return result;
    }

    private List<(string Key, string Value)> ParseRuleSet(JsonElement ruleSetElement)
    {
        var rules = new List<(string Key, string Value)>();

        foreach (var ruleArray in ruleSetElement.EnumerateArray())
        {
            if (ruleArray.ValueKind != JsonValueKind.Array)
                continue;

            var items = ruleArray.EnumerateArray().ToList();
            if (items.Count < 2)
                continue;

            var key = items[0].GetString();
            var value = items[1].GetString();

            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                rules.Add((key, value));
            }
        }

        return rules;
    }

    private string GenerateSpelloutClass(string locale, string className, SpelloutFileData data)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("// Generated from CLDR RBNF data. Do not modify manually.");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using Rh.MessageFormat.Formatting.Spellout;");
        sb.AppendLine();
        sb.AppendLine("namespace Rh.MessageFormat.CldrData.Spellout.Generated;");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Spellout data for locale '{locale}'.");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"internal static class {className}");
        sb.AppendLine("{");
        sb.AppendLine($"    public static readonly string Locale = \"{locale}\";");
        sb.AppendLine();
        sb.AppendLine("    private static SpelloutData? _instance;");
        sb.AppendLine();
        sb.AppendLine("    public static SpelloutData Instance => _instance ??= CreateData();");
        sb.AppendLine();
        sb.AppendLine("    private static SpelloutData CreateData()");
        sb.AppendLine("    {");
        sb.AppendLine("        var ruleSets = new Dictionary<string, RbnfRuleSet>(StringComparer.Ordinal);");
        sb.AppendLine();

        foreach (var (ruleSetName, rules) in data.RuleSets.OrderBy(kvp => kvp.Key))
        {
            var safeRuleSetName = GetSafeIdentifier(ruleSetName);
            sb.AppendLine($"        // Rule set: {ruleSetName}");
            sb.AppendLine($"        ruleSets[\"{EscapeString(ruleSetName)}\"] = new RbnfRuleSet(\"{EscapeString(ruleSetName)}\", new RbnfRule[]");
            sb.AppendLine("        {");

            foreach (var (key, value) in rules)
            {
                sb.AppendLine($"            RbnfRule.Parse(\"{EscapeString(key)}\", \"{EscapeString(value)}\"),");
            }

            sb.AppendLine("        });");
            sb.AppendLine();
        }

        sb.AppendLine($"        return new SpelloutData(\"{locale}\", ruleSets);");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private async Task GenerateProviderPartialAsync(string outputDir, List<(string Locale, string ClassName)> locales, CancellationToken ct)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("// Generated from CLDR RBNF data. Do not modify manually.");
        sb.AppendLine($"// Generated: {DateTime.UtcNow:O}");
        sb.AppendLine($"// Locale count: {locales.Count}");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine("using Rh.MessageFormat.Formatting.Spellout;");
        sb.AppendLine("using Rh.MessageFormat.CldrData.Services;");
        sb.AppendLine("using Rh.MessageFormat.CldrData.Spellout.Generated;");
        sb.AppendLine();
        sb.AppendLine("namespace Rh.MessageFormat.CldrData.Spellout;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Provides spellout data lookup and auto-registers with CldrDataProvider.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class SpelloutDataProvider");
        sb.AppendLine("{");
        sb.AppendLine("    private static readonly Dictionary<string, Func<SpelloutData>> _locales =");
        sb.AppendLine("        new(StringComparer.OrdinalIgnoreCase)");
        sb.AppendLine("    {");

        foreach (var (locale, className) in locales.OrderBy(l => l.Locale))
        {
            sb.AppendLine($"        {{ \"{locale}\", () => {className}.Instance }},");
        }

        sb.AppendLine("    };");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Module initializer that auto-registers this provider with CldrDataProvider.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    [ModuleInitializer]");
        sb.AppendLine("    public static void Initialize()");
        sb.AppendLine("    {");
        sb.AppendLine("        CldrDataProvider.SpelloutDataProvider = GetSpelloutData;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets spellout data for a locale.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static SpelloutData? GetSpelloutData(string locale)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_locales.TryGetValue(locale, out var factory))");
        sb.AppendLine("        {");
        sb.AppendLine("            return factory();");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // Try base language fallback");
        sb.AppendLine("        var dashIndex = locale.IndexOf('-');");
        sb.AppendLine("        if (dashIndex > 0)");
        sb.AppendLine("        {");
        sb.AppendLine("            var baseLocale = locale.Substring(0, dashIndex);");
        sb.AppendLine("            if (_locales.TryGetValue(baseLocale, out factory))");
        sb.AppendLine("            {");
        sb.AppendLine("                return factory();");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        return null;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets all available spellout locales.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static IReadOnlyCollection<string> AvailableLocales => _locales.Keys;");
        sb.AppendLine("}");

        var filePath = Path.Combine(outputDir, "SpelloutDataProvider.g.cs");
        await File.WriteAllTextAsync(filePath, sb.ToString(), ct);

        Console.WriteLine($"  Generated spellout provider with {locales.Count} locales.");
    }

    public static string GetClassName(string locale)
    {
        // Convert locale to valid C# identifier
        var safe = locale.Replace('-', '_').Replace('.', '_');
        return $"SpelloutData_{safe}";
    }

    private static string GetSafeIdentifier(string name)
    {
        return name.Replace("%", "").Replace("-", "_");
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

    private class SpelloutFileData
    {
        public Dictionary<string, List<(string Key, string Value)>> RuleSets { get; } = new();
    }
}
