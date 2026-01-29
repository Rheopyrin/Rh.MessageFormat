using System;
using System.Collections.Generic;

namespace Rh.MessageFormat.Formatting.Spellout;

/// <summary>
/// Represents a single RBNF rule.
/// </summary>
public sealed class RbnfRule
{
    /// <summary>
    /// The base value this rule applies to.
    /// For special rules: -1 = negative, -2 = decimal, -3 = infinity, -4 = NaN
    /// </summary>
    public double BaseValue { get; }

    /// <summary>
    /// The divisor for division-based rules (e.g., 100 for "1000/100").
    /// </summary>
    public double Divisor { get; }

    /// <summary>
    /// The rule output template.
    /// </summary>
    public string Output { get; }

    /// <summary>
    /// Special rule type.
    /// </summary>
    public RbnfRuleType RuleType { get; }

    public RbnfRule(double baseValue, double divisor, string output, RbnfRuleType ruleType = RbnfRuleType.Normal)
    {
        BaseValue = baseValue;
        Divisor = divisor;
        Output = output;
        RuleType = ruleType;
    }

    public static RbnfRule Parse(string key, string value)
    {
        var output = value.TrimEnd(';');
        var ruleType = RbnfRuleType.Normal;
        double baseValue = 0;
        double divisor = 1;

        // Handle special keys
        switch (key)
        {
            case "-x":
                ruleType = RbnfRuleType.Negative;
                baseValue = -1;
                break;
            case "x.x":
            case "x,x": // Some locales use comma as decimal separator in RBNF
                ruleType = RbnfRuleType.Decimal;
                baseValue = -2;
                break;
            case "Inf":
                ruleType = RbnfRuleType.Infinity;
                baseValue = -3;
                break;
            case "NaN":
                ruleType = RbnfRuleType.NotANumber;
                baseValue = -4;
                break;
            default:
                // Check for divisor notation: "1000/100"
                var slashIndex = key.IndexOf('/');
                if (slashIndex > 0)
                {
                    baseValue = double.Parse(key.Substring(0, slashIndex), System.Globalization.CultureInfo.InvariantCulture);
                    divisor = double.Parse(key.Substring(slashIndex + 1), System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    baseValue = double.Parse(key, System.Globalization.CultureInfo.InvariantCulture);
                    // Default divisor is the base value's magnitude
                    divisor = baseValue > 0 ? Math.Pow(10, Math.Floor(Math.Log10(baseValue))) : 1;
                }
                break;
        }

        return new RbnfRule(baseValue, divisor, output, ruleType);
    }
}

/// <summary>
/// Type of RBNF rule.
/// </summary>
public enum RbnfRuleType
{
    Normal,
    Negative,
    Decimal,
    Infinity,
    NotANumber
}
