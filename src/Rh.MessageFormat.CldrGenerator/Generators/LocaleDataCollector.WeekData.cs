namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// Week data collection from supplemental data.
/// </summary>
public partial class LocaleDataCollector
{
    private async Task CollectWeekDataAsync(Dictionary<string, LocaleData> locales, CancellationToken ct)
    {
        var weekDataPath = Path.Combine(_cldrExtractedDir, _config.Paths.WeekDataJson);

        using var doc = await ReadJsonFileOptionalAsync(weekDataPath, ct);
        if (doc == null)
        {
            Console.WriteLine("Warning: weekData.json not found, week info will use defaults");
            return;
        }

        if (!doc.RootElement.TryGetProperty("supplemental", out var supplemental) ||
            !supplemental.TryGetProperty("weekData", out var weekData))
        {
            return;
        }

        // Parse minDays by region
        var minDaysByRegion = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (weekData.TryGetProperty("minDays", out var minDays))
        {
            foreach (var prop in minDays.EnumerateObject())
            {
                if (int.TryParse(prop.Value.GetString(), out var value))
                    minDaysByRegion[prop.Name] = value;
            }
        }

        // Parse firstDay by region
        var firstDayByRegion = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (weekData.TryGetProperty("firstDay", out var firstDay))
        {
            foreach (var prop in firstDay.EnumerateObject())
            {
                var dayValue = ParseDayOfWeek(prop.Value.GetString());
                firstDayByRegion[prop.Name] = dayValue;
            }
        }

        // Apply week data to locales based on region
        foreach (var (locale, data) in locales)
        {
            var region = GetRegionFromLocale(locale);
            if (string.IsNullOrEmpty(region))
                region = "001"; // World default

            var weekInfo = new LocaleWeekData();

            // Get first day (default to Monday from "001")
            if (firstDayByRegion.TryGetValue(region, out var fd))
                weekInfo.FirstDay = fd;
            else if (firstDayByRegion.TryGetValue("001", out fd))
                weekInfo.FirstDay = fd;
            else
                weekInfo.FirstDay = 1; // Monday

            // Get min days (default to 1 from "001")
            if (minDaysByRegion.TryGetValue(region, out var md))
                weekInfo.MinDays = md;
            else if (minDaysByRegion.TryGetValue("001", out md))
                weekInfo.MinDays = md;
            else
                weekInfo.MinDays = 1;

            data.WeekInfo = weekInfo;
        }
    }

    private static int ParseDayOfWeek(string? day)
    {
        return day?.ToLowerInvariant() switch
        {
            "sun" => 0,
            "mon" => 1,
            "tue" => 2,
            "wed" => 3,
            "thu" => 4,
            "fri" => 5,
            "sat" => 6,
            _ => 1 // Default to Monday
        };
    }

    private static string? GetRegionFromLocale(string locale)
    {
        // Extract region from locale (e.g., "en-US" -> "US", "de-DE" -> "DE")
        var dashIndex = locale.IndexOf('-');
        if (dashIndex < 0)
            dashIndex = locale.IndexOf('_');

        if (dashIndex > 0 && dashIndex < locale.Length - 1)
        {
            return locale.Substring(dashIndex + 1).ToUpperInvariant();
        }

        return null;
    }
}
