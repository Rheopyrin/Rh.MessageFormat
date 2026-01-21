using System.Collections.Generic;

namespace Rh.MessageFormat.Abstractions.Models;

/// <summary>
/// Contains interval (date range) format patterns for a locale.
/// Used for formatting date/time ranges like "Jan 5 – 15, 2024".
/// </summary>
public readonly struct IntervalFormatData
{
    /// <summary>
    /// Fallback pattern when no specific skeleton match is found.
    /// Typically "{0} – {1}" where {0} is start and {1} is end.
    /// </summary>
    public readonly string FallbackPattern;

    /// <summary>
    /// Interval patterns indexed by skeleton.
    /// Each skeleton maps to patterns for different greatest-difference fields.
    /// </summary>
    public readonly IReadOnlyDictionary<string, IntervalPatterns>? Skeletons;

    /// <summary>
    /// Creates a new IntervalFormatData instance.
    /// </summary>
    /// <param name="fallbackPattern">The fallback pattern when no match is found.</param>
    /// <param name="skeletons">The skeleton-to-patterns mapping.</param>
    public IntervalFormatData(string fallbackPattern, IReadOnlyDictionary<string, IntervalPatterns>? skeletons = null)
    {
        FallbackPattern = fallbackPattern;
        Skeletons = skeletons;
    }

    /// <summary>
    /// Indicates whether this data has been explicitly set.
    /// </summary>
    public bool HasData => !string.IsNullOrEmpty(FallbackPattern);

    /// <summary>
    /// Default interval format data with a simple fallback pattern.
    /// </summary>
    public static IntervalFormatData Default => new("{0} – {1}");
}

/// <summary>
/// Contains interval patterns for different greatest-difference fields.
/// </summary>
public readonly struct IntervalPatterns
{
    /// <summary>
    /// Patterns indexed by the greatest difference field character.
    /// Keys: 'y' (year), 'M' (month), 'd' (day), 'H' (hour), 'm' (minute)
    /// Value: The interval pattern with placeholders for start/end dates
    /// </summary>
    public readonly IReadOnlyDictionary<char, string>? ByGreatestDiff;

    /// <summary>
    /// Creates a new IntervalPatterns instance.
    /// </summary>
    /// <param name="byGreatestDiff">Patterns by greatest difference field.</param>
    public IntervalPatterns(IReadOnlyDictionary<char, string>? byGreatestDiff)
    {
        ByGreatestDiff = byGreatestDiff;
    }

    /// <summary>
    /// Tries to get a pattern for the specified greatest difference field.
    /// </summary>
    /// <param name="greatestDiff">The greatest difference field character.</param>
    /// <param name="pattern">The pattern if found.</param>
    /// <returns>True if a pattern was found.</returns>
    public bool TryGetPattern(char greatestDiff, out string pattern)
    {
        if (ByGreatestDiff != null && ByGreatestDiff.TryGetValue(greatestDiff, out var p))
        {
            pattern = p;
            return true;
        }
        pattern = string.Empty;
        return false;
    }
}
