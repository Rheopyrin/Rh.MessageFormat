using System.Runtime.CompilerServices;
using System.Text;

namespace Rh.MessageFormat.Ast.Elements;

/// <summary>
/// Represents a case in a select element (e.g., "male", "female", "other").
/// </summary>
internal sealed class SelectCase
{
    private readonly string _key;
    private readonly ParsedMessage _content;

    public SelectCase(string key, ParsedMessage content)
    {
        _key = key;
        _content = content;
    }

    public string Key
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _key;
    }

    public ParsedMessage Content
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _content;
    }

    /// <summary>
    /// Formats the case content.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Format(ref FormatterContext ctx, StringBuilder output)
    {
        _content.Format(ref ctx, output);
    }
}
