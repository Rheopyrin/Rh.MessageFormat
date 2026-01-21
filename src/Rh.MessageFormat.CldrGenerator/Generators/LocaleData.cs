namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// Collected CLDR data for a single locale.
/// </summary>
public class LocaleData
{
    public string Locale { get; set; } = string.Empty;

    /// <summary>
    /// If set, this locale should use the class from another locale instead of generating its own.
    /// For example, "en-US" might use class from "en" if data is identical.
    /// </summary>
    public string? UseClassFrom { get; set; }

    /// <summary>
    /// Plural rules as raw CLDR expressions (e.g., "i = 1 and v = 0 @integer 1").
    /// Key is the count (one, two, few, many, other).
    /// </summary>
    public Dictionary<string, string> PluralRules { get; set; } = new();

    /// <summary>
    /// Ordinal rules as raw CLDR expressions.
    /// Key is the count (one, two, few, many, other).
    /// </summary>
    public Dictionary<string, string> OrdinalRules { get; set; } = new();

    /// <summary>
    /// Currency data for this locale.
    /// </summary>
    public Dictionary<string, LocaleCurrencyData> Currencies { get; set; } = new();

    /// <summary>
    /// Unit data for this locale.
    /// </summary>
    public Dictionary<string, LocaleUnitData> Units { get; set; } = new();

    /// <summary>
    /// Date/time patterns for this locale.
    /// </summary>
    public LocaleDatePatternData? DatePatterns { get; set; }

    /// <summary>
    /// List patterns for this locale.
    /// </summary>
    public Dictionary<string, LocaleListPatternData> ListPatterns { get; set; } = new();
}

public class LocaleCurrencyData
{
    public string Code { get; set; } = string.Empty;
    public string? Symbol { get; set; }
    public string? NarrowSymbol { get; set; }
    public string? DisplayName { get; set; }
    public string? DisplayNameOne { get; set; }
    public string? DisplayNameOther { get; set; }
}

public class LocaleUnitData
{
    public string Id { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    /// <summary>
    /// Patterns keyed by "width:count" (e.g., "long:one", "short:other").
    /// </summary>
    public Dictionary<string, string> Patterns { get; set; } = new();
}

public class LocaleDatePatternData
{
    public LocaleDateFormatStyles? DateFormats { get; set; }
    public LocaleDateFormatStyles? TimeFormats { get; set; }
    public LocaleDateFormatStyles? DateTimeFormats { get; set; }
}

public class LocaleDateFormatStyles
{
    public string? Full { get; set; }
    public string? Long { get; set; }
    public string? Medium { get; set; }
    public string? Short { get; set; }
}

public class LocaleListPatternData
{
    public string Type { get; set; } = string.Empty;
    public string? Start { get; set; }
    public string? Middle { get; set; }
    public string? End { get; set; }
    public string? Two { get; set; }
}
