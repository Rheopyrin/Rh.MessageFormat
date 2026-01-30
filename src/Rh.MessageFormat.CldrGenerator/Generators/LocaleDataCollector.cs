using System.Text.Json;

namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// Collects all CLDR data for each locale from the extracted CLDR files.
/// Split into partial classes by feature for maintainability.
/// </summary>
public partial class LocaleDataCollector
{
    private readonly CldrConfig _config;
    private readonly string _cldrExtractedDir;

    public LocaleDataCollector(CldrConfig config, string cldrExtractedDir)
    {
        _config = config;
        _cldrExtractedDir = cldrExtractedDir;
    }

    /// <summary>
    /// Collects all locale data from CLDR files.
    /// </summary>
    public async Task<Dictionary<string, LocaleData>> CollectAllAsync(CancellationToken ct = default)
    {
        var result = new Dictionary<string, LocaleData>(StringComparer.OrdinalIgnoreCase);

        // Collect plural and ordinal rules (supplemental data - applies to all locales)
        var pluralRules = await CollectPluralRulesAsync(ct);
        var ordinalRules = await CollectOrdinalRulesAsync(ct);

        // Initialize locale data with rules
        foreach (var (locale, rules) in pluralRules)
        {
            if (!result.TryGetValue(locale, out var data))
            {
                data = new LocaleData { Locale = locale };
                result[locale] = data;
            }
            data.PluralRules = rules;
        }

        foreach (var (locale, rules) in ordinalRules)
        {
            if (!result.TryGetValue(locale, out var data))
            {
                data = new LocaleData { Locale = locale };
                result[locale] = data;
            }
            data.OrdinalRules = rules;
        }

        // Collect per-locale data
        await CollectCurrenciesAsync(result, ct);
        // Note: Units, ListPatterns, RelativeTime, and IntervalFormats have been moved to optional packages:
        // - Rh.MessageFormat.CldrData.Units
        // - Rh.MessageFormat.CldrData.Lists
        // - Rh.MessageFormat.CldrData.RelativeTime
        // - Rh.MessageFormat.CldrData.DateRange
        await CollectDatePatternsAsync(result, ct);
        await CollectQuarterDataAsync(result, ct);

        // Collect supplemental data (applies to regions, not locales)
        await CollectWeekDataAsync(result, ct);

        // Collect ordinal suffixes from RBNF data
        await CollectOrdinalSuffixesAsync(result, ct);

        // Collect number system data
        await CollectNumberSystemsAsync(result, ct);

        // Create regional variant entries for explicitly requested locales
        CreateRegionalVariantEntries(result);

        return result;
    }

    #region Helper Methods

    private IEnumerable<string> GetSupportedLocaleDirectories(string basePath)
    {
        if (!Directory.Exists(basePath))
            yield break;

        foreach (var dir in Directory.GetDirectories(basePath))
        {
            var localeName = Path.GetFileName(dir);
            if (LocaleFilter.IsSupported(localeName))
                yield return dir;
        }
    }

    private static string? GetStringProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;
    }

    private static async Task<JsonDocument> ReadJsonFileAsync(string path, CancellationToken ct)
    {
        await using var stream = File.OpenRead(path);
        return await JsonDocument.ParseAsync(stream, cancellationToken: ct);
    }

    private static async Task<JsonDocument?> ReadJsonFileOptionalAsync(string path, CancellationToken ct)
    {
        if (!File.Exists(path))
            return null;

        await using var stream = File.OpenRead(path);
        return await JsonDocument.ParseAsync(stream, cancellationToken: ct);
    }

    /// <summary>
    /// Gets or creates locale data for a given locale.
    /// </summary>
    private static LocaleData GetOrCreateLocaleData(Dictionary<string, LocaleData> locales, string normalizedLocale)
    {
        if (!locales.TryGetValue(normalizedLocale, out var localeData))
        {
            localeData = new LocaleData { Locale = normalizedLocale };
            locales[normalizedLocale] = localeData;
        }
        return localeData;
    }

    #endregion
}
