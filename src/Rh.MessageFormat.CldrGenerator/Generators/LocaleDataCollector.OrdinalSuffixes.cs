using System.Text.Json;
using System.Text.RegularExpressions;

namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// Ordinal suffix collection from RBNF data.
/// </summary>
public partial class LocaleDataCollector
{
    // Pattern to extract ordinal suffixes from RBNF format: $(ordinal,one{st}two{nd}few{rd}other{th})$
    private static readonly Regex OrdinalSuffixPattern = new(
        @"\$\(ordinal,([^)]+)\)\$",
        RegexOptions.Compiled);

    // Pattern to extract individual category-suffix pairs: one{st}
    private static readonly Regex CategorySuffixPattern = new(
        @"(\w+)\{([^}]*)\}",
        RegexOptions.Compiled);

    /// <summary>
    /// Collects ordinal suffixes from RBNF JSON files.
    /// </summary>
    public async Task CollectOrdinalSuffixesAsync(Dictionary<string, LocaleData> locales, CancellationToken ct = default)
    {
        var rbnfPath = Path.Combine(_cldrExtractedDir, _config.Paths.RbnfFolder);

        if (!Directory.Exists(rbnfPath))
        {
            Console.WriteLine($"RBNF folder not found: {rbnfPath}");
            return;
        }

        foreach (var jsonFile in Directory.GetFiles(rbnfPath, "*.json"))
        {
            var fileName = Path.GetFileNameWithoutExtension(jsonFile);

            // Skip files with suffixes (like language-Script)
            if (fileName.Contains('-'))
                continue;

            var normalizedLocale = LocaleFilter.Normalize(fileName);

            try
            {
                var suffixes = await ExtractOrdinalSuffixesFromJsonAsync(jsonFile, ct);

                if (suffixes != null && suffixes.Count > 0)
                {
                    // Apply to base locale
                    if (locales.TryGetValue(normalizedLocale, out var localeData))
                    {
                        localeData.OrdinalSuffixes = suffixes;
                    }

                    // Also apply to regional variants that inherit from this locale
                    foreach (var (locale, data) in locales)
                    {
                        if (locale.StartsWith(normalizedLocale + "_", StringComparison.OrdinalIgnoreCase) ||
                            locale.StartsWith(normalizedLocale + "-", StringComparison.OrdinalIgnoreCase))
                        {
                            data.OrdinalSuffixes ??= suffixes;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to process RBNF file {jsonFile}: {ex.Message}");
            }
        }
    }

    private async Task<Dictionary<string, string>?> ExtractOrdinalSuffixesFromJsonAsync(string jsonPath, CancellationToken ct)
    {
        using var doc = await ReadJsonFileAsync(jsonPath, ct);

        // Navigate to rbnf.rbnf.OrdinalRules
        if (!doc.RootElement.TryGetProperty("rbnf", out var rbnfRoot))
            return null;

        if (!rbnfRoot.TryGetProperty("rbnf", out var rbnf))
            return null;

        if (!rbnf.TryGetProperty("OrdinalRules", out var ordinalRules))
            return null;

        // Look for %digits-ordinal rule (the default ordinal formatter)
        if (!ordinalRules.TryGetProperty("%digits-ordinal", out var digitsOrdinal))
            return null;

        // The rules are in an array format: [["0", "=#,##0=$(ordinal,one{st}two{nd}few{rd}other{th})$;"]]
        if (digitsOrdinal.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var rule in digitsOrdinal.EnumerateArray())
        {
            if (rule.ValueKind != JsonValueKind.Array)
                continue;

            var ruleArray = rule.EnumerateArray().ToList();
            if (ruleArray.Count < 2)
                continue;

            var ruleText = ruleArray[1].GetString();
            if (string.IsNullOrEmpty(ruleText))
                continue;

            var suffixes = ParseOrdinalSuffixes(ruleText);
            if (suffixes != null && suffixes.Count > 0)
                return suffixes;
        }

        return null;
    }

    private static Dictionary<string, string>? ParseOrdinalSuffixes(string ruleText)
    {
        // Match: $(ordinal,one{st}two{nd}few{rd}other{th})$
        var match = OrdinalSuffixPattern.Match(ruleText);
        if (!match.Success)
            return null;

        var content = match.Groups[1].Value;
        var suffixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Match individual category-suffix pairs: one{st}
        var categoryMatches = CategorySuffixPattern.Matches(content);
        foreach (Match categoryMatch in categoryMatches)
        {
            var category = categoryMatch.Groups[1].Value.ToLowerInvariant();
            var suffix = categoryMatch.Groups[2].Value;
            suffixes[category] = suffix;
        }

        return suffixes.Count > 0 ? suffixes : null;
    }
}
