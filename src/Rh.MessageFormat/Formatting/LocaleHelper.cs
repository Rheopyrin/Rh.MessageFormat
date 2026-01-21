using System;
using System.Runtime.CompilerServices;
using Rh.MessageFormat.Abstractions.Interfaces;
using Rh.MessageFormat.Ast;

namespace Rh.MessageFormat.Formatting;

/// <summary>
/// Helper class for locale-related operations with optimized performance.
/// Centralizes locale fallback logic to avoid code duplication and allocations.
/// </summary>
internal static class LocaleHelper
{
    /// <summary>
    /// Tries to get locale data with automatic fallback to base locale and fallback locale.
    /// Uses ReadOnlySpan to avoid string allocations for base locale extraction.
    /// </summary>
    /// <param name="ctx">The formatter context.</param>
    /// <param name="dataGetter">Function to extract specific data from locale data.</param>
    /// <param name="hasData">Function to check if data is valid.</param>
    /// <param name="data">The extracted data if found.</param>
    /// <typeparam name="T">The type of data to extract.</typeparam>
    /// <returns>True if valid data was found.</returns>
    public static bool TryGetLocaleData<T>(
        ref FormatterContext ctx,
        Func<ICldrLocaleData, T> dataGetter,
        Func<T, bool> hasData,
        out T data)
    {
        var provider = ctx.CldrDataProvider;
        var locale = ctx.Locale;

        // Try exact locale
        if (provider.TryGetLocaleData(locale, out var localeData) && localeData != null)
        {
            data = dataGetter(localeData);
            if (hasData(data))
            {
                return true;
            }
        }

        // Try base locale (e.g., "en" from "en-US")
        var baseLocale = GetBaseLocale(locale);
        if (baseLocale != null)
        {
            if (provider.TryGetLocaleData(baseLocale, out localeData) && localeData != null)
            {
                data = dataGetter(localeData);
                if (hasData(data))
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
                data = dataGetter(localeData);
                if (hasData(data))
                {
                    return true;
                }
            }
        }

        data = default!;
        return false;
    }

    /// <summary>
    /// Extracts the base locale from a locale string (e.g., "en" from "en-US").
    /// Returns null if no separator is found.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? GetBaseLocale(string locale)
    {
        var span = locale.AsSpan();
        var dashIndex = span.IndexOf('-');
        if (dashIndex < 0)
            dashIndex = span.IndexOf('_');

        if (dashIndex > 0)
        {
            return locale.Substring(0, dashIndex);
        }

        return null;
    }

    /// <summary>
    /// Extracts the region code from a locale string (e.g., "US" from "en-US").
    /// Returns empty span if no separator is found.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> GetRegionSpan(ReadOnlySpan<char> locale)
    {
        var dashIndex = locale.IndexOf('-');
        if (dashIndex < 0)
            dashIndex = locale.IndexOf('_');

        if (dashIndex > 0 && dashIndex < locale.Length - 1)
        {
            return locale.Slice(dashIndex + 1);
        }

        return ReadOnlySpan<char>.Empty;
    }

    /// <summary>
    /// Extracts the region code from a locale string and returns it as uppercase string.
    /// Optimized to avoid intermediate allocations when possible.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetRegionCode(string locale)
    {
        var span = locale.AsSpan();
        var dashIndex = span.IndexOf('-');
        if (dashIndex < 0)
            dashIndex = span.IndexOf('_');

        if (dashIndex <= 0 || dashIndex >= locale.Length - 1)
            return string.Empty;

        var regionStart = dashIndex + 1;
        var regionLength = locale.Length - regionStart;

        // Check if already uppercase to avoid allocation
        var isUppercase = true;
        for (var i = regionStart; i < locale.Length; i++)
        {
            if (char.IsLower(locale[i]))
            {
                isUppercase = false;
                break;
            }
        }

        if (isUppercase)
        {
            return locale.Substring(regionStart);
        }

        // Need to convert to uppercase - use stackalloc for small regions
        Span<char> buffer = stackalloc char[regionLength];
        for (var i = 0; i < regionLength; i++)
        {
            buffer[i] = char.ToUpperInvariant(locale[regionStart + i]);
        }

        return new string(buffer);
    }
}
