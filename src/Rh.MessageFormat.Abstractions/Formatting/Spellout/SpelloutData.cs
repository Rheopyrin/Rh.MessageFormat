using System;
using System.Collections.Generic;
using System.Globalization;

namespace Rh.MessageFormat.Formatting.Spellout;

/// <summary>
/// Contains RBNF spellout data for a locale.
/// </summary>
public sealed class SpelloutData
{
    private readonly Dictionary<string, RbnfRuleSet> _ruleSets;
    private RbnfEngine? _engine;
    private readonly CultureInfo _culture;

    public SpelloutData(string locale, Dictionary<string, RbnfRuleSet> ruleSets)
    {
        _ruleSets = ruleSets;
        _culture = CultureInfo.GetCultureInfo(locale.Replace('_', '-'));
    }

    /// <summary>
    /// Gets the available rule set names.
    /// </summary>
    public IEnumerable<string> RuleSetNames => _ruleSets.Keys;

    /// <summary>
    /// Checks if a rule set exists.
    /// </summary>
    public bool HasRuleSet(string name) => _ruleSets.ContainsKey(name);

    /// <summary>
    /// Formats a number using the specified rule set.
    /// </summary>
    public string Format(double number, string ruleSetName)
    {
        _engine ??= new RbnfEngine(_ruleSets, _culture);
        return _engine.Format(number, ruleSetName);
    }

    /// <summary>
    /// Formats a number as cardinal words (e.g., "one hundred twenty-three").
    /// </summary>
    public string FormatCardinal(double number)
    {
        return Format(number, "%spellout-cardinal");
    }

    /// <summary>
    /// Formats a number as ordinal words (e.g., "first", "second", "third").
    /// </summary>
    public string FormatOrdinal(double number)
    {
        return Format(number, "%spellout-ordinal");
    }

    /// <summary>
    /// Formats a number using verbose spellout (e.g., "one hundred and twenty-three").
    /// </summary>
    public string FormatVerbose(double number)
    {
        return Format(number, "%spellout-cardinal-verbose");
    }

    /// <summary>
    /// Formats a year (e.g., "nineteen ninety-nine").
    /// </summary>
    public string FormatYear(double number)
    {
        return Format(number, "%spellout-numbering-year");
    }
}
