using Rh.MessageFormat.Abstractions.Models;

namespace Rh.MessageFormat.Abstractions;

/// <summary>
/// Provides access to CLDR data for a specific locale.
/// </summary>
public interface ICldrLocaleData
{
    /// <summary>
    /// Gets the plural category for the given number context.
    /// Compiled to C# if/else chains - no runtime parsing.
    /// </summary>
    /// <param name="ctx">The plural context containing number operands.</param>
    /// <returns>The plural category: "zero", "one", "two", "few", "many", or "other".</returns>
    string GetPluralCategory(PluralContext ctx);

    /// <summary>
    /// Gets the ordinal category for the given number context.
    /// Compiled to C# if/else chains - no runtime parsing.
    /// </summary>
    /// <param name="ctx">The plural context containing number operands.</param>
    /// <returns>The ordinal category: "zero", "one", "two", "few", "many", or "other".</returns>
    string GetOrdinalCategory(PluralContext ctx);

    /// <summary>
    /// Tries to get currency data for the specified currency code.
    /// </summary>
    /// <param name="code">The ISO 4217 currency code (e.g., "USD", "EUR").</param>
    /// <param name="data">The currency data if found.</param>
    /// <returns>True if the currency was found, false otherwise.</returns>
    bool TryGetCurrency(string code, out CurrencyData data);

    /// <summary>
    /// Tries to get unit data for the specified unit identifier.
    /// </summary>
    /// <param name="unitId">The unit identifier (e.g., "length-meter", "duration-hour").</param>
    /// <param name="data">The unit data if found.</param>
    /// <returns>True if the unit was found, false otherwise.</returns>
    bool TryGetUnit(string unitId, out UnitData data);

    /// <summary>
    /// Gets the date/time patterns for this locale.
    /// </summary>
    DatePatternData DatePatterns { get; }

    /// <summary>
    /// Tries to get list pattern data for the specified type.
    /// </summary>
    /// <param name="type">The list type (e.g., "standard", "or", "unit").</param>
    /// <param name="data">The list pattern data if found.</param>
    /// <returns>True if the list pattern was found, false otherwise.</returns>
    bool TryGetListPattern(string type, out ListPatternData data);
}
