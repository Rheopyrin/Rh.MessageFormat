using System;
using System.Runtime.CompilerServices;
using static Rh.MessageFormat.Constants.Numbers;
using static Rh.MessageFormat.Constants.Numbers.Skeleton;

namespace Rh.MessageFormat.Formatting.Skeletons;

/// <summary>
/// Parses ICU number skeleton strings into NumberFormatOptions.
///
/// Supported skeleton tokens:
/// - percent, % - Format as percentage
/// - permille - Format as permille
/// - currency/XXX - Format as currency with given code
/// - compact-short, K - Compact notation (short)
/// - compact-long, KK - Compact notation (long)
/// - scientific - Scientific notation
/// - engineering - Engineering notation
/// - scale/N - Scale by factor N
/// - .00 - Exactly 2 fraction digits
/// - .## - At most 2 fraction digits
/// - .0# - 1 to 2 fraction digits
/// - @@@ - Exactly 3 significant digits
/// - sign-always, +! - Always show sign
/// - sign-never, +_ - Never show sign
/// - sign-except-zero, +? - Show sign except for zero
/// - sign-accounting, () - Accounting format
/// - group-off, ,_ - No grouping
/// - group-min2, ,? - Group only with 2+ digits
/// - integer-width/*000 - Minimum integer digits
/// - unit/XXX - Unit formatting
/// </summary>
internal static class NumberSkeletonParser
{
    /// <summary>
    /// Parses a number skeleton string into options.
    /// </summary>
    /// <param name="skeleton">The skeleton string (without :: prefix).</param>
    /// <returns>The parsed options.</returns>
    public static NumberFormatOptions Parse(string skeleton)
    {
        var options = new NumberFormatOptions();

        if (string.IsNullOrWhiteSpace(skeleton))
        {
            return options;
        }

        var span = skeleton.AsSpan().Trim();
        var pos = 0;

        while (pos < span.Length)
        {
            // Skip whitespace
            while (pos < span.Length && char.IsWhiteSpace(span[pos]))
            {
                pos++;
            }

            if (pos >= span.Length) break;

            // Find end of token (space or end)
            var tokenStart = pos;
            while (pos < span.Length && !char.IsWhiteSpace(span[pos]))
            {
                pos++;
            }

            var token = span.Slice(tokenStart, pos - tokenStart);
            ParseToken(token, options);
        }

        return options;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ParseToken(ReadOnlySpan<char> token, NumberFormatOptions options)
    {
        if (token.IsEmpty) return;

        // Check for precision patterns first (start with . or @)
        if (token[0] == Symbols.FractionStart)
        {
            ParseFractionPrecision(token, options);
            return;
        }

        if (token[0] == Symbols.SignificantDigit)
        {
            ParseSignificantDigitsPrecision(token, options);
            return;
        }

        // Check for concise notations
        if (token.Length == 1)
        {
            switch (token[0])
            {
                case PercentChar:
                    options.IsPercent = true;
                    return;
                case CompactShortChar:
                    options.Notation = NumberNotation.CompactShort;
                    return;
            }
        }

        if (token.Length == 2)
        {
            if (token[0] == CompactShortChar && token[1] == CompactShortChar)
            {
                options.Notation = NumberNotation.CompactLong;
                return;
            }
            if (token[0] == Symbols.Plus && token[1] == '!')
            {
                options.SignDisplay = SignDisplay.Always;
                return;
            }
            if (token[0] == Symbols.Plus && token[1] == '_')
            {
                options.SignDisplay = SignDisplay.Never;
                return;
            }
            if (token[0] == Symbols.Plus && token[1] == '?')
            {
                options.SignDisplay = SignDisplay.ExceptZero;
                return;
            }
            if (token[0] == '(' && token[1] == ')')
            {
                options.SignDisplay = SignDisplay.Accounting;
                return;
            }
            if (token[0] == ',' && token[1] == '_')
            {
                options.Grouping = GroupingStrategy.Off;
                return;
            }
            if (token[0] == ',' && token[1] == '?')
            {
                options.Grouping = GroupingStrategy.Min2;
                return;
            }
            if (token[0] == ',' && token[1] == '!')
            {
                options.Grouping = GroupingStrategy.Always;
                return;
            }
        }

        // Check for tokens with options (stem/option)
        var slashIndex = token.IndexOf('/');
        if (slashIndex > 0)
        {
            var stem = token.Slice(0, slashIndex);
            var option = token.Slice(slashIndex + 1);

            if (stem.SequenceEqual(CurrencyStem.AsSpan()))
            {
                options.CurrencyCode = option.ToString().ToUpperInvariant();
                return;
            }

            if (stem.SequenceEqual(ScaleStem.AsSpan()))
            {
                if (double.TryParse(option.ToString(), out var scale))
                {
                    options.Scale = scale;
                }
                return;
            }

            if (stem.SequenceEqual(UnitStem.AsSpan()) || stem.SequenceEqual(MeasureUnitStem.AsSpan()))
            {
                options.Unit = option.ToString();
                return;
            }

            if (stem.SequenceEqual(IntegerWidthStem.AsSpan()))
            {
                ParseIntegerWidth(option, options);
                return;
            }
        }

        // Check for string tokens
        if (token.SequenceEqual(Percent.AsSpan()))
        {
            options.IsPercent = true;
            return;
        }

        if (token.SequenceEqual(Permille.AsSpan()))
        {
            options.IsPermille = true;
            return;
        }

        if (token.SequenceEqual(Ordinal.AsSpan()))
        {
            options.IsOrdinal = true;
            return;
        }

        if (token.SequenceEqual(CompactShort.AsSpan()))
        {
            options.Notation = NumberNotation.CompactShort;
            return;
        }

        if (token.SequenceEqual(CompactLong.AsSpan()))
        {
            options.Notation = NumberNotation.CompactLong;
            return;
        }

        if (token.SequenceEqual(Scientific.AsSpan()))
        {
            options.Notation = NumberNotation.Scientific;
            return;
        }

        if (token.SequenceEqual(Engineering.AsSpan()))
        {
            options.Notation = NumberNotation.Engineering;
            return;
        }

        if (token.SequenceEqual(SignAlways.AsSpan()))
        {
            options.SignDisplay = SignDisplay.Always;
            return;
        }

        if (token.SequenceEqual(SignNever.AsSpan()))
        {
            options.SignDisplay = SignDisplay.Never;
            return;
        }

        if (token.SequenceEqual(SignExceptZero.AsSpan()))
        {
            options.SignDisplay = SignDisplay.ExceptZero;
            return;
        }

        if (token.SequenceEqual(SignAccounting.AsSpan()))
        {
            options.SignDisplay = SignDisplay.Accounting;
            return;
        }

        if (token.SequenceEqual(SignAccountingAlways.AsSpan()))
        {
            options.SignDisplay = SignDisplay.AccountingAlways;
            return;
        }

        if (token.SequenceEqual(SignAccountingExceptZero.AsSpan()))
        {
            options.SignDisplay = SignDisplay.AccountingExceptZero;
            return;
        }

        if (token.SequenceEqual(GroupOff.AsSpan()))
        {
            options.Grouping = GroupingStrategy.Off;
            return;
        }

        if (token.SequenceEqual(GroupMin2.AsSpan()))
        {
            options.Grouping = GroupingStrategy.Min2;
            return;
        }

        if (token.SequenceEqual(GroupAuto.AsSpan()))
        {
            options.Grouping = GroupingStrategy.Auto;
            return;
        }

        if (token.SequenceEqual(GroupOnAligned.AsSpan()) || token.SequenceEqual(GroupAlways.AsSpan()))
        {
            options.Grouping = GroupingStrategy.Always;
            return;
        }

        if (token.SequenceEqual(UnitWidthShort.AsSpan()))
        {
            options.UnitDisplay = UnitDisplay.Short;
            return;
        }

        if (token.SequenceEqual(UnitWidthNarrow.AsSpan()))
        {
            options.UnitDisplay = UnitDisplay.Narrow;
            return;
        }

        if (token.SequenceEqual(UnitWidthFullName.AsSpan()))
        {
            options.UnitDisplay = UnitDisplay.Long;
            options.CurrencyDisplay = CurrencyDisplay.Name;
            return;
        }

        if (token.SequenceEqual(UnitWidthIsoCode.AsSpan()))
        {
            options.CurrencyDisplay = CurrencyDisplay.Code;
            return;
        }

        if (token.SequenceEqual(CurrencySymbol.AsSpan()))
        {
            options.CurrencyDisplay = CurrencyDisplay.Symbol;
            return;
        }

        if (token.SequenceEqual(CurrencyNarrowSymbol.AsSpan()))
        {
            options.CurrencyDisplay = CurrencyDisplay.NarrowSymbol;
            return;
        }

        // Integer digits pattern: 000, 0000, etc.
        if (token.Length > 0 && token[0] == Symbols.RequiredDigit)
        {
            var allZeros = true;
            for (var i = 0; i < token.Length; i++)
            {
                if (token[i] != Symbols.RequiredDigit)
                {
                    allZeros = false;
                    break;
                }
            }

            if (allZeros)
            {
                options.MinimumIntegerDigits = token.Length;
            }
        }
    }

    private static void ParseFractionPrecision(ReadOnlySpan<char> token, NumberFormatOptions options)
    {
        // Patterns: .00, .##, .0#, .00#, .00*, etc.
        if (token.Length < 2) return;

        var minFraction = 0;
        var maxFraction = 0;
        var hasMinimum = false;

        for (var i = 1; i < token.Length; i++)
        {
            var c = token[i];
            if (c == Symbols.RequiredDigit)
            {
                minFraction++;
                maxFraction++;
                hasMinimum = true;
            }
            else if (c == Symbols.OptionalDigit)
            {
                maxFraction++;
            }
            else if (c == Symbols.Wildcard)
            {
                // .00* means at least minFraction digits, no maximum
                maxFraction = 15; // Arbitrary high number
            }
            else if (c == Symbols.Plus)
            {
                // Trailing zero display, ignore for now
            }
        }

        if (hasMinimum || maxFraction > 0)
        {
            options.MinimumFractionDigits = minFraction;
            options.MaximumFractionDigits = maxFraction;
        }
    }

    private static void ParseSignificantDigitsPrecision(ReadOnlySpan<char> token, NumberFormatOptions options)
    {
        // Patterns: @@@, @@#, @##, etc.
        var minSigDigits = 0;
        var maxSigDigits = 0;

        for (var i = 0; i < token.Length; i++)
        {
            var c = token[i];
            if (c == Symbols.SignificantDigit)
            {
                minSigDigits++;
                maxSigDigits++;
            }
            else if (c == Symbols.OptionalDigit)
            {
                maxSigDigits++;
            }
        }

        if (minSigDigits > 0 || maxSigDigits > 0)
        {
            options.MinimumSignificantDigits = minSigDigits;
            options.MaximumSignificantDigits = maxSigDigits;
        }
    }

    private static void ParseIntegerWidth(ReadOnlySpan<char> option, NumberFormatOptions options)
    {
        // Format: *000 or just 000
        var start = 0;
        if (option.Length > 0 && option[0] == Symbols.Wildcard)
        {
            start = 1;
        }

        var minDigits = 0;
        for (var i = start; i < option.Length; i++)
        {
            if (option[i] == Symbols.RequiredDigit)
            {
                minDigits++;
            }
            else
            {
                break;
            }
        }

        if (minDigits > 0)
        {
            options.MinimumIntegerDigits = minDigits;
        }
    }
}