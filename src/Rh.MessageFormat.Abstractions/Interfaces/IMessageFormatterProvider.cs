namespace Rh.MessageFormat.Abstractions.Interfaces;

/// <summary>
///     Provides access to message formatters for different locales.
/// </summary>
public interface IMessageFormatterProvider
{
    /// <summary>
    ///     Gets a message formatter for the specified locale code.
    /// </summary>
    /// <param name="localeCode">
    ///     The locale code (e.g., "en", "en-US", "de-DE").
    /// </param>
    /// <returns>
    ///     A message formatter configured for the specified locale.
    /// </returns>
    IMessageFormatter GetFormatter(string localeCode);

    /// <summary>
    ///     Initializes the provider by pre-loading formatters for configured locales.
    /// </summary>
    void Initialize();
}
