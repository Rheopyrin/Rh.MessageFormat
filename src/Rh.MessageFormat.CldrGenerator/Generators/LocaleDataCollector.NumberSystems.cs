namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// Number systems data collection.
/// </summary>
public partial class LocaleDataCollector
{
    private Dictionary<string, string>? _numberSystemDigits;

    /// <summary>
    /// Gets the collected number system digits mapping (system id -> 10 digit characters).
    /// </summary>
    public IReadOnlyDictionary<string, string> NumberSystemDigits =>
        _numberSystemDigits ?? new Dictionary<string, string>();

    /// <summary>
    /// Loads numbering system digit data from CLDR supplemental data.
    /// </summary>
    private async Task LoadNumberSystemDigitsAsync(CancellationToken ct)
    {
        _numberSystemDigits = new Dictionary<string, string>(StringComparer.Ordinal);

        var numberSystemsPath = Path.Combine(_cldrExtractedDir, _config.Paths.NumberSystemsJson);

        using var doc = await ReadJsonFileOptionalAsync(numberSystemsPath, ct);
        if (doc == null) return;

        // Structure: supplemental.numberingSystems.{system}._digits or supplemental.numberingSystems.{system}.digits
        if (doc.RootElement.TryGetProperty("supplemental", out var supplemental) &&
            supplemental.TryGetProperty("numberingSystems", out var numberingSystems))
        {
            foreach (var systemProperty in numberingSystems.EnumerateObject())
            {
                var systemId = systemProperty.Name;
                var systemData = systemProperty.Value;

                // Get the digits string - try both _digits and digits
                string? digits = null;
                if (systemData.TryGetProperty("_digits", out var digitsElement))
                {
                    digits = digitsElement.GetString();
                }
                else if (systemData.TryGetProperty("digits", out digitsElement))
                {
                    digits = digitsElement.GetString();
                }

                // Only include numeric systems (those with 10 digits)
                // Skip algorithmic systems like "roman" which don't have digit strings
                if (!string.IsNullOrEmpty(digits))
                {
                    // Verify it has exactly 10 characters (digits 0-9)
                    // Note: Some systems use surrogate pairs, so we check grapheme count
                    var graphemeCount = CountGraphemes(digits);
                    if (graphemeCount == 10)
                    {
                        _numberSystemDigits[systemId] = digits;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Counts the number of grapheme clusters (user-perceived characters) in a string.
    /// </summary>
    private static int CountGraphemes(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        var enumerator = System.Globalization.StringInfo.GetTextElementEnumerator(text);
        var count = 0;
        while (enumerator.MoveNext())
        {
            count++;
        }
        return count;
    }

    /// <summary>
    /// Collects default numbering system for each locale from numbers.json files.
    /// </summary>
    private async Task CollectNumberSystemsAsync(Dictionary<string, LocaleData> locales, CancellationToken ct)
    {
        // Load supplemental digit data first
        await LoadNumberSystemDigitsAsync(ct);

        var numbersFolder = Path.Combine(_cldrExtractedDir, _config.Paths.NumbersFolder);
        if (!Directory.Exists(numbersFolder))
            return;

        foreach (var localeDir in GetSupportedLocaleDirectories(numbersFolder))
        {
            var localeName = Path.GetFileName(localeDir);
            var normalizedLocale = LocaleFilter.Normalize(localeName);
            var numbersPath = Path.Combine(localeDir, _config.Paths.NumbersFile);

            using var doc = await ReadJsonFileOptionalAsync(numbersPath, ct);
            if (doc == null) continue;

            var localeData = GetOrCreateLocaleData(locales, normalizedLocale);

            // Path: main.{locale}.numbers.defaultNumberingSystem
            if (doc.RootElement.TryGetProperty("main", out var main))
            {
                foreach (var localeProperty in main.EnumerateObject())
                {
                    if (localeProperty.Value.TryGetProperty("numbers", out var numbers) &&
                        numbers.TryGetProperty("defaultNumberingSystem", out var defaultNs))
                    {
                        var numberingSystem = defaultNs.GetString();
                        // Only store if it's not "latn" (the default) to save space
                        if (!string.IsNullOrEmpty(numberingSystem) && numberingSystem != "latn")
                        {
                            localeData.DefaultNumberingSystem = numberingSystem;
                        }
                    }
                }
            }
        }
    }
}
