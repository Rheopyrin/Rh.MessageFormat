using System;
using System.Collections.Generic;
using BitFaster.Caching.Lru;
using Rh.MessageFormat.Abstractions.Interfaces;
using Rh.MessageFormat.Options;

namespace Rh.MessageFormat.Caches;

/// <summary>
///     A cached provider for message formatters that maintains a dictionary of formatters by locale code
///     with LRU eviction.
/// </summary>
public class MessageFormatterCachedProvider : IMessageFormatterProvider
{
    /// <summary>
    ///     The cache of message formatters keyed by locale code.
    /// </summary>
    private readonly ConcurrentLru<string, MessageFormatter> _cache;

    /// <summary>
    ///     The formatter options used to create new formatters.
    /// </summary>
    private readonly IMessageFormatterOptions _options;

    /// <summary>
    ///     The list of locales to pre-initialize (optional).
    /// </summary>
    private readonly IReadOnlyList<string>? _locales;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MessageFormatterCachedProvider" /> class
    ///     with the specified options.
    /// </summary>
    /// <param name="options">
    ///     The formatter options used to create new formatters.
    /// </param>
    /// <param name="capacity">
    ///     The maximum number of formatters to cache. Default is 1024.
    /// </param>
    public MessageFormatterCachedProvider(IMessageFormatterOptions options, int capacity = 1024)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _locales = null;
        _cache = new ConcurrentLru<string, MessageFormatter>(
            concurrencyLevel: Environment.ProcessorCount,
            capacity: capacity,
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MessageFormatterCachedProvider" /> class
    ///     with the specified locales and options.
    /// </summary>
    /// <param name="locales">
    ///     The list of locale codes to pre-initialize during <see cref="Initialize"/>.
    /// </param>
    /// <param name="options">
    ///     The formatter options used to create new formatters.
    /// </param>
    /// <param name="capacity">
    ///     The maximum number of formatters to cache. Default is 1024.
    /// </param>
    public MessageFormatterCachedProvider(IReadOnlyList<string> locales, IMessageFormatterOptions options, int capacity = 1024)
    {
        _locales = locales ?? throw new ArgumentNullException(nameof(locales));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _cache = new ConcurrentLru<string, MessageFormatter>(
            concurrencyLevel: Environment.ProcessorCount,
            capacity: capacity,
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Gets a message formatter for the specified locale code.
    ///     Returns a cached formatter if available, otherwise creates and caches a new one.
    /// </summary>
    /// <param name="localeCode">
    ///     The locale code (e.g., "en", "en-US", "de-DE").
    /// </param>
    /// <returns>
    ///     A message formatter configured for the specified locale.
    /// </returns>
    public IMessageFormatter GetFormatter(string localeCode)
    {
        if (string.IsNullOrEmpty(localeCode))
            throw new ArgumentNullException(nameof(localeCode));

        return _cache.GetOrAdd(localeCode, locale => new MessageFormatter(locale, _options));
    }

    /// <summary>
    ///     Initializes the provider by pre-loading formatters for the configured locales.
    ///     If no locales were specified in the constructor, this method does nothing.
    /// </summary>
    public void Initialize()
    {
        if (_locales == null || _locales.Count == 0)
            return;

        foreach (var locale in _locales)
        {
            if (!string.IsNullOrEmpty(locale))
            {
                _cache.GetOrAdd(locale, l => new MessageFormatter(l, _options));
            }
        }
    }
}