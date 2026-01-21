using System.Text.Json;
using System.Text.RegularExpressions;

namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// Collects all CLDR data for each locale from the extracted CLDR files.
/// </summary>
public class LocaleDataCollector
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
        await CollectUnitsAsync(result, ct);
        await CollectDatePatternsAsync(result, ct);
        await CollectListPatternsAsync(result, ct);

        // Create regional variant entries for explicitly requested locales
        CreateRegionalVariantEntries(result);

        return result;
    }

    /// <summary>
    /// Creates locale data entries for regional variants (e.g., en-US, en-GB)
    /// and determines whether they can reuse the base locale's class.
    /// Also ensures all regional variants inherit plural/ordinal rules from their base language.
    /// </summary>
    private void CreateRegionalVariantEntries(Dictionary<string, LocaleData> locales)
    {
        // First, handle explicitly requested regional variants (when user filter is active)
        var explicitRegionalVariants = LocaleFilter.GetRegionalVariantBases();
        foreach (var (regionalLocale, baseLocale) in explicitRegionalVariants)
        {
            if (!locales.ContainsKey(regionalLocale) && locales.TryGetValue(baseLocale, out _))
            {
                // No CLDR folder for this regional variant - use base class
                locales[regionalLocale] = new LocaleData
                {
                    Locale = regionalLocale,
                    UseClassFrom = baseLocale
                };
                Console.WriteLine($"  {regionalLocale}: no CLDR data, reusing class from '{baseLocale}'");
            }
        }

        // Then, inherit plural/ordinal rules for ALL regional variants in the collection
        // CLDR stores plural rules at the base language level (e.g., "en"), not regional variants (e.g., "en-GB")
        var regionalLocales = locales
            .Where(kvp => kvp.Key.Contains('-') && kvp.Value.UseClassFrom == null)
            .ToList();

        Console.WriteLine($"Inheriting plural rules for {regionalLocales.Count} regional variants...");

        foreach (var (locale, data) in regionalLocales)
        {
            var dashIndex = locale.IndexOf('-');
            var baseLocale = locale.Substring(0, dashIndex);

            if (!locales.TryGetValue(baseLocale, out var baseData))
                continue;

            // Inherit plural/ordinal rules from base if not set
            if (data.PluralRules.Count == 0 && baseData.PluralRules.Count > 0)
            {
                data.PluralRules = baseData.PluralRules;
            }
            if (data.OrdinalRules.Count == 0 && baseData.OrdinalRules.Count > 0)
            {
                data.OrdinalRules = baseData.OrdinalRules;
            }

            // Compare with base - if identical, use base class
            if (IsDataIdentical(data, baseData))
            {
                data.UseClassFrom = baseLocale;
                Console.WriteLine($"  {locale}: identical to '{baseLocale}', reusing class");
            }
        }
    }

    /// <summary>
    /// Compares two LocaleData instances to determine if they contain identical data.
    /// </summary>
    private static bool IsDataIdentical(LocaleData a, LocaleData b)
    {
        // Compare plural rules
        if (!DictionariesEqual(a.PluralRules, b.PluralRules))
            return false;

        // Compare ordinal rules
        if (!DictionariesEqual(a.OrdinalRules, b.OrdinalRules))
            return false;

        // Compare currencies (just check count for now - deep compare is expensive)
        if (a.Currencies.Count != b.Currencies.Count)
            return false;

        // Compare units (just check count)
        if (a.Units.Count != b.Units.Count)
            return false;

        // Compare date patterns
        if (!DatePatternsEqual(a.DatePatterns, b.DatePatterns))
            return false;

        // Compare list patterns (just check count)
        if (a.ListPatterns.Count != b.ListPatterns.Count)
            return false;

        return true;
    }

    private static bool DictionariesEqual(Dictionary<string, string> a, Dictionary<string, string> b)
    {
        if (a.Count != b.Count)
            return false;

        foreach (var (key, value) in a)
        {
            if (!b.TryGetValue(key, out var bValue) || value != bValue)
                return false;
        }

        return true;
    }

    private static bool DatePatternsEqual(LocaleDatePatternData? a, LocaleDatePatternData? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;

        return FormatStylesEqual(a.DateFormats, b.DateFormats) &&
               FormatStylesEqual(a.TimeFormats, b.TimeFormats) &&
               FormatStylesEqual(a.DateTimeFormats, b.DateTimeFormats);
    }

    private static bool FormatStylesEqual(LocaleDateFormatStyles? a, LocaleDateFormatStyles? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;

        return a.Full == b.Full && a.Long == b.Long && a.Medium == b.Medium && a.Short == b.Short;
    }

    private async Task<Dictionary<string, Dictionary<string, string>>> CollectPluralRulesAsync(CancellationToken ct)
    {
        var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        var pluralsPath = Path.Combine(_cldrExtractedDir, _config.Paths.PluralsJson);

        using var doc = await ReadJsonFileAsync(pluralsPath, ct);

        if (doc.RootElement.TryGetProperty("supplemental", out var supplemental) &&
            supplemental.TryGetProperty("plurals-type-cardinal", out var cardinalRules))
        {
            foreach (var localeProperty in cardinalRules.EnumerateObject())
            {
                if (!LocaleFilter.IsSupported(localeProperty.Name))
                    continue;

                var normalizedLocale = LocaleFilter.Normalize(localeProperty.Name);
                var rules = new Dictionary<string, string>();

                foreach (var ruleProperty in localeProperty.Value.EnumerateObject())
                {
                    // "pluralRule-count-one" -> "one"
                    var count = ruleProperty.Name.Replace("pluralRule-count-", "");
                    rules[count] = ruleProperty.Value.GetString() ?? "";
                }

                result[normalizedLocale] = rules;
            }
        }

        return result;
    }

    private async Task<Dictionary<string, Dictionary<string, string>>> CollectOrdinalRulesAsync(CancellationToken ct)
    {
        var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        var ordinalsPath = Path.Combine(_cldrExtractedDir, _config.Paths.OrdinalsJson);

        using var doc = await ReadJsonFileAsync(ordinalsPath, ct);

        if (doc.RootElement.TryGetProperty("supplemental", out var supplemental) &&
            supplemental.TryGetProperty("plurals-type-ordinal", out var ordinalRules))
        {
            foreach (var localeProperty in ordinalRules.EnumerateObject())
            {
                if (!LocaleFilter.IsSupported(localeProperty.Name))
                    continue;

                var normalizedLocale = LocaleFilter.Normalize(localeProperty.Name);
                var rules = new Dictionary<string, string>();

                foreach (var ruleProperty in localeProperty.Value.EnumerateObject())
                {
                    var count = ruleProperty.Name.Replace("pluralRule-count-", "");
                    rules[count] = ruleProperty.Value.GetString() ?? "";
                }

                result[normalizedLocale] = rules;
            }
        }

        return result;
    }

    private async Task CollectCurrenciesAsync(Dictionary<string, LocaleData> locales, CancellationToken ct)
    {
        var mainPath = Path.Combine(_cldrExtractedDir, _config.Paths.CurrenciesFolder);

        foreach (var localeDir in GetSupportedLocaleDirectories(mainPath))
        {
            var localeName = Path.GetFileName(localeDir);
            var normalizedLocale = LocaleFilter.Normalize(localeName);
            var currenciesPath = Path.Combine(localeDir, _config.Paths.CurrenciesFile);

            using var doc = await ReadJsonFileOptionalAsync(currenciesPath, ct);
            if (doc == null) continue;

            if (!locales.TryGetValue(normalizedLocale, out var localeData))
            {
                localeData = new LocaleData { Locale = normalizedLocale };
                locales[normalizedLocale] = localeData;
            }

            if (doc.RootElement.TryGetProperty("main", out var main))
            {
                foreach (var localeProperty in main.EnumerateObject())
                {
                    if (localeProperty.Value.TryGetProperty("numbers", out var numbers) &&
                        numbers.TryGetProperty("currencies", out var currenciesObj))
                    {
                        foreach (var currencyProperty in currenciesObj.EnumerateObject())
                        {
                            var code = currencyProperty.Name;
                            var value = currencyProperty.Value;

                            localeData.Currencies[code] = new LocaleCurrencyData
                            {
                                Code = code,
                                Symbol = GetStringProperty(value, "symbol"),
                                NarrowSymbol = GetStringProperty(value, "symbol-alt-narrow"),
                                DisplayName = GetStringProperty(value, "displayName"),
                                DisplayNameOne = GetStringProperty(value, "displayName-count-one"),
                                DisplayNameOther = GetStringProperty(value, "displayName-count-other")
                            };
                        }
                    }
                }
            }
        }
    }

    private async Task CollectUnitsAsync(Dictionary<string, LocaleData> locales, CancellationToken ct)
    {
        var mainPath = Path.Combine(_cldrExtractedDir, _config.Paths.UnitsFolder);

        foreach (var localeDir in GetSupportedLocaleDirectories(mainPath))
        {
            var localeName = Path.GetFileName(localeDir);
            var normalizedLocale = LocaleFilter.Normalize(localeName);
            var unitsPath = Path.Combine(localeDir, _config.Paths.UnitsFile);

            using var doc = await ReadJsonFileOptionalAsync(unitsPath, ct);
            if (doc == null) continue;

            if (!locales.TryGetValue(normalizedLocale, out var localeData))
            {
                localeData = new LocaleData { Locale = normalizedLocale };
                locales[normalizedLocale] = localeData;
            }

            if (doc.RootElement.TryGetProperty("main", out var main))
            {
                foreach (var localeProperty in main.EnumerateObject())
                {
                    if (localeProperty.Value.TryGetProperty("units", out var unitsObj))
                    {
                        ProcessUnitWidth(unitsObj, "long", localeData);
                        ProcessUnitWidth(unitsObj, "short", localeData);
                        ProcessUnitWidth(unitsObj, "narrow", localeData);
                    }
                }
            }
        }
    }

    private void ProcessUnitWidth(JsonElement unitsObj, string width, LocaleData localeData)
    {
        if (!unitsObj.TryGetProperty(width, out var widthObj))
            return;

        foreach (var unitProperty in widthObj.EnumerateObject())
        {
            var unitId = unitProperty.Name;
            if (unitId.StartsWith("per") || unitId.StartsWith("times") || unitId.StartsWith("power"))
                continue;

            if (!localeData.Units.TryGetValue(unitId, out var unitData))
            {
                unitData = new LocaleUnitData { Id = unitId };
                localeData.Units[unitId] = unitData;
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

    private async Task CollectDatePatternsAsync(Dictionary<string, LocaleData> locales, CancellationToken ct)
    {
        var mainPath = Path.Combine(_cldrExtractedDir, _config.Paths.DatePatternsFolder);

        foreach (var localeDir in GetSupportedLocaleDirectories(mainPath))
        {
            var localeName = Path.GetFileName(localeDir);
            var normalizedLocale = LocaleFilter.Normalize(localeName);
            var gregorianPath = Path.Combine(localeDir, _config.Paths.DatePatternsFile);

            using var doc = await ReadJsonFileOptionalAsync(gregorianPath, ct);
            if (doc == null) continue;

            if (!locales.TryGetValue(normalizedLocale, out var localeData))
            {
                localeData = new LocaleData { Locale = normalizedLocale };
                locales[normalizedLocale] = localeData;
            }

            if (doc.RootElement.TryGetProperty("main", out var main))
            {
                foreach (var localeProperty in main.EnumerateObject())
                {
                    if (localeProperty.Value.TryGetProperty("dates", out var dates) &&
                        dates.TryGetProperty("calendars", out var calendars) &&
                        calendars.TryGetProperty("gregorian", out var gregorian))
                    {
                        localeData.DatePatterns = new LocaleDatePatternData();

                        if (gregorian.TryGetProperty("dateFormats", out var dateFormats))
                            localeData.DatePatterns.DateFormats = ExtractFormats(dateFormats, convertToNet: true);

                        if (gregorian.TryGetProperty("timeFormats", out var timeFormats))
                            localeData.DatePatterns.TimeFormats = ExtractFormats(timeFormats, convertToNet: true);

                        if (gregorian.TryGetProperty("dateTimeFormats", out var dateTimeFormats))
                            localeData.DatePatterns.DateTimeFormats = ExtractFormats(dateTimeFormats, convertToNet: false);
                    }
                }
            }
        }
    }

    private LocaleDateFormatStyles? ExtractFormats(JsonElement element, bool convertToNet)
    {
        var formats = new LocaleDateFormatStyles();
        var hasValue = false;

        if (element.TryGetProperty("full", out var full))
        {
            formats.Full = convertToNet ? ConvertToNetFormat(GetPatternValue(full)) : GetPatternValue(full);
            hasValue = true;
        }
        if (element.TryGetProperty("long", out var longFormat))
        {
            formats.Long = convertToNet ? ConvertToNetFormat(GetPatternValue(longFormat)) : GetPatternValue(longFormat);
            hasValue = true;
        }
        if (element.TryGetProperty("medium", out var medium))
        {
            formats.Medium = convertToNet ? ConvertToNetFormat(GetPatternValue(medium)) : GetPatternValue(medium);
            hasValue = true;
        }
        if (element.TryGetProperty("short", out var shortFormat))
        {
            formats.Short = convertToNet ? ConvertToNetFormat(GetPatternValue(shortFormat)) : GetPatternValue(shortFormat);
            hasValue = true;
        }

        return hasValue ? formats : null;
    }

    private static string? GetPatternValue(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
            return element.GetString();
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("_value", out var val))
            return val.GetString();
        return null;
    }

    private static string? ConvertToNetFormat(string? icuPattern)
    {
        if (string.IsNullOrEmpty(icuPattern))
            return icuPattern;

        var result = icuPattern;
        result = Regex.Replace(result, @"y{5,}", "yyyy");
        result = Regex.Replace(result, @"y{4}", "yyyy");
        result = Regex.Replace(result, @"y{3}", "yyyy");
        result = Regex.Replace(result, @"y{2}", "yy");
        result = Regex.Replace(result, @"y{1}(?!y)", "yyyy");
        result = Regex.Replace(result, @"E{4,}", "dddd");
        result = Regex.Replace(result, @"E{1,3}", "ddd");
        result = Regex.Replace(result, @"c{4,}", "dddd");
        result = Regex.Replace(result, @"c{1,3}", "ddd");
        result = Regex.Replace(result, @"a+", "tt");
        result = Regex.Replace(result, @"G{4,}", "gg");
        result = Regex.Replace(result, @"G{1,3}", "g");
        result = Regex.Replace(result, @"L{4,}", "MMMM");
        result = Regex.Replace(result, @"L{3}", "MMM");
        result = Regex.Replace(result, @"L{2}", "MM");
        result = Regex.Replace(result, @"L(?!L)", "M");

        return result;
    }

    private async Task CollectListPatternsAsync(Dictionary<string, LocaleData> locales, CancellationToken ct)
    {
        var mainPath = Path.Combine(_cldrExtractedDir, _config.Paths.ListPatternsFolder);

        foreach (var localeDir in GetSupportedLocaleDirectories(mainPath))
        {
            var localeName = Path.GetFileName(localeDir);
            var normalizedLocale = LocaleFilter.Normalize(localeName);
            var listPatternsPath = Path.Combine(localeDir, _config.Paths.ListPatternsFile);

            using var doc = await ReadJsonFileOptionalAsync(listPatternsPath, ct);
            if (doc == null) continue;

            if (!locales.TryGetValue(normalizedLocale, out var localeData))
            {
                localeData = new LocaleData { Locale = normalizedLocale };
                locales[normalizedLocale] = localeData;
            }

            if (doc.RootElement.TryGetProperty("main", out var main))
            {
                foreach (var localeProperty in main.EnumerateObject())
                {
                    if (localeProperty.Value.TryGetProperty("listPatterns", out var listPatternsObj))
                    {
                        foreach (var patternType in listPatternsObj.EnumerateObject())
                        {
                            var shortName = patternType.Name.Replace("listPattern-type-", "");
                            var typeElement = patternType.Value;

                            localeData.ListPatterns[shortName] = new LocaleListPatternData
                            {
                                Type = shortName,
                                Start = GetStringProperty(typeElement, "start"),
                                Middle = GetStringProperty(typeElement, "middle"),
                                End = GetStringProperty(typeElement, "end"),
                                Two = GetStringProperty(typeElement, "2")
                            };
                        }
                    }
                }
            }
        }
    }

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
}
