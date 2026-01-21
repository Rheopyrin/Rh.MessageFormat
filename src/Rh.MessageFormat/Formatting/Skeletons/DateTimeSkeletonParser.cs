using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using static Rh.MessageFormat.Constants.DateTime;

namespace Rh.MessageFormat.Formatting.Skeletons;

/// <summary>
/// Parses ICU datetime skeleton strings and converts them to .NET format strings.
///
/// ICU Skeleton patterns:
/// - y, yy, yyyy: Year (2-digit or 4-digit)
/// - M, MM: Month (1-2 digits)
/// - MMM: Month abbreviation
/// - MMMM: Month full name
/// - d, dd: Day of month
/// - E, EE, EEE: Day of week abbreviation
/// - EEEE: Day of week full name
/// - j, jj: Hour (locale-aware, 12 or 24 hour)
/// - h, hh: Hour 12-hour
/// - H, HH: Hour 24-hour
/// - m, mm: Minutes
/// - s, ss: Seconds
/// - a: AM/PM marker
/// - z, zz, zzz: Timezone abbreviation
/// - Z: Timezone offset
/// </summary>
internal static class DateTimeSkeletonParser
{
    /// <summary>
    /// Converts an ICU datetime skeleton to a .NET format string.
    /// </summary>
    /// <param name="skeleton">The ICU skeleton pattern (without :: prefix).</param>
    /// <param name="culture">The culture for locale-aware patterns (j).</param>
    /// <returns>A .NET datetime format string.</returns>
    public static string ToFormatString(string skeleton, CultureInfo? culture = null)
    {
        if (string.IsNullOrEmpty(skeleton))
        {
            return Formats.General; // General date/time pattern
        }

        culture ??= CultureInfo.CurrentCulture;
        var result = new StringBuilder(skeleton.Length * 2);
        var i = 0;
        var prevTokenType = TokenType.None;
        var hadTimeSeparator = false;
        var hadDateTimeSeparator = false;

        while (i < skeleton.Length)
        {
            var c = skeleton[i];
            var count = CountConsecutive(skeleton, i, c);
            var currentTokenType = GetTokenType(c);

            // Track if we've seen separators as literals
            if (c == Separators.Colon)
            {
                hadTimeSeparator = true;
            }
            else if (c == Separators.Space || c == Separators.T)
            {
                hadDateTimeSeparator = true;
            }

            // Add separators between time components (only if not already present)
            if (ShouldAddTimeSeparator(prevTokenType, currentTokenType) && !hadTimeSeparator)
            {
                result.Append(Separators.Colon);
            }
            // Add space between date and time components (only if not already present)
            else if (ShouldAddDateTimeSeparator(prevTokenType, currentTokenType) && !hadDateTimeSeparator)
            {
                result.Append(Separators.Space);
            }

            var format = ConvertToken(c, count, culture);
            result.Append(format);

            // Update previous token type and reset separator flags when we see a new significant token
            if (currentTokenType != TokenType.Literal)
            {
                prevTokenType = currentTokenType;
                hadTimeSeparator = false;
                hadDateTimeSeparator = false;
            }

            i += count;
        }

        return result.ToString();
    }

    private enum TokenType
    {
        None,
        Year,
        Month,
        Day,
        DayOfWeek,
        Hour,
        Minute,
        Second,
        FractionalSecond,
        AmPm,
        Timezone,
        Literal
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TokenType GetTokenType(char c)
    {
        return c switch
        {
            SkeletonChars.Year => TokenType.Year,
            SkeletonChars.Month or SkeletonChars.StandaloneMonth => TokenType.Month,
            SkeletonChars.Day => TokenType.Day,
            SkeletonChars.DayOfWeek or SkeletonChars.StandaloneDayOfWeek => TokenType.DayOfWeek,
            SkeletonChars.HourLocale or SkeletonChars.HourLocaleNoAmPm or SkeletonChars.Hour12 or SkeletonChars.Hour24 or SkeletonChars.Hour1To24 or SkeletonChars.Hour0To11 => TokenType.Hour,
            SkeletonChars.Minute => TokenType.Minute,
            SkeletonChars.Second => TokenType.Second,
            SkeletonChars.FractionalSecond => TokenType.FractionalSecond,
            SkeletonChars.AmPm => TokenType.AmPm,
            SkeletonChars.TimezoneAbbr or SkeletonChars.TimezoneOffset or SkeletonChars.TimezoneOffsetZ or SkeletonChars.TimezoneOffsetNoZ or SkeletonChars.TimezoneId => TokenType.Timezone,
            Separators.Space or Separators.Comma or Separators.Dot or Separators.Colon or Separators.Dash or Separators.Slash => TokenType.Literal,
            _ => TokenType.Literal
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ShouldAddTimeSeparator(TokenType prev, TokenType current)
    {
        // Add colon between hour and minute, or minute and second
        return (prev == TokenType.Hour && current == TokenType.Minute) ||
               (prev == TokenType.Minute && current == TokenType.Second);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ShouldAddDateTimeSeparator(TokenType prev, TokenType current)
    {
        // Add space between date components and time components
        var prevIsDate = prev is TokenType.Year or TokenType.Month or TokenType.Day or TokenType.DayOfWeek;
        var currentIsTime = current is TokenType.Hour or TokenType.Minute or TokenType.Second;
        return prevIsDate && currentIsTime;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CountConsecutive(string s, int start, char c)
    {
        var count = 1;
        while (start + count < s.Length && s[start + count] == c)
        {
            count++;
        }
        return count;
    }

    private static string ConvertToken(char c, int count, CultureInfo culture)
    {
        return c switch
        {
            // Year - ICU 'y' without count usually means full year
            SkeletonChars.Year => count switch
            {
                1 => Formats.YearFull, // Single y in ICU skeleton = full year
                2 => Formats.YearShort,
                _ => Formats.YearFull
            },

            // Month
            SkeletonChars.Month => count switch
            {
                1 => Formats.MonthNumeric,
                2 => Formats.MonthPadded,
                3 => Formats.MonthAbbr,
                4 => Formats.MonthFull,
                5 => Formats.MonthNarrow, // Narrow month (J, F, M, etc.) - .NET uses 5 Ms
                _ => Formats.MonthFull
            },

            // Day of month
            SkeletonChars.Day => count switch
            {
                1 => Formats.DayNumeric,
                _ => Formats.DayPadded
            },

            // Day of week
            SkeletonChars.DayOfWeek => count switch
            {
                1 or 2 or 3 => Formats.DayOfWeekAbbr,
                4 => Formats.DayOfWeekFull,
                5 => Formats.DayOfWeekAbbr, // Narrow - .NET doesn't support, use abbrev
                _ => Formats.DayOfWeekFull
            },

            // Standalone day of week (same as E for .NET)
            SkeletonChars.StandaloneDayOfWeek => count switch
            {
                1 or 2 or 3 => Formats.DayOfWeekAbbr,
                4 => Formats.DayOfWeekFull,
                _ => Formats.DayOfWeekFull
            },

            // Hour - locale aware (j maps to h or H based on locale)
            SkeletonChars.HourLocale => IsLocale12Hour(culture) ? (count >= 2 ? Formats.Hour12Padded : Formats.Hour12Numeric) : (count >= 2 ? Formats.Hour24Padded : Formats.Hour24Numeric),

            // Hour - locale aware, no AM/PM (J)
            SkeletonChars.HourLocaleNoAmPm => count >= 2 ? Formats.Hour24Padded : Formats.Hour24Numeric,

            // Hour 12-hour
            SkeletonChars.Hour12 => count >= 2 ? Formats.Hour12Padded : Formats.Hour12Numeric,

            // Hour 24-hour
            SkeletonChars.Hour24 => count >= 2 ? Formats.Hour24Padded : Formats.Hour24Numeric,

            // Hour 1-24 (k) - .NET doesn't have this, use H
            SkeletonChars.Hour1To24 => count >= 2 ? Formats.Hour24Padded : Formats.Hour24Numeric,

            // Hour 0-11 (K) - .NET doesn't have this, use h
            SkeletonChars.Hour0To11 => count >= 2 ? Formats.Hour12Padded : Formats.Hour12Numeric,

            // Minute
            SkeletonChars.Minute => count >= 2 ? Formats.MinutePadded : Formats.MinuteNumeric,

            // Second
            SkeletonChars.Second => count >= 2 ? Formats.SecondPadded : Formats.SecondNumeric,

            // Fractional seconds
            SkeletonChars.FractionalSecond => new string('f', Math.Min(count, 7)), // .NET supports up to 7 f's

            // AM/PM marker
            SkeletonChars.AmPm => Formats.AmPm,

            // Era
            SkeletonChars.Era => count switch
            {
                1 or 2 or 3 => Formats.EraShort,
                _ => Formats.EraLong
            },

            // Quarter - uses marker characters for post-processing (wrapped in quotes to be literal)
            SkeletonChars.Quarter => count switch
            {
                1 or 2 => $"'{Constants.DateTime.SkeletonMarkers.QuarterAbbreviated}'",
                3 or 4 => $"'{Constants.DateTime.SkeletonMarkers.QuarterWide}'",
                _ => $"'{Constants.DateTime.SkeletonMarkers.QuarterNarrow}'"
            },

            // Standalone Quarter - uses marker characters for post-processing
            SkeletonChars.StandaloneQuarter => count switch
            {
                1 or 2 => $"'{Constants.DateTime.SkeletonMarkers.StandaloneQuarterAbbreviated}'",
                3 or 4 => $"'{Constants.DateTime.SkeletonMarkers.StandaloneQuarterWide}'",
                _ => $"'{Constants.DateTime.SkeletonMarkers.StandaloneQuarterNarrow}'"
            },

            // Week of year - uses marker characters for post-processing
            SkeletonChars.WeekOfYear => count >= 2
                ? $"'{Constants.DateTime.SkeletonMarkers.WeekOfYearPadded}'"
                : $"'{Constants.DateTime.SkeletonMarkers.WeekOfYear}'",

            // Day of year - uses marker characters for post-processing (wrapped in quotes to be literal)
            SkeletonChars.DayOfYear => count switch
            {
                1 => $"'{Constants.DateTime.SkeletonMarkers.DayOfYear}'",
                2 => $"'{Constants.DateTime.SkeletonMarkers.DayOfYearPadded2}'",
                _ => $"'{Constants.DateTime.SkeletonMarkers.DayOfYearPadded3}'"
            },

            // Timezone
            SkeletonChars.TimezoneAbbr => count switch
            {
                1 or 2 or 3 => Formats.Timezone, // .NET timezone abbreviations
                _ => Formats.TimezoneLong
            },

            // Timezone offset
            SkeletonChars.TimezoneOffset => count switch
            {
                1 or 2 or 3 => Formats.Timezone,
                4 => Formats.Timezone,
                5 => Formats.Timezone, // .NET max is zzz
                _ => Formats.Timezone
            },

            // Timezone offset with Z for UTC
            SkeletonChars.TimezoneOffsetZ => count switch
            {
                1 => Formats.TimezoneOffset,
                2 or 3 => Formats.Timezone,
                _ => Formats.Timezone
            },

            // Timezone offset
            SkeletonChars.TimezoneOffsetNoZ => Formats.Timezone,

            // Timezone ID (not supported in .NET standard format, use offset)
            SkeletonChars.TimezoneId => Formats.Timezone,

            // Standalone month (L) - same as M for .NET
            SkeletonChars.StandaloneMonth => count switch
            {
                1 => Formats.MonthNumeric,
                2 => Formats.MonthPadded,
                3 => Formats.MonthAbbr,
                4 => Formats.MonthFull,
                _ => Formats.MonthFull
            },

            // Literals - pass through as-is
            Separators.Space or Separators.Comma or Separators.Dot or Separators.Colon or Separators.Dash or Separators.Slash => c.ToString(),

            // Unknown - pass through
            _ => c.ToString()
        };
    }

    /// <summary>
    /// Determines if a culture uses 12-hour time format.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsLocale12Hour(CultureInfo culture)
    {
        var shortTimePattern = culture.DateTimeFormat.ShortTimePattern;
        return shortTimePattern.Contains(SkeletonChars.Hour12) && !shortTimePattern.Contains(SkeletonChars.Hour24);
    }
}
