namespace Rh.MessageFormat.Formatting.Skeletons;

/// <summary>
/// Notation style for number formatting.
/// </summary>
internal enum NumberNotation
{
    Standard,
    Scientific,
    Engineering,
    CompactShort,
    CompactLong
}

/// <summary>
/// Sign display options for number formatting.
/// </summary>
internal enum SignDisplay
{
    Auto,
    Always,
    ExceptZero,
    Never,
    Accounting,
    AccountingAlways,
    AccountingExceptZero
}

/// <summary>
/// Unit display options.
/// </summary>
internal enum UnitDisplay
{
    Short,
    Narrow,
    Long
}

/// <summary>
/// Currency display options.
/// </summary>
internal enum CurrencyDisplay
{
    Symbol,
    Code,
    Name,
    NarrowSymbol
}

/// <summary>
/// Grouping options for number formatting.
/// </summary>
internal enum GroupingStrategy
{
    Auto,
    Always,
    Min2,
    Off
}

/// <summary>
/// Options for number skeleton formatting.
/// Represents parsed ICU number skeleton tokens.
/// </summary>
internal sealed class NumberFormatOptions
{
    /// <summary>
    /// The notation style (standard, scientific, compact, etc.)
    /// </summary>
    public NumberNotation Notation { get; set; } = NumberNotation.Standard;

    /// <summary>
    /// Currency code (e.g., "USD", "EUR", "GBP")
    /// </summary>
    public string? CurrencyCode { get; set; }

    /// <summary>
    /// How to display currency (symbol, code, name)
    /// </summary>
    public CurrencyDisplay CurrencyDisplay { get; set; } = CurrencyDisplay.Symbol;

    /// <summary>
    /// Unit for measurement (e.g., "kilometer", "celsius")
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// How to display units (short, narrow, long)
    /// </summary>
    public UnitDisplay UnitDisplay { get; set; } = UnitDisplay.Short;

    /// <summary>
    /// Whether to format as percent (0.5 -> "50%")
    /// </summary>
    public bool IsPercent { get; set; }

    /// <summary>
    /// Whether to format as permille (0.5 -> "500‰")
    /// </summary>
    public bool IsPermille { get; set; }

    /// <summary>
    /// Whether to format as ordinal (1 -> "1st", 2 -> "2nd", etc.)
    /// </summary>
    public bool IsOrdinal { get; set; }

    /// <summary>
    /// Scale factor to multiply the value by before formatting.
    /// </summary>
    public double Scale { get; set; } = 1.0;

    /// <summary>
    /// Minimum fraction digits.
    /// </summary>
    public int? MinimumFractionDigits { get; set; }

    /// <summary>
    /// Maximum fraction digits.
    /// </summary>
    public int? MaximumFractionDigits { get; set; }

    /// <summary>
    /// Minimum integer digits.
    /// </summary>
    public int? MinimumIntegerDigits { get; set; }

    /// <summary>
    /// Minimum significant digits.
    /// </summary>
    public int? MinimumSignificantDigits { get; set; }

    /// <summary>
    /// Maximum significant digits.
    /// </summary>
    public int? MaximumSignificantDigits { get; set; }

    /// <summary>
    /// Sign display option.
    /// </summary>
    public SignDisplay SignDisplay { get; set; } = SignDisplay.Auto;

    /// <summary>
    /// Grouping strategy (thousands separators).
    /// </summary>
    public GroupingStrategy Grouping { get; set; } = GroupingStrategy.Auto;

    /// <summary>
    /// Whether to use grouping separators.
    /// </summary>
    public bool UseGrouping => Grouping != GroupingStrategy.Off;
}
