using System.Text.Json;

namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// Interval format data collection.
/// </summary>
public partial class LocaleDataCollector
{
    private async Task CollectIntervalFormatsAsync(Dictionary<string, LocaleData> locales, CancellationToken ct)
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
                        gregorian.TryGetProperty("dateTimeFormats", out var dtFormats) &&
                        dtFormats.TryGetProperty("intervalFormats", out var intervalFormats))
                    {
                        localeData.IntervalFormats = ExtractIntervalFormats(intervalFormats);
                    }
                }
            }
        }
    }

    private static LocaleIntervalFormatData ExtractIntervalFormats(JsonElement element)
    {
        var data = new LocaleIntervalFormatData();

        // Get fallback pattern
        if (element.TryGetProperty("intervalFormatFallback", out var fallback))
        {
            data.FallbackPattern = fallback.GetString() ?? "{0} – {1}";
        }

        // Get skeleton patterns
        foreach (var skeletonProp in element.EnumerateObject())
        {
            if (skeletonProp.Name == "intervalFormatFallback")
                continue;

            var skeleton = skeletonProp.Name;
            var patterns = new Dictionary<char, string>();

            foreach (var diffProp in skeletonProp.Value.EnumerateObject())
            {
                // Key is the greatest difference field (y, M, d, H, m, etc.)
                if (diffProp.Name.Length == 1)
                {
                    var diffChar = diffProp.Name[0];
                    var pattern = diffProp.Value.GetString();
                    if (!string.IsNullOrEmpty(pattern))
                    {
                        patterns[diffChar] = pattern;
                    }
                }
            }

            if (patterns.Count > 0)
            {
                data.Skeletons[skeleton] = patterns;
            }
        }

        return data;
    }
}
