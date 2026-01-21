using System.Runtime.CompilerServices;
using System.Text;

namespace Rh.MessageFormat.Ast;

/// <summary>
/// Represents a simple variable argument: {name}
/// </summary>
internal sealed class ArgumentElement : MessageElement
{
    private readonly string _variable;

    public ArgumentElement(string variable, SourceSpan location) : base(location)
    {
        _variable = variable;
    }

    public string Variable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _variable;
    }

    public override ElementType Type => ElementType.Argument;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Format(ref FormatterContext ctx, StringBuilder output)
    {
        var value = ctx.GetValue(_variable);
        if (value != null)
        {
            output.Append(value.ToString());
        }
    }
}