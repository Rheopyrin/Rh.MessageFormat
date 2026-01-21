using System.Collections.Generic;

namespace Rh.MessageFormat.Exceptions;

/// <summary>
///     Thrown when the requested locale is not supported by the CLDR data provider.
/// </summary>
public class InvalidLocaleException : MessageFormatterException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidLocaleException" /> class.
    /// </summary>
    /// <param name="locale">The locale that was not found.</param>
    /// <param name="availableLocales">The list of available locales.</param>
    public InvalidLocaleException(string locale, IEnumerable<string> availableLocales)
        : base($"The locale '{locale}' is not supported. Available locales: {string.Join(", ", availableLocales)}")
    {
        Locale = locale;
        AvailableLocales = availableLocales;
    }

    /// <summary>
    ///     Gets the locale that was not found.
    /// </summary>
    public string Locale { get; }

    /// <summary>
    ///     Gets the list of available locales.
    /// </summary>
    public IEnumerable<string> AvailableLocales { get; }
}