using Rh.MessageFormat.Abstractions.Models;

namespace Rh.MessageFormat.Abstractions.Interfaces;

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
    /// Gets the quarter format patterns for this locale.
    /// Returns empty data by default for backward compatibility.
    /// </summary>
    QuarterData Quarters => default;

    /// <summary>
    /// Gets the week configuration for this locale/region.
    /// Returns empty data by default for backward compatibility.
    /// </summary>
    WeekData WeekInfo => default;

    /// <summary>
    /// Tries to get list pattern data for the specified type.
    /// </summary>
    /// <param name="type">The list type (e.g., "standard", "or", "unit").</param>
    /// <param name="data">The list pattern data if found.</param>
    /// <returns>True if the list pattern was found, false otherwise.</returns>
    bool TryGetListPattern(string type, out ListPatternData data);

    /// <summary>
    /// Tries to get relative time data for the specified field and width.
    /// </summary>
    /// <param name="field">The field (e.g., "year", "month", "day", "hour", "minute", "second").</param>
    /// <param name="width">The width: "long" (default), "short", or "narrow".</param>
    /// <param name="data">The relative time data if found.</param>
    /// <returns>True if the relative time data was found, false otherwise.</returns>
    bool TryGetRelativeTime(string field, string width, out RelativeTimeData data);

    /// <summary>
    /// Gets the interval format data for this locale.
    /// Returns default data for backward compatibility.
    /// </summary>
    IntervalFormatData IntervalFormats => default;

    /// <summary>
    /// Tries to get an interval pattern for the specified skeleton and greatest difference field.
    /// </summary>
    /// <param name="skeleton">The date/time skeleton (e.g., "yMMMd").</param>
    /// <param name="greatestDiff">The greatest difference field character ('y', 'M', 'd', 'H', 'm').</param>
    /// <param name="pattern">The interval pattern if found.</param>
    /// <returns>True if the pattern was found, false otherwise.</returns>
    bool TryGetIntervalPattern(string skeleton, char greatestDiff, out string pattern)
    {
        var data = IntervalFormats;
        if (data.Skeletons != null &&
            data.Skeletons.TryGetValue(skeleton, out var patterns) &&
            patterns.TryGetPattern(greatestDiff, out pattern))
        {
            return true;
        }
        pattern = string.Empty;
        return false;
    }
}