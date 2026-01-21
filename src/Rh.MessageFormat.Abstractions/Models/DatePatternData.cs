namespace Rh.MessageFormat.Abstractions.Models;

/// <summary>
/// Contains date/time format patterns for a locale.
/// </summary>
public readonly struct DatePatternData
{
    /// <summary>
    /// Date format patterns for different lengths.
    /// </summary>
    public readonly DateFormats Date;

    /// <summary>
    /// Time format patterns for different lengths.
    /// </summary>
    public readonly TimeFormats Time;

    /// <summary>
    /// DateTime combination patterns for different lengths.
    /// </summary>
    public readonly DateTimeFormats DateTime;

    /// <summary>
    /// Creates a new DatePatternData instance.
    /// </summary>
    public DatePatternData(DateFormats date, TimeFormats time, DateTimeFormats dateTime)
    {
        Date = date;
        Time = time;
        DateTime = dateTime;
    }
}

/// <summary>
/// Date format patterns for different lengths.
/// </summary>
public readonly struct DateFormats
{
    /// <summary>Full date pattern (e.g., "EEEE, MMMM d, y").</summary>
    public readonly string Full;
    /// <summary>Long date pattern (e.g., "MMMM d, y").</summary>
    public readonly string Long;
    /// <summary>Medium date pattern (e.g., "MMM d, y").</summary>
    public readonly string Medium;
    /// <summary>Short date pattern (e.g., "M/d/yy").</summary>
    public readonly string Short;

    public DateFormats(string full, string @long, string medium, string @short)
    {
        Full = full;
        Long = @long;
        Medium = medium;
        Short = @short;
    }
}

/// <summary>
/// Time format patterns for different lengths.
/// </summary>
public readonly struct TimeFormats
{
    /// <summary>Full time pattern (e.g., "h:mm:ss a zzzz").</summary>
    public readonly string Full;
    /// <summary>Long time pattern (e.g., "h:mm:ss a z").</summary>
    public readonly string Long;
    /// <summary>Medium time pattern (e.g., "h:mm:ss a").</summary>
    public readonly string Medium;
    /// <summary>Short time pattern (e.g., "h:mm a").</summary>
    public readonly string Short;

    public TimeFormats(string full, string @long, string medium, string @short)
    {
        Full = full;
        Long = @long;
        Medium = medium;
        Short = @short;
    }
}

/// <summary>
/// DateTime combination patterns for different lengths.
/// Uses {0} for time and {1} for date placeholders.
/// </summary>
public readonly struct DateTimeFormats
{
    /// <summary>Full datetime combination pattern.</summary>
    public readonly string Full;
    /// <summary>Long datetime combination pattern.</summary>
    public readonly string Long;
    /// <summary>Medium datetime combination pattern.</summary>
    public readonly string Medium;
    /// <summary>Short datetime combination pattern.</summary>
    public readonly string Short;

    public DateTimeFormats(string full, string @long, string medium, string @short)
    {
        Full = full;
        Long = @long;
        Medium = medium;
        Short = @short;
    }
}
