using System;
using Rh.MessageFormat.Abstractions;
using Rh.MessageFormat.Abstractions.Models;
using Rh.MessageFormat.Ast;

namespace Rh.MessageFormat.Formatting.Formatters;

/// <summary>
/// Provides currency display data for various locales using CldrData.
/// </summary>
internal static class CurrencyMetadata
{
    /// <summary>
    /// Gets the currency symbol for a locale and currency code.
    /// </summary>
    public static string GetSymbol(ref FormatterContext ctx, string currencyCode)
    {
        if (TryGetCurrency(ref ctx, currencyCode, out var data))
        {
            return data.Symbol;
        }
        return currencyCode; // Fallback to code
    }

    /// <summary>
    /// Gets the narrow currency symbol for a locale and currency code.
    /// </summary>
    public static string GetNarrowSymbol(ref FormatterContext ctx, string currencyCode)
    {
        if (TryGetCurrency(ref ctx, currencyCode, out var data))
        {
            return !string.IsNullOrEmpty(data.NarrowSymbol) ? data.NarrowSymbol : data.Symbol;
        }
        return currencyCode; // Fallback to code
    }

    /// <summary>
    /// Gets the currency display name for a locale and currency code.
    /// </summary>
    public static string GetDisplayName(ref FormatterContext ctx, string currencyCode, bool isPlural = false)
    {
        if (TryGetCurrency(ref ctx, currencyCode, out var data))
        {
            if (isPlural && !string.IsNullOrEmpty(data.DisplayNameOther))
            {
                return data.DisplayNameOther;
            }
            if (!isPlural && !string.IsNullOrEmpty(data.DisplayNameOne))
            {
                return data.DisplayNameOne;
            }
            return data.DisplayName;
        }
        return currencyCode; // Fallback to code
    }

    private static bool TryGetCurrency(ref FormatterContext ctx, string currencyCode, out CurrencyData data)
    {
        var provider = ctx.CldrDataProvider;
        var locale = ctx.Locale;
        var fallbackLocale = ctx.FallbackLocale;

        // Try exact locale
        if (provider.TryGetLocaleData(locale, out var localeData) && localeData != null)
        {
            if (localeData.TryGetCurrency(currencyCode, out data))
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
                if (localeData.TryGetCurrency(currencyCode, out data))
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
                if (localeData.TryGetCurrency(currencyCode, out data))
                {
                    return true;
                }
            }
        }

        data = default;
        return false;
    }
}