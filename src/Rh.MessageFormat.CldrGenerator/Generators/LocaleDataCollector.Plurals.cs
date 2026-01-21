namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// Plural and ordinal rule collection.
/// </summary>
public partial class LocaleDataCollector
{
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
}
