using System;
using System.Runtime.CompilerServices;
using System.Text;
using Rh.MessageFormat.Abstractions.Models;
using Rh.MessageFormat.Exceptions;
using Rh.MessageFormat.Formatting.Formatters;
using static Rh.MessageFormat.Constants;

namespace Rh.MessageFormat.Ast;

/// <summary>
/// Represents a plural format element: {n, plural, offset:N one {...} other {...}}
/// </summary>
internal sealed class PluralElement : MessageElement
{
    private readonly string _variable;
    private readonly PluralCase[] _cases;
    private readonly double _offset;

    public PluralElement(string variable, PluralCase[] cases, double offset, SourceSpan location)
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

    public override ElementType Type => ElementType.Plural;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Format(ref FormatterContext ctx, StringBuilder output)
    {
        var rawValue = ctx.GetDoubleValue(_variable);
        var n = rawValue - _offset;

        // Get plural form from context
        var pluralCtx = new PluralContext(n);
        var pluralForm = ctx.GetPluralForm(pluralCtx);

        // Find matching case
        var selectedCase = FindCase(rawValue, pluralForm);

        if (selectedCase == null)
        {
            throw new MessageFormatterException("'other' option not found in plural pattern.");
        }

        selectedCase.Format(ref ctx, output, n);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private PluralCase? FindCase(double rawValue, string pluralForm)
    {
        PluralCase? other = null;
        var cases = _cases;

        for (int i = 0; i < cases.Length; i++)
        {
            var c = cases[i];

            if (c.Key == Plurals.Other)
            {
                other = c;
            }

            // Check exact match first (e.g., =5)
            if (c.ExactMatch.HasValue && c.ExactMatch.Value == rawValue)
            {
                return c;
            }

            // Check plural category match
            if (c.Key == pluralForm)
            {
                return c;
            }
        }

        return other;
    }
}