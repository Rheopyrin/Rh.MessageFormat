using System;
using System.Runtime.CompilerServices;
using Rh.MessageFormat.Ast;
using Rh.MessageFormat.Ast.Elements;
using Rh.MessageFormat.Formatting.Formatters;
using static Rh.MessageFormat.Constants.DateTime;

namespace Rh.MessageFormat.Formatting.Skeletons;

/// <summary>
/// Post-processes formatted datetime strings to replace marker characters
/// with values that .NET's DateTime.ToString() cannot directly produce.
/// </summary>
internal static class SkeletonPostProcessor
{
    /// <summary>
    /// Processes a formatted datetime string, replacing any skeleton marker characters
    /// with the actual computed values (day of year, quarter, week of year).
    /// </summary>
    /// <param name="formatted">The formatted string from DateTime.ToString().</param>
    /// <param name="dateTime">The original DateTime value for computing day of year, etc.</param>
    /// <param name="ctx">The formatter context for locale-aware formatting.</param>
    /// <returns>The processed string with all markers replaced.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Process(string formatted, DateTime dateTime, ref FormatterContext ctx)
    {
        // Quick check: if no markers present, return as-is
        if (!ContainsMarkers(formatted))
        {
            return formatted;
        }

        return ProcessMarkers(formatted, dateTime, ref ctx);
    }

    /// <summary>
    /// Checks if the formatted string contains any skeleton marker characters.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ContainsMarkers(string formatted)
    {
        // Check for any private use area characters (U+E000 - U+E00F range we use)
        foreach (var c in formatted)
        {
            if (c >= '\uE001' && c <= '\uE00B')
            {
                return true;
            }
        }
        return false;
    }

    private static string ProcessMarkers(string formatted, DateTime dateTime, ref FormatterContext ctx)
    {
        var result = formatted;

        // Day of year markers
        if (result.Contains(SkeletonMarkers.DayOfYear))
        {
            result = result.Replace(
                SkeletonMarkers.DayOfYear.ToString(),
                dateTime.DayOfYear.ToString());
        }

        if (result.Contains(SkeletonMarkers.DayOfYearPadded2))
        {
            result = result.Replace(
                SkeletonMarkers.DayOfYearPadded2.ToString(),
                dateTime.DayOfYear.ToString("D2"));
        }

        if (result.Contains(SkeletonMarkers.DayOfYearPadded3))
        {
            result = result.Replace(
                SkeletonMarkers.DayOfYearPadded3.ToString(),
                dateTime.DayOfYear.ToString("D3"));
        }

        // Quarter markers (format context)
        if (result.Contains(SkeletonMarkers.QuarterAbbreviated))
        {
            result = result.Replace(
                SkeletonMarkers.QuarterAbbreviated.ToString(),
                QuarterMetadata.GetAbbreviated(ref ctx, dateTime));
        }

        if (result.Contains(SkeletonMarkers.QuarterWide))
        {
            result = result.Replace(
                SkeletonMarkers.QuarterWide.ToString(),
                QuarterMetadata.GetWide(ref ctx, dateTime));
        }

        if (result.Contains(SkeletonMarkers.QuarterNarrow))
        {
            result = result.Replace(
                SkeletonMarkers.QuarterNarrow.ToString(),
                QuarterMetadata.GetNarrow(ref ctx, dateTime));
        }

        // Standalone quarter markers
        if (result.Contains(SkeletonMarkers.StandaloneQuarterAbbreviated))
        {
            result = result.Replace(
                SkeletonMarkers.StandaloneQuarterAbbreviated.ToString(),
                QuarterMetadata.GetStandaloneAbbreviated(ref ctx, dateTime));
        }

        if (result.Contains(SkeletonMarkers.StandaloneQuarterWide))
        {
            result = result.Replace(
                SkeletonMarkers.StandaloneQuarterWide.ToString(),
                QuarterMetadata.GetStandaloneWide(ref ctx, dateTime));
        }

        if (result.Contains(SkeletonMarkers.StandaloneQuarterNarrow))
        {
            result = result.Replace(
                SkeletonMarkers.StandaloneQuarterNarrow.ToString(),
                QuarterMetadata.GetStandaloneNarrow(ref ctx, dateTime));
        }

        // Week of year markers
        if (result.Contains(SkeletonMarkers.WeekOfYear))
        {
            result = result.Replace(
                SkeletonMarkers.WeekOfYear.ToString(),
                WeekMetadata.GetWeekOfYearString(ref ctx, dateTime));
        }

        if (result.Contains(SkeletonMarkers.WeekOfYearPadded))
        {
            result = result.Replace(
                SkeletonMarkers.WeekOfYearPadded.ToString(),
                WeekMetadata.GetWeekOfYearPadded(ref ctx, dateTime));
        }

        return result;
    }
}
