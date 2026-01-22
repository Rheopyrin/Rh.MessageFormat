using System;
using System.Collections.Generic;
using System.Globalization;
using Rh.MessageFormat.Ast.Elements;
using Rh.MessageFormat.Exceptions;
using static Rh.MessageFormat.Constants;
using static Rh.MessageFormat.Constants.Parser;

namespace Rh.MessageFormat.Ast.Parser;

/// <summary>
/// Plural and select parsing methods.
/// </summary>
internal sealed partial class MessageParser
{
    private PluralElement CreatePluralElement(string variable, string? arguments, SourceSpan span)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            throw new MessageFormatterException($"Plural element requires cases at line {span.Line}, column {span.Column}");
        }

        var (offset, cases) = ParsePluralArguments(arguments);
        return new PluralElement(variable, cases, offset, span);
    }

    private SelectElement CreateSelectElement(string variable, string? arguments, SourceSpan span)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            throw new MessageFormatterException($"Select element requires cases at line {span.Line}, column {span.Column}");
        }

        var cases = ParseSelectArguments(arguments);
        return new SelectElement(variable, cases, span);
    }

    private SelectOrdinalElement CreateSelectOrdinalElement(string variable, string? arguments, SourceSpan span)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            throw new MessageFormatterException($"Selectordinal element requires cases at line {span.Line}, column {span.Column}");
        }

        var (offset, cases) = ParsePluralArguments(arguments);
        return new SelectOrdinalElement(variable, cases, offset, span);
    }

    private (double offset, PluralCase[] cases) ParsePluralArguments(string arguments)
    {
        double offset = 0;
        var cases = new List<PluralCase>();

        var pos = 0;
        var line = 1;
        var column = 1;

        // Check for offset
        SkipWhitespace(arguments, ref pos, ref line, ref column);

        if (pos < arguments.Length && arguments.AsSpan(pos).StartsWith(Sequences.OffsetKeyword.AsSpan()))
        {
            pos += Sequences.OffsetKeyword.Length;
            column += Sequences.OffsetKeyword.Length;

            // Parse offset value
            var offsetStart = pos;
            while (pos < arguments.Length && (char.IsDigit(arguments[pos]) || arguments[pos] == Chars.Dot || arguments[pos] == Chars.Dash))
            {
                pos++;
                column++;
            }

            if (pos > offsetStart)
            {
                offset = double.Parse(arguments.AsSpan(offsetStart, pos - offsetStart), CultureInfo.InvariantCulture);
            }

            SkipWhitespace(arguments, ref pos, ref line, ref column);
        }

        // Parse cases
        while (pos < arguments.Length)
        {
            SkipWhitespace(arguments, ref pos, ref line, ref column);

            if (pos >= arguments.Length)
                break;

            // Parse key
            var key = ParseKey(arguments, ref pos, ref column);
            if (string.IsNullOrEmpty(key))
                break;

            SkipWhitespace(arguments, ref pos, ref line, ref column);

            // Parse content block
            if (pos >= arguments.Length || arguments[pos] != Chars.OpenBrace)
            {
                throw new MessageFormatterException($"Expected '{{' after key '{key}' at line {line}, column {column}");
            }

            var content = ParseNestedBlock(arguments, ref pos, ref line, ref column);
            var parsedContent = Parse(content);
            cases.Add(new PluralCase(InternKey(key), parsedContent));
        }

        return (offset, cases.ToArray());
    }

    private SelectCase[] ParseSelectArguments(string arguments)
    {
        var cases = new List<SelectCase>();

        var pos = 0;
        var line = 1;
        var column = 1;

        // Parse cases
        while (pos < arguments.Length)
        {
            SkipWhitespace(arguments, ref pos, ref line, ref column);

            if (pos >= arguments.Length)
                break;

            // Parse key
            var key = ParseKey(arguments, ref pos, ref column);
            if (string.IsNullOrEmpty(key))
                break;

            SkipWhitespace(arguments, ref pos, ref line, ref column);

            // Parse content block
            if (pos >= arguments.Length || arguments[pos] != Chars.OpenBrace)
            {
                throw new MessageFormatterException($"Expected '{{' after key '{key}' at line {line}, column {column}");
            }

            var content = ParseNestedBlock(arguments, ref pos, ref line, ref column);
            var parsedContent = Parse(content);
            cases.Add(new SelectCase(InternKey(key), parsedContent));
        }

        return cases.ToArray();
    }

    private string ParseNestedBlock(string text, ref int pos, ref int line, ref int column)
    {
        if (pos >= text.Length || text[pos] != Chars.OpenBrace)
        {
            return Common.Empty;
        }

        pos++;
        column++;

        var start = pos;
        var braceBalance = 1;
        var insideEscapeSequence = false;

        while (pos < text.Length && braceBalance > 0)
        {
            var c = text[pos];

            if (c == Chars.Quote)
            {
                if (pos + 1 < text.Length)
                {
                    var next = text[pos + 1];
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
            }
            else if (!insideEscapeSequence)
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
                        var result = text.Substring(start, pos - start);
                        pos++;
                        column++;
                        return result;
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

        return text.Substring(start);
    }
}