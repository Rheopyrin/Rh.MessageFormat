namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// List pattern collection.
/// </summary>
public partial class LocaleDataCollector
{
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

            var localeData = GetOrCreateLocaleData(locales, normalizedLocale);

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
}
