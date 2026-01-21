using System.Runtime.CompilerServices;

namespace Rh.MessageFormat.Ast;

/// <summary>
/// Represents a position span in the source pattern.
/// </summary>
internal readonly struct SourceSpan
{
    public readonly int StartIndex;
    public readonly int EndIndex;
    public readonly int Line;
    public readonly int Column;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SourceSpan(int startIndex, int endIndex, int line, int column)
    {
        StartIndex = startIndex;
        EndIndex = endIndex;
        Line = line;
        Column = column;
    }

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => EndIndex - StartIndex + 1;
    }

    public static SourceSpan Empty => default;
}
