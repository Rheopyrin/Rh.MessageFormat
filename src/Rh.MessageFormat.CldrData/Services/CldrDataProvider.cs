using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Rh.MessageFormat.Abstractions.Interfaces;
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
}