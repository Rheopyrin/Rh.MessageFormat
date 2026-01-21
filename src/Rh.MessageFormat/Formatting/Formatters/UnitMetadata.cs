using System;
using System.Collections.Generic;
using System.Linq;
using Rh.MessageFormat.Abstractions;
using Rh.MessageFormat.Abstractions.Models;
using Rh.MessageFormat.Ast;

namespace Rh.MessageFormat.Formatting.Formatters;

/// <summary>
/// Provides unit display name data for various locales using CldrData.
/// </summary>
internal static class UnitMetadata
{
    /// <summary>
    /// CLDR unit category prefixes for identifying full unit IDs.
    /// </summary>
    private static readonly string[] CldrUnitPrefixes =
    {
        "length-", "temperature-", "mass-", "volume-", "duration-",
        "digital-", "speed-", "area-", "pressure-", "energy-",
        "acceleration-", "angle-", "concentr-", "consumption-",
        "electric-", "frequency-", "force-", "graphics-", "light-",
        "power-", "torque-"
    };

    /// <summary>
    /// Maps short unit names (used in skeletons) to CLDR unit IDs.
    /// E.g., "kilometer" -> "length-kilometer", "celsius" -> "temperature-celsius"
    /// </summary>
    private static readonly Dictionary<string, string> UnitIdMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Length
        { "kilometer", "length-kilometer" },
        { "meter", "length-meter" },
        { "centimeter", "length-centimeter" },
        { "millimeter", "length-millimeter" },
        { "micrometer", "length-micrometer" },
        { "nanometer", "length-nanometer" },
        { "mile", "length-mile" },
        { "yard", "length-yard" },
        { "foot", "length-foot" },
        { "inch", "length-inch" },

        // Temperature
        { "celsius", "temperature-celsius" },
        { "fahrenheit", "temperature-fahrenheit" },
        { "kelvin", "temperature-kelvin" },

        // Mass/Weight
        { "kilogram", "mass-kilogram" },
        { "gram", "mass-gram" },
        { "milligram", "mass-milligram" },
        { "pound", "mass-pound" },
        { "ounce", "mass-ounce" },
        { "ton", "mass-ton" },
        { "metric-ton", "mass-metric-ton" },

        // Volume
        { "liter", "volume-liter" },
        { "milliliter", "volume-milliliter" },
        { "gallon", "volume-gallon" },
        { "quart", "volume-quart" },
        { "pint", "volume-pint" },
        { "cup", "volume-cup" },
        { "fluid-ounce", "volume-fluid-ounce" },

        // Time
        { "hour", "duration-hour" },
        { "minute", "duration-minute" },
        { "second", "duration-second" },
        { "millisecond", "duration-millisecond" },
        { "day", "duration-day" },
        { "week", "duration-week" },
        { "month", "duration-month" },
        { "year", "duration-year" },

        // Digital
        { "byte", "digital-byte" },
        { "kilobyte", "digital-kilobyte" },
        { "megabyte", "digital-megabyte" },
        { "gigabyte", "digital-gigabyte" },
        { "terabyte", "digital-terabyte" },
        { "petabyte", "digital-petabyte" },
        { "bit", "digital-bit" },
        { "kilobit", "digital-kilobit" },
        { "megabit", "digital-megabit" },
        { "gigabit", "digital-gigabit" },

        // Speed
        { "kilometer-per-hour", "speed-kilometer-per-hour" },
        { "mile-per-hour", "speed-mile-per-hour" },
        { "meter-per-second", "speed-meter-per-second" },

        // Area
        { "square-kilometer", "area-square-kilometer" },
        { "square-meter", "area-square-meter" },
        { "square-centimeter", "area-square-centimeter" },
        { "square-mile", "area-square-mile" },
        { "square-yard", "area-square-yard" },
        { "square-foot", "area-square-foot" },
        { "square-inch", "area-square-inch" },
        { "hectare", "area-hectare" },
        { "acre", "area-acre" },

        // Pressure
        { "hectopascal", "pressure-hectopascal" },
        { "millibar", "pressure-millibar" },
        { "bar", "pressure-bar" },

        // Energy
        { "joule", "energy-joule" },
        { "kilojoule", "energy-kilojoule" },
        { "calorie", "energy-calorie" },
        { "kilocalorie", "energy-kilocalorie" },
    };

    /// <summary>
    /// Gets the unit string for a locale, unit ID, width, and plurality.
    /// </summary>
    public static string GetUnitString(ref FormatterContext ctx, string unitId, string width, bool isPlural)
    {
        var count = isPlural ? "other" : "one";

        // Map short unit name to full CLDR unit ID
        var cldrUnitId = MapToCldrUnitId(unitId);

        if (TryGetUnit(ref ctx, cldrUnitId, out var data))
        {
            if (data.TryGetDisplayName(width, count, out var displayName))
            {
                return displayName;
            }

            // Try "long" width as fallback
            if (width != "long" && data.TryGetDisplayName("long", count, out displayName))
            {
                return displayName;
            }
        }

        return unitId; // Fallback to original unit ID
    }

    /// <summary>
    /// Maps a short unit name to its CLDR unit ID.
    /// </summary>
    private static string MapToCldrUnitId(string shortName)
    {
        // If already a full CLDR ID (contains hyphen and category prefix), return as-is
        if (shortName.Contains("-") && CldrUnitPrefixes.Any(shortName.StartsWith))
        {
            return shortName;
        }

        // Look up in mapping table
        if (UnitIdMap.TryGetValue(shortName, out var cldrId))
        {
            return cldrId;
        }

        // Return as-is if not found (might already be a valid CLDR ID)
        return shortName;
    }

    private static bool TryGetUnit(ref FormatterContext ctx, string unitId, out UnitData data)
    {
        var provider = ctx.CldrDataProvider;
        var locale = ctx.Locale;
        var fallbackLocale = ctx.FallbackLocale;

        // Try exact locale
        if (provider.TryGetLocaleData(locale, out var localeData) && localeData != null)
        {
            if (localeData.TryGetUnit(unitId, out data))
            {
                return true;
            }
        }

        // Try base locale
        var dashIndex = locale.IndexOf('-');
        if (dashIndex < 0) dashIndex = locale.IndexOf('_');
        if (dashIndex > 0)
        {
            var baseLocale = locale.Substring(0, dashIndex);
            if (provider.TryGetLocaleData(baseLocale, out localeData) && localeData != null)
            {
                if (localeData.TryGetUnit(unitId, out data))
                {
                    return true;
                }
            }
        }

        // Try fallback locale
        if (!string.Equals(locale, fallbackLocale, StringComparison.OrdinalIgnoreCase))
        {
            if (provider.TryGetLocaleData(fallbackLocale, out localeData) && localeData != null)
            {
                if (localeData.TryGetUnit(unitId, out data))
                {
                    return true;
                }
            }
        }

        data = default;
        return false;
    }
}