using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Rh.MessageFormat.Formatting.Skeletons;
using static Rh.MessageFormat.Constants;
using static Rh.MessageFormat.Constants.Parser;

namespace Rh.MessageFormat.Ast;

/// <summary>
/// Parses message patterns into ParsedMessage AST. Designed for performance with span-based parsing.
/// </summary>
internal sealed class MessageParser
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

        return new ParsedMessage(elements.ToArray(), pattern);
    }

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
            // Check for closing tag
            if (pattern.Length - pos >= closingTag.Length &&
                pattern.Substring(pos, closingTag.Length) == closingTag)
            {
                // Skip closing tag
                pos += closingTag.Length;
                column += closingTag.Length;

                var span = new SourceSpan(startPos, pos - 1, startLine, startColumn);
                var content = new ParsedMessage(contentElements.ToArray(), pattern.Substring(startPos, pos - startPos));
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
                var literal = ParseTagContent(pattern, ref pos, ref line, ref column, tagName);
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
    private LiteralElement ParseTagContent(string pattern, ref int pos, ref int line, ref int column, string tagName)
    {
        var start = pos;
        var startLine = line;
        var startColumn = column;
        var sb = StringBuilderPool.Get();
        var closingTag = Sequences.CloseTag + tagName + Chars.GreaterThan;

        try
        {
            while (pos < pattern.Length)
            {
                // Check for closing tag (don't consume it)
                if (pattern.Length - pos >= closingTag.Length &&
                    pattern.Substring(pos, closingTag.Length) == closingTag)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private LiteralElement ParseLiteralText(string pattern, ref int pos, ref int line, ref int column, bool ignoreTag = false)
    {
        var start = pos;
        var startLine = line;
        var startColumn = column;
        var sb = StringBuilderPool.Get();

        try
        {
            while (pos < pattern.Length)
            {
                var c = pattern[pos];

                if (c == Chars.OpenBrace)
                {
                    // End of literal - placeholder starts
                    break;
                }

                if (!ignoreTag && c == Chars.LessThan && IsTagStart(pattern, pos))
                {
                    // End of literal - tag starts (only when ignoreTag is false)
                    break;
                }

                if (c == Chars.Quote)
                {
                    // Handle escape sequences
                    if (pos + 1 < pattern.Length)
                    {
                        var next = pattern[pos + 1];
                        if (next == Chars.Quote)
                        {
                            // '' -> '
                            sb.Append(Chars.Quote);
                            pos += 2;
                            column += 2;
                            continue;
                        }
                        if (next == Chars.OpenBrace || next == Chars.CloseBrace || next == Chars.Hash)
                        {
                            // Escape sequence - include the escaped char
                            sb.Append(next);
                            pos += 2;
                            column += 2;

                            // Continue until closing quote
                            while (pos < pattern.Length && pattern[pos] != Chars.Quote)
                            {
                                if (pattern[pos] == Chars.Newline)
                                {
                                    line++;
                                    column = 1;
                                }
                                else
                                {
                                    column++;
                                }
                                sb.Append(pattern[pos]);
                                pos++;
                            }

                            // Skip closing quote if present
                            if (pos < pattern.Length && pattern[pos] == Chars.Quote)
                            {
                                pos++;
                                column++;
                            }
                            continue;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MessageElement CreateElement(string variable, string? formatter, string? arguments, SourceSpan span)
    {
        if (formatter == null)
        {
            return new ArgumentElement(variable, span);
        }

        return formatter switch
        {
            Formatters.Number => CreateNumberElement(variable, arguments, span),
            Formatters.Date => CreateDateElement(variable, arguments, span),
            Formatters.Time => CreateTimeElement(variable, arguments, span),
            Formatters.DateTime => CreateDateTimeElement(variable, arguments, span),
            Formatters.Plural => CreatePluralElement(variable, arguments, span),
            Formatters.Select => CreateSelectElement(variable, arguments, span),
            Formatters.SelectOrdinal => CreateSelectOrdinalElement(variable, arguments, span),
            Formatters.List => CreateListElement(variable, arguments, span),
            _ => new CustomFormatterElement(variable, formatter, arguments?.Trim(), span) // Custom formatter
        };
    }

    private NumberElement CreateNumberElement(string variable, string? arguments, SourceSpan span)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return new NumberElement(variable, NumberStyle.Default, span);
        }

        var trimmed = arguments.Trim();

        // Check for skeleton syntax (prefixed with ::)
        if (trimmed.StartsWith(Sequences.SkeletonPrefix))
        {
            var skeleton = trimmed.Substring(2);
            var options = NumberSkeletonParser.Parse(skeleton);
            return new NumberElement(variable, options, span);
        }

        // Check for predefined styles
        var lower = trimmed.ToLowerInvariant();
        var style = lower switch
        {
            Styles.Integer => NumberStyle.Integer,
            Styles.Currency => NumberStyle.Currency,
            Styles.Percent => NumberStyle.Percent,
            _ => NumberStyle.Default
        };

        // If not a predefined style, treat as custom format string
        if (style == NumberStyle.Default && trimmed != Common.Empty)
        {
            return new NumberElement(variable, trimmed, span);
        }

        return new NumberElement(variable, style, span);
    }

    private DateElement CreateDateElement(string variable, string? arguments, SourceSpan span)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return new DateElement(variable, DateStyle.Default, span);
        }

        var trimmed = arguments.Trim();

        // Check for skeleton syntax (prefixed with ::)
        if (trimmed.StartsWith(Sequences.SkeletonPrefix))
        {
            var skeleton = trimmed.Substring(2);
            return new DateElement(variable, skeleton, isSkeleton: true, span);
        }

        var lower = trimmed.ToLowerInvariant();

        // Check for predefined styles
        var style = lower switch
        {
            Styles.Short => DateStyle.Short,
            Styles.Medium => DateStyle.Medium,
            Styles.Long => DateStyle.Long,
            Styles.Full => DateStyle.Full,
            _ => DateStyle.Custom
        };

        // If it's a custom format, use the original (non-lowercased) format string
        if (style == DateStyle.Custom)
        {
            return new DateElement(variable, trimmed, span);
        }

        return new DateElement(variable, style, span);
    }

    private TimeElement CreateTimeElement(string variable, string? arguments, SourceSpan span)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return new TimeElement(variable, TimeStyle.Default, span);
        }

        var trimmed = arguments.Trim();

        // Check for skeleton syntax (prefixed with ::)
        if (trimmed.StartsWith(Sequences.SkeletonPrefix))
        {
            var skeleton = trimmed.Substring(2);
            return new TimeElement(variable, skeleton, isSkeleton: true, span);
        }

        var lower = trimmed.ToLowerInvariant();

        // Check for predefined styles
        var style = lower switch
        {
            Styles.Short => TimeStyle.Short,
            Styles.Medium => TimeStyle.Medium,
            Styles.Long => TimeStyle.Long,
            Styles.Full => TimeStyle.Full,
            _ => TimeStyle.Custom
        };

        // If it's a custom format, use the original (non-lowercased) format string
        if (style == TimeStyle.Custom)
        {
            return new TimeElement(variable, trimmed, span);
        }

        return new TimeElement(variable, style, span);
    }

    private DateTimeElement CreateDateTimeElement(string variable, string? arguments, SourceSpan span)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return new DateTimeElement(variable, DateTimeStyle.Default, span);
        }

        var trimmed = arguments.Trim();

        // Check for skeleton syntax (prefixed with ::)
        if (trimmed.StartsWith(Sequences.SkeletonPrefix))
        {
            var skeleton = trimmed.Substring(2);
            return new DateTimeElement(variable, skeleton, isSkeleton: true, span);
        }

        var lower = trimmed.ToLowerInvariant();

        // Check for predefined styles
        var style = lower switch
        {
            Styles.Short => DateTimeStyle.Short,
            Styles.Medium => DateTimeStyle.Medium,
            Styles.Long => DateTimeStyle.Long,
            Styles.Full => DateTimeStyle.Full,
            _ => DateTimeStyle.Custom
        };

        // If it's a custom format, use the original (non-lowercased) format string
        if (style == DateTimeStyle.Custom)
        {
            return new DateTimeElement(variable, trimmed, span);
        }

        return new DateTimeElement(variable, style, span);
    }

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

    private ListElement CreateListElement(string variable, string? arguments, SourceSpan span)
    {
        var style = ListStyle.Conjunction;
        var width = ListWidth.Long;

        if (!string.IsNullOrWhiteSpace(arguments))
        {
            var trimmed = arguments.Trim().ToLowerInvariant();

            // Parse style: conjunction, disjunction, unit
            // Parse width: long, short, narrow
            // Can be combined: "conjunction long", "disjunction short", etc.

            if (trimmed.Contains(List.StyleTypes.Disjunction))
            {
                style = ListStyle.Disjunction;
            }
            else if (trimmed.Contains(List.StyleTypes.Unit))
            {
                style = ListStyle.Unit;
            }

            if (trimmed.Contains(Styles.Narrow))
            {
                width = ListWidth.Narrow;
            }
            else if (trimmed.Contains(Styles.Short))
            {
                width = ListWidth.Short;
            }
        }

        return new ListElement(variable, style, width, span);
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

        if (pos < arguments.Length && arguments.Substring(pos).StartsWith(Sequences.OffsetKeyword))
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
                offset = double.Parse(arguments.Substring(offsetStart, pos - offsetStart), CultureInfo.InvariantCulture);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SkipWhitespace(string text, ref int pos, ref int line, ref int column)
    {
        while (pos < text.Length && char.IsWhiteSpace(text[pos]))
        {
            if (text[pos] == Chars.Newline)
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
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ParseIdentifier(string text, ref int pos, ref int column)
    {
        var start = pos;
        while (pos < text.Length && (char.IsLetterOrDigit(text[pos]) || text[pos] == Chars.Underscore || text[pos] == Chars.Dash))
        {
            pos++;
            column++;
        }
        return text.Substring(start, pos - start);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ParseKey(string text, ref int pos, ref int column)
    {
        var start = pos;

        // Keys can be: identifiers, =N (exact match)
        if (pos < text.Length && text[pos] == Chars.EqualsSign)
        {
            pos++;
            column++;

            // Parse number after =
            while (pos < text.Length && (char.IsDigit(text[pos]) || text[pos] == Chars.Dot || text[pos] == Chars.Dash))
            {
                pos++;
                column++;
            }
        }
        else
        {
            // Regular identifier
            while (pos < text.Length && (char.IsLetterOrDigit(text[pos]) || text[pos] == Chars.Underscore || text[pos] == Chars.Dash))
            {
                pos++;
                column++;
            }
        }

        return text.Substring(start, pos - start);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string InternKey(string key)
    {
        var commonKeys = Plurals.CommonKeys;
        for (int i = 0; i < commonKeys.Length; i++)
        {
            if (key == commonKeys[i])
            {
                return commonKeys[i];
            }
        }
        return key;
    }
}
