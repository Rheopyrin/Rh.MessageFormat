using System;
using System.Runtime.CompilerServices;
using System.Text;
using Rh.MessageFormat.Exceptions;

namespace Rh.MessageFormat.Ast.Elements;

/// <summary>
/// Represents a select format element: {gender, select, male {...} female {...} other {...}}
/// </summary>
internal sealed class SelectElement : MessageElement
{
    private const string NullKey = "null";

    private readonly string _variable;
    private readonly SelectCase[] _cases;

    public SelectElement(string variable, SelectCase[] cases, SourceSpan location)
        : base(location)
    {
        _variable = variable;
        _cases = cases;
    }

    public string Variable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _variable;
    }

    public ReadOnlySpan<SelectCase> Cases
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _cases;
    }

    public override ElementType Type => ElementType.Select;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Format(ref FormatterContext ctx, StringBuilder output)
    {
        var value = ctx.GetValue(_variable);
        var key = ConvertToKey(value);

        // Find matching case
        var selectedCase = FindCase(key);

        if (selectedCase == null)
        {
            throw new MessageFormatterException("'other' option not found in select pattern.");
        }

        selectedCase.Format(ref ctx, output);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ConvertToKey(object? value)
    {
        // Handle null specially: allow matching "null" case in select patterns
        if (value is null)
        {
            return NullKey;
        }

        // Handle booleans specially: C# ToString() returns "True"/"False",
        // but users typically write lowercase "true"/"false" in select patterns
        if (value is bool b)
        {
            return b ? "true" : "false";
        }

        return value.ToString() ?? NullKey;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SelectCase? FindCase(string key)
    {
        SelectCase? other = null;
        var cases = _cases;

        for (int i = 0; i < cases.Length; i++)
        {
            var c = cases[i];

            if (c.Key == "other")
            {
                other = c;
            }

            if (c.Key == key)
            {
                return c;
            }
        }

        return other;
    }
}
