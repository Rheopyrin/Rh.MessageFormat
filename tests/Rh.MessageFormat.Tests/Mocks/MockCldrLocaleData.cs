using System;
using System.Collections.Generic;
using Rh.MessageFormat.Abstractions.Interfaces;
using Rh.MessageFormat.Abstractions.Models;

namespace Rh.MessageFormat.Tests.Mocks;

/// <summary>
/// Mock implementation of ICldrLocaleData for testing.
/// </summary>
public class MockCldrLocaleData : ICldrLocaleData
{
    private readonly Dictionary<string, CurrencyData> _currencies = new();
    private readonly Dictionary<string, UnitData> _units = new();
    private readonly Dictionary<string, ListPatternData> _listPatterns = new();
    private readonly Dictionary<string, RelativeTimeData> _relativeTimeData = new();

    public string Locale { get; set; } = "en";
    public DatePatternData DatePatterns { get; set; }
    public QuarterData Quarters { get; set; }
    public WeekData WeekInfo { get; set; }
    public IntervalFormatData IntervalFormats { get; set; }

    /// <summary>
    /// Function to compute plural category. Defaults to simple English rules.
    /// </summary>
    public Func<PluralContext, string>? PluralRule { get; set; }

    /// <summary>
    /// Function to compute ordinal category. Defaults to simple English rules.
    /// </summary>
    public Func<PluralContext, string>? OrdinalRule { get; set; }

    public MockCldrLocaleData()
    {
        // Default English-like date patterns using .NET-compatible format strings
        DatePatterns = new DatePatternData(
            new DateFormats("dddd, MMMM d, yyyy", "MMMM d, yyyy", "MMM d, yyyy", "M/d/yyyy"),
            new TimeFormats("h:mm:ss tt K", "h:mm:ss tt z", "h:mm:ss tt", "h:mm tt"),
            new DateTimeFormats("{1} 'at' {0}", "{1} 'at' {0}", "{1}, {0}", "{1}, {0}")
        );

        // Default English quarter patterns
        Quarters = new QuarterData(
            new QuarterFormats(
                new[] { "Q1", "Q2", "Q3", "Q4" },
                new[] { "1st quarter", "2nd quarter", "3rd quarter", "4th quarter" },
                new[] { "1", "2", "3", "4" }
            ),
            new QuarterFormats(
                new[] { "Q1", "Q2", "Q3", "Q4" },
                new[] { "1st quarter", "2nd quarter", "3rd quarter", "4th quarter" },
                new[] { "1", "2", "3", "4" }
            )
        );

        // Default week info (ISO 8601 standard)
        WeekInfo = WeekData.Iso8601;

        // Default interval format (fallback pattern)
        IntervalFormats = IntervalFormatData.Default;

        // Default English plural rule: 1 = one, else = other
        PluralRule = ctx => ctx.I == 1 && ctx.V == 0 ? "one" : "other";

        // Default English ordinal rule
        OrdinalRule = ctx =>
        {
            var n = ctx.I;
            var mod10 = n % 10;
            var mod100 = n % 100;
            if (mod10 == 1 && mod100 != 11) return "one";
            if (mod10 == 2 && mod100 != 12) return "two";
            if (mod10 == 3 && mod100 != 13) return "few";
            return "other";
        };
    }

    public string GetPluralCategory(PluralContext ctx)
    {
        return PluralRule?.Invoke(ctx) ?? "other";
    }

    public string GetOrdinalCategory(PluralContext ctx)
    {
        return OrdinalRule?.Invoke(ctx) ?? "other";
    }

    public bool TryGetCurrency(string code, out CurrencyData data)
    {
        return _currencies.TryGetValue(code.ToUpperInvariant(), out data);
    }

    public bool TryGetUnit(string unitId, out UnitData data)
    {
        return _units.TryGetValue(unitId, out data);
    }

    public bool TryGetListPattern(string type, out ListPatternData data)
    {
        return _listPatterns.TryGetValue(type, out data);
    }

    public bool TryGetRelativeTime(string field, string width, out RelativeTimeData data)
    {
        var key = $"{field}:{width}";
        return _relativeTimeData.TryGetValue(key, out data);
    }

    #region Builder Methods

    /// <summary>
    /// Adds a currency to this locale data.
    /// </summary>
    public MockCldrLocaleData WithCurrency(string code, string symbol, string displayName,
        string? narrowSymbol = null, string? displayNameOne = null, string? displayNameOther = null,
        string? displayNameFew = null, string? displayNameMany = null)
    {
        _currencies[code.ToUpperInvariant()] = new CurrencyData(
            code.ToUpperInvariant(),
            symbol,
            narrowSymbol ?? symbol,
            displayName,
            displayNameOne ?? displayName,
            displayNameFew,
            displayNameMany,
            displayNameOther ?? displayName + "s"
        );
        return this;
    }

    /// <summary>
    /// Adds a unit to this locale data.
    /// </summary>
    public MockCldrLocaleData WithUnit(string unitId, Dictionary<string, string> displayNames)
    {
        _units[unitId] = new UnitData(unitId, displayNames);
        return this;
    }

    /// <summary>
    /// Adds a list pattern to this locale data.
    /// </summary>
    public MockCldrLocaleData WithListPattern(string type, string start, string middle, string end, string two)
    {
        _listPatterns[type] = new ListPatternData(type, start, middle, end, two);
        return this;
    }

    /// <summary>
    /// Sets the date patterns for this locale data.
    /// </summary>
    public MockCldrLocaleData WithDatePatterns(DatePatternData patterns)
    {
        DatePatterns = patterns;
        return this;
    }

    /// <summary>
    /// Sets the quarter patterns for this locale data.
    /// </summary>
    public MockCldrLocaleData WithQuarters(QuarterData quarters)
    {
        Quarters = quarters;
        return this;
    }

    /// <summary>
    /// Sets the quarter patterns using arrays for abbreviated, wide, and narrow forms.
    /// </summary>
    public MockCldrLocaleData WithQuarters(
        string[] abbreviated, string[] wide, string[] narrow,
        string[]? standaloneAbbreviated = null, string[]? standaloneWide = null, string[]? standaloneNarrow = null)
    {
        Quarters = new QuarterData(
            new QuarterFormats(abbreviated, wide, narrow),
            new QuarterFormats(
                standaloneAbbreviated ?? abbreviated,
                standaloneWide ?? wide,
                standaloneNarrow ?? narrow
            )
        );
        return this;
    }

    /// <summary>
    /// Sets the week info for this locale data.
    /// </summary>
    public MockCldrLocaleData WithWeekInfo(WeekData weekInfo)
    {
        WeekInfo = weekInfo;
        return this;
    }

    /// <summary>
    /// Sets the week info using first day and minimum days.
    /// </summary>
    public MockCldrLocaleData WithWeekInfo(DayOfWeek firstDay, int minDays)
    {
        WeekInfo = new WeekData(firstDay, minDays);
        return this;
    }

    /// <summary>
    /// Sets the interval format data for this locale.
    /// </summary>
    public MockCldrLocaleData WithIntervalFormats(IntervalFormatData data)
    {
        IntervalFormats = data;
        return this;
    }

    /// <summary>
    /// Sets the interval format fallback pattern.
    /// </summary>
    public MockCldrLocaleData WithIntervalFormatFallback(string fallbackPattern)
    {
        IntervalFormats = new IntervalFormatData(fallbackPattern);
        return this;
    }

    /// <summary>
    /// Adds relative time data for a field and width.
    /// </summary>
    public MockCldrLocaleData WithRelativeTime(
        string field,
        string width,
        string? displayName = null,
        Dictionary<string, string>? relativeTypes = null,
        Dictionary<string, string>? futurePatterns = null,
        Dictionary<string, string>? pastPatterns = null)
    {
        var key = $"{field}:{width}";
        _relativeTimeData[key] = new RelativeTimeData(
            field,
            displayName,
            relativeTypes,
            futurePatterns,
            pastPatterns
        );
        return this;
    }

    /// <summary>
    /// Sets the plural rule function.
    /// </summary>
    public MockCldrLocaleData WithPluralRule(Func<PluralContext, string> rule)
    {
        PluralRule = rule;
        return this;
    }

    /// <summary>
    /// Sets the ordinal rule function.
    /// </summary>
    public MockCldrLocaleData WithOrdinalRule(Func<PluralContext, string> rule)
    {
        OrdinalRule = rule;
        return this;
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Creates English locale data with standard patterns.
    /// </summary>
    public static MockCldrLocaleData CreateEnglish()
    {
        return new MockCldrLocaleData { Locale = "en" }
            .WithCurrency("USD", "$", "US Dollar", "$", "US dollar", "US dollars")
            .WithCurrency("EUR", "\u20AC", "Euro", "\u20AC", "euro", "euros")
            .WithCurrency("GBP", "\u00A3", "British Pound", "\u00A3", "British pound", "British pounds")
            .WithUnit("length-kilometer", new Dictionary<string, string>
            {
                ["long:one"] = "{0} kilometer",
                ["long:other"] = "{0} kilometers",
                ["short:one"] = "{0} km",
                ["short:other"] = "{0} km",
                ["narrow:one"] = "{0}km",
                ["narrow:other"] = "{0}km"
            })
            .WithUnit("length-meter", new Dictionary<string, string>
            {
                ["long:one"] = "{0} meter",
                ["long:other"] = "{0} meters",
                ["short:one"] = "{0} m",
                ["short:other"] = "{0} m"
            })
            .WithUnit("length-mile", new Dictionary<string, string>
            {
                ["long:one"] = "{0} mile",
                ["long:other"] = "{0} miles",
                ["short:one"] = "{0} mi",
                ["short:other"] = "{0} mi"
            })
            .WithUnit("temperature-celsius", new Dictionary<string, string>
            {
                ["long:one"] = "{0} degree Celsius",
                ["long:other"] = "{0} degrees Celsius",
                ["short:one"] = "{0}\u00B0C",
                ["short:other"] = "{0}\u00B0C"
            })
            .WithUnit("temperature-fahrenheit", new Dictionary<string, string>
            {
                ["long:one"] = "{0} degree Fahrenheit",
                ["long:other"] = "{0} degrees Fahrenheit",
                ["short:one"] = "{0}\u00B0F",
                ["short:other"] = "{0}\u00B0F"
            })
            .WithUnit("mass-kilogram", new Dictionary<string, string>
            {
                ["long:one"] = "{0} kilogram",
                ["long:other"] = "{0} kilograms",
                ["short:one"] = "{0} kg",
                ["short:other"] = "{0} kg"
            })
            .WithUnit("mass-pound", new Dictionary<string, string>
            {
                ["long:one"] = "{0} pound",
                ["long:other"] = "{0} pounds",
                ["short:one"] = "{0} lb",
                ["short:other"] = "{0} lb"
            })
            .WithUnit("volume-liter", new Dictionary<string, string>
            {
                ["long:one"] = "{0} liter",
                ["long:other"] = "{0} liters",
                ["short:one"] = "{0} L",
                ["short:other"] = "{0} L"
            })
            .WithUnit("duration-year", new Dictionary<string, string>
            {
                ["long:one"] = "year",
                ["long:other"] = "years",
                ["short:one"] = "yr",
                ["short:other"] = "yrs",
                ["narrow:one"] = "y",
                ["narrow:other"] = "y"
            })
            .WithUnit("duration-month", new Dictionary<string, string>
            {
                ["long:one"] = "month",
                ["long:other"] = "months",
                ["short:one"] = "mo",
                ["short:other"] = "mos",
                ["narrow:one"] = "m",
                ["narrow:other"] = "m"
            })
            .WithUnit("duration-week", new Dictionary<string, string>
            {
                ["long:one"] = "week",
                ["long:other"] = "weeks",
                ["short:one"] = "wk",
                ["short:other"] = "wks",
                ["narrow:one"] = "w",
                ["narrow:other"] = "w"
            })
            .WithUnit("duration-day", new Dictionary<string, string>
            {
                ["long:one"] = "day",
                ["long:other"] = "days",
                ["short:one"] = "day",
                ["short:other"] = "days",
                ["narrow:one"] = "d",
                ["narrow:other"] = "d"
            })
            .WithUnit("duration-hour", new Dictionary<string, string>
            {
                ["long:one"] = "hour",
                ["long:other"] = "hours",
                ["short:one"] = "hr",
                ["short:other"] = "hrs",
                ["narrow:one"] = "h",
                ["narrow:other"] = "h"
            })
            .WithUnit("duration-minute", new Dictionary<string, string>
            {
                ["long:one"] = "minute",
                ["long:other"] = "minutes",
                ["short:one"] = "min",
                ["short:other"] = "mins",
                ["narrow:one"] = "m",
                ["narrow:other"] = "m"
            })
            .WithUnit("duration-second", new Dictionary<string, string>
            {
                ["long:one"] = "second",
                ["long:other"] = "seconds",
                ["short:one"] = "sec",
                ["short:other"] = "secs",
                ["narrow:one"] = "s",
                ["narrow:other"] = "s"
            })
            .WithUnit("duration-millisecond", new Dictionary<string, string>
            {
                ["long:one"] = "millisecond",
                ["long:other"] = "milliseconds",
                ["short:one"] = "ms",
                ["short:other"] = "ms",
                ["narrow:one"] = "ms",
                ["narrow:other"] = "ms"
            })
            .WithUnit("digital-byte", new Dictionary<string, string>
            {
                ["long:one"] = "{0} byte",
                ["long:other"] = "{0} bytes",
                ["short:one"] = "{0} byte",
                ["short:other"] = "{0} byte"
            })
            .WithUnit("digital-megabyte", new Dictionary<string, string>
            {
                ["long:one"] = "{0} megabyte",
                ["long:other"] = "{0} megabytes",
                ["short:one"] = "{0} MB",
                ["short:other"] = "{0} MB"
            })
            .WithUnit("speed-kilometer-per-hour", new Dictionary<string, string>
            {
                ["long:one"] = "{0} kilometer per hour",
                ["long:other"] = "{0} kilometers per hour",
                ["short:one"] = "{0} km/h",
                ["short:other"] = "{0} km/h"
            })
            .WithUnit("speed-mile-per-hour", new Dictionary<string, string>
            {
                ["long:one"] = "{0} mile per hour",
                ["long:other"] = "{0} miles per hour",
                ["short:one"] = "{0} mph",
                ["short:other"] = "{0} mph"
            })
            .WithUnit("area-square-meter", new Dictionary<string, string>
            {
                ["long:one"] = "{0} square meter",
                ["long:other"] = "{0} square meters",
                ["short:one"] = "{0} m\u00B2",
                ["short:other"] = "{0} m\u00B2"
            })
            .WithListPattern("standard", "{0}, {1}", "{0}, {1}", "{0}, and {1}", "{0} and {1}")
            .WithListPattern("standard-short", "{0}, {1}", "{0}, {1}", "{0}, & {1}", "{0} & {1}")
            .WithListPattern("standard-narrow", "{0}, {1}", "{0}, {1}", "{0}, {1}", "{0}, {1}")
            .WithListPattern("or", "{0}, {1}", "{0}, {1}", "{0}, or {1}", "{0} or {1}")
            .WithListPattern("or-short", "{0}, {1}", "{0}, {1}", "{0}, or {1}", "{0} or {1}")
            .WithListPattern("or-narrow", "{0}, {1}", "{0}, {1}", "{0}, or {1}", "{0} or {1}")
            .WithListPattern("unit", "{0}, {1}", "{0}, {1}", "{0}, {1}", "{0}, {1}")
            .WithListPattern("unit-short", "{0}, {1}", "{0}, {1}", "{0}, {1}", "{0}, {1}")
            .WithListPattern("unit-narrow", "{0} {1}", "{0} {1}", "{0} {1}", "{0} {1}")
            // Relative time data
            .WithRelativeTime("day", "long", "day",
                new Dictionary<string, string> { { "-1", "yesterday" }, { "0", "today" }, { "1", "tomorrow" } },
                new Dictionary<string, string> { { "one", "in {0} day" }, { "other", "in {0} days" } },
                new Dictionary<string, string> { { "one", "{0} day ago" }, { "other", "{0} days ago" } })
            .WithRelativeTime("day", "short", "day",
                new Dictionary<string, string> { { "-1", "yesterday" }, { "0", "today" }, { "1", "tomorrow" } },
                new Dictionary<string, string> { { "one", "in {0} day" }, { "other", "in {0} days" } },
                new Dictionary<string, string> { { "one", "{0} day ago" }, { "other", "{0} days ago" } })
            .WithRelativeTime("day", "narrow", "day",
                new Dictionary<string, string> { { "-1", "yesterday" }, { "0", "today" }, { "1", "tomorrow" } },
                new Dictionary<string, string> { { "one", "in {0}d" }, { "other", "in {0}d" } },
                new Dictionary<string, string> { { "one", "{0}d ago" }, { "other", "{0}d ago" } })
            .WithRelativeTime("year", "long", "year",
                new Dictionary<string, string> { { "-1", "last year" }, { "0", "this year" }, { "1", "next year" } },
                new Dictionary<string, string> { { "one", "in {0} year" }, { "other", "in {0} years" } },
                new Dictionary<string, string> { { "one", "{0} year ago" }, { "other", "{0} years ago" } })
            .WithRelativeTime("month", "long", "month",
                new Dictionary<string, string> { { "-1", "last month" }, { "0", "this month" }, { "1", "next month" } },
                new Dictionary<string, string> { { "one", "in {0} month" }, { "other", "in {0} months" } },
                new Dictionary<string, string> { { "one", "{0} month ago" }, { "other", "{0} months ago" } })
            .WithRelativeTime("week", "long", "week",
                new Dictionary<string, string> { { "-1", "last week" }, { "0", "this week" }, { "1", "next week" } },
                new Dictionary<string, string> { { "one", "in {0} week" }, { "other", "in {0} weeks" } },
                new Dictionary<string, string> { { "one", "{0} week ago" }, { "other", "{0} weeks ago" } })
            .WithRelativeTime("hour", "long", "hour",
                new Dictionary<string, string> { { "0", "this hour" } },
                new Dictionary<string, string> { { "one", "in {0} hour" }, { "other", "in {0} hours" } },
                new Dictionary<string, string> { { "one", "{0} hour ago" }, { "other", "{0} hours ago" } })
            .WithRelativeTime("minute", "long", "minute",
                new Dictionary<string, string> { { "0", "this minute" } },
                new Dictionary<string, string> { { "one", "in {0} minute" }, { "other", "in {0} minutes" } },
                new Dictionary<string, string> { { "one", "{0} minute ago" }, { "other", "{0} minutes ago" } })
            .WithRelativeTime("second", "long", "second",
                new Dictionary<string, string> { { "0", "now" } },
                new Dictionary<string, string> { { "one", "in {0} second" }, { "other", "in {0} seconds" } },
                new Dictionary<string, string> { { "one", "{0} second ago" }, { "other", "{0} seconds ago" } });
    }

    /// <summary>
    /// Creates English (US) locale data with US week rules.
    /// </summary>
    public static MockCldrLocaleData CreateEnglishUS()
    {
        var data = CreateEnglish();
        data.Locale = "en-US";
        return data.WithWeekInfo(WeekData.US);
    }

    /// <summary>
    /// Creates German locale data with standard patterns.
    /// </summary>
    public static MockCldrLocaleData CreateGerman()
    {
        return new MockCldrLocaleData { Locale = "de-DE" }
            .WithCurrency("USD", "$", "US-Dollar", "$", "US-Dollar", "US-Dollar")
            .WithCurrency("EUR", "\u20AC", "Euro", "\u20AC", "Euro", "Euro")
            .WithCurrency("GBP", "\u00A3", "Britisches Pfund", "\u00A3", "Britisches Pfund", "Britische Pfund")
            .WithListPattern("standard", "{0}, {1}", "{0}, {1}", "{0} und {1}", "{0} und {1}")
            .WithListPattern("or", "{0}, {1}", "{0}, {1}", "{0} oder {1}", "{0} oder {1}")
            .WithDatePatterns(new DatePatternData(
                new DateFormats("EEEE, d. MMMM y", "d. MMMM y", "dd.MM.y", "dd.MM.yy"),
                new TimeFormats("HH:mm:ss zzzz", "HH:mm:ss z", "HH:mm:ss", "HH:mm"),
                new DateTimeFormats("{1} 'um' {0}", "{1} 'um' {0}", "{1}, {0}", "{1}, {0}")
            ))
            .WithQuarters(
                new[] { "Q1", "Q2", "Q3", "Q4" },
                new[] { "1. Quartal", "2. Quartal", "3. Quartal", "4. Quartal" },
                new[] { "1", "2", "3", "4" }
            );
    }

    /// <summary>
    /// Creates French locale data with standard patterns.
    /// </summary>
    public static MockCldrLocaleData CreateFrench()
    {
        return new MockCldrLocaleData { Locale = "fr-FR" }
            .WithCurrency("USD", "$", "dollar des \u00C9tats-Unis", "$", "dollar des \u00C9tats-Unis", "dollars des \u00C9tats-Unis")
            .WithCurrency("EUR", "\u20AC", "euro", "\u20AC", "euro", "euros")
            .WithListPattern("standard", "{0}, {1}", "{0}, {1}", "{0} et {1}", "{0} et {1}")
            .WithListPattern("or", "{0}, {1}", "{0}, {1}", "{0} ou {1}", "{0} ou {1}")
            .WithPluralRule(ctx => ctx.I == 0 || ctx.I == 1 ? "one" : "other")
            .WithDatePatterns(new DatePatternData(
                new DateFormats("EEEE d MMMM y", "d MMMM y", "d MMM y", "dd/MM/y"),
                new TimeFormats("HH:mm:ss zzzz", "HH:mm:ss z", "HH:mm:ss", "HH:mm"),
                new DateTimeFormats("{1} '\u00E0' {0}", "{1} '\u00E0' {0}", "{1} {0}", "{1} {0}")
            ))
            .WithQuarters(
                new[] { "T1", "T2", "T3", "T4" },
                new[] { "1er trimestre", "2e trimestre", "3e trimestre", "4e trimestre" },
                new[] { "1", "2", "3", "4" }
            );
    }

    #endregion
}
