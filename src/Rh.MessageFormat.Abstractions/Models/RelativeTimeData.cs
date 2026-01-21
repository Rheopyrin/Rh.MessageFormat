using System.Collections.Generic;

namespace Rh.MessageFormat.Abstractions.Models;

/// <summary>
/// Contains localized relative time display data for a specific field and width.
/// Used for formatting relative time expressions like "yesterday", "in 3 days", "2 hours ago".
/// </summary>
public readonly struct RelativeTimeData
{
    /// <summary>
    /// The field identifier (e.g., "year", "month", "day", "hour").
    /// </summary>
    public readonly string Field;

    /// <summary>
    /// The display name of the field (e.g., "day", "yr.").
    /// </summary>
    public readonly string? DisplayName;

    /// <summary>
    /// Relative type strings for specific offsets.
    /// Key format: offset as string (e.g., "-1", "0", "1").
    /// Value: the relative type string (e.g., "yesterday", "today", "tomorrow").
    /// </summary>
    public readonly IReadOnlyDictionary<string, string>? RelativeTypes;

    /// <summary>
    /// Future patterns by plural category.
    /// Key: plural category ("zero", "one", "two", "few", "many", "other").
    /// Value: pattern with {0} placeholder (e.g., "in {0} day", "in {0} days").
    /// </summary>
    public readonly IReadOnlyDictionary<string, string>? FuturePatterns;

    /// <summary>
    /// Past patterns by plural category.
    /// Key: plural category ("zero", "one", "two", "few", "many", "other").
    /// Value: pattern with {0} placeholder (e.g., "{0} day ago", "{0} days ago").
    /// </summary>
    public readonly IReadOnlyDictionary<string, string>? PastPatterns;

    /// <summary>
    /// Creates a new RelativeTimeData instance.
    /// </summary>
    public RelativeTimeData(
        string field,
        string? displayName = null,
        IReadOnlyDictionary<string, string>? relativeTypes = null,
        IReadOnlyDictionary<string, string>? futurePatterns = null,
        IReadOnlyDictionary<string, string>? pastPatterns = null)
    {
        Field = field;
        DisplayName = displayName;
        RelativeTypes = relativeTypes;
        FuturePatterns = futurePatterns;
        PastPatterns = pastPatterns;
    }

    /// <summary>
    /// Cached string representations of common offsets to avoid allocations.
    /// </summary>
    private static readonly string[] CachedOffsets = { "-2", "-1", "0", "1", "2" };

    /// <summary>
    /// Tries to get a relative type string for a specific offset (e.g., -1 for "yesterday").
    /// Uses cached strings for common offsets (-2 to 2) to avoid allocations.
    /// </summary>
    /// <param name="offset">The offset value (-1, 0, 1, etc.).</param>
    /// <param name="value">The relative type string if found.</param>
    /// <returns>True if found, false otherwise.</returns>
    public bool TryGetRelativeType(int offset, out string value)
    {
        if (RelativeTypes == null)
        {
            value = string.Empty;
            return false;
        }

        // Use cached string for common offsets to avoid int.ToString() allocation
        var key = offset >= -2 && offset <= 2
            ? CachedOffsets[offset + 2]
            : offset.ToString();

        if (RelativeTypes.TryGetValue(key, out var result))
        {
            value = result;
            return true;
        }

        value = string.Empty;
        return false;
    }

    /// <summary>
    /// Tries to get a future pattern for the specified plural category.
    /// </summary>
    /// <param name="pluralCategory">The plural category ("one", "other", "few", "many", etc.).</param>
    /// <param name="pattern">The pattern if found.</param>
    /// <returns>True if found, false otherwise.</returns>
    public bool TryGetFuturePattern(string pluralCategory, out string pattern)
    {
        if (FuturePatterns == null)
        {
            pattern = string.Empty;
            return false;
        }

        if (FuturePatterns.TryGetValue(pluralCategory, out var result))
        {
            pattern = result;
            return true;
        }

        // Fallback to "other" if specific category not found
        if (pluralCategory != "other" && FuturePatterns.TryGetValue("other", out result))
        {
            pattern = result;
            return true;
        }

        pattern = string.Empty;
        return false;
    }

    /// <summary>
    /// Tries to get a past pattern for the specified plural category.
    /// </summary>
    /// <param name="pluralCategory">The plural category ("one", "other", "few", "many", etc.).</param>
    /// <param name="pattern">The pattern if found.</param>
    /// <returns>True if found, false otherwise.</returns>
    public bool TryGetPastPattern(string pluralCategory, out string pattern)
    {
        if (PastPatterns == null)
        {
            pattern = string.Empty;
            return false;
        }

        if (PastPatterns.TryGetValue(pluralCategory, out var result))
        {
            pattern = result;
            return true;
        }

        // Fallback to "other" if specific category not found
        if (pluralCategory != "other" && PastPatterns.TryGetValue("other", out result))
        {
            pattern = result;
            return true;
        }

        pattern = string.Empty;
        return false;
    }
}
