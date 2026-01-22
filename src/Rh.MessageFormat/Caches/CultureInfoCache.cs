using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using BitFaster.Caching.Lru;
using Rh.MessageFormat.Abstractions.Interfaces;
using Rh.MessageFormat.Exceptions;

namespace Rh.MessageFormat.Caches;

/// <summary>
/// Thread-safe cache for CultureInfo instances with LRU eviction.
/// </summary>
public sealed class CultureInfoCache : ICultureInfoCache
{
    private readonly ConcurrentLru<string, CultureInfo> _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="CultureInfoCache"/> class.
    /// </summary>
    /// <param name="capacity">The maximum number of CultureInfo instances to cache. Default is 1024.</param>
    public CultureInfoCache(int capacity = 1024)
    {
        _cache = new ConcurrentLru<string, CultureInfo>(
            concurrencyLevel: Environment.ProcessorCount,
            capacity: capacity,
            StringComparer.OrdinalIgnoreCase);
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