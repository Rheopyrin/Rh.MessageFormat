using System.Collections.Generic;

namespace Rh.MessageFormat.Abstractions.Models;

/// <summary>
/// Contains localized unit display data.
/// </summary>
public readonly struct UnitData
{
    /// <summary>
    /// The unit identifier in CLDR format (e.g., "length-meter", "duration-hour").
    /// </summary>
    public readonly string Id;

    /// <summary>
    /// Display names by width and count.
    /// Key format: "width:count" (e.g., "long:one", "long:other", "short:one", "narrow:other").
    /// </summary>
    public readonly IReadOnlyDictionary<string, string>? DisplayNames;

    /// <summary>
    /// Creates a new UnitData instance.
    /// </summary>
    public UnitData(string id, IReadOnlyDictionary<string, string>? displayNames = null)
    {
        Id = id;
        DisplayNames = displayNames;
    }

    /// <summary>
    /// Tries to get a display name for the specified width and plural count.
    /// </summary>
    /// <param name="width">The width: "long", "short", or "narrow".</param>
    /// <param name="count">The plural category: "one", "other", "few", "many", etc.</param>
    /// <param name="displayName">The display name if found.</param>
    /// <returns>True if found, false otherwise.</returns>
    public bool TryGetDisplayName(string width, string count, out string displayName)
    {
        if (DisplayNames == null)
        {
            displayName = string.Empty;
            return false;
        }

        var key = $"{width}:{count}";
        if (DisplayNames.TryGetValue(key, out var name))
        {
            displayName = name;
            return true;
        }

        // Fallback to "other" if specific count not found
        if (count != "other")
        {
            key = $"{width}:other";
            if (DisplayNames.TryGetValue(key, out name))
            {
                displayName = name;
                return true;
            }
        }

        displayName = string.Empty;
        return false;
    }
}