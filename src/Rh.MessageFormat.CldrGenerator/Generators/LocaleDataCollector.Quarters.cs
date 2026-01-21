using System.Text.Json;

namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// Quarter data collection.
/// </summary>
public partial class LocaleDataCollector
{
    private async Task CollectQuarterDataAsync(Dictionary<string, LocaleData> locales, CancellationToken ct)
    {
        var mainPath = Path.Combine(_cldrExtractedDir, _config.Paths.DatePatternsFolder);

        foreach (var localeDir in GetSupportedLocaleDirectories(mainPath))
        {
            var localeName = Path.GetFileName(localeDir);
            var normalizedLocale = LocaleFilter.Normalize(localeName);
            var gregorianPath = Path.Combine(localeDir, _config.Paths.DatePatternsFile);

            using var doc = await ReadJsonFileOptionalAsync(gregorianPath, ct);
            if (doc == null) continue;

            var localeData = GetOrCreateLocaleData(locales, normalizedLocale);

            if (doc.RootElement.TryGetProperty("main", out var main))
            {
                foreach (var localeProperty in main.EnumerateObject())
                {
                    if (localeProperty.Value.TryGetProperty("dates", out var dates) &&
                        dates.TryGetProperty("calendars", out var calendars) &&
                        calendars.TryGetProperty("gregorian", out var gregorian) &&
                        gregorian.TryGetProperty("quarters", out var quarters))
                    {
                        localeData.Quarters = new LocaleQuarterData();

                        if (quarters.TryGetProperty("format", out var format))
                            localeData.Quarters.Format = ExtractQuarterFormats(format);

                        if (quarters.TryGetProperty("stand-alone", out var standAlone))
                            localeData.Quarters.StandAlone = ExtractQuarterFormats(standAlone);
                    }
                }
            }
        }
    }

    private static LocaleQuarterFormats? ExtractQuarterFormats(JsonElement element)
    {
        var formats = new LocaleQuarterFormats();
        var hasValue = false;

        if (element.TryGetProperty("abbreviated", out var abbreviated))
        {
            formats.Abbreviated = ExtractQuarterArray(abbreviated);
            hasValue = true;
        }
        if (element.TryGetProperty("wide", out var wide))
        {
            formats.Wide = ExtractQuarterArray(wide);
            hasValue = true;
        }
        if (element.TryGetProperty("narrow", out var narrow))
        {
            formats.Narrow = ExtractQuarterArray(narrow);
            hasValue = true;
        }

        return hasValue ? formats : null;
    }

    private static string[] ExtractQuarterArray(JsonElement element)
    {
        var result = new string[4];
        for (int i = 1; i <= 4; i++)
        {
            if (element.TryGetProperty(i.ToString(), out var q))
                result[i - 1] = q.GetString() ?? $"Q{i}";
            else
                result[i - 1] = $"Q{i}";
        }
        return result;
    }
}
