using Rh.MessageFormat.Exceptions;
using static Rh.MessageFormat.Constants.Parser;

namespace Rh.MessageFormat.Ast;

/// <summary>
/// Placeholder parsing methods.
/// </summary>
internal sealed partial class MessageParser
{
    private MessageElement ParsePlaceholder(string pattern, ref int pos, ref int line, ref int column,
        int startPos, int startLine, int startColumn)
    {
        // Skip opening brace
        pos++;
        column++;

        // Parse variable name
        SkipWhitespace(pattern, ref pos, ref line, ref column);
        var variable = ParseIdentifier(pattern, ref pos, ref column);

        SkipWhitespace(pattern, ref pos, ref line, ref column);

        // Check for formatter type
        string? formatterName = null;
        string? arguments = null;

        if (pos < pattern.Length && pattern[pos] == Chars.Comma)
        {
            pos++;
            column++;

            SkipWhitespace(pattern, ref pos, ref line, ref column);
            formatterName = ParseIdentifier(pattern, ref pos, ref column);

            SkipWhitespace(pattern, ref pos, ref line, ref column);

            // Check for arguments
            if (pos < pattern.Length && pattern[pos] == Chars.Comma)
            {
                pos++;
                column++;

                SkipWhitespace(pattern, ref pos, ref line, ref column);

                // Collect everything until the matching closing brace
                arguments = CollectArguments(pattern, ref pos, ref line, ref column);
            }
        }

        // Find and skip closing brace
        SkipWhitespace(pattern, ref pos, ref line, ref column);
        if (pos < pattern.Length && pattern[pos] == Chars.CloseBrace)
        {
            pos++;
            column++;
        }
        else
        {
            throw new MessageFormatterException($"Expected closing brace at line {line}, column {column}");
        }

        var span = new SourceSpan(startPos, pos - 1, startLine, startColumn);

        // Create appropriate element type
        return CreateElement(variable, formatterName, arguments, span);
    }

    private string CollectArguments(string pattern, ref int pos, ref int line, ref int column)
    {
        var start = pos;
        var braceBalance = 1; // We're inside the main placeholder's braces
        var insideEscapeSequence = false;

        while (pos < pattern.Length && braceBalance > 0)
        {
            var c = pattern[pos];

            if (c == Chars.Quote)
            {
                if (pos + 1 < pattern.Length)
                {
                    var next = pattern[pos + 1];
                    if (next == Chars.Quote)
                    {
                        pos += 2;
                        column += 2;
                        continue;
                    }
                    if (next == Chars.OpenBrace || next == Chars.CloseBrace || next == Chars.Hash)
                    {
                        insideEscapeSequence = true;
                        pos += 2;
                        column += 2;
                        continue;
                    }
                }

                if (insideEscapeSequence)
                {
                    insideEscapeSequence = false;
                }

                pos++;
                column++;
                continue;
            }

            if (!insideEscapeSequence)
            {
                if (c == Chars.OpenBrace)
                {
                    braceBalance++;
                }
                else if (c == Chars.CloseBrace)
                {
                    braceBalance--;
                    if (braceBalance == 0)
                    {
                        // Don't include the final closing brace
                        return pattern.Substring(start, pos - start);
                    }
                }
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

            pos++;
        }

        return pattern.Substring(start, pos - start);
    }
}
