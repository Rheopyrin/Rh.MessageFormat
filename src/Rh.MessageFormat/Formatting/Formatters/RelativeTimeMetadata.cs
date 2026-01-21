using System;
using Rh.MessageFormat.Abstractions.Models;
using Rh.MessageFormat.Ast;
using static Rh.MessageFormat.Constants;
using static Rh.MessageFormat.Constants.RelativeTime;

namespace Rh.MessageFormat.Formatting.Formatters;

/// <summary>
/// Provides relative time formatting using CLDR data.
/// Formats values like "yesterday", "in 3 days", "2 hours ago".
/// </summary>
internal static class RelativeTimeMetadata
{
    /// <summary>
    /// Formats a relative time value.
    /// </summary>
    /// <param name="ctx">The formatter context.</param>
    /// <param name="value">The relative time value (negative for past, positive for future, 0 for now/today).</param>
    /// <param name="field">The field: "year", "month", "week", "day", "hour", "minute", "second", or weekday names.</param>
    /// <param name="style">The style: "long" (default), "short", or "narrow".</param>
    /// <param name="numeric">The numeric mode: "always" (always use numbers) or "auto" (use "yesterday" when available).</param>
    /// <returns>The formatted relative time string.</returns>
    public static string Format(
        ref FormatterContext ctx,
        double value,
        string field,
        string style = Styles.Long,
        string numeric = NumericMode.Always)
    {
        // Validate field
        if (!IsValidField(field))
        {
            return FormatFallback(value, field);
        }

        // Validate style
        if (!IsValidStyle(style))
        {
            style = Styles.Long;
        }

        // Normalize numeric parameter
        var useNumeric = !string.Equals(numeric, NumericMode.Auto, StringComparison.OrdinalIgnoreCase);

        // Try to get relative time data
        if (!TryGetRelativeTime(ref ctx, field, style, out var data))
        {
            return FormatFallback(value, field);
        }

        // For numeric="auto", try relative type strings first (yesterday, today, tomorrow)
        if (!useNumeric)
        {
            var intValue = (int)Math.Round(value);
            if (Math.Abs(value - intValue) < IntegerTolerance && data.TryGetRelativeType(intValue, out var relativeType))
            {
                return relativeType;
            }
        }

        // Get the absolute value for formatting
        var absValue = Math.Abs(value);

        // Get plural category
        var pluralCategory = PluralHelper.GetPluralCategory(ref ctx, absValue);

        // Get the appropriate pattern
        string pattern;
        if (value < 0)
        {
            // Past
            if (!data.TryGetPastPattern(pluralCategory, out pattern))
            {
                return FormatFallback(value, field);
            }
        }
        else
        {
            // Future (including 0)
            if (!data.TryGetFuturePattern(pluralCategory, out pattern))
            {
                return FormatFallback(value, field);
            }
        }

        // Replace {0} with the absolute value
        return pattern.Replace(Placeholder, FormatNumber(absValue));
    }

    private static bool IsValidField(string field)
    {
        foreach (var validField in ValidFields)
        {
            if (string.Equals(field, validField, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsValidStyle(string style)
    {
        foreach (var validStyle in ValidStyles)
        {
            if (string.Equals(style, validStyle, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private static bool TryGetRelativeTime(ref FormatterContext ctx, string field, string style, out RelativeTimeData data)
    {
        var provider = ctx.CldrDataProvider;
        var locale = ctx.Locale;
        var fallbackLocale = ctx.FallbackLocale;

        // Try exact locale
        if (provider.TryGetLocaleData(locale, out var localeData) && localeData != null)
        {
            if (localeData.TryGetRelativeTime(field, style, out data))
            {
                return true;
            }

            // Try fallback to "long" style
            if (style != Styles.Long && localeData.TryGetRelativeTime(field, Styles.Long, out data))
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
                if (localeData.TryGetRelativeTime(field, style, out data))
                {
                    return true;
                }

                // Try fallback to "long" style
                if (style != Styles.Long && localeData.TryGetRelativeTime(field, Styles.Long, out data))
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
                if (localeData.TryGetRelativeTime(field, style, out data))
                {
                    return true;
                }

                // Try fallback to "long" style
                if (style != Styles.Long && localeData.TryGetRelativeTime(field, Styles.Long, out data))
                {
                    return true;
                }
            }
        }

        data = default;
        return false;
    }

    private static string FormatNumber(double value)
    {
        // Format as integer if it's a whole number
        if (Math.Abs(value - Math.Round(value)) < IntegerTolerance)
        {
            return ((long)Math.Round(value)).ToString();
        }
        return value.ToString(GeneralNumberFormat);
    }

    private static string FormatFallback(double value, string field)
    {
        var absValue = Math.Abs(value);
        var formattedNumber = FormatNumber(absValue);

        if (value < 0)
        {
            return string.Format(FallbackPatterns.Past, formattedNumber, field);
        }
        else if (value > 0)
        {
            return string.Format(FallbackPatterns.Future, formattedNumber, field);
        }
        else
        {
            return string.Format(FallbackPatterns.Present, field);
        }
    }
}
