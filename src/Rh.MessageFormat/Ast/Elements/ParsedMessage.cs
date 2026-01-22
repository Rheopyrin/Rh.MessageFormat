using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Rh.MessageFormat.Ast.Elements;

/// <summary>
/// Represents a parsed message with all elements pre-parsed.
/// Immutable and safe for caching (no Clone() needed).
/// </summary>
internal sealed class ParsedMessage
{
    private readonly List<MessageElement>? _elementsList;
    private readonly MessageElement[]? _elementsArray;
    private readonly string _originalPattern;

    public ParsedMessage(MessageElement[] elements, string originalPattern)
    {
        _elementsArray = elements;
        _elementsList = null;
        _originalPattern = originalPattern;
    }

    /// <summary>
    /// Creates a ParsedMessage from a list without copying.
    /// The list should not be modified after passing to this constructor.
    /// </summary>
    public ParsedMessage(List<MessageElement> elements, string originalPattern)
    {
        _elementsList = elements;
        _elementsArray = null;
        _originalPattern = originalPattern;
    }

    public ReadOnlySpan<MessageElement> Elements
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _elementsList != null
            ? CollectionsMarshal.AsSpan(_elementsList)
            : _elementsArray.AsSpan();
    }

    public string OriginalPattern
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _originalPattern;
    }

    public int ElementCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _elementsList?.Count ?? _elementsArray!.Length;
    }

    /// <summary>
    /// Formats the entire message to the output.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Format(ref FormatterContext ctx, StringBuilder output)
    {
        var elements = Elements;
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