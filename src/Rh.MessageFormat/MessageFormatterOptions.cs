using System;
using System.Collections.Generic;
using Rh.MessageFormat.Abstractions;
using Rh.MessageFormat.CldrData.Services;

namespace Rh.MessageFormat;

/// <summary>
/// Options for configuring the MessageFormatter.
/// </summary>
public sealed class MessageFormatterOptions : IMessageFormatterOptions
{
    /// <summary>
    /// Gets the default options with default values.
    /// </summary>
    public static MessageFormatterOptions Default => new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageFormatterOptions"/> class with default values.
    /// </summary>
    public MessageFormatterOptions()
    {
        CldrDataProvider = new CldrDataProvider();
        CultureInfoCache = new CultureInfoCache();
        DefaultFallbackLocale = "en";
        CustomFormatters = new Dictionary<string, CustomFormatterDelegate>(StringComparer.OrdinalIgnoreCase);
        TagHandlers = new Dictionary<string, TagHandler>(StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public ICldrDataProvider CldrDataProvider { get; set; }

    /// <inheritdoc />
    public ICultureInfoCache CultureInfoCache { get; set; }

    /// <inheritdoc />
    public string DefaultFallbackLocale { get; set; }

    /// <inheritdoc />
    public IDictionary<string, CustomFormatterDelegate> CustomFormatters { get; set; }

    /// <inheritdoc />
    public IDictionary<string, TagHandler> TagHandlers { get; set; }

    /// <inheritdoc />
    public bool RequireAllVariables { get; set; }
}
