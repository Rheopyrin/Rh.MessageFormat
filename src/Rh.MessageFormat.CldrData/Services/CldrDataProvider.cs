using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rh.MessageFormat.Abstractions;

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
    public IReadOnlyList<string> AvailableLocales
    {
        get
        {
            if (_availableLocales == null)
            {
                _availableLocales = _locales.Keys.ToList().AsReadOnly();
            }
            return _availableLocales;
        }
    }
}