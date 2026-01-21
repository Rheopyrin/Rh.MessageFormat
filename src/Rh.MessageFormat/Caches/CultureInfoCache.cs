using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using Rh.MessageFormat.Abstractions;
using Rh.MessageFormat.Abstractions.Interfaces;
using Rh.MessageFormat.Exceptions;

namespace Rh.MessageFormat.Caches;

/// <summary>
/// Thread-safe cache for CultureInfo instances.
/// </summary>
public sealed class CultureInfoCache : ICultureInfoCache
{
    private readonly ConcurrentDictionary<string, CultureInfo> _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="CultureInfoCache"/> class.
    /// </summary>
    public CultureInfoCache()
    {
        _cache = new ConcurrentDictionary<string, CultureInfo>(StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CultureInfo GetCulture(string locale)
    {
        return _cache.GetOrAdd(locale, CreateCulture);
    }

    private static CultureInfo CreateCulture(string locale)
    {
        try
        {
            return CultureInfo.GetCultureInfo(locale);
        }
        catch (Exception ex)
        {
            throw new MessageFormatterException($"Failed to get culture info for locale '{locale}'", ex);
        }
    }
}