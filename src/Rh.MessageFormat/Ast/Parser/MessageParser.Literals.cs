using System.Runtime.CompilerServices;
using Rh.MessageFormat.Ast.Elements;
using Rh.MessageFormat.Pools;
using static Rh.MessageFormat.Constants.Parser;

namespace Rh.MessageFormat.Ast.Parser;

/// <summary>
/// Literal text parsing methods.
/// </summary>
internal sealed partial class MessageParser
{
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
}