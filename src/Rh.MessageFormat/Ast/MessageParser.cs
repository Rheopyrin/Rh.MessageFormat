using System.Collections.Generic;
using static Rh.MessageFormat.Constants.Parser;

namespace Rh.MessageFormat.Ast;

/// <summary>
/// Parses message patterns into ParsedMessage AST. Designed for performance with span-based parsing.
/// </summary>
internal sealed partial class MessageParser
{
    /// <summary>
    /// Parses a pattern into a ParsedMessage.
    /// </summary>
    public ParsedMessage Parse(string pattern) => Parse(pattern, ignoreTag: false);

    /// <summary>
    /// Parses a pattern into a ParsedMessage with optional tag ignoring.
    /// </summary>
    /// <param name="pattern">The message pattern to parse.</param>
    /// <param name="ignoreTag">
    /// When true, treats &lt;...&gt; as literal text instead of rich text tags.
    /// This is useful for HTML messages where tags should be preserved as-is.
    /// </param>
    public ParsedMessage Parse(string pattern, bool ignoreTag)
    {
        var elements = new List<MessageElement>();
        var pos = 0;
        var line = 1;
        var column = 1;

        while (pos < pattern.Length)
        {
            var c = pattern[pos];

            if (c == Chars.OpenBrace)
            {
                var startPos = pos;
                var startLine = line;
                var startColumn = column;

                // Parse placeholder
                var element = ParsePlaceholder(pattern, ref pos, ref line, ref column, startPos, startLine, startColumn);
                elements.Add(element);
            }
            else if (!ignoreTag && c == Chars.LessThan && IsTagStart(pattern, pos))
            {
                var startPos = pos;
                var startLine = line;
                var startColumn = column;

                // Parse rich text tag (only when ignoreTag is false)
                var element = ParseTag(pattern, ref pos, ref line, ref column, startPos, startLine, startColumn);
                elements.Add(element);
            }
            else
            {
                // Parse literal text
                var literal = ParseLiteralText(pattern, ref pos, ref line, ref column, ignoreTag);
                if (!string.IsNullOrEmpty(literal.Text))
                {
                    elements.Add(literal);
                }
            }
        }

        return new ParsedMessage(elements, pattern);
    }
}
