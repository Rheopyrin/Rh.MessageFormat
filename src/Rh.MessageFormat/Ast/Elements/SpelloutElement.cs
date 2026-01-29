using System;
using System.Runtime.CompilerServices;
using System.Text;
using Rh.MessageFormat.Formatting.Spellout;

namespace Rh.MessageFormat.Ast.Elements;

/// <summary>
/// Spellout style for number-to-words formatting.
/// </summary>
internal enum SpelloutStyle
{
    /// <summary>
    /// Cardinal spellout (one, two, three, etc.).
    /// </summary>
    Cardinal,

    /// <summary>
    /// Ordinal spellout (first, second, third, etc.).
    /// </summary>
    Ordinal,

    /// <summary>
    /// Verbose cardinal (one hundred and twenty-three).
    /// </summary>
    Verbose,

    /// <summary>
    /// Year formatting (nineteen ninety-nine).
    /// </summary>
    Year
}

/// <summary>
/// Represents a spellout format element: {n, spellout} or {n, spellout, style}
/// Converts numbers to words using CLDR RBNF data.
/// </summary>
internal sealed class SpelloutElement : MessageElement
{
    private readonly string _variable;
    private readonly SpelloutStyle _style;
    private readonly string? _customRuleSet;

    public SpelloutElement(string variable, SpelloutStyle style, SourceSpan location)
        : base(location)
    {
        _variable = variable;
        _style = style;
        _customRuleSet = null;
    }

    public SpelloutElement(string variable, string customRuleSet, SourceSpan location)
        : base(location)
    {
        _variable = variable;
        _style = SpelloutStyle.Cardinal;
        _customRuleSet = customRuleSet;
    }

    public string Variable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _variable;
    }

    public SpelloutStyle Style
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _style;
    }

    public string? CustomRuleSet
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _customRuleSet;
    }

    public override ElementType Type => ElementType.Spellout;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Format(ref FormatterContext ctx, StringBuilder output)
    {
        var value = ctx.GetDoubleValue(_variable);

        // Try to get spellout data from CLDR
        if (ctx.CldrDataProvider.TryGetSpelloutData(ctx.Locale, out var spelloutData) && spelloutData != null)
        {
            string result;

            if (_customRuleSet != null)
            {
                result = spelloutData.Format(value, _customRuleSet);
            }
            else
            {
                result = _style switch
                {
                    SpelloutStyle.Ordinal => spelloutData.FormatOrdinal(value),
                    SpelloutStyle.Verbose => spelloutData.FormatVerbose(value),
                    SpelloutStyle.Year => spelloutData.FormatYear(value),
                    _ => spelloutData.FormatCardinal(value)
                };
            }

            output.Append(result);
        }
        else
        {
            // Fallback: Try fallback locale
            if (ctx.FallbackLocale != null &&
                ctx.CldrDataProvider.TryGetSpelloutData(ctx.FallbackLocale, out var fallbackData) &&
                fallbackData != null)
            {
                string result;

                if (_customRuleSet != null)
                {
                    result = fallbackData.Format(value, _customRuleSet);
                }
                else
                {
                    result = _style switch
                    {
                        SpelloutStyle.Ordinal => fallbackData.FormatOrdinal(value),
                        SpelloutStyle.Verbose => fallbackData.FormatVerbose(value),
                        SpelloutStyle.Year => fallbackData.FormatYear(value),
                        _ => fallbackData.FormatCardinal(value)
                    };
                }

                output.Append(result);
            }
            else
            {
                // Final fallback: just format the number
                output.Append(value.ToString("N0", ctx.Culture));
            }
        }
    }
}
