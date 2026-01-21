using System.Text.Json;
using System.Text.RegularExpressions;

namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// Date and time pattern collection.
/// </summary>
public partial class LocaleDataCollector
{
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

            var localeData = GetOrCreateLocaleData(locales, normalizedLocale);

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
}
