using System;
using System.Runtime.CompilerServices;
using Rh.MessageFormat.Ast;
using Rh.MessageFormat.Formatting.Skeletons;
using static Rh.MessageFormat.Constants;
using SysDateTime = System.DateTime;

namespace Rh.MessageFormat.Formatting.Formatters;

/// <summary>
/// Helper class for formatting date/time ranges.
/// Supports locale-aware interval formatting using CLDR patterns.
/// </summary>
internal static class DateRangeMetadata
{
    /// <summary>
    /// Common interval separators in CLDR patterns.
    /// Static readonly to avoid allocation on every call.
    /// Ordered by length (longest first) for proper matching.
    /// </summary>
    private static readonly string[] IntervalSeparators =
    {
        DateRange.EnDashSeparator,
        DateRange.HyphenSeparator,
        DateRange.ToSeparator,
        DateRange.EnDashNoSpace,
        DateRange.HyphenNoSpace
    };

    /// <summary>
    /// Cached single-character strings for ConvertToken to avoid char.ToString() allocations.
    /// </summary>
    private static readonly string[] SingleCharCache = new string[128];

    static DateRangeMetadata()
    {
        // Pre-cache common single-character strings
        for (var i = 0; i < 128; i++)
        {
            SingleCharCache[i] = ((char)i).ToString();
        }
    }

    /// <summary>
    /// Gets a cached single-character string.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetCachedChar(char c)
    {
        return c < 128 ? SingleCharCache[c] : c.ToString();
    }

    /// <summary>
    /// Formats a date range using the specified style or skeleton.
    /// </summary>
    /// <param name="ctx">The formatter context.</param>
    /// <param name="start">The start date/time.</param>
    /// <param name="end">The end date/time.</param>
    /// <param name="style">The style (short, medium, long, full) or null for default.</param>
    /// <param name="skeleton">The skeleton pattern or null for style-based formatting.</param>
    /// <returns>The formatted date range string.</returns>
    public static string Format(ref FormatterContext ctx, SysDateTime start, SysDateTime end, string? style, string? skeleton)
    {
        // If dates are identical, just format as a single date
        if (start == end)
        {
            return FormatSingleDate(ref ctx, start, style, skeleton);
        }

        // Get the greatest difference field
        var greatestDiff = DetermineGreatestDifference(start, end);

        // Try to get a locale-specific interval pattern
        if (TryGetIntervalPattern(ref ctx, skeleton ?? style, greatestDiff, out var pattern))
        {
            return FormatWithPattern(ref ctx, start, end, pattern);
        }

        // Fall back to simple concatenation
        return FormatFallback(ref ctx, start, end, style, skeleton);
    }

    /// <summary>
    /// Determines the greatest difference field between two dates.
    /// </summary>
    /// <param name="start">The start date/time.</param>
    /// <param name="end">The end date/time.</param>
    /// <returns>The character representing the greatest difference field.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char DetermineGreatestDifference(SysDateTime start, SysDateTime end)
    {
        if (start.Year != end.Year) return 'y';
        if (start.Month != end.Month) return 'M';
        if (start.Day != end.Day) return 'd';
        if (start.Hour != end.Hour) return 'H';
        if (start.Minute != end.Minute) return 'm';
        return 's';
    }

    /// <summary>
    /// Formats a single date (when start and end are identical).
    /// </summary>
    private static string FormatSingleDate(ref FormatterContext ctx, SysDateTime date, string? style, string? skeleton)
    {
        if (!string.IsNullOrEmpty(skeleton))
        {
            var format = DateTimeSkeletonParser.ToFormatString(skeleton, ctx.Culture);
            var formatted = date.ToString(format, ctx.Culture);
            return SkeletonPostProcessor.Process(formatted, date, ref ctx);
        }

        return style switch
        {
            Styles.Short => date.ToString(DateRange.NetFormats.ShortDate, ctx.Culture),
            Styles.Long => date.ToString(DateRange.NetFormats.LongDate, ctx.Culture),
            Styles.Full => date.ToString(DateRange.NetFormats.FullDateTime, ctx.Culture),
            _ => date.ToString(DateRange.NetFormats.GeneralShortTime, ctx.Culture) // Medium or default
        };
    }

    /// <summary>
    /// Tries to get an interval pattern from CLDR data.
    /// Uses shared LocaleHelper for optimized locale fallback.
    /// </summary>
    private static bool TryGetIntervalPattern(ref FormatterContext ctx, string? skeletonOrStyle, char greatestDiff, out string pattern)
    {
        pattern = string.Empty;

        if (string.IsNullOrEmpty(skeletonOrStyle))
            return false;

        var provider = ctx.CldrDataProvider;
        var locale = ctx.Locale;

        // Try exact locale
        if (provider.TryGetLocaleData(locale, out var localeData) && localeData != null)
        {
            if (localeData.TryGetIntervalPattern(skeletonOrStyle, greatestDiff, out pattern))
            {
                return true;
            }
        }

        // Try base locale
        var baseLocale = LocaleHelper.GetBaseLocale(locale);
        if (baseLocale != null)
        {
            if (provider.TryGetLocaleData(baseLocale, out localeData) && localeData != null)
            {
                if (localeData.TryGetIntervalPattern(skeletonOrStyle, greatestDiff, out pattern))
                {
                    return true;
                }
            }
        }

        // Try fallback locale
        var fallbackLocale = ctx.FallbackLocale;
        if (!string.IsNullOrEmpty(fallbackLocale) &&
            !string.Equals(locale, fallbackLocale, StringComparison.OrdinalIgnoreCase))
        {
            if (provider.TryGetLocaleData(fallbackLocale, out localeData) && localeData != null)
            {
                if (localeData.TryGetIntervalPattern(skeletonOrStyle, greatestDiff, out pattern))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Formats a date range using a CLDR interval pattern.
    /// CLDR interval patterns contain both start and end format in a single string,
    /// separated by a special character sequence.
    /// </summary>
    private static string FormatWithPattern(ref FormatterContext ctx, SysDateTime start, SysDateTime end, string pattern)
    {
        // CLDR interval patterns are in the format "MMM d – d, y" where the repeated
        // field indicates where to split between start and end.
        // For simplicity, we'll look for common separators like " – ", " - ", " to "
        var separatorIndex = FindIntervalSeparator(pattern, out var separatorLength);
        if (separatorIndex < 0)
        {
            // No separator found, use fallback
            return FormatSimpleFallback(ref ctx, start, end);
        }

        // Use span-based approach to avoid multiple Substring allocations
        var patternSpan = pattern.AsSpan();
        var startPart = patternSpan.Slice(0, separatorIndex);
        var separatorPart = patternSpan.Slice(separatorIndex, separatorLength);
        var endPart = patternSpan.Slice(separatorIndex + separatorLength);

        // Format each part
        var startFormatted = FormatDatePart(ref ctx, start, startPart);
        var endFormatted = FormatDatePart(ref ctx, end, endPart);

        // Use string.Concat with spans to minimize allocations
        return string.Concat(startFormatted, separatorPart, endFormatted);
    }

    /// <summary>
    /// Finds the separator index in an interval pattern.
    /// Uses static array to avoid allocation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FindIntervalSeparator(string pattern, out int length)
    {
        // Check for longer separators first (they're at the beginning of the array)
        foreach (var sep in IntervalSeparators)
        {
            var idx = pattern.IndexOf(sep, StringComparison.Ordinal);
            if (idx > 0)
            {
                length = sep.Length;
                return idx;
            }
        }

        length = 0;
        return -1;
    }

    /// <summary>
    /// Formats a date using a partial pattern (span-based overload).
    /// </summary>
    private static string FormatDatePart(ref FormatterContext ctx, SysDateTime date, ReadOnlySpan<char> partPattern)
    {
        // Convert ICU pattern tokens to .NET format
        var netFormat = ConvertToNetFormat(partPattern);
        var formatted = date.ToString(netFormat, ctx.Culture);
        return SkeletonPostProcessor.Process(formatted, date, ref ctx);
    }

    /// <summary>
    /// Converts ICU pattern tokens to .NET format strings.
    /// Uses stackalloc for small patterns to avoid heap allocation.
    /// </summary>
    private static string ConvertToNetFormat(ReadOnlySpan<char> icuPattern)
    {
        if (icuPattern.IsEmpty)
            return string.Empty;

        // Most patterns are short, use stackalloc for small buffers
        // Estimate: worst case is each char expands to 5 chars (MMMMM)
        var maxLength = icuPattern.Length * 5;
        Span<char> buffer = maxLength <= 128 ? stackalloc char[maxLength] : new char[maxLength];
        var writePos = 0;

        var i = 0;
        while (i < icuPattern.Length)
        {
            var c = icuPattern[i];
            var count = 1;
            while (i + count < icuPattern.Length && icuPattern[i + count] == c)
                count++;

            var converted = ConvertToken(c, count);
            converted.AsSpan().CopyTo(buffer.Slice(writePos));
            writePos += converted.Length;
            i += count;
        }

        return new string(buffer.Slice(0, writePos));
    }

    /// <summary>
    /// Converts a single ICU token to .NET format.
    /// Uses cached strings for single characters.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ConvertToken(char c, int count)
    {
        return c switch
        {
            'y' => count == 2 ? "yy" : "yyyy",
            'M' => count switch { 1 => "M", 2 => "MM", 3 => "MMM", 4 => "MMMM", _ => "MMMMM" },
            'd' => count == 1 ? "d" : "dd",
            'E' => count <= 3 ? "ddd" : "dddd",
            'H' => count == 1 ? "H" : "HH",
            'h' => count == 1 ? "h" : "hh",
            'm' => count == 1 ? "m" : "mm",
            's' => count == 1 ? "s" : "ss",
            'a' => "tt",
            // Use cached single-char strings to avoid allocation
            _ => GetCachedChar(c)
        };
    }

    /// <summary>
    /// Formats a date range using a fallback pattern.
    /// </summary>
    private static string FormatFallback(ref FormatterContext ctx, SysDateTime start, SysDateTime end, string? style, string? skeleton)
    {
        var startStr = FormatSingleDate(ref ctx, start, style, skeleton);
        var endStr = FormatSingleDate(ref ctx, end, style, skeleton);

        // Get fallback pattern from CLDR data
        var fallbackPattern = GetFallbackPattern(ref ctx);

        // Optimize common fallback pattern "{0} – {1}"
        if (ReferenceEquals(fallbackPattern, DateRange.DefaultFallbackPattern) ||
            fallbackPattern == DateRange.DefaultFallbackPattern)
        {
            return string.Concat(startStr, DateRange.EnDashSeparator, endStr);
        }

        return string.Format(fallbackPattern, startStr, endStr);
    }

    /// <summary>
    /// Formats a date range using a simple fallback pattern.
    /// </summary>
    private static string FormatSimpleFallback(ref FormatterContext ctx, SysDateTime start, SysDateTime end)
    {
        var startStr = start.ToString(DateRange.NetFormats.GeneralShortTime, ctx.Culture);
        var endStr = end.ToString(DateRange.NetFormats.GeneralShortTime, ctx.Culture);
        var fallbackPattern = GetFallbackPattern(ref ctx);

        // Optimize common fallback pattern "{0} – {1}"
        if (ReferenceEquals(fallbackPattern, DateRange.DefaultFallbackPattern) ||
            fallbackPattern == DateRange.DefaultFallbackPattern)
        {
            return string.Concat(startStr, DateRange.EnDashSeparator, endStr);
        }

        return string.Format(fallbackPattern, startStr, endStr);
    }

    /// <summary>
    /// Gets the fallback pattern for interval formatting.
    /// </summary>
    private static string GetFallbackPattern(ref FormatterContext ctx)
    {
        var provider = ctx.CldrDataProvider;
        var locale = ctx.Locale;

        if (provider.TryGetLocaleData(locale, out var localeData) && localeData != null)
        {
            var intervalData = localeData.IntervalFormats;
            if (intervalData.HasData && !string.IsNullOrEmpty(intervalData.FallbackPattern))
            {
                return intervalData.FallbackPattern;
            }
        }

        return DateRange.DefaultFallbackPattern;
    }
}
