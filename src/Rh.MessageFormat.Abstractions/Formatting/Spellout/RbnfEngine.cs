using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Rh.MessageFormat.Formatting.Spellout;

/// <summary>
/// RBNF (Rule-Based Number Format) interpreter engine.
/// Executes CLDR RBNF rules to convert numbers to words.
/// </summary>
public sealed class RbnfEngine
{
    private readonly Dictionary<string, RbnfRuleSet> _ruleSets;
    private readonly CultureInfo _culture;
    private const int MaxRecursionDepth = 50;

    public RbnfEngine(Dictionary<string, RbnfRuleSet> ruleSets, CultureInfo culture)
    {
        _ruleSets = ruleSets;
        _culture = culture;
    }

    /// <summary>
    /// Formats a number using the specified rule set.
    /// </summary>
    public string Format(double number, string ruleSetName)
    {
        if (!_ruleSets.TryGetValue(ruleSetName, out var ruleSet))
        {
            // Fallback to numeric format if rule set not found
            return number.ToString("N0", _culture);
        }

        var sb = new StringBuilder();
        FormatInternal(number, ruleSet, sb, 0);
        return sb.ToString();
    }

    private void FormatInternal(double number, RbnfRuleSet ruleSet, StringBuilder output, int depth)
    {
        if (depth > MaxRecursionDepth)
        {
            // Prevent infinite recursion
            output.Append(number.ToString("N0", _culture));
            return;
        }

        var rule = ruleSet.FindRule(number);
        if (rule == null)
        {
            output.Append(number.ToString("N0", _culture));
            return;
        }

        ProcessRule(number, rule, ruleSet, output, depth);
    }

    private void ProcessRule(double number, RbnfRule rule, RbnfRuleSet currentRuleSet, StringBuilder output, int depth)
    {
        var template = rule.Output;
        var i = 0;

        while (i < template.Length)
        {
            var ch = template[i];

            // Handle optional sections: [text]
            if (ch == '[')
            {
                var endBracket = FindMatchingBracket(template, i);
                if (endBracket > i)
                {
                    var optionalContent = template.Substring(i + 1, endBracket - i - 1);

                    // Process optional content only if there's a remainder
                    var remainder = GetRemainder(number, rule);
                    if (remainder > 0)
                    {
                        ProcessOptionalSection(number, rule, currentRuleSet, optionalContent, output, depth);
                    }

                    i = endBracket + 1;
                    continue;
                }
            }

            // Handle rule references: =%ruleset= or =%ruleset=suffix
            if (ch == '=')
            {
                var endEquals = template.IndexOf('=', i + 1);
                if (endEquals > i)
                {
                    var reference = template.Substring(i + 1, endEquals - i - 1);
                    ProcessRuleReference(number, reference, output, depth);
                    i = endEquals + 1;
                    continue;
                }
            }

            // Handle right arrow substitution: ←←← (process remainder)
            if (ch == '\u2192' && i + 1 < template.Length && template[i + 1] == '\u2192')
            {
                var remainder = GetRemainder(number, rule);
                FormatInternal(remainder, currentRuleSet, output, depth + 1);
                i += 2;
                continue;
            }

            // Handle right arrow with rule reference: ←%ruleset←
            if (ch == '\u2192' && i + 1 < template.Length)
            {
                var endArrow = template.IndexOf('\u2192', i + 1);
                if (endArrow > i + 1)
                {
                    var reference = template.Substring(i + 1, endArrow - i - 1);
                    var remainder = GetRemainder(number, rule);
                    ProcessRuleReferenceWithValue(remainder, reference, output, depth);
                    i = endArrow + 1;
                    continue;
                }
            }

            // Handle left arrow substitution: ←← (process quotient)
            if (ch == '\u2190' && i + 1 < template.Length && template[i + 1] == '\u2190')
            {
                var quotient = GetQuotient(number, rule);
                FormatInternal(quotient, currentRuleSet, output, depth + 1);
                i += 2;
                continue;
            }

            // Handle left arrow with rule reference: ←%ruleset←
            if (ch == '\u2190' && i + 1 < template.Length)
            {
                var endArrow = template.IndexOf('\u2190', i + 1);
                if (endArrow > i + 1)
                {
                    var reference = template.Substring(i + 1, endArrow - i - 1);
                    var quotient = GetQuotient(number, rule);
                    ProcessRuleReferenceWithValue(quotient, reference, output, depth);
                    i = endArrow + 1;
                    continue;
                }
            }

            // Handle plural selection: $(plural,one{...}other{...})$
            if (ch == '$' && i + 1 < template.Length && template[i + 1] == '(')
            {
                var endDollar = template.IndexOf(")$", i);
                if (endDollar > i)
                {
                    // Skip plural selection for now, just output the number
                    output.Append(number.ToString("N0", _culture));
                    i = endDollar + 2;
                    continue;
                }
            }

            // Handle escaped characters and special notation
            if (ch == '#')
            {
                // # represents the number itself in some formats
                output.Append(number.ToString("N0", _culture));
                i++;
                continue;
            }

            // Regular character
            output.Append(ch);
            i++;
        }
    }

    private void ProcessOptionalSection(double number, RbnfRule rule, RbnfRuleSet currentRuleSet, string content, StringBuilder output, int depth)
    {
        // Process the optional content, handling substitutions
        var tempSb = new StringBuilder();
        var tempRule = new RbnfRule(rule.BaseValue, rule.Divisor, content, rule.RuleType);
        ProcessRule(number, tempRule, currentRuleSet, tempSb, depth);

        if (tempSb.Length > 0)
        {
            output.Append(tempSb);
        }
    }

    private void ProcessRuleReference(double number, string reference, StringBuilder output, int depth)
    {
        var ruleSetName = reference.StartsWith("%") ? reference : "%" + reference;

        if (_ruleSets.TryGetValue(ruleSetName, out var ruleSet))
        {
            FormatInternal(number, ruleSet, output, depth + 1);
        }
        else
        {
            output.Append(number.ToString("N0", _culture));
        }
    }

    private void ProcessRuleReferenceWithValue(double value, string reference, StringBuilder output, int depth)
    {
        var ruleSetName = reference.StartsWith("%") ? reference : "%" + reference;

        if (_ruleSets.TryGetValue(ruleSetName, out var ruleSet))
        {
            FormatInternal(value, ruleSet, output, depth + 1);
        }
        else
        {
            output.Append(value.ToString("N0", _culture));
        }
    }

    private static double GetQuotient(double number, RbnfRule rule)
    {
        if (rule.Divisor <= 0 || rule.BaseValue <= 0)
            return number;

        // For rules like "100", the divisor is 100
        var divisor = rule.BaseValue;
        return Math.Floor(Math.Abs(number) / divisor);
    }

    private static double GetRemainder(double number, RbnfRule rule)
    {
        if (rule.BaseValue <= 0)
            return 0;

        // For rules like "100", get the remainder after dividing by 100
        var divisor = rule.BaseValue;
        return Math.Abs(number) % divisor;
    }

    private static int FindMatchingBracket(string text, int start)
    {
        var depth = 1;
        for (var i = start + 1; i < text.Length; i++)
        {
            if (text[i] == '[')
                depth++;
            else if (text[i] == ']')
            {
                depth--;
                if (depth == 0)
                    return i;
            }
        }
        return -1;
    }
}
