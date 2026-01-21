using System.Globalization;

namespace Rh.MessageFormat.CldrGenerator;

/// <summary>
/// Filters CLDR locales to only those supported by .NET's CultureInfo.
/// Supports optional user-specified locale filtering.
/// </summary>
public static class LocaleFilter
{
    private static readonly HashSet<string> AllSupportedLocales;
    private static HashSet<string>? _userFilteredLocales;
    private static bool _initialized;

    static LocaleFilter()
    {
        // Get all cultures supported by .NET runtime
        var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
        AllSupportedLocales = new HashSet<string>(
            cultures.Select(c => c.Name),
            StringComparer.OrdinalIgnoreCase
        );
    }

    /// <summary>
    /// Initializes the locale filter with user-specified locales.
    /// Each locale is normalized and validated against .NET CultureInfo.
    /// </summary>
    /// <param name="localesInput">Comma-separated list of locales (e.g., "en-US, es-MX, de-DE")</param>
    /// <returns>List of validation errors, empty if all locales are valid</returns>
    public static IReadOnlyList<string> Initialize(string? localesInput)
    {
        if (string.IsNullOrWhiteSpace(localesInput))
        {
            // No filter specified - use all supported locales
            _userFilteredLocales = null;
            _initialized = true;
            return Array.Empty<string>();
        }

        var errors = new List<string>();
        var validLocales = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var rawLocales = localesInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var rawLocale in rawLocales)
        {
            var result = NormalizeAndValidate(rawLocale);
            if (result.IsValid)
            {
                validLocales.Add(result.NormalizedLocale!);
            }
            else
            {
                errors.Add(result.Error!);
            }
        }

        if (validLocales.Count > 0)
        {
            _userFilteredLocales = validLocales;
        }

        _initialized = true;
        return errors;
    }

    /// <summary>
    /// Checks if a CLDR locale should be included based on the filter.
    /// Matches exact locales and base languages when regional variants are in the filter.
    /// </summary>
    public static bool IsSupported(string cldrLocale)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("LocaleFilter has not been initialized. Call Initialize() first.");
        }

        // CLDR uses underscores in some places, .NET uses hyphens (e.g., "en_US" vs "en-US")
        var normalizedLocale = Normalize(cldrLocale);

        // First check if it's a valid .NET locale
        if (!AllSupportedLocales.Contains(normalizedLocale))
        {
            return false;
        }

        // No user filter - include all .NET-supported locales
        if (_userFilteredLocales == null)
        {
            return true;
        }

        // Check exact match
        if (_userFilteredLocales.Contains(normalizedLocale))
        {
            return true;
        }

        // Check if this is a base language (e.g., "en") and any regional variant is in the filter (e.g., "en-US")
        // This handles CLDR files that use base language codes for data like plurals/ordinals
        if (!normalizedLocale.Contains('-'))
        {
            return _userFilteredLocales.Any(l => l.StartsWith(normalizedLocale + "-", StringComparison.OrdinalIgnoreCase));
        }

        return false;
    }

    /// <summary>
    /// Normalizes a CLDR locale to .NET format.
    /// </summary>
    public static string Normalize(string cldrLocale)
    {
        return cldrLocale.Replace('_', '-');
    }

    /// <summary>
    /// Gets the base language from a locale (e.g., "en" from "en-US").
    /// </summary>
    public static string? GetBaseLanguage(string locale)
    {
        var dashIndex = locale.IndexOf('-');
        return dashIndex > 0 ? locale.Substring(0, dashIndex) : null;
    }

    /// <summary>
    /// Gets the count of locales that will be included.
    /// </summary>
    public static int Count => _userFilteredLocales?.Count ?? AllSupportedLocales.Count;

    /// <summary>
    /// Gets whether a user filter is active.
    /// </summary>
    public static bool HasUserFilter => _userFilteredLocales != null;

    /// <summary>
    /// Gets the user-filtered locales, or null if no filter is active.
    /// </summary>
    public static IReadOnlySet<string>? GetUserFilteredLocales() => _userFilteredLocales;

    /// <summary>
    /// Gets all .NET-supported locales.
    /// </summary>
    public static IReadOnlySet<string> GetAllSupportedLocales() => AllSupportedLocales;

    /// <summary>
    /// Gets the explicitly requested regional variants (locales with a region subtag like "en-US").
    /// Returns empty if no user filter is active.
    /// </summary>
    public static IEnumerable<string> GetRequestedRegionalVariants()
    {
        if (_userFilteredLocales == null)
            return Enumerable.Empty<string>();

        return _userFilteredLocales.Where(l => l.Contains('-'));
    }

    /// <summary>
    /// Gets the base language for each requested regional variant.
    /// Returns a dictionary mapping regional variant to base language (e.g., "en-US" -> "en").
    /// </summary>
    public static Dictionary<string, string> GetRegionalVariantBases()
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var locale in GetRequestedRegionalVariants())
        {
            var baseLang = GetBaseLanguage(locale);
            if (baseLang != null)
            {
                result[locale] = baseLang;
            }
        }

        return result;
    }

    /// <summary>
    /// Normalizes and validates a single locale string.
    /// </summary>
    private static (bool IsValid, string? NormalizedLocale, string? Error) NormalizeAndValidate(string locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            return (false, null, "Empty locale specified");
        }

        // Normalize: replace underscores with hyphens
        var normalized = locale.Trim().Replace('_', '-');

        // Validate against .NET CultureInfo
        if (!IsValidNetLocale(normalized))
        {
            // Try to find a suggestion
            var suggestion = FindSimilarLocale(normalized);
            var errorMsg = $"Invalid locale '{locale}': not a valid .NET CultureInfo";
            if (suggestion != null)
            {
                errorMsg += $". Did you mean '{suggestion}'?";
            }
            return (false, null, errorMsg);
        }

        // Get the properly-cased locale name from CultureInfo
        try
        {
            var culture = CultureInfo.GetCultureInfo(normalized);
            return (true, culture.Name, null);
        }
        catch
        {
            return (false, null, $"Invalid locale '{locale}': failed to create CultureInfo");
        }
    }

    /// <summary>
    /// Checks if a locale is a valid .NET CultureInfo.
    /// Uses the pre-loaded set of known cultures to ensure strict validation.
    /// </summary>
    private static bool IsValidNetLocale(string locale)
    {
        // Use the pre-loaded set of known cultures for strict validation
        // This prevents .NET from accepting arbitrary locale strings
        return AllSupportedLocales.Contains(locale);
    }

    /// <summary>
    /// Tries to find a similar valid locale for error suggestions.
    /// </summary>
    private static string? FindSimilarLocale(string invalidLocale)
    {
        var lower = invalidLocale.ToLowerInvariant();

        // Try exact match with different casing
        var exactMatch = AllSupportedLocales.FirstOrDefault(l =>
            l.Equals(invalidLocale, StringComparison.OrdinalIgnoreCase));
        if (exactMatch != null)
        {
            return exactMatch;
        }

        // Try to find locales starting with the same prefix
        var prefix = lower.Split('-')[0];
        var similar = AllSupportedLocales
            .Where(l => l.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Take(3)
            .ToList();

        return similar.FirstOrDefault();
    }
}
