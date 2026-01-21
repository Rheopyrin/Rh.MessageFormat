using System;
using System.Runtime.CompilerServices;
using Rh.MessageFormat.Abstractions.Models;
using Rh.MessageFormat.Ast;

namespace Rh.MessageFormat.Formatting.Formatters;

/// <summary>
/// Helper class for quarter formatting using CLDR data.
/// </summary>
internal static class QuarterMetadata
{
    /// <summary>
    /// Fallback quarter abbreviations (English).
    /// </summary>
    private static readonly string[] FallbackAbbreviated = { "Q1", "Q2", "Q3", "Q4" };

    /// <summary>
    /// Fallback quarter wide names (English).
    /// </summary>
    private static readonly string[] FallbackWide = { "1st quarter", "2nd quarter", "3rd quarter", "4th quarter" };

    /// <summary>
    /// Fallback quarter narrow names.
    /// </summary>
    private static readonly string[] FallbackNarrow = { "1", "2", "3", "4" };

    /// <summary>
    /// Gets the quarter number (1-4) for a given month (1-12).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetQuarter(int month)
    {
        return (month - 1) / 3 + 1;
    }

    /// <summary>
    /// Gets the quarter number (1-4) for a given DateTime.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetQuarter(DateTime dateTime)
    {
        return GetQuarter(dateTime.Month);
    }

    /// <summary>
    /// Gets the abbreviated quarter name (e.g., "Q1") for the given DateTime.
    /// </summary>
    public static string GetAbbreviated(ref FormatterContext ctx, DateTime dateTime)
    {
        var quarter = GetQuarter(dateTime);
        return GetQuarterName(ref ctx, quarter, QuarterWidth.Abbreviated, standAlone: false);
    }

    /// <summary>
    /// Gets the wide quarter name (e.g., "1st quarter") for the given DateTime.
    /// </summary>
    public static string GetWide(ref FormatterContext ctx, DateTime dateTime)
    {
        var quarter = GetQuarter(dateTime);
        return GetQuarterName(ref ctx, quarter, QuarterWidth.Wide, standAlone: false);
    }

    /// <summary>
    /// Gets the narrow quarter name (e.g., "1") for the given DateTime.
    /// </summary>
    public static string GetNarrow(ref FormatterContext ctx, DateTime dateTime)
    {
        var quarter = GetQuarter(dateTime);
        return GetQuarterName(ref ctx, quarter, QuarterWidth.Narrow, standAlone: false);
    }

    /// <summary>
    /// Gets the standalone abbreviated quarter name for the given DateTime.
    /// </summary>
    public static string GetStandaloneAbbreviated(ref FormatterContext ctx, DateTime dateTime)
    {
        var quarter = GetQuarter(dateTime);
        return GetQuarterName(ref ctx, quarter, QuarterWidth.Abbreviated, standAlone: true);
    }

    /// <summary>
    /// Gets the standalone wide quarter name for the given DateTime.
    /// </summary>
    public static string GetStandaloneWide(ref FormatterContext ctx, DateTime dateTime)
    {
        var quarter = GetQuarter(dateTime);
        return GetQuarterName(ref ctx, quarter, QuarterWidth.Wide, standAlone: true);
    }

    /// <summary>
    /// Gets the standalone narrow quarter name for the given DateTime.
    /// </summary>
    public static string GetStandaloneNarrow(ref FormatterContext ctx, DateTime dateTime)
    {
        var quarter = GetQuarter(dateTime);
        return GetQuarterName(ref ctx, quarter, QuarterWidth.Narrow, standAlone: true);
    }

    /// <summary>
    /// Quarter width enum to avoid string comparisons.
    /// </summary>
    private enum QuarterWidth : byte
    {
        Abbreviated,
        Wide,
        Narrow
    }

    /// <summary>
    /// Gets the quarter name for the given quarter and width.
    /// </summary>
    /// <param name="ctx">The formatter context.</param>
    /// <param name="quarter">The quarter number (1-4).</param>
    /// <param name="width">The width enum value.</param>
    /// <param name="standAlone">Whether to use standalone patterns.</param>
    /// <returns>The quarter name.</returns>
    private static string GetQuarterName(ref FormatterContext ctx, int quarter, QuarterWidth width, bool standAlone)
    {
        if (quarter < 1 || quarter > 4)
        {
            return GetFallback(quarter, width);
        }

        if (TryGetQuarterData(ref ctx, out var quarterData))
        {
            var formats = standAlone ? quarterData.StandAlone : quarterData.Format;
            var name = GetQuarterFromFormats(formats, quarter, width);
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }

            // Fallback to format patterns if standalone not available
            if (standAlone)
            {
                name = GetQuarterFromFormats(quarterData.Format, quarter, width);
                if (!string.IsNullOrEmpty(name))
                {
                    return name;
                }
            }
        }

        return GetFallback(quarter, width);
    }

    /// <summary>
    /// Gets quarter name from formats using enum-based width selection.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string? GetQuarterFromFormats(QuarterFormats formats, int quarter, QuarterWidth width)
    {
        var index = quarter - 1;
        var array = width switch
        {
            QuarterWidth.Wide => formats.Wide,
            QuarterWidth.Narrow => formats.Narrow,
            _ => formats.Abbreviated
        };

        return array is { Length: > 0 } && index < array.Length ? array[index] : null;
    }

    /// <summary>
    /// Gets the fallback quarter name.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetFallback(int quarter, QuarterWidth width)
    {
        var index = Math.Clamp(quarter - 1, 0, 3);
        return width switch
        {
            QuarterWidth.Wide => FallbackWide[index],
            QuarterWidth.Narrow => FallbackNarrow[index],
            _ => FallbackAbbreviated[index]
        };
    }

    /// <summary>
    /// Tries to get quarter data from the CLDR provider.
    /// Uses shared LocaleHelper for optimized locale fallback.
    /// </summary>
    private static bool TryGetQuarterData(ref FormatterContext ctx, out QuarterData quarterData)
    {
        return LocaleHelper.TryGetLocaleData(
            ref ctx,
            localeData => localeData.Quarters,
            data => data.HasData,
            out quarterData);
    }
}
