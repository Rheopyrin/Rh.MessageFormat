using System.Runtime.CompilerServices;
using static Rh.MessageFormat.Constants;
using static Rh.MessageFormat.Constants.Parser;

namespace Rh.MessageFormat.Ast;

/// <summary>
/// Utility methods for parsing.
/// </summary>
internal sealed partial class MessageParser
{
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
