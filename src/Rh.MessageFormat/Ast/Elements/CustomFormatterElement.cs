using System.Runtime.CompilerServices;
using System.Text;

namespace Rh.MessageFormat.Ast.Elements;

/// <summary>
/// Represents a custom formatter element: {var, customType} or {var, customType, style}
/// Used for user-registered custom formatters.
/// </summary>
internal sealed class CustomFormatterElement : MessageElement
{
    private readonly string _variable;
    private readonly string _formatterName;
    private readonly string? _style;

    public CustomFormatterElement(string variable, string formatterName, string? style, SourceSpan location)
        : base(location)
    {
        _variable = variable;
        _formatterName = formatterName;
        _style = style;
    }

    public string Variable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _variable;
    }

    public string FormatterName
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _formatterName;
    }

    public string? Style
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _style;
    }

    public override ElementType Type => ElementType.Custom;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Format(ref FormatterContext ctx, StringBuilder output)
    {
        var value = ctx.GetValue(_variable);

        if (ctx.TryGetCustomFormatter(_formatterName, out var formatter))
        {
            var result = formatter(value, _style, ctx.Locale, ctx.Culture);
            output.Append(result);
        }
        else
        {
            // Fallback: just output the value as string
            if (value != null)
            {
                output.Append(value.ToString());
            }
        }
    }
}