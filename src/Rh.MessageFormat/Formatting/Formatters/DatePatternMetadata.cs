using System;
using Rh.MessageFormat.Abstractions.Models;
using Rh.MessageFormat.Ast;
using static Rh.MessageFormat.Constants;
using static Rh.MessageFormat.Constants.DateTime;

namespace Rh.MessageFormat.Formatting.Formatters;

/// <summary>
/// Provides CLDR-based date/time patterns for locale-specific formatting using CldrData.
/// </summary>
internal static class DatePatternMetadata
{
    /// <summary>
    /// Gets the date pattern for a locale and style.
    /// </summary>
    public static string GetDatePattern(ref FormatterContext ctx, string style)
    {
        if (TryGetPatterns(ref ctx, out var patterns))
        {
            var pattern = style.ToLowerInvariant() switch
            {
                Styles.Short => patterns.Date.Short,
                Styles.Medium => patterns.Date.Medium,
                Styles.Long => patterns.Date.Long,
                Styles.Full => patterns.Date.Full,
                _ => patterns.Date.Short
            };

            if (!string.IsNullOrEmpty(pattern))
            {
                return pattern;
            }
        }

        // Fallback patterns
        return style.ToLowerInvariant() switch
        {
            Styles.Short => FallbackPatterns.DateShort,
            Styles.Medium => FallbackPatterns.DateMedium,
            Styles.Long => FallbackPatterns.DateLong,
            Styles.Full => FallbackPatterns.DateFull,
            _ => FallbackPatterns.DateShort
        };
    }

    /// <summary>
    /// Gets the time pattern for a locale and style.
    /// </summary>
    public static string GetTimePattern(ref FormatterContext ctx, string style)
    {
        if (TryGetPatterns(ref ctx, out var patterns))
        {
            var pattern = style.ToLowerInvariant() switch
            {
                Styles.Short => patterns.Time.Short,
                Styles.Medium => patterns.Time.Medium,
                Styles.Long => patterns.Time.Long,
                Styles.Full => patterns.Time.Full,
                _ => patterns.Time.Medium
            };

            if (!string.IsNullOrEmpty(pattern))
            {
                return pattern;
            }
        }

        // Fallback patterns
        return style.ToLowerInvariant() switch
        {
            Styles.Short => FallbackPatterns.TimeShort,
            Styles.Medium => FallbackPatterns.TimeMedium,
            Styles.Long => FallbackPatterns.TimeLong,
            Styles.Full => FallbackPatterns.TimeFull,
            _ => FallbackPatterns.TimeMedium
        };
    }

    /// <summary>
    /// Gets the datetime combination pattern for a locale and style.
    /// Returns a template with {0} for time and {1} for date placeholders.
    /// </summary>
    public static string GetDateTimePattern(ref FormatterContext ctx, string style)
    {
        if (TryGetPatterns(ref ctx, out var patterns))
        {
            var combinationPattern = style.ToLowerInvariant() switch
            {
                Styles.Short => patterns.DateTime.Short,
                Styles.Medium => patterns.DateTime.Medium,
                Styles.Long => patterns.DateTime.Long,
                Styles.Full => patterns.DateTime.Full,
                _ => patterns.DateTime.Short
            };

            if (!string.IsNullOrEmpty(combinationPattern))
            {
                return combinationPattern;
            }
        }

        // Fallback - simple concatenation template
        return FallbackPatterns.DateTimeCombination;
    }

    private static bool TryGetPatterns(ref FormatterContext ctx, out DatePatternData patterns)
    {
        var provider = ctx.CldrDataProvider;
        var locale = ctx.Locale;
        var fallbackLocale = ctx.FallbackLocale;

        // Try exact locale
        if (provider.TryGetLocaleData(locale, out var localeData) && localeData != null)
        {
            patterns = localeData.DatePatterns;
            if (!string.IsNullOrEmpty(patterns.Date.Short))
            {
                return true;
            }
        }

        // Try base locale
        var dashIndex = locale.IndexOf(Common.DashChar);
        if (dashIndex < 0) dashIndex = locale.IndexOf(Common.UnderscoreChar);
        if (dashIndex > 0)
        {
            var baseLocale = locale.Substring(0, dashIndex);
            if (provider.TryGetLocaleData(baseLocale, out localeData) && localeData != null)
            {
                patterns = localeData.DatePatterns;
                if (!string.IsNullOrEmpty(patterns.Date.Short))
                {
                    return true;
                }
            }
        }

        // Try fallback locale
        if (!string.Equals(locale, fallbackLocale, StringComparison.OrdinalIgnoreCase))
        {
            if (fallbackLocale != null && provider.TryGetLocaleData(fallbackLocale, out localeData) && localeData != null)
            {
                patterns = localeData.DatePatterns;
                if (!string.IsNullOrEmpty(patterns.Date.Short))
                {
                    return true;
                }
            }
        }

        patterns = default;
        return false;
    }
}