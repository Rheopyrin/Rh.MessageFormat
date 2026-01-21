namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// Regional variant handling and locale comparison.
/// </summary>
public partial class LocaleDataCollector
{
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

        // Compare relative time data (just check count)
        if (a.RelativeTimeData.Count != b.RelativeTimeData.Count)
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
}
