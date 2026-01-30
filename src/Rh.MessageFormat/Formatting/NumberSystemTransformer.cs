using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Rh.MessageFormat.Formatting;

/// <summary>
/// Transforms digits between numbering systems using CLDR data.
/// </summary>
internal static class NumberSystemTransformer
{
    /// <summary>
    /// Transforms Latin digits (0-9) to the specified numbering system.
    /// </summary>
    /// <param name="input">The input string containing Latin digits.</param>
    /// <param name="digits">The 10 digit characters for the target numbering system (0-9).</param>
    /// <returns>The transformed string, or the original if no transformation is needed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Transform(string input, string digits)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(digits))
            return input;

        // Handle both simple (single char per digit) and complex (multi-char) numbering systems
        var digitChars = GetDigitChars(digits);
        if (digitChars == null || digitChars.Length != 10)
            return input;

        // Fast path: check if any Latin digits are present
        var hasDigits = false;
        foreach (var c in input)
        {
            if (c >= '0' && c <= '9')
            {
                hasDigits = true;
                break;
            }
        }

        if (!hasDigits)
            return input;

        // Check if we can use the fast path (all target digits are single chars)
        var allSingleChar = true;
        foreach (var d in digitChars)
        {
            if (d.Length != 1)
            {
                allSingleChar = false;
                break;
            }
        }

        if (allSingleChar)
        {
            return TransformSingleChar(input, digitChars);
        }
        else
        {
            return TransformMultiChar(input, digitChars);
        }
    }

    /// <summary>
    /// Fast path transformation when all target digits are single characters.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string TransformSingleChar(string input, string[] digitChars)
    {
        Span<char> result = input.Length <= 256
            ? stackalloc char[input.Length]
            : new char[input.Length];

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            result[i] = (c >= '0' && c <= '9') ? digitChars[c - '0'][0] : c;
        }

        return new string(result);
    }

    /// <summary>
    /// Slower path for numbering systems where some digits are multi-character (e.g., surrogate pairs).
    /// </summary>
    private static string TransformMultiChar(string input, string[] digitChars)
    {
        // Calculate max possible output length
        var maxLen = 0;
        var maxDigitLen = 1;
        foreach (var d in digitChars)
        {
            if (d.Length > maxDigitLen)
                maxDigitLen = d.Length;
        }
        maxLen = input.Length * maxDigitLen;

        Span<char> result = maxLen <= 512
            ? stackalloc char[maxLen]
            : new char[maxLen];

        var resultIndex = 0;
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c >= '0' && c <= '9')
            {
                var digit = digitChars[c - '0'];
                foreach (var dc in digit)
                {
                    result[resultIndex++] = dc;
                }
            }
            else
            {
                result[resultIndex++] = c;
            }
        }

        return new string(result.Slice(0, resultIndex));
    }

    /// <summary>
    /// Parses the digit string into individual grapheme clusters.
    /// </summary>
    private static string[]? GetDigitChars(string digits)
    {
        var enumerator = StringInfo.GetTextElementEnumerator(digits);
        var result = new string[10];
        var index = 0;

        while (enumerator.MoveNext() && index < 10)
        {
            result[index++] = enumerator.GetTextElement();
        }

        return index == 10 ? result : null;
    }
}
