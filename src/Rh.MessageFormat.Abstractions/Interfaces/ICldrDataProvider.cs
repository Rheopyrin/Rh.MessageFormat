using System.Collections.Generic;
using Rh.MessageFormat.Abstractions.Models;
using Rh.MessageFormat.Formatting.Spellout;

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
    /// Tries to get spellout data (RBNF rules) for a locale.
    /// </summary>
    /// <param name="locale">The locale identifier.</param>
    /// <param name="data">The spellout data if found.</param>
    /// <returns>True if spellout data was found, false otherwise.</returns>
    bool TryGetSpelloutData(string locale, out SpelloutData? data);

    /// <summary>
    /// Tries to get relative time data for a locale.
    /// </summary>
    /// <param name="locale">The locale identifier.</param>
    /// <param name="field">The field (e.g., "year", "month", "day", "hour").</param>
    /// <param name="width">The width (e.g., "long", "short", "narrow").</param>
    /// <param name="data">The relative time data if found.</param>
    /// <returns>True if relative time data was found, false otherwise.</returns>
    bool TryGetRelativeTimeData(string locale, string field, string width, out RelativeTimeData? data);

    /// <summary>
    /// Tries to get list pattern data for a locale.
    /// </summary>
    /// <param name="locale">The locale identifier.</param>
    /// <param name="type">The list type (e.g., "standard", "or", "unit").</param>
    /// <param name="data">The list pattern data if found.</param>
    /// <returns>True if list pattern data was found, false otherwise.</returns>
    bool TryGetListData(string locale, string type, out ListPatternData? data);

    /// <summary>
    /// Tries to get date range/interval data for a locale.
    /// </summary>
    /// <param name="locale">The locale identifier.</param>
    /// <param name="data">The interval format data if found.</param>
    /// <returns>True if interval format data was found, false otherwise.</returns>
    bool TryGetDateRangeData(string locale, out IntervalFormatData? data);

    /// <summary>
    /// Tries to get unit data for a locale.
    /// </summary>
    /// <param name="locale">The locale identifier.</param>
    /// <param name="unitId">The unit identifier (e.g., "length-meter", "duration-hour").</param>
    /// <param name="data">The unit data if found.</param>
    /// <returns>True if unit data was found, false otherwise.</returns>
    bool TryGetUnitData(string locale, string unitId, out UnitData? data);

    /// <summary>
    /// Gets the list of all available locales.
    /// </summary>
    IReadOnlyList<string> AvailableLocales { get; }
}