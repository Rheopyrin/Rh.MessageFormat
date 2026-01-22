using System.Collections.Generic;
using Rh.MessageFormat.Abstractions.Interfaces;
using Rh.MessageFormat.Custom;

namespace Rh.MessageFormat.Options;

/// <summary>
/// Options for configuring the MessageFormatter.
/// </summary>
public interface IMessageFormatterOptions
{
    /// <summary>
    /// Gets the CLDR data provider for locale data.
    /// </summary>
    ICldrDataProvider CldrDataProvider { get; }

    /// <summary>
    /// Gets the culture info cache.
    /// </summary>
    ICultureInfoCache CultureInfoCache { get; }

    /// <summary>
    /// Gets the default fallback locale when requested locale is not found.
    /// When null, an exception is thrown for unsupported locales.
    /// </summary>
    string? DefaultFallbackLocale { get; }

    /// <summary>
    /// Gets the custom formatters registered by the consumer.
    /// </summary>
    IDictionary<string, CustomFormatterDelegate> CustomFormatters { get; }

    /// <summary>
    /// Gets the tag handlers registered by the consumer for rich text formatting.
    /// </summary>
    IDictionary<string, TagHandler> TagHandlers { get; }

    /// <summary>
    /// Gets a value indicating whether to throw an exception when a variable
    /// referenced in the message pattern is not found in the provided arguments.
    /// When false (default), missing variables are treated as empty strings.
    /// </summary>
    bool RequireAllVariables { get; }

    /// <summary>
    /// Gets the parser cache options for caching parsed messages.
    /// </summary>
    ParserCacheOptions ParserCache { get; }
}