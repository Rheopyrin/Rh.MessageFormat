namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// Currency data collection.
/// </summary>
public partial class LocaleDataCollector
{
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

            var localeData = GetOrCreateLocaleData(locales, normalizedLocale);

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
                                DisplayNameFew = GetStringProperty(value, "displayName-count-few"),
                                DisplayNameMany = GetStringProperty(value, "displayName-count-many"),
                                DisplayNameOther = GetStringProperty(value, "displayName-count-other")
                            };
                        }
                    }
                }
            }
        }
    }
}