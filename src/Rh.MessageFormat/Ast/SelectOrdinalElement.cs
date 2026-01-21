using System;
using System.Runtime.CompilerServices;
using System.Text;
using Rh.MessageFormat.Abstractions.Models;
using Rh.MessageFormat.Exceptions;
using Rh.MessageFormat.Formatting.Formatters;

namespace Rh.MessageFormat.Ast;

/// <summary>
/// Represents a selectordinal format element: {n, selectordinal, one {#st} two {#nd} few {#rd} other {#th}}
/// </summary>
internal sealed class SelectOrdinalElement : MessageElement
{
    private readonly string _variable;
    private readonly PluralCase[] _cases;
    private readonly double _offset;

    public SelectOrdinalElement(string variable, PluralCase[] cases, double offset, SourceSpan location)
        : base(location)
    {
        _variable = variable;
        _cases = cases;
        _offset = offset;
    }

    public string Variable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _variable;
    }

    public ReadOnlySpan<PluralCase> Cases
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _cases;
    }

    public double Offset
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _offset;
    }

    public override ElementType Type => ElementType.SelectOrdinal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Format(ref FormatterContext ctx, StringBuilder output)
    {
        var rawValue = ctx.GetDoubleValue(_variable);
        var n = rawValue - _offset;

        // Get ordinal form from context (uses locale-specific rules)
        var pluralCtx = new PluralContext(n);
        var ordinalForm = ctx.GetOrdinalForm(pluralCtx);

        // Find matching case
        var selectedCase = FindCase(rawValue, ordinalForm);

        if (selectedCase == null)
        {
            throw new MessageFormatterException("'other' option not found in selectordinal pattern.");
        }

        selectedCase.Format(ref ctx, output, n);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private PluralCase? FindCase(double rawValue, string ordinalForm)
    {
        PluralCase? other = null;
        var cases = _cases;

        for (int i = 0; i < cases.Length; i++)
        {
            var c = cases[i];

            if (c.Key == "other")
            {
                other = c;
            }

            // Check exact match first (e.g., =5)
            if (c.ExactMatch.HasValue && c.ExactMatch.Value == rawValue)
            {
                return c;
            }

            // Check ordinal category match
            if (c.Key == ordinalForm)
            {
                return c;
            }
        }

        return other;
    }
}