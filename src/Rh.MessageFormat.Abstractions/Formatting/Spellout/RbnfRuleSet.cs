using System;
using System.Collections.Generic;

namespace Rh.MessageFormat.Formatting.Spellout;

/// <summary>
/// Represents a named set of RBNF rules.
/// </summary>
public sealed class RbnfRuleSet
{
    /// <summary>
    /// The name of this rule set (e.g., "%spellout-cardinal").
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Whether this is a private rule set (starts with %%).
    /// </summary>
    public bool IsPrivate => Name.StartsWith("%%");

    /// <summary>
    /// The rules in this set, ordered by base value.
    /// </summary>
    public IReadOnlyList<RbnfRule> Rules { get; }

    private readonly RbnfRule? _negativeRule;
    private readonly RbnfRule? _decimalRule;
    private readonly RbnfRule? _infinityRule;
    private readonly RbnfRule? _nanRule;

    public RbnfRuleSet(string name, IReadOnlyList<RbnfRule> rules)
    {
        Name = name;
        Rules = rules;

        // Cache special rules for quick access
        foreach (var rule in rules)
        {
            switch (rule.RuleType)
            {
                case RbnfRuleType.Negative:
                    _negativeRule = rule;
                    break;
                case RbnfRuleType.Decimal:
                    _decimalRule = rule;
                    break;
                case RbnfRuleType.Infinity:
                    _infinityRule = rule;
                    break;
                case RbnfRuleType.NotANumber:
                    _nanRule = rule;
                    break;
            }
        }
    }

    /// <summary>
    /// Gets the rule for a negative number.
    /// </summary>
    public RbnfRule? NegativeRule => _negativeRule;

    /// <summary>
    /// Gets the rule for a decimal number.
    /// </summary>
    public RbnfRule? DecimalRule => _decimalRule;

    /// <summary>
    /// Gets the rule for infinity.
    /// </summary>
    public RbnfRule? InfinityRule => _infinityRule;

    /// <summary>
    /// Gets the rule for NaN.
    /// </summary>
    public RbnfRule? NanRule => _nanRule;

    /// <summary>
    /// Finds the appropriate rule for a given number.
    /// </summary>
    public RbnfRule? FindRule(double number)
    {
        // Handle special cases
        if (double.IsNaN(number))
            return _nanRule;

        if (double.IsInfinity(number))
            return _infinityRule;

        if (number < 0 && _negativeRule != null)
            return _negativeRule;

        if (number != Math.Floor(number) && _decimalRule != null)
            return _decimalRule;

        // Find the appropriate rule by base value
        // Rules are ordered, so we find the largest base value <= number
        RbnfRule? bestMatch = null;
        foreach (var rule in Rules)
        {
            if (rule.RuleType != RbnfRuleType.Normal)
                continue;

            if (rule.BaseValue <= number)
            {
                bestMatch = rule;
            }
            else
            {
                break;
            }
        }

        return bestMatch;
    }
}
