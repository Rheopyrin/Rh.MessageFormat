using System.Globalization;

namespace Rh.MessageFormat.Abstractions;

/// <summary>
/// Provides cached access to CultureInfo instances.
/// </summary>
public interface ICultureInfoCache
{
    /// <summary>
    /// Gets the CultureInfo for the specified locale, caching the result for future calls.
    /// </summary>
    /// <param name="locale">The locale identifier (e.g., "en", "en-US", "de").</param>
    /// <returns>The CultureInfo for the locale, or InvariantCulture if the locale is invalid.</returns>
    CultureInfo GetCulture(string locale);
}
