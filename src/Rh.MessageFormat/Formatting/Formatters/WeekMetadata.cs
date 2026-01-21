using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rh.MessageFormat.Abstractions.Models;
using Rh.MessageFormat.Ast;

namespace Rh.MessageFormat.Formatting.Formatters;

/// <summary>
/// Helper class for week-of-year calculations and formatting.
/// Supports locale-aware week calculations using CLDR week data.
/// </summary>
internal static class WeekMetadata
{
    /// <summary>
    /// Countries/regions using Sunday as first day of week with minDays=1.
    /// Using HashSet for O(1) lookup instead of linear search.
    /// </summary>
    private static readonly HashSet<string> SundayFirstRegions = new(StringComparer.OrdinalIgnoreCase)
    {
        "US", "AS", "GU", "MP", "PR", "VI", // US territories
        "CA", // Canada (varies by province but federal is Sunday)
        "JP", // Japan
        "IL", // Israel (also uses Saturday)
        "KR", // South Korea
        "TW", // Taiwan
        "PH", // Philippines
        "TH", // Thailand
        "BR", // Brazil
        "MX", // Mexico
        "CO", // Colombia
        "AR", // Argentina
        "PE", // Peru
        "VE", // Venezuela
        "CL", // Chile
        "GT", // Guatemala
        "EC", // Ecuador
        "HN", // Honduras
        "NI", // Nicaragua
        "SV", // El Salvador
        "PA", // Panama
        "PY", // Paraguay
        "BO", // Bolivia
        "DO", // Dominican Republic
        "CU", // Cuba
        "HT", // Haiti
        "ZW", // Zimbabwe
        "BW", // Botswana
        "MO", // Macau
        "AU", // Australia (varies by state but commonly Sunday)
        "NZ", // New Zealand
        "ZA"  // South Africa
    };

    /// <summary>
    /// Gets the week of year for the given DateTime using the locale's week rules.
    /// </summary>
    /// <param name="ctx">The formatter context.</param>
    /// <param name="dateTime">The date to get the week for.</param>
    /// <returns>The week number (1-53).</returns>
    public static int GetWeekOfYear(ref FormatterContext ctx, DateTime dateTime)
    {
        var weekData = GetWeekData(ref ctx);
        return CalculateWeekOfYear(dateTime, weekData.FirstDay, weekData.MinDays);
    }

    /// <summary>
    /// Gets the week of year as a formatted string (no padding).
    /// </summary>
    public static string GetWeekOfYearString(ref FormatterContext ctx, DateTime dateTime)
    {
        return GetWeekOfYear(ref ctx, dateTime).ToString();
    }

    /// <summary>
    /// Gets the week of year as a formatted string with 2-digit padding.
    /// </summary>
    public static string GetWeekOfYearPadded(ref FormatterContext ctx, DateTime dateTime)
    {
        return GetWeekOfYear(ref ctx, dateTime).ToString("D2");
    }

    /// <summary>
    /// Gets the week data for the current locale.
    /// Falls back to ISO 8601 if not available.
    /// </summary>
    private static WeekData GetWeekData(ref FormatterContext ctx)
    {
        if (TryGetWeekData(ref ctx, out var weekData))
        {
            return weekData;
        }

        // Default fallback: derive from locale
        return GetFallbackWeekData(ctx.Locale);
    }

    /// <summary>
    /// Tries to get week data from the CLDR provider.
    /// </summary>
    private static bool TryGetWeekData(ref FormatterContext ctx, out WeekData weekData)
    {
        return LocaleHelper.TryGetLocaleData(
            ref ctx,
            localeData => localeData.WeekInfo,
            data => data.HasData,
            out weekData);
    }

    /// <summary>
    /// Gets fallback week data based on locale.
    /// Uses region code to determine week rules with O(1) HashSet lookup.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static WeekData GetFallbackWeekData(string locale)
    {
        // Extract region code using optimized span-based method
        var region = LocaleHelper.GetRegionCode(locale);

        if (!string.IsNullOrEmpty(region) && SundayFirstRegions.Contains(region))
        {
            return WeekData.US;
        }

        // Default: ISO 8601 (Monday first, minDays 4)
        return WeekData.Iso8601;
    }

    /// <summary>
    /// Calculates the week of year using the specified week rules.
    /// </summary>
    /// <param name="date">The date to calculate the week for.</param>
    /// <param name="firstDay">The first day of the week.</param>
    /// <param name="minDays">The minimum days in the first week of the year.</param>
    /// <returns>The week number (1-53).</returns>
    public static int CalculateWeekOfYear(DateTime date, DayOfWeek firstDay, int minDays)
    {
        // Find Jan 1 of the year
        var jan1 = new DateTime(date.Year, 1, 1);

        // Calculate what day of the week Jan 1 is (0-6, where 0 = firstDay)
        var jan1DayOffset = ((int)jan1.DayOfWeek - (int)firstDay + 7) % 7;

        // Days in the partial first week (before the first full week starts)
        var daysInFirstPartialWeek = (7 - jan1DayOffset) % 7;

        // Does the partial week count as week 1?
        var firstWeekHasEnoughDays = daysInFirstPartialWeek == 0 || (7 - jan1DayOffset) >= minDays;

        // Day of year (1-366)
        var dayOfYear = date.DayOfYear;

        if (firstWeekHasEnoughDays || daysInFirstPartialWeek == 0)
        {
            // Jan 1 is in week 1
            var adjustedDay = dayOfYear + jan1DayOffset;
            return (adjustedDay - 1) / 7 + 1;
        }
        else
        {
            // Jan 1 is in week 52/53 of the previous year
            // The first full week of this year is week 1
            if (dayOfYear <= daysInFirstPartialWeek)
            {
                // Date is in the partial week that belongs to the previous year
                return CalculateWeekOfYear(jan1.AddDays(-1), firstDay, minDays);
            }
            else
            {
                // Date is after the partial week
                var adjustedDay = dayOfYear - daysInFirstPartialWeek;
                return (adjustedDay - 1) / 7 + 1;
            }
        }
    }
}
