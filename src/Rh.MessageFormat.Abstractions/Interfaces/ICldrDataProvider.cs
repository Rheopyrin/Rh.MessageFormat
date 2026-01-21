using System.Collections.Generic;

namespace Rh.MessageFormat.Abstractions.Interfaces;

/// <summary>
/// Provides access to CLDR locale data with lazy loading support.
/// </summary>
public interface ICldrDataProvider
{
    /// <summary>
    /// Gets locale data. Uses Lazy&lt;T&gt; internally - first access triggers JIT compilation.
    /// Very fast (no parsing), just class instantiation.
    /// </summary>
    /// <param name="locale">The locale identifier (e.g., "en", "en-US", "de").</param>
    /// <returns>The locale data, or null if not found.</returns>
    ICldrLocaleData? GetLocaleData(string locale);

    /// <summary>
    /// Tries to get locale data without throwing if not found.
    /// </summary>
    /// <param name="locale">The locale identifier.</param>
    /// <param name="data">The locale data if found.</param>
    /// <returns>True if locale data was found, false otherwise.</returns>
    bool TryGetLocaleData(string locale, out ICldrLocaleData? data);

    /// <summary>
    /// Gets the list of all available locales.
    /// </summary>
    IReadOnlyList<string> AvailableLocales { get; }
}