using System.Text.Json;

namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// Unit data collection.
/// </summary>
public partial class LocaleDataCollector
{
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

            var localeData = GetOrCreateLocaleData(locales, normalizedLocale);

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
}
