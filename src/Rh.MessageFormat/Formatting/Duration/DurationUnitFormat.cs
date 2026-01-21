using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rh.MessageFormat.Formatting.Duration;

/// <summary>
/// Formats duration values (in seconds) to human-readable strings.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// var formatter = new DurationUnitFormat("en", new DurationUnitFormatOptions
/// {
///     Style = DurationUnitFormat.Styles.TIMER,
///     Format = "{hours}:{minutes}:{seconds}"
/// });
/// var result = formatter.Format(3661); // "1:01:01"
/// </code>
/// </remarks>
public sealed class DurationUnitFormat
{
    #region Static Constants

    /// <summary>
    /// Duration unit identifiers.
    /// </summary>
    public static class Units
    {
        /// <summary>Year unit.</summary>
        public const string YEAR = "year";
        /// <summary>Month unit.</summary>
        public const string MONTH = "month";
        /// <summary>Day unit.</summary>
        public const string DAY = "day";
        /// <summary>Hour unit.</summary>
        public const string HOUR = "hour";
        /// <summary>Minute unit.</summary>
        public const string MINUTE = "minute";
        /// <summary>Second unit.</summary>
        public const string SECOND = "second";
    }

    /// <summary>
    /// Duration formatting styles.
    /// </summary>
    public static class Styles
    {
        /// <summary>Custom style with user-defined format templates.</summary>
        public const string CUSTOM = "custom";
        /// <summary>Timer style with zero-padding and colons (e.g., "01:30:45").</summary>
        public const string TIMER = "timer";
        /// <summary>Long style with full unit names (e.g., "1 hour").</summary>
        public const string LONG = "long";
        /// <summary>Short style with abbreviated units (e.g., "1 hr").</summary>
        public const string SHORT = "short";
        /// <summary>Narrow style with minimal units (e.g., "1h").</summary>
        public const string NARROW = "narrow";
    }

    /// <summary>
    /// Custom format style variants (used with FormattedDurationService).
    /// </summary>
    public static class CustomStyles
    {
        /// <summary>Custom long format with context-aware labels (e.g., "Daily", "Weekly").</summary>
        public const string CUSTOM_LONG = "custom-long";
        /// <summary>Custom short format with abbreviated units.</summary>
        public const string CUSTOM_SHORT = "custom-short";
        /// <summary>Hyphenated format (e.g., "2-days").</summary>
        public const string CUSTOM_HYPHENATED = "custom-hyphenated";
        /// <summary>Context-aware hyphenated format (e.g., "daily", "weekly").</summary>
        public const string CUSTOM_CONTEXT_HYPHENATED = "custom-context-hyphenated";
    }

    /// <summary>
    /// Seconds per unit for conversion.
    /// </summary>
    public static class SecondsIn
    {
        /// <summary>Seconds in a year (365 days).</summary>
        public const int YEAR = 365 * 24 * 60 * 60;
        /// <summary>Seconds in a month (30 days).</summary>
        public const int MONTH = 30 * 24 * 60 * 60;
        /// <summary>Seconds in a day.</summary>
        public const int DAY = 24 * 60 * 60;
        /// <summary>Seconds in an hour.</summary>
        public const int HOUR = 60 * 60;
        /// <summary>Seconds in a minute.</summary>
        public const int MINUTE = 60;
        /// <summary>Seconds in a second.</summary>
        public const int SECOND = 1;

        internal static int Get(string unit) => unit switch
        {
            Units.YEAR => YEAR,
            Units.MONTH => MONTH,
            Units.DAY => DAY,
            Units.HOUR => HOUR,
            Units.MINUTE => MINUTE,
            Units.SECOND => SECOND,
            _ => throw new ArgumentException($"Unknown unit: {unit}")
        };
    }

    #endregion

    #region Private Fields

    private static readonly Regex SplitPointsRegex = new(@"(\{value\}|\{unit\})", RegexOptions.Compiled);
    private static readonly Regex FormatPlaceholderRegex = new(@"\{(seconds?|minutes?|hours?|days?|months?|years?)\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly string[] UnitOrder = { Units.YEAR, Units.MONTH, Units.DAY, Units.HOUR, Units.MINUTE, Units.SECOND };
    private static readonly string[] UnitOrderReversed = { Units.SECOND, Units.MINUTE, Units.HOUR, Units.DAY, Units.MONTH, Units.YEAR };

    private static readonly Dictionary<string, string> DefaultFormatUnits = new()
    {
        [Units.YEAR] = "{value, plural, one {year} other {years}}",
        [Units.MONTH] = "{value, plural, one {month} other {months}}",
        [Units.DAY] = "{value, plural, one {day} other {days}}",
        [Units.HOUR] = "{value, plural, one {hour} other {hours}}",
        [Units.MINUTE] = "{value, plural, one {minute} other {minutes}}",
        [Units.SECOND] = "{value, plural, one {second} other {seconds}}"
    };

    private readonly string _locale;
    private readonly CultureInfo _culture;
    private readonly string _style;
    private readonly bool _isTimer;
    private readonly string _format;
    private readonly Dictionary<string, string> _formatUnits;
    private readonly string _formatDuration;
    private readonly bool _shouldRound;
    private readonly MessageFormatter? _messageFormatter;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new DurationUnitFormat instance.
    /// </summary>
    /// <param name="locale">The locale for formatting (e.g., "en", "de-DE").</param>
    /// <param name="options">Optional formatting options.</param>
    public DurationUnitFormat(string locale, DurationUnitFormatOptions? options = null)
    {
        options ??= new DurationUnitFormatOptions();

        _locale = locale;
        _culture = GetCultureInfo(locale);
        _style = options.Style ?? Styles.LONG;
        _isTimer = _style == Styles.TIMER;
        _format = options.Format ?? (_isTimer ? "{minutes}:{seconds}" : "{seconds}");
        _formatUnits = options.FormatUnits ?? new Dictionary<string, string>(DefaultFormatUnits);
        _formatDuration = options.FormatDuration ?? "{value} {unit}";
        _shouldRound = options.Round;

        // Create a MessageFormatter for CUSTOM style ICU message processing
        if (_style == Styles.CUSTOM || !IsSpecialStyle(_style))
        {
            _messageFormatter = new MessageFormatter(_locale);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Formats a duration value (in seconds) to a string.
    /// </summary>
    /// <param name="value">The duration in seconds.</param>
    /// <returns>The formatted duration string.</returns>
    public string Format(double value)
    {
        return string.Join("", FormatToParts(value).Select(p => p.Value));
    }

    /// <summary>
    /// Formats a duration value (in seconds) to an array of parts.
    /// </summary>
    /// <param name="value">The duration in seconds.</param>
    /// <returns>An array of duration format parts.</returns>
    public DurationFormatPart[] FormatToParts(double value)
    {
        // Extract which units are used in the format template
        var usedUnits = GetUsedUnits(_format);

        // Compute the value of each bucket depending on which parts are used
        var buckets = SplitSecondsInBuckets(value, usedUnits);

        // Process each placeholder in the format
        var result = new List<DurationFormatPart>();
        var lastIndex = 0;

        foreach (Match match in FormatPlaceholderRegex.Matches(_format))
        {
            // Add literal text before the placeholder
            if (match.Index > lastIndex)
            {
                var literal = _format.Substring(lastIndex, match.Index - lastIndex);
                if (!string.IsNullOrEmpty(literal))
                {
                    result.Add(new DurationFormatPart(DurationFormatPartType.Literal, literal));
                }
            }

            var placeholder = match.Value;
            var unit = NormalizeUnitFromPlaceholder(placeholder);
            var number = buckets.GetValueOrDefault(unit, 0);

            // For non-timer style, skip zero values
            if (number != 0 || _isTimer)
            {
                result.AddRange(FormatDurationToParts(unit, number));
            }

            lastIndex = match.Index + match.Length;
        }

        // Add any remaining literal text
        if (lastIndex < _format.Length)
        {
            var literal = _format.Substring(lastIndex);
            if (!string.IsNullOrEmpty(literal))
            {
                result.Add(new DurationFormatPart(DurationFormatPartType.Literal, literal));
            }
        }

        return TrimOutput(result, usedUnits);
    }

    #endregion

    #region Private Methods

    private HashSet<string> GetUsedUnits(string format)
    {
        var units = new HashSet<string>();
        foreach (Match match in FormatPlaceholderRegex.Matches(format))
        {
            var unit = NormalizeUnitFromPlaceholder(match.Value);
            units.Add(unit);
        }
        return units;
    }

    private static string NormalizeUnitFromPlaceholder(string placeholder)
    {
        // Remove braces and normalize singular/plural
        var inner = placeholder.Trim('{', '}').ToLowerInvariant();
        return inner switch
        {
            "second" or "seconds" => Units.SECOND,
            "minute" or "minutes" => Units.MINUTE,
            "hour" or "hours" => Units.HOUR,
            "day" or "days" => Units.DAY,
            "month" or "months" => Units.MONTH,
            "year" or "years" => Units.YEAR,
            _ => inner
        };
    }

    private Dictionary<string, long> SplitSecondsInBuckets(double value, HashSet<string> usedUnits)
    {
        var seconds = value;

        // Rounding will only affect the lowest unit
        if (_shouldRound)
        {
            var lowestUnit = UnitOrderReversed.FirstOrDefault(u => usedUnits.Contains(u));
            if (lowestUnit != null)
            {
                var unitSeconds = SecondsIn.Get(lowestUnit);
                var remainder = seconds % unitSeconds;
                if (2 * remainder >= unitSeconds)
                {
                    seconds += unitSeconds - remainder;
                }
            }
        }

        var buckets = new Dictionary<string, long>();

        foreach (var unit in UnitOrder)
        {
            if (usedUnits.Contains(unit))
            {
                var unitSeconds = SecondsIn.Get(unit);
                buckets[unit] = (long)(seconds / unitSeconds);
                seconds %= unitSeconds;
            }
        }

        return buckets;
    }

    private DurationFormatPart[] FormatDurationToParts(string unit, long number)
    {
        if (_isTimer)
        {
            // Timer style: only show zero-padded value
            return new[] { new DurationFormatPart(unit, FormatValue(number)) };
        }

        if (IsSpecialStyle(_style))
        {
            // LONG/SHORT/NARROW: use .NET NumberFormat with unit display
            return FormatWithUnitDisplay(unit, number);
        }

        // CUSTOM style: check if formatUnits contains ICU message (complete output) or simple unit name
        if (_formatUnits.TryGetValue(unit, out var messagePattern) && IsIcuMessagePattern(messagePattern))
        {
            // ICU message pattern - produces complete formatted output
            var formattedUnit = FormatUnitName(unit, number);
            return new[] { new DurationFormatPart(unit, formattedUnit) };
        }

        // Use formatDuration template with {value} and {unit}
        var result = new List<DurationFormatPart>();
        var parts = SplitPointsRegex.Split(_formatDuration);

        foreach (var part in parts)
        {
            if (part == "{value}")
            {
                result.Add(new DurationFormatPart(unit, FormatValue(number)));
            }
            else if (part == "{unit}")
            {
                var unitText = FormatUnitName(unit, number);
                result.Add(new DurationFormatPart(DurationFormatPartType.Unit, unitText));
            }
            else if (!string.IsNullOrEmpty(part))
            {
                result.Add(new DurationFormatPart(DurationFormatPartType.Literal, part));
            }
        }

        return result.ToArray();
    }

    private DurationFormatPart[] FormatWithUnitDisplay(string unit, long number)
    {
        // Map our units to .NET unit format names
        var dotNetUnit = unit switch
        {
            Units.YEAR => "year",
            Units.MONTH => "month",
            Units.DAY => "day",
            Units.HOUR => "hour",
            Units.MINUTE => "minute",
            Units.SECOND => "second",
            _ => unit
        };

        // Get the appropriate unit name based on style
        var unitName = GetLocalizedUnitName(dotNetUnit, number, _style);

        var result = new List<DurationFormatPart>();

        // Format: "value unit" (e.g., "1 hour", "2 hours")
        result.Add(new DurationFormatPart(unit, number.ToString(_culture)));
        result.Add(new DurationFormatPart(DurationFormatPartType.Literal, " "));
        result.Add(new DurationFormatPart(DurationFormatPartType.Unit, unitName));

        return result.ToArray();
    }

    private string GetLocalizedUnitName(string unit, long value, string style)
    {
        // Use CLDR-like patterns based on style
        var isPlural = value != 1;

        return style switch
        {
            Styles.NARROW => unit switch
            {
                "year" => "y",
                "month" => "m",
                "day" => "d",
                "hour" => "h",
                "minute" => "m",
                "second" => "s",
                _ => unit
            },
            Styles.SHORT => unit switch
            {
                "year" => isPlural ? "yrs" : "yr",
                "month" => isPlural ? "mos" : "mo",
                "day" => isPlural ? "days" : "day",
                "hour" => isPlural ? "hrs" : "hr",
                "minute" => isPlural ? "mins" : "min",
                "second" => isPlural ? "secs" : "sec",
                _ => unit
            },
            _ => unit switch // LONG
            {
                "year" => isPlural ? "years" : "year",
                "month" => isPlural ? "months" : "month",
                "day" => isPlural ? "days" : "day",
                "hour" => isPlural ? "hours" : "hour",
                "minute" => isPlural ? "minutes" : "minute",
                "second" => isPlural ? "seconds" : "second",
                _ => unit
            }
        };
    }

    private string FormatUnitName(string unit, long number)
    {
        if (_formatUnits.TryGetValue(unit, out var messagePattern))
        {
            // Use ICU MessageFormat for unit names
            if (_messageFormatter != null)
            {
                try
                {
                    return _messageFormatter.FormatMessage(messagePattern, new Dictionary<string, object?> { ["value"] = number });
                }
                catch
                {
                    // Fallback if message formatting fails
                }
            }
        }

        // Fallback to default plural
        return number == 1 ? unit : unit + "s";
    }

    private string FormatValue(long number)
    {
        return _isTimer ? number.ToString().PadLeft(2, '0') : number.ToString();
    }

    private DurationFormatPart[] TrimOutput(List<DurationFormatPart> result, HashSet<string> usedUnits)
    {
        var trimmed = Trim(result, _isTimer);

        // If everything cancels out and there are only literals, return 0 on the lowest available unit
        if (!trimmed.Any(p => p.Type != DurationFormatPartType.Literal))
        {
            var minUnit = UnitOrderReversed.FirstOrDefault(u => usedUnits.Contains(u));
            if (minUnit != null)
            {
                return FormatDurationToParts(minUnit, 0);
            }
        }

        return trimmed;
    }

    private static DurationFormatPart[] Trim(List<DurationFormatPart> parts, bool trimFirstPaddedValue)
    {
        // Left trim: remove leading empty literals
        var leftTrimmed = LeftTrim(parts);

        // Right trim: reverse, left trim, reverse back
        var rightTrimmed = LeftTrim(leftTrimmed.AsEnumerable().Reverse().ToList());
        rightTrimmed.Reverse();

        // Remove leading zero from first value in timer mode, but preserve at least one character
        if (trimFirstPaddedValue && rightTrimmed.Count > 0)
        {
            var firstValueIndex = rightTrimmed.FindIndex(p => p.Type != DurationFormatPartType.Literal);
            if (firstValueIndex >= 0)
            {
                var first = rightTrimmed[firstValueIndex];
                if (first.Value.StartsWith("0") && first.Value.Length > 1)
                {
                    var trimmed = first.Value.TrimStart('0');
                    // Preserve at least one character (e.g., "00" -> "0", not "")
                    if (string.IsNullOrEmpty(trimmed))
                    {
                        trimmed = "0";
                    }
                    rightTrimmed[firstValueIndex] = new DurationFormatPart(first.Type, trimmed);
                }
            }
        }

        return rightTrimmed.ToArray();
    }

    private static List<DurationFormatPart> LeftTrim(List<DurationFormatPart> parts)
    {
        var result = new List<DurationFormatPart>();
        var previousEmpty = true;

        foreach (var part in parts)
        {
            if (part.Type == DurationFormatPartType.Literal && string.IsNullOrWhiteSpace(part.Value))
            {
                if (previousEmpty) continue;
                previousEmpty = true;
                result.Add(part);
            }
            else
            {
                previousEmpty = false;
                result.Add(part);
            }
        }

        return result;
    }

    private static bool IsSpecialStyle(string style)
    {
        return style == Styles.LONG || style == Styles.SHORT || style == Styles.NARROW;
    }

    private static bool IsIcuMessagePattern(string pattern)
    {
        // Check if the pattern is an ICU message that embeds the number directly in output.
        // Patterns containing '#' within plural branches embed the number:
        // e.g., "{value, plural, =1 {Daily} other {# Days}}" -> "Daily" or "3 Days"
        // Default patterns like "{value, plural, one {hour} other {hours}}" produce just unit names
        // and should NOT be treated as complete output.

        // Look for # within braces (plural branch output) which indicates number embedding
        var inBrace = false;
        var braceDepth = 0;

        foreach (var c in pattern)
        {
            if (c == '{')
            {
                braceDepth++;
                if (braceDepth > 1) inBrace = true; // Inside a plural branch
            }
            else if (c == '}')
            {
                braceDepth--;
                if (braceDepth <= 1) inBrace = false;
            }
            else if (c == '#' && inBrace)
            {
                return true; // Found # in a plural branch
            }
        }

        return false;
    }

    private static CultureInfo GetCultureInfo(string locale)
    {
        try
        {
            return CultureInfo.GetCultureInfo(locale.Replace('_', '-'));
        }
        catch
        {
            return CultureInfo.InvariantCulture;
        }
    }

    #endregion
}

/// <summary>
/// Options for DurationUnitFormat.
/// </summary>
public sealed class DurationUnitFormatOptions
{
    /// <summary>
    /// The formatting style. Default is "long".
    /// Values: "custom", "timer", "long", "short", "narrow"
    /// </summary>
    public string? Style { get; set; }

    /// <summary>
    /// The format template specifying which units to display.
    /// Uses placeholders: {seconds}, {minutes}, {hours}, {days}, {months}, {years}
    /// Default: "{seconds}" for normal styles, "{minutes}:{seconds}" for timer style.
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Template for formatting each duration part. Default: "{value} {unit}"
    /// Use {value} for the number and {unit} for the unit name.
    /// </summary>
    public string? FormatDuration { get; set; }

    /// <summary>
    /// Custom ICU MessageFormat templates for each unit.
    /// Keys are unit names (year, month, day, hour, minute, second).
    /// Values are ICU MessageFormat patterns with {value} placeholder.
    /// Example: { ["day"] = "{value, plural, =1 {Daily} =7 {Weekly} other {# Days}}" }
    /// </summary>
    public Dictionary<string, string>? FormatUnits { get; set; }

    /// <summary>
    /// Whether to round to the lowest displayed unit. Default: false.
    /// When true, if remainder >= 50% of the lowest unit, rounds up.
    /// </summary>
    public bool Round { get; set; }
}

/// <summary>
/// Represents a part of a formatted duration.
/// </summary>
public readonly struct DurationFormatPart
{
    /// <summary>
    /// The type of this part (unit name like "hour", "minute", or "literal" or "unit").
    /// </summary>
    public readonly string Type;

    /// <summary>
    /// The formatted string value.
    /// </summary>
    public readonly string Value;

    /// <summary>
    /// Creates a new DurationFormatPart.
    /// </summary>
    public DurationFormatPart(string type, string value)
    {
        Type = type;
        Value = value;
    }

    /// <inheritdoc />
    public override string ToString() => $"{{ type: '{Type}', value: '{Value}' }}";
}

/// <summary>
/// Standard part types for duration formatting.
/// </summary>
public static class DurationFormatPartType
{
    /// <summary>Literal text (separators, spaces).</summary>
    public const string Literal = "literal";
    /// <summary>Unit label text.</summary>
    public const string Unit = "unit";
}