using System.Runtime.CompilerServices;
using System.Text;
using Rh.MessageFormat.Pools;

namespace Rh.MessageFormat.Ast.Elements;

/// <summary>
/// Represents a rich text tag element: &lt;tagName&gt;content&lt;/tagName&gt;
/// Tags can contain nested message elements and are processed by tag handlers.
/// </summary>
internal sealed class TagElement : MessageElement
{
    private readonly string _tagName;
    private readonly ParsedMessage _content;

    public TagElement(string tagName, ParsedMessage content, SourceSpan location)
        : base(location)
    {
        _tagName = tagName;
        _content = content;
    }

    /// <summary>
    /// The name of the tag (e.g., "bold", "link", "italic").
    /// </summary>
    public string TagName
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _tagName;
    }

    /// <summary>
    /// The parsed content within the tag.
    /// </summary>
    public ParsedMessage Content
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _content;
    }

    public override ElementType Type => ElementType.Tag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Format(ref FormatterContext ctx, StringBuilder output)
    {
        // First, format the inner content
        var innerContent = StringBuilderPool.Get();
        try
        {
            _content.Format(ref ctx, innerContent);
            var formattedContent = innerContent.ToString();

            // Check if there's a tag handler registered for this tag
            if (ctx.TryGetTagHandler(_tagName, out var handler))
            {
                // Apply the tag handler to transform the content
                var result = handler(formattedContent);
                output.Append(result);
            }
            else
            {
                // No handler - output content as-is (tag is essentially stripped)
                output.Append(formattedContent);
            }
        }
        finally
        {
            StringBuilderPool.Return(innerContent);
        }
    }
}
