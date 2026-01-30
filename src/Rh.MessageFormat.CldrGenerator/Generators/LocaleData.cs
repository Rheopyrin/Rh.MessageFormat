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
    /// Ordinal suffixes extracted from RBNF data.
    /// Key is the ordinal category (one, two, few, other), value is the suffix (st, nd, rd, th).
    /// </summary>
    public Dictionary<string, string>? OrdinalSuffixes { get; set; }

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

    /// <summary>
    /// Relative time data for this locale.
    /// Key format: "field:width" (e.g., "day:long", "year:short").
    /// </summary>
    public Dictionary<string, LocaleRelativeTimeData> RelativeTimeData { get; set; } = new();

    /// <summary>
    /// Quarter format patterns for this locale.
    /// </summary>
    public LocaleQuarterData? Quarters { get; set; }

    /// <summary>
    /// Week configuration for this locale (first day, min days).
    /// </summary>
    public LocaleWeekData? WeekInfo { get; set; }

    /// <summary>
    /// Interval format patterns for date ranges.
    /// </summary>
    public LocaleIntervalFormatData? IntervalFormats { get; set; }

    /// <summary>
    /// Default numbering system for this locale (e.g., "latn", "beng", "arab").
    /// </summary>
    public string? DefaultNumberingSystem { get; set; }
}

public class LocaleCurrencyData
{
    public string Code { get; set; } = string.Empty;
    public string? Symbol { get; set; }
    public string? NarrowSymbol { get; set; }
    public string? DisplayName { get; set; }
    public string? DisplayNameOne { get; set; }
    public string? DisplayNameFew { get; set; }
    public string? DisplayNameMany { get; set; }
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

public class LocaleRelativeTimeData
{
    /// <summary>
    /// The field identifier (e.g., "year", "month", "day").
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// The width ("long", "short", "narrow").
    /// </summary>
    public string Width { get; set; } = "long";

    /// <summary>
    /// The display name of the field.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Relative type strings keyed by offset ("-1", "0", "1").
    /// </summary>
    public Dictionary<string, string> RelativeTypes { get; set; } = new();

    /// <summary>
    /// Future patterns keyed by plural category ("one", "other", etc.).
    /// </summary>
    public Dictionary<string, string> FuturePatterns { get; set; } = new();

    /// <summary>
    /// Past patterns keyed by plural category ("one", "other", etc.).
    /// </summary>
    public Dictionary<string, string> PastPatterns { get; set; } = new();
}

/// <summary>
/// Quarter format patterns for a locale.
/// </summary>
public class LocaleQuarterData
{
    /// <summary>
    /// Format context patterns (used in dates).
    /// </summary>
    public LocaleQuarterFormats? Format { get; set; }

    /// <summary>
    /// Stand-alone context patterns (used independently).
    /// </summary>
    public LocaleQuarterFormats? StandAlone { get; set; }
}

/// <summary>
/// Quarter format strings by style (abbreviated, wide, narrow).
/// </summary>
public class LocaleQuarterFormats
{
    /// <summary>
    /// Abbreviated quarter names (e.g., "Q1", "Q2", "Q3", "Q4").
    /// </summary>
    public string[] Abbreviated { get; set; } = new string[4];

    /// <summary>
    /// Wide quarter names (e.g., "1st quarter", "2nd quarter").
    /// </summary>
    public string[] Wide { get; set; } = new string[4];

    /// <summary>
    /// Narrow quarter names (e.g., "1", "2", "3", "4").
    /// </summary>
    public string[] Narrow { get; set; } = new string[4];
}

/// <summary>
/// Week configuration for a locale.
/// </summary>
public class LocaleWeekData
{
    /// <summary>
    /// First day of the week (0=Sunday, 1=Monday, etc.).
    /// </summary>
    public int FirstDay { get; set; }

    /// <summary>
    /// Minimum days in the first week of the year.
    /// </summary>
    public int MinDays { get; set; } = 1;
}

/// <summary>
/// Interval format patterns for date ranges.
/// </summary>
public class LocaleIntervalFormatData
{
    /// <summary>
    /// Fallback pattern when no skeleton match (e.g., "{0} – {1}").
    /// </summary>
    public string FallbackPattern { get; set; } = "{0} – {1}";

    /// <summary>
    /// Interval patterns by skeleton.
    /// Key: skeleton (e.g., "yMd", "MMMd")
    /// Value: patterns by greatest difference field
    /// </summary>
    public Dictionary<string, Dictionary<char, string>> Skeletons { get; set; } = new();
}
