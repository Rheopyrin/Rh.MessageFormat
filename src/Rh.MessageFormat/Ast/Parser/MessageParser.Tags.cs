using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rh.MessageFormat.Ast.Elements;
using Rh.MessageFormat.Exceptions;
using Rh.MessageFormat.Pools;
using static Rh.MessageFormat.Constants.Parser;

namespace Rh.MessageFormat.Ast.Parser;

/// <summary>
/// Tag parsing methods.
/// </summary>
internal sealed partial class MessageParser
{
    /// <summary>
    /// Checks if the position marks the start of a tag (not just a literal &lt; character).
    /// A tag starts with &lt; followed by a letter (opening tag) or &lt;/ (closing tag).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsTagStart(string pattern, int pos)
    {
        if (pos + 1 >= pattern.Length)
            return false;

        var next = pattern[pos + 1];

        // Opening tag: <letter
        if (char.IsLetter(next))
            return true;

        // Closing tag: </
        if (next == Chars.Slash && pos + 2 < pattern.Length && char.IsLetter(pattern[pos + 2]))
            return true;

        return false;
    }

    /// <summary>
    /// Parses an XML-like tag: &lt;tagName&gt;content&lt;/tagName&gt;
    /// </summary>
    private TagElement ParseTag(string pattern, ref int pos, ref int line, ref int column,
        int startPos, int startLine, int startColumn)
    {
        // Skip opening <
        pos++;
        column++;

        // Parse tag name
        var tagName = ParseIdentifier(pattern, ref pos, ref column);
        if (string.IsNullOrEmpty(tagName))
        {
            throw new MessageFormatterException($"Expected tag name at line {line}, column {column}");
        }

        // Skip > (closing of opening tag)
        if (pos >= pattern.Length || pattern[pos] != Chars.GreaterThan)
        {
            throw new MessageFormatterException($"Expected '>' after tag name '{tagName}' at line {line}, column {column}");
        }
        pos++;
        column++;

        // Parse content until closing tag </tagName>
        var contentElements = new List<MessageElement>();
        var closingTag = Sequences.CloseTag + tagName + Chars.GreaterThan;

        while (pos < pattern.Length)
        {
            // Check for closing tag using span comparison to avoid allocation
            if (pattern.Length - pos >= closingTag.Length &&
                pattern.AsSpan(pos, closingTag.Length).SequenceEqual(closingTag.AsSpan()))
            {
                // Skip closing tag
                pos += closingTag.Length;
                column += closingTag.Length;

                var span = new SourceSpan(startPos, pos - 1, startLine, startColumn);
                var content = new ParsedMessage(contentElements, pattern.Substring(startPos, pos - startPos));
                return new TagElement(tagName, content, span);
            }

            var c = pattern[pos];

            if (c == Chars.OpenBrace)
            {
                var innerStartPos = pos;
                var innerStartLine = line;
                var innerStartColumn = column;

                var element = ParsePlaceholder(pattern, ref pos, ref line, ref column, innerStartPos, innerStartLine, innerStartColumn);
                contentElements.Add(element);
            }
            else if (c == Chars.LessThan && IsTagStart(pattern, pos))
            {
                var innerStartPos = pos;
                var innerStartLine = line;
                var innerStartColumn = column;

                // Nested tag
                var element = ParseTag(pattern, ref pos, ref line, ref column, innerStartPos, innerStartLine, innerStartColumn);
                contentElements.Add(element);
            }
            else
            {
                // Parse literal text (stops at { or <)
                var literal = ParseTagContent(pattern, ref pos, ref line, ref column, closingTag);
                if (!string.IsNullOrEmpty(literal.Text))
                {
                    contentElements.Add(literal);
                }
            }
        }

        throw new MessageFormatterException($"Unclosed tag '<{tagName}>' starting at line {startLine}, column {startColumn}");
    }

    /// <summary>
    /// Parses literal text inside a tag, stopping at { or &lt; or the closing tag.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private LiteralElement ParseTagContent(string pattern, ref int pos, ref int line, ref int column, string closingTag)
    {
        var start = pos;
        var startLine = line;
        var startColumn = column;
        var sb = StringBuilderPool.Get();

        try
        {
            while (pos < pattern.Length)
            {
                // Check for closing tag using span comparison (don't consume it)
                if (pattern.Length - pos >= closingTag.Length &&
                    pattern.AsSpan(pos, closingTag.Length).SequenceEqual(closingTag.AsSpan()))
                {
                    break;
                }

                var c = pattern[pos];

                if (c == Chars.OpenBrace)
                {
                    // End of literal - placeholder starts
                    break;
                }

                if (c == Chars.LessThan && IsTagStart(pattern, pos))
                {
                    // End of literal - nested tag starts
                    break;
                }

                if (c == Chars.Newline)
                {
                    line++;
                    column = 1;
                }
                else
                {
                    column++;
                }

                sb.Append(c);
                pos++;
            }

            var text = sb.ToString();
            var span = new SourceSpan(start, pos - 1, startLine, startColumn);
            return new LiteralElement(text, span);
        }
        finally
        {
            StringBuilderPool.Return(sb);
        }
    }
}