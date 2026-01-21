using System.Text.Json;

namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// Relative time data collection.
/// </summary>
public partial class LocaleDataCollector
{
    /// <summary>
    /// Fields that support relative time formatting.
    /// </summary>
    private static readonly string[] RelativeTimeFields =
    {
        "year", "quarter", "month", "week", "day", "hour", "minute", "second",
        "sun", "mon", "tue", "wed", "thu", "fri", "sat"
    };

    /// <summary>
    /// Widths supported for relative time formatting.
    /// </summary>
    private static readonly string[] RelativeTimeWidths = { "long", "short", "narrow" };

    private async Task CollectRelativeTimeDataAsync(Dictionary<string, LocaleData> locales, CancellationToken ct)
    {
        // Uses the same folder as date patterns
        var mainPath = Path.Combine(_cldrExtractedDir, _config.Paths.DatePatternsFolder);

        foreach (var localeDir in GetSupportedLocaleDirectories(mainPath))
        {
            var localeName = Path.GetFileName(localeDir);
            var normalizedLocale = LocaleFilter.Normalize(localeName);
            var dateFieldsPath = Path.Combine(localeDir, _config.Paths.DateFieldsFile);

            using var doc = await ReadJsonFileOptionalAsync(dateFieldsPath, ct);
            if (doc == null) continue;

            var localeData = GetOrCreateLocaleData(locales, normalizedLocale);

            if (doc.RootElement.TryGetProperty("main", out var main))
            {
                foreach (var localeProperty in main.EnumerateObject())
                {
                    if (localeProperty.Value.TryGetProperty("dates", out var dates) &&
                        dates.TryGetProperty("fields", out var fields))
                    {
                        ProcessRelativeTimeFields(fields, localeData);
                    }
                }
            }
        }
    }

    private void ProcessRelativeTimeFields(JsonElement fields, LocaleData localeData)
    {
        foreach (var baseField in RelativeTimeFields)
        {
            foreach (var width in RelativeTimeWidths)
            {
                // Field names: "year" (long), "year-short", "year-narrow"
                var fieldName = width == "long" ? baseField : $"{baseField}-{width}";

                if (!fields.TryGetProperty(fieldName, out var fieldElement))
                    continue;

                var key = $"{baseField}:{width}";
                var data = new LocaleRelativeTimeData
                {
                    Field = baseField,
                    Width = width,
                    DisplayName = GetStringProperty(fieldElement, "displayName")
                };

                // Collect relative types (-1, 0, 1)
                foreach (var offset in new[] { -1, 0, 1 })
                {
                    var propName = $"relative-type-{offset}";
                    var value = GetStringProperty(fieldElement, propName);
                    if (value != null)
                    {
                        data.RelativeTypes[offset.ToString()] = value;
                    }
                }

                // Collect future patterns
                if (fieldElement.TryGetProperty("relativeTime-type-future", out var futureObj))
                {
                    foreach (var count in new[] { "zero", "one", "two", "few", "many", "other" })
                    {
                        var pattern = GetStringProperty(futureObj, $"relativeTimePattern-count-{count}");
                        if (pattern != null)
                        {
                            data.FuturePatterns[count] = pattern;
                        }
                    }
                }

                // Collect past patterns
                if (fieldElement.TryGetProperty("relativeTime-type-past", out var pastObj))
                {
                    foreach (var count in new[] { "zero", "one", "two", "few", "many", "other" })
                    {
                        var pattern = GetStringProperty(pastObj, $"relativeTimePattern-count-{count}");
                        if (pattern != null)
                        {
                            data.PastPatterns[count] = pattern;
                        }
                    }
                }

                // Only add if we have some data
                if (data.RelativeTypes.Count > 0 || data.FuturePatterns.Count > 0 || data.PastPatterns.Count > 0)
                {
                    localeData.RelativeTimeData[key] = data;
                }
            }
        }
    }
}
