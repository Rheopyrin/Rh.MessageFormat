namespace Rh.MessageFormat.Abstractions.Models;

/// <summary>
/// Contains list pattern data for formatting lists in a locale.
/// </summary>
public readonly struct ListPatternData
{
    /// <summary>
    /// Pattern type: "standard", "standard-short", "standard-narrow", "or", "or-short", "or-narrow", "unit", "unit-short", "unit-narrow".
    /// </summary>
    public readonly string Type;

    /// <summary>
    /// Pattern for the start of a list (3+ items): "{0}, {1}".
    /// </summary>
    public readonly string Start;

    /// <summary>
    /// Pattern for middle items (3+ items): "{0}, {1}".
    /// </summary>
    public readonly string Middle;

    /// <summary>
    /// Pattern for the end of a list (3+ items): "{0}, and {1}".
    /// </summary>
    public readonly string End;

    /// <summary>
    /// Pattern for exactly 2 items: "{0} and {1}".
    /// </summary>
    public readonly string Two;

    /// <summary>
    /// Creates a new ListPatternData instance.
    /// </summary>
    public ListPatternData(string type, string start, string middle, string end, string two)
    {
        Type = type;
        Start = start;
        Middle = middle;
        End = end;
        Two = two;
    }
}
