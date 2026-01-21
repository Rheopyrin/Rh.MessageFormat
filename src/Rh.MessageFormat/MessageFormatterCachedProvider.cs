using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Rh.MessageFormat.Abstractions;

namespace Rh.MessageFormat;

/// <summary>
///     A cached provider for message formatters that maintains a dictionary of formatters by locale code.
/// </summary>
public class MessageFormatterCachedProvider : IMessageFormatterProvider
{
    /// <summary>
    ///     The cache of message formatters keyed by locale code.
    /// </summary>
    private static readonly ConcurrentDictionary<string, MessageFormatter> Cache = new(StringComparer.OrdinalIgnoreCase);

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
    public MessageFormatterCachedProvider(IMessageFormatterOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _locales = null;
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
    public MessageFormatterCachedProvider(IReadOnlyList<string> locales, IMessageFormatterOptions options)
    {
        _locales = locales ?? throw new ArgumentNullException(nameof(locales));
        _options = options ?? throw new ArgumentNullException(nameof(options));
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
    public Abstractions.IMessageFormatter GetFormatter(string localeCode)
    {
        if (string.IsNullOrEmpty(localeCode))
            throw new ArgumentNullException(nameof(localeCode));

        return Cache.GetOrAdd(localeCode, locale => new MessageFormatter(locale, _options));
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
                Cache.GetOrAdd(locale, l => new MessageFormatter(l, _options));
            }
        }
    }
}
