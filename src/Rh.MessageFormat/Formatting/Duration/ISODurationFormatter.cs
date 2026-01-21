using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Rh.MessageFormat.Formatting.Duration;

/// <summary>
/// Parses and formats ISO 8601 duration strings (e.g., "P1Y2M3DT4H5M6S").
/// Can be used as a custom formatter in MessageFormatterOptions.
/// </summary>
/// <remarks>
/// ISO 8601 duration format: P[n]Y[n]M[n]DT[n]H[n]M[n]S
/// where:
/// - P is the duration designator (required)
/// - Y is years, M is months (before T), D is days
/// - T is the time designator (required if H, M, or S follow)
/// - H is hours, M is minutes (after T), S is seconds
///
/// Examples:
/// - P1Y = 1 year
/// - P2M = 2 months
/// - P3D = 3 days
/// - PT4H = 4 hours
/// - PT5M = 5 minutes
/// - PT6S = 6 seconds
/// - P1Y2M3DT4H5M6S = 1 year, 2 months, 3 days, 4 hours, 5 minutes, 6 seconds
/// - P1.5Y = 1.5 years (fractional values supported)
///
/// Usage as a custom formatter:
/// <code>
/// var options = new MessageFormatterOptions();
/// var isoFormatter = new ISODurationFormatter("en");
/// options.CustomFormatters["isoduration"] = isoFormatter.Format;
///
/// // Then use in messages: {duration, isoduration, long} or {duration, isoduration, short}
/// </code>
/// </remarks>
public sealed class ISODurationFormatter
{
    private readonly string _locale;
    private readonly DurationUnitFormatOptions? _defaultOptions;

    // Regex for ISO 8601 duration parsing
    // Matches: P[nY][nM][nW][nD][T[nH][nM][nS]]
    private static readonly Regex ISODurationRegex = new(
        @"^(-)?P(?:(\d+(?:\.\d+)?)[Yy])?(?:(\d+(?:\.\d+)?)[Mm])?(?:(\d+(?:\.\d+)?)[Ww])?(?:(\d+(?:\.\d+)?)[Dd])?(?:T(?:(\d+(?:\.\d+)?)[Hh])?(?:(\d+(?:\.\d+)?)[Mm])?(?:(\d+(?:\.\d+)?)[Ss])?)?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Creates a new ISODurationFormatter with the specified locale.
    /// </summary>
    /// <param name="locale">The locale for formatting.</param>
    /// <param name="defaultOptions">Optional default formatting options.</param>
    public ISODurationFormatter(string locale, DurationUnitFormatOptions? defaultOptions = null)
    {
        _locale = locale;
        _defaultOptions = defaultOptions;
    }

    /// <summary>
    /// Formats an ISO 8601 duration string using the delegate signature for custom formatters.
    /// This method can be registered directly with MessageFormatterOptions.CustomFormatters.
    /// </summary>
    /// <param name="value">The ISO 8601 duration string (e.g., "P1Y2M3D") or numeric seconds.</param>
    /// <param name="style">The format style: "long", "short", "narrow", "timer", or "custom".</param>
    /// <param name="locale">The locale for formatting.</param>
    /// <param name="culture">The culture info for number formatting.</param>
    /// <returns>The formatted duration string.</returns>
    public string Format(object? value, string? style, string locale, CultureInfo culture)
    {
        if (value == null)
            return string.Empty;

        var isoString = value.ToString();
        if (string.IsNullOrEmpty(isoString))
            return string.Empty;

        // If it's already a number (seconds), format directly
        if (double.TryParse(isoString, NumberStyles.Any, CultureInfo.InvariantCulture, out var seconds))
        {
            return FormatSeconds(seconds, style, locale);
        }

        // Parse ISO duration and convert to seconds
        if (!TryParseToSeconds(isoString, out var totalSeconds))
        {
            // Return original value if parsing fails
            return isoString;
        }

        return FormatSeconds(totalSeconds, style, locale);
    }

    /// <summary>
    /// Formats seconds using DurationUnitFormat with the specified style.
    /// </summary>
    private string FormatSeconds(double seconds, string? style, string locale)
    {
        var options = CreateOptions(style);
        var formatter = new DurationUnitFormat(locale, options);
        return formatter.Format(seconds);
    }

    private DurationUnitFormatOptions CreateOptions(string? style)
    {
        var options = new DurationUnitFormatOptions
        {
            Style = style ?? _defaultOptions?.Style ?? DurationUnitFormat.Styles.LONG,
            Format = _defaultOptions?.Format,
            FormatDuration = _defaultOptions?.FormatDuration,
            FormatUnits = _defaultOptions?.FormatUnits,
            Round = _defaultOptions?.Round ?? false
        };

        // Set appropriate format based on style if not specified
        if (options.Format == null)
        {
            options.Format = options.Style switch
            {
                DurationUnitFormat.Styles.TIMER => "{hours}:{minutes}:{seconds}",
                _ => "{years} {months} {days} {hours} {minutes} {seconds}"
            };
        }

        return options;
    }

    /// <summary>
    /// Parses an ISO 8601 duration string and returns the result as a ParsedISODuration.
    /// </summary>
    /// <param name="isoDuration">The ISO 8601 duration string.</param>
    /// <returns>The parsed duration components.</returns>
    /// <exception cref="FormatException">Thrown when the string is not a valid ISO 8601 duration.</exception>
    public static ParsedISODuration Parse(string isoDuration)
    {
        if (!TryParse(isoDuration, out var result))
        {
            throw new FormatException($"Invalid ISO 8601 duration format: '{isoDuration}'");
        }
        return result;
    }

    /// <summary>
    /// Tries to parse an ISO 8601 duration string.
    /// </summary>
    /// <param name="isoDuration">The ISO 8601 duration string.</param>
    /// <param name="result">The parsed duration components if successful.</param>
    /// <returns>True if parsing succeeded, false otherwise.</returns>
    public static bool TryParse(string isoDuration, out ParsedISODuration result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(isoDuration))
            return false;

        var match = ISODurationRegex.Match(isoDuration.Trim());
        if (!match.Success)
            return false;

        var isNegative = match.Groups[1].Success;
        var years = ParseGroup(match.Groups[2]);
        var months = ParseGroup(match.Groups[3]);
        var weeks = ParseGroup(match.Groups[4]);
        var days = ParseGroup(match.Groups[5]);
        var hours = ParseGroup(match.Groups[6]);
        var minutes = ParseGroup(match.Groups[7]);
        var seconds = ParseGroup(match.Groups[8]);

        result = new ParsedISODuration(years, months, weeks, days, hours, minutes, seconds, isNegative);
        return true;
    }

    /// <summary>
    /// Tries to parse an ISO 8601 duration string and convert it to total seconds.
    /// </summary>
    /// <param name="isoDuration">The ISO 8601 duration string.</param>
    /// <param name="totalSeconds">The total duration in seconds if successful.</param>
    /// <returns>True if parsing succeeded, false otherwise.</returns>
    public static bool TryParseToSeconds(string isoDuration, out double totalSeconds)
    {
        if (!TryParse(isoDuration, out var parsed))
        {
            totalSeconds = 0;
            return false;
        }

        totalSeconds = parsed.ToTotalSeconds();
        return true;
    }

    /// <summary>
    /// Converts an ISO 8601 duration string to total seconds.
    /// </summary>
    /// <param name="isoDuration">The ISO 8601 duration string.</param>
    /// <returns>The total duration in seconds.</returns>
    /// <exception cref="FormatException">Thrown when the string is not a valid ISO 8601 duration.</exception>
    public static double ToSeconds(string isoDuration)
    {
        var parsed = Parse(isoDuration);
        return parsed.ToTotalSeconds();
    }

    /// <summary>
    /// Converts an ISO 8601 duration string to a TimeSpan.
    /// Note: Years and months are approximated (365 days/year, 30 days/month).
    /// </summary>
    /// <param name="isoDuration">The ISO 8601 duration string.</param>
    /// <returns>The duration as a TimeSpan.</returns>
    /// <exception cref="FormatException">Thrown when the string is not a valid ISO 8601 duration.</exception>
    public static TimeSpan ToTimeSpan(string isoDuration)
    {
        var seconds = ToSeconds(isoDuration);
        return TimeSpan.FromSeconds(seconds);
    }

    /// <summary>
    /// Creates an ISO 8601 duration string from a TimeSpan.
    /// </summary>
    /// <param name="timeSpan">The time span to convert.</param>
    /// <returns>An ISO 8601 duration string.</returns>
    public static string ToISOString(TimeSpan timeSpan)
    {
        return ToISOString(timeSpan.TotalSeconds);
    }

    /// <summary>
    /// Creates an ISO 8601 duration string from total seconds.
    /// </summary>
    /// <param name="totalSeconds">The total duration in seconds.</param>
    /// <returns>An ISO 8601 duration string.</returns>
    public static string ToISOString(double totalSeconds)
    {
        var isNegative = totalSeconds < 0;
        var remaining = Math.Abs(totalSeconds);

        var days = (int)(remaining / DurationUnitFormat.SecondsIn.DAY);
        remaining %= DurationUnitFormat.SecondsIn.DAY;

        var hours = (int)(remaining / DurationUnitFormat.SecondsIn.HOUR);
        remaining %= DurationUnitFormat.SecondsIn.HOUR;

        var minutes = (int)(remaining / DurationUnitFormat.SecondsIn.MINUTE);
        remaining %= DurationUnitFormat.SecondsIn.MINUTE;

        var seconds = remaining;

        var result = isNegative ? "-P" : "P";
        var hasDatePart = false;
        var hasTimePart = false;

        if (days > 0)
        {
            result += $"{days}D";
            hasDatePart = true;
        }

        if (hours > 0 || minutes > 0 || seconds > 0)
        {
            hasTimePart = true;
            result += "T";

            if (hours > 0)
                result += $"{hours}H";

            if (minutes > 0)
                result += $"{minutes}M";

            if (seconds > 0)
            {
                // Format seconds, avoiding unnecessary decimals
                if (seconds == Math.Floor(seconds))
                    result += $"{(int)seconds}S";
                else
                    result += $"{seconds:F3}S".TrimEnd('0').TrimEnd('.');
            }
        }

        // Handle zero duration
        if (!hasDatePart && !hasTimePart)
            result += "T0S";

        return result;
    }

    private static double ParseGroup(Group group)
    {
        if (!group.Success)
            return 0;

        return double.TryParse(group.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0;
    }
}

/// <summary>
/// Represents a parsed ISO 8601 duration.
/// </summary>
public readonly struct ParsedISODuration
{
    /// <summary>Number of years (approximate: 365 days).</summary>
    public readonly double Years;

    /// <summary>Number of months (approximate: 30 days).</summary>
    public readonly double Months;

    /// <summary>Number of weeks.</summary>
    public readonly double Weeks;

    /// <summary>Number of days.</summary>
    public readonly double Days;

    /// <summary>Number of hours.</summary>
    public readonly double Hours;

    /// <summary>Number of minutes.</summary>
    public readonly double Minutes;

    /// <summary>Number of seconds.</summary>
    public readonly double Seconds;

    /// <summary>Whether the duration is negative.</summary>
    public readonly bool IsNegative;

    /// <summary>
    /// Creates a new ParsedISODuration.
    /// </summary>
    public ParsedISODuration(double years, double months, double weeks, double days, double hours, double minutes, double seconds, bool isNegative = false)
    {
        Years = years;
        Months = months;
        Weeks = weeks;
        Days = days;
        Hours = hours;
        Minutes = minutes;
        Seconds = seconds;
        IsNegative = isNegative;
    }

    /// <summary>
    /// Converts the parsed duration to total seconds.
    /// Note: Years are approximated as 365 days, months as 30 days.
    /// </summary>
    public double ToTotalSeconds()
    {
        var total = Years * DurationUnitFormat.SecondsIn.YEAR
                  + Months * DurationUnitFormat.SecondsIn.MONTH
                  + Weeks * DurationUnitFormat.SecondsIn.DAY * 7
                  + Days * DurationUnitFormat.SecondsIn.DAY
                  + Hours * DurationUnitFormat.SecondsIn.HOUR
                  + Minutes * DurationUnitFormat.SecondsIn.MINUTE
                  + Seconds;

        return IsNegative ? -total : total;
    }

    /// <summary>
    /// Converts the parsed duration to a TimeSpan.
    /// Note: Years and months are approximated (365 days/year, 30 days/month).
    /// </summary>
    public TimeSpan ToTimeSpan()
    {
        return TimeSpan.FromSeconds(ToTotalSeconds());
    }

    /// <summary>
    /// Returns the ISO 8601 string representation.
    /// </summary>
    public override string ToString()
    {
        var result = IsNegative ? "-P" : "P";

        if (Years > 0) result += $"{Years}Y";
        if (Months > 0) result += $"{Months}M";
        if (Weeks > 0) result += $"{Weeks}W";
        if (Days > 0) result += $"{Days}D";

        if (Hours > 0 || Minutes > 0 || Seconds > 0)
        {
            result += "T";
            if (Hours > 0) result += $"{Hours}H";
            if (Minutes > 0) result += $"{Minutes}M";
            if (Seconds > 0) result += $"{Seconds}S";
        }

        // Handle zero duration
        if (result == "P" || result == "-P")
            result = "PT0S";

        return result;
    }
}
