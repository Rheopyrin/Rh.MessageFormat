using System.Runtime.CompilerServices;
using System.Text;

namespace Rh.MessageFormat.Ast;

/// <summary>
/// Represents literal text in a message pattern.
/// </summary>
internal sealed class LiteralElement : MessageElement
{
    private readonly string _text;

    public LiteralElement(string text, SourceSpan location) : base(location)
    {
        _text = text;
    }

    public string Text
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _text;
    }

    public override ElementType Type => ElementType.Literal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Format(ref FormatterContext ctx, StringBuilder output)
    {
        output.Append(_text);
    }
}
