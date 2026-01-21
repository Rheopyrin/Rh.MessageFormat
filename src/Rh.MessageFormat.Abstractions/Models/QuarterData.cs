namespace Rh.MessageFormat.Abstractions.Models;

/// <summary>
/// Contains quarter format patterns for a locale.
/// </summary>
public readonly struct QuarterData
{
    /// <summary>
    /// Format context quarter patterns (used in formatting dates).
    /// </summary>
    public readonly QuarterFormats Format;

    /// <summary>
    /// Stand-alone quarter patterns (used when quarter appears alone).
    /// </summary>
    public readonly QuarterFormats StandAlone;

    /// <summary>
    /// Creates a new QuarterData instance.
    /// </summary>
    public QuarterData(QuarterFormats format, QuarterFormats standAlone)
    {
        Format = format;
        StandAlone = standAlone;
    }

    /// <summary>
    /// Indicates whether this data has any quarter patterns.
    /// </summary>
    public bool HasData => Format.HasData || StandAlone.HasData;
}

/// <summary>
/// Quarter format patterns for different widths.
/// Quarters are 0-indexed: [0]=Q1, [1]=Q2, [2]=Q3, [3]=Q4.
/// </summary>
public readonly struct QuarterFormats
{
    /// <summary>
    /// Abbreviated quarter names (e.g., "Q1", "Q2", "Q3", "Q4").
    /// </summary>
    public readonly string[] Abbreviated;

    /// <summary>
    /// Wide quarter names (e.g., "1st quarter", "2nd quarter", "3rd quarter", "4th quarter").
    /// </summary>
    public readonly string[] Wide;

    /// <summary>
    /// Narrow quarter names (e.g., "1", "2", "3", "4").
    /// </summary>
    public readonly string[] Narrow;

    /// <summary>
    /// Creates a new QuarterFormats instance.
    /// </summary>
    public QuarterFormats(string[] abbreviated, string[] wide, string[] narrow)
    {
        Abbreviated = abbreviated;
        Wide = wide;
        Narrow = narrow;
    }

    /// <summary>
    /// Indicates whether this format has any patterns.
    /// </summary>
    public bool HasData => Abbreviated is { Length: > 0 } || Wide is { Length: > 0 } || Narrow is { Length: > 0 };

    /// <summary>
    /// Gets the quarter name for the given quarter (1-4) and width.
    /// </summary>
    /// <param name="quarter">The quarter number (1-4).</param>
    /// <param name="width">The width: "abbreviated", "wide", or "narrow".</param>
    /// <returns>The quarter name, or null if not found.</returns>
    public string? GetQuarter(int quarter, string width)
    {
        if (quarter < 1 || quarter > 4)
            return null;

        var index = quarter - 1;
        var array = width switch
        {
            "wide" => Wide,
            "narrow" => Narrow,
            _ => Abbreviated
        };

        return array is { Length: > 0 } && index < array.Length ? array[index] : null;
    }
}
