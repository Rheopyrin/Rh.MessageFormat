using System;
using System.Collections.Generic;
using Rh.MessageFormat.Abstractions.Interfaces;

namespace Rh.MessageFormat.Tests.Mocks;

/// <summary>
/// Mock implementation of ICldrDataProvider for testing.
/// </summary>
public class MockCldrDataProvider : ICldrDataProvider
{
    private readonly Dictionary<string, ICldrLocaleData> _locales = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _availableLocales = new();

    public IReadOnlyList<string> AvailableLocales => _availableLocales;

    /// <summary>
    /// Adds locale data to the provider.
    /// </summary>
    public MockCldrDataProvider WithLocale(string locale, ICldrLocaleData data)
    {
        _locales[locale] = data;
        if (!_availableLocales.Contains(locale))
        {
            _availableLocales.Add(locale);
        }
        return this;
    }

    /// <summary>
    /// Adds locale data using a MockCldrLocaleData instance.
    /// </summary>
    public MockCldrDataProvider WithLocale(MockCldrLocaleData data)
    {
        return WithLocale(data.Locale, data);
    }

    public ICldrLocaleData? GetLocaleData(string locale)
    {
        TryGetLocaleData(locale, out var data);
        return data;
    }

    public bool TryGetLocaleData(string locale, out ICldrLocaleData? data)
    {
        if (_locales.TryGetValue(locale, out var localeData))
        {
            data = localeData;
            return true;
        }

        data = null;
        return false;
    }

    #region Static Factory Methods

    /// <summary>
    /// Creates a provider with English locale data.
    /// </summary>
    public static MockCldrDataProvider CreateWithEnglish()
    {
        return new MockCldrDataProvider()
            .WithLocale(MockCldrLocaleData.CreateEnglish());
    }

    /// <summary>
    /// Creates a provider with English and German locale data.
    /// </summary>
    public static MockCldrDataProvider CreateWithEnglishAndGerman()
    {
        return new MockCldrDataProvider()
            .WithLocale(MockCldrLocaleData.CreateEnglish())
            .WithLocale(MockCldrLocaleData.CreateGerman());
    }

    /// <summary>
    /// Creates a provider with common locales (en, de, fr).
    /// </summary>
    public static MockCldrDataProvider CreateWithCommonLocales()
    {
        return new MockCldrDataProvider()
            .WithLocale(MockCldrLocaleData.CreateEnglish())
            .WithLocale(MockCldrLocaleData.CreateGerman())
            .WithLocale(MockCldrLocaleData.CreateFrench());
    }

    /// <summary>
    /// Creates an empty provider (no locales).
    /// </summary>
    public static MockCldrDataProvider CreateEmpty()
    {
        return new MockCldrDataProvider();
    }

    #endregion
}