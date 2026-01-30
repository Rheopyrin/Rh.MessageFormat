using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Rh.MessageFormat.Abstractions.Interfaces;
using Rh.MessageFormat.Abstractions.Models;
using Rh.MessageFormat.Formatting.Spellout;

namespace Rh.MessageFormat.CldrData.Services;

/// <summary>
/// Provides access to CLDR locale data with lazy loading.
/// Each locale class is only JIT compiled when first accessed.
/// </summary>
public sealed partial class CldrDataProvider : ICldrDataProvider
{
    // The _locales dictionary is defined in the generated partial class (CldrDataProvider.g.cs)
    // Each locale maps to a lazy singleton - thread safety handled by Lazy<T> inside each class

    private IReadOnlyList<string>? _availableLocales;

    /// <summary>
    /// Static delegate for spellout data lookup. Set by Rh.MessageFormat.CldrData.Spellout package when loaded.
    /// </summary>
    public static Func<string, SpelloutData?>? SpelloutDataProvider { get; set; }

    /// <summary>
    /// Static delegate for relative time data lookup. Set by Rh.MessageFormat.CldrData.RelativeTime package when loaded.
    /// </summary>
    public static Func<string, string, string, RelativeTimeData?>? RelativeTimeDataProvider { get; set; }

    /// <summary>
    /// Static delegate for list pattern data lookup. Set by Rh.MessageFormat.CldrData.Lists package when loaded.
    /// </summary>
    public static Func<string, string, ListPatternData?>? ListDataProvider { get; set; }

    /// <summary>
    /// Static delegate for date range/interval data lookup. Set by Rh.MessageFormat.CldrData.DateRange package when loaded.
    /// </summary>
    public static Func<string, IntervalFormatData?>? DateRangeDataProvider { get; set; }

    /// <summary>
    /// Static delegate for unit data lookup. Set by Rh.MessageFormat.CldrData.Units package when loaded.
    /// </summary>
    public static Func<string, string, UnitData?>? UnitDataProvider { get; set; }

    /// <inheritdoc />
    public ICldrLocaleData? GetLocaleData(string locale)
    {
        if (string.IsNullOrEmpty(locale))
            return null;

        return _locales.TryGetValue(locale, out var factory) ? factory() : null;
    }

    /// <inheritdoc />
    public bool TryGetLocaleData(string locale, out ICldrLocaleData? data)
    {
        if (string.IsNullOrEmpty(locale))
        {
            data = null;
            return false;
        }

        if (_locales.TryGetValue(locale, out var factory))
        {
            data = factory();
            return true;
        }

        data = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetSpelloutData(string locale, out SpelloutData? data)
    {
        if (string.IsNullOrEmpty(locale))
        {
            data = null;
            return false;
        }

        // Use the registered spellout provider if available
        var provider = SpelloutDataProvider;
        if (provider != null)
        {
            data = provider(locale);
            return data != null;
        }

        data = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetRelativeTimeData(string locale, string field, string width, out RelativeTimeData? data)
    {
        if (string.IsNullOrEmpty(locale) || string.IsNullOrEmpty(field))
        {
            data = null;
            return false;
        }

        // Use the registered relative time provider if available
        var provider = RelativeTimeDataProvider;
        if (provider != null)
        {
            data = provider(locale, field, width ?? "long");
            return data != null;
        }

        data = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetListData(string locale, string type, out ListPatternData? data)
    {
        if (string.IsNullOrEmpty(locale))
        {
            data = null;
            return false;
        }

        // Use the registered list provider if available
        var provider = ListDataProvider;
        if (provider != null)
        {
            data = provider(locale, type ?? "standard");
            return data != null;
        }

        data = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetDateRangeData(string locale, out IntervalFormatData? data)
    {
        if (string.IsNullOrEmpty(locale))
        {
            data = null;
            return false;
        }

        // Use the registered date range provider if available
        var provider = DateRangeDataProvider;
        if (provider != null)
        {
            data = provider(locale);
            return data != null;
        }

        data = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetUnitData(string locale, string unitId, out UnitData? data)
    {
        if (string.IsNullOrEmpty(locale) || string.IsNullOrEmpty(unitId))
        {
            data = null;
            return false;
        }

        // Use the registered unit provider if available
        var provider = UnitDataProvider;
        if (provider != null)
        {
            data = provider(locale, unitId);
            return data != null;
        }

        data = null;
        return false;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> AvailableLocales
    {
        get
        {
            if (_availableLocales != null)
                return _availableLocales;

            var locales = _locales.Keys.ToList().AsReadOnly();
            Interlocked.CompareExchange(ref _availableLocales, locales, null);
            return _availableLocales;
        }
    }

    /// <inheritdoc />
    public bool TryGetNumberSystemDigits(string numberingSystem, out string digits)
    {
        if (string.IsNullOrEmpty(numberingSystem))
        {
            digits = string.Empty;
            return false;
        }

        return Generated.NumberSystems.TryGetDigits(numberingSystem, out digits!);
    }
}