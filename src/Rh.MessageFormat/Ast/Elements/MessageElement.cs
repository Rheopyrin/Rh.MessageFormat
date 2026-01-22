using System.Text;

namespace Rh.MessageFormat.Ast.Elements;

internal interface IMessageElement
{
    /// <summary>
    /// The source location of this element.
    /// </summary>
    SourceSpan Location { get; }

    /// <summary>
    /// The type of this element for fast dispatch.
    /// </summary>
    ElementType Type { get; }
}

/// <summary>
/// Base class for all message elements. All elements are immutable for caching.
/// </summary>
internal abstract class MessageElement : IMessageElement
{
    protected MessageElement(SourceSpan location)
    {
        Location = location;
    }

    /// <summary>
    /// The source location of this element.
    /// </summary>
    public SourceSpan Location { get; }

    /// <summary>
    /// The type of this element for fast dispatch.
    /// </summary>
    public abstract ElementType Type { get; }

    /// <summary>
    /// Formats this element to the output.
    /// </summary>
    /// <param name="ctx">The formatting context.</param>
    /// <param name="output">The output builder.</param>
    public abstract void Format(ref FormatterContext ctx, StringBuilder output);
}