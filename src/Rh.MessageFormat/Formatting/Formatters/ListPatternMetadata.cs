using System;
using Rh.MessageFormat.Abstractions;
using Rh.MessageFormat.Abstractions.Models;
using Rh.MessageFormat.Ast;
using static Rh.MessageFormat.Constants;
using static Rh.MessageFormat.Constants.List;

namespace Rh.MessageFormat.Formatting.Formatters;

/// <summary>
/// Provides list pattern data for various locales using CldrData.
/// </summary>
internal static class ListPatternMetadata
{
    /// <summary>
    /// Gets the list connectors for a locale, style, and width.
    /// </summary>
    /// <param name="ctx">The formatter context containing locale and CLDR provider.</param>
    /// <param name="style">The list style: "conjunction", "disjunction", or "unit".</param>
    /// <param name="width">The display width: "long", "short", or "narrow".</param>
    /// <returns>Tuple of (separator, lastSeparator, pairSeparator).</returns>
    public static (string separator, string lastSeparator, string pairSeparator) GetConnectors(
        ref FormatterContext ctx, string style, string width)
    {
        // Map style + width to CLDR pattern type
        var patternType = GetPatternType(style, width);

        if (TryGetListPattern(ref ctx, patternType, out var pattern))
        {
            var separator = ExtractConnector(pattern.Middle);
            var lastSeparator = ExtractConnector(pattern.End);
            var pairSeparator = ExtractConnector(pattern.Two);

            return (separator, lastSeparator, pairSeparator);
        }

        // Fallback to English defaults based on style
        return style switch
        {
            StyleTypes.Disjunction => (FallbackConnectors.Separator, FallbackConnectors.DisjunctionLast, FallbackConnectors.DisjunctionPair),
            StyleTypes.Unit => (FallbackConnectors.Separator, FallbackConnectors.Separator, FallbackConnectors.Separator),
            _ => (FallbackConnectors.Separator, FallbackConnectors.ConjunctionLast, FallbackConnectors.ConjunctionPair) // conjunction default
        };
    }

    private static string GetPatternType(string style, string width)
    {
        // Map style and width to CLDR pattern type name
        var baseType = style switch
        {
            StyleTypes.Disjunction => StyleTypes.Or,
            StyleTypes.Unit => StyleTypes.Unit,
            _ => StyleTypes.Standard // conjunction
        };

        return width switch
        {
            WidthTypes.Short => $"{baseType}-{WidthTypes.Short}",
            WidthTypes.Narrow => $"{baseType}-{WidthTypes.Narrow}",
            _ => baseType // long
        };
    }

    /// <summary>
    /// Extracts the connector text between {0} and {1} from a CLDR pattern.
    /// </summary>
    private static string ExtractConnector(string? pattern)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            return FallbackConnectors.Separator;
        }

        // Pattern format: "{0}connector{1}"
        var startIndex = pattern.IndexOf(Placeholders.First);
        var endIndex = pattern.IndexOf(Placeholders.Second);

        if (startIndex >= 0 && endIndex > startIndex)
        {
            var connectorStart = startIndex + Placeholders.PlaceholderLength; // Skip "{0}"
            return pattern.Substring(connectorStart, endIndex - connectorStart);
        }

        return FallbackConnectors.Separator; // Fallback
    }

    private static bool TryGetListPattern(ref FormatterContext ctx, string patternType, out ListPatternData pattern)
    {
        var provider = ctx.CldrDataProvider;
        var locale = ctx.Locale;
        var fallbackLocale = ctx.FallbackLocale;

        // Try exact locale
        if (provider.TryGetLocaleData(locale, out var localeData) && localeData != null)
        {
            if (localeData.TryGetListPattern(patternType, out pattern))
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
                if (localeData.TryGetListPattern(patternType, out pattern))
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
                if (localeData.TryGetListPattern(patternType, out pattern))
                {
                    return true;
                }
            }
        }

        pattern = default;
        return false;
    }
}