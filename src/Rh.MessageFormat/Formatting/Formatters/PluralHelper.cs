using System;
using Rh.MessageFormat.Abstractions.Models;
using Rh.MessageFormat.Ast;
using static Rh.MessageFormat.Constants;

namespace Rh.MessageFormat.Formatting.Formatters;

/// <summary>
/// Helper methods for plural category determination.
/// </summary>
internal static class PluralHelper
{
    /// <summary>
    /// Gets the plural category for a value using the locale's plural rules.
    /// </summary>
    public static string GetPluralCategory(ref FormatterContext ctx, double value)
    {
        var provider = ctx.CldrDataProvider;
        var locale = ctx.Locale;

        // Try exact locale
        if (provider.TryGetLocaleData(locale, out var localeData) && localeData != null)
        {
            var absValue = Math.Abs(value);
            var context = new PluralContext(absValue);
            return localeData.GetPluralCategory(context);
        }

        // Try base locale
        var dashIndex = locale.IndexOf('-');
        if (dashIndex < 0) dashIndex = locale.IndexOf('_');
        if (dashIndex > 0)
        {
            var baseLocale = locale.Substring(0, dashIndex);
            if (provider.TryGetLocaleData(baseLocale, out localeData) && localeData != null)
            {
                var absValue = Math.Abs(value);
                var context = new PluralContext(absValue);
                return localeData.GetPluralCategory(context);
            }
        }

        // Simple fallback
        return IsOne(value) ? Plurals.One : Plurals.Other;
    }

    /// <summary>
    /// Checks if a floating-point value represents exactly 1 (for plural "one" category fallback).
    /// Uses epsilon comparison to handle floating-point precision issues.
    /// </summary>
    public static bool IsOne(double value)
    {
        var absVal = Math.Abs(value);
        return absVal >= 0.9999999 && absVal <= 1.0000001 && Math.Abs(value - Math.Round(value)) < 0.0000001;
    }
}
