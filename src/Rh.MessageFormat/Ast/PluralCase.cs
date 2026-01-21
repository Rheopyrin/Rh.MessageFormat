using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Rh.MessageFormat.Pools;

namespace Rh.MessageFormat.Ast;

/// <summary>
/// Represents a case in a plural element (e.g., "one", "other", "=5").
/// </summary>
internal sealed class PluralCase
{
    private readonly string _key;
    private readonly ParsedMessage _content;
    private readonly double? _exactMatch;

    public PluralCase(string key, ParsedMessage content)
    {
        _key = key;
        _content = content;

        // Pre-parse exact match keys like "=5"
        if (key.Length > 1 && key[0] == '=')
        {
            if (double.TryParse(key.Substring(1), NumberStyles.Any, CultureInfo.InvariantCulture, out var exactValue))
            {
                _exactMatch = exactValue;
            }
        }
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
    /// If this case is an exact match (e.g., "=5"), returns the exact value.
    /// </summary>
    public double? ExactMatch
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _exactMatch;
    }

    /// <summary>
    /// Formats the case content with # replaced by the actual number.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Format(ref FormatterContext ctx, StringBuilder output, double number)
    {
        // First format the content
        var contentSb = StringBuilderPool.Get();
        try
        {
            _content.Format(ref ctx, contentSb);

            // Replace # with the actual number
            ReplaceNumberLiteral(contentSb, number, output);
        }
        finally
        {
            StringBuilderPool.Return(contentSb);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ReplaceNumberLiteral(StringBuilder source, double number, StringBuilder output)
    {
        const char Pound = '#';
        const char OpenBrace = '{';
        const char CloseBrace = '}';
        const char EscapeChar = '\'';

        var braceBalance = 0;
        var insideEscapeSequence = false;

        for (int i = 0; i < source.Length; i++)
        {
            var c = source[i];

            if (c == EscapeChar)
            {
                output.Append(EscapeChar);

                if (i == source.Length - 1)
                {
                    if (insideEscapeSequence)
                        insideEscapeSequence = false;
                    continue;
                }

                var nextChar = source[i + 1];
                if (nextChar == EscapeChar)
                {
                    output.Append(EscapeChar);
                    i++;
                    continue;
                }

                if (insideEscapeSequence)
                {
                    insideEscapeSequence = false;
                    continue;
                }

                if (nextChar == '{' || nextChar == '}' || nextChar == '#')
                {
                    output.Append(nextChar);
                    insideEscapeSequence = true;
                    i++;
                    continue;
                }

                continue;
            }

            if (insideEscapeSequence)
            {
                output.Append(c);
                continue;
            }

            if (c == OpenBrace)
            {
                braceBalance++;
            }
            else if (c == CloseBrace)
            {
                braceBalance--;
            }
            else if (c == Pound && braceBalance == 0)
            {
                output.Append(number);
                continue;
            }

            output.Append(c);
        }
    }
}
