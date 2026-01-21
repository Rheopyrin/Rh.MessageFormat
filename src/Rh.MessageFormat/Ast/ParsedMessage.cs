using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rh.MessageFormat.Ast;

/// <summary>
/// Represents a parsed message with all elements pre-parsed.
/// Immutable and safe for caching (no Clone() needed).
/// </summary>
internal sealed class ParsedMessage
{
    private readonly MessageElement[] _elements;
    private readonly string _originalPattern;

    public ParsedMessage(MessageElement[] elements, string originalPattern)
    {
        _elements = elements;
        _originalPattern = originalPattern;
    }

    public ReadOnlySpan<MessageElement> Elements
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _elements;
    }

    public string OriginalPattern
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _originalPattern;
    }

    public int ElementCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _elements.Length;
    }

    /// <summary>
    /// Formats the entire message to the output.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Format(ref FormatterContext ctx, StringBuilder output)
    {
        var elements = _elements;
        var len = elements.Length;

        // Unroll for common case: 1-4 elements
        if (len <= 4)
        {
            if (len > 0) elements[0].Format(ref ctx, output);
            if (len > 1) elements[1].Format(ref ctx, output);
            if (len > 2) elements[2].Format(ref ctx, output);
            if (len > 3) elements[3].Format(ref ctx, output);
            return;
        }

        // Fall back to loop for larger messages
        for (int i = 0; i < len; i++)
        {
            elements[i].Format(ref ctx, output);
        }
    }
}