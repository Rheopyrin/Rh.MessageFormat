using System;
using System.Collections.Generic;
using Rh.MessageFormat.Abstractions.Interfaces;
using Rh.MessageFormat.Caches;
using Rh.MessageFormat.CldrData.Services;
using Rh.MessageFormat.Custom;

namespace Rh.MessageFormat.Options;

/// <summary>
/// Options for configuring the MessageFormatter.
/// </summary>
public sealed class MessageFormatterOptions : IMessageFormatterOptions
{
    private static readonly MessageFormatterOptions _default = new();

    /// <summary>
    /// Gets the default options instance (singleton).
    /// Do not modify this instance; create a new <see cref="MessageFormatterOptions"/> for custom configuration.
    /// </summary>
    public static MessageFormatterOptions Default => _default;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageFormatterOptions"/> class with default values.
    /// </summary>
    public MessageFormatterOptions()
    {
        CldrDataProvider = new CldrDataProvider();
        CultureInfoCache = new CultureInfoCache();
        DefaultFallbackLocale = null;
        CustomFormatters = new Dictionary<string, CustomFormatterDelegate>(StringComparer.OrdinalIgnoreCase);
        TagHandlers = new Dictionary<string, TagHandler>(StringComparer.OrdinalIgnoreCase);
        ParserCache = new ParserCacheOptions();
    }

    /// <inheritdoc />
    public ICldrDataProvider CldrDataProvider { get; set; }

    /// <inheritdoc />
    public ICultureInfoCache CultureInfoCache { get; set; }

    /// <inheritdoc />
    public string? DefaultFallbackLocale { get; set; }

    /// <inheritdoc />
    public IDictionary<string, CustomFormatterDelegate> CustomFormatters { get; set; }

    /// <inheritdoc />
    public IDictionary<string, TagHandler> TagHandlers { get; set; }

    /// <inheritdoc />
    public bool RequireAllVariables { get; set; }

    /// <inheritdoc />
    public ParserCacheOptions ParserCache { get; set; }

    /// <inheritdoc />
    public bool IgnoreTag { get; set; }
}