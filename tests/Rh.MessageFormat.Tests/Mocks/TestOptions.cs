using System;
using System.Collections.Generic;
using Rh.MessageFormat.Abstractions;
using Rh.MessageFormat.Abstractions.Interfaces;
using Rh.MessageFormat.Exceptions;
using Rh.MessageFormat.Options;

namespace Rh.MessageFormat.Tests.Mocks;

/// <summary>
/// Helper class to create MessageFormatterOptions for testing.
/// </summary>
public static class TestOptions
{
    /// <summary>
    /// Creates options with English locale using mock provider.
    /// </summary>
    public static MessageFormatterOptions WithEnglish()
    {
        return new MessageFormatterOptions
        {
            CldrDataProvider = MockCldrDataProvider.CreateWithEnglish(),
            DefaultFallbackLocale = "en"
        };
    }

    /// <summary>
    /// Creates options with German locale using mock provider.
    /// </summary>
    public static MessageFormatterOptions WithGerman()
    {
        return new MessageFormatterOptions
        {
            CldrDataProvider = new MockCldrDataProvider().WithLocale(MockCldrLocaleData.CreateGerman()),
            DefaultFallbackLocale = "de-DE"
        };
    }

    /// <summary>
    /// Creates options with French locale using mock provider.
    /// </summary>
    public static MessageFormatterOptions WithFrench()
    {
        return new MessageFormatterOptions
        {
            CldrDataProvider = new MockCldrDataProvider().WithLocale(MockCldrLocaleData.CreateFrench()),
            DefaultFallbackLocale = "fr-FR"
        };
    }

    /// <summary>
    /// Creates options with common locales (en, de, fr) using mock provider.
    /// </summary>
    public static MessageFormatterOptions WithCommonLocales(string defaultLocale = "en")
    {
        return new MessageFormatterOptions
        {
            CldrDataProvider = MockCldrDataProvider.CreateWithCommonLocales(),
            DefaultFallbackLocale = defaultLocale
        };
    }

    /// <summary>
    /// Creates options with a custom mock provider.
    /// </summary>
    public static MessageFormatterOptions WithProvider(ICldrDataProvider provider, string defaultLocale = "en")
    {
        return new MessageFormatterOptions
        {
            CldrDataProvider = provider,
            DefaultFallbackLocale = defaultLocale
        };
    }

    /// <summary>
    /// Creates options with a custom mock locale data.
    /// </summary>
    public static MessageFormatterOptions WithLocaleData(MockCldrLocaleData localeData)
    {
        return new MessageFormatterOptions
        {
            CldrDataProvider = new MockCldrDataProvider().WithLocale(localeData),
            DefaultFallbackLocale = localeData.Locale
        };
    }

    /// <summary>
    /// Creates options with multiple custom mock locale data.
    /// </summary>
    public static MessageFormatterOptions WithLocaleData(string defaultLocale, params MockCldrLocaleData[] localeDataArray)
    {
        var provider = new MockCldrDataProvider();
        foreach (var localeData in localeDataArray)
        {
            provider.WithLocale(localeData);
        }
        return new MessageFormatterOptions
        {
            CldrDataProvider = provider,
            DefaultFallbackLocale = defaultLocale
        };
    }

    /// <summary>
    /// Creates options with an empty provider (no CLDR data).
    /// </summary>
    public static MessageFormatterOptions WithEmptyProvider(string? defaultLocale = "en")
    {
        return new MessageFormatterOptions
        {
            CldrDataProvider = MockCldrDataProvider.CreateEmpty(),
            DefaultFallbackLocale = defaultLocale
        };
    }

    /// <summary>
    /// Creates options with English locale and strict validation (no fallback).
    /// Unsupported locales will throw <see cref="InvalidLocaleException"/>.
    /// </summary>
    public static MessageFormatterOptions WithEnglishStrict()
    {
        return new MessageFormatterOptions
        {
            CldrDataProvider = MockCldrDataProvider.CreateWithEnglish(),
            DefaultFallbackLocale = null
        };
    }

    /// <summary>
    /// Creates options with common locales (en, de, fr) and strict validation (no fallback).
    /// Unsupported locales will throw <see cref="InvalidLocaleException"/>.
    /// </summary>
    public static MessageFormatterOptions WithCommonLocalesStrict()
    {
        return new MessageFormatterOptions
        {
            CldrDataProvider = MockCldrDataProvider.CreateWithCommonLocales(),
            DefaultFallbackLocale = null
        };
    }
}
