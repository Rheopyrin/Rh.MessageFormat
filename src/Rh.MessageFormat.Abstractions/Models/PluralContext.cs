using System;
using System.Globalization;

namespace Rh.MessageFormat.Abstractions.Models;

/// <summary>
/// Contains the operands used in CLDR plural rules.
/// Based on Unicode CLDR plural operand definitions.
/// </summary>
public readonly struct PluralContext
{
    /// <summary>
    /// Creates a PluralContext from an integer.
    /// </summary>
    public PluralContext(int number)
    {
        Number = number;
        // Use long cast to avoid overflow for int.MinValue
        N = Math.Abs((long)number);
        I = number;
        V = 0;
        W = 0;
        F = 0;
        T = 0;
        C = 0;
        E = 0;
    }

    /// <summary>
    /// Creates a PluralContext from a decimal.
    /// </summary>
    public PluralContext(decimal number) : this(number.ToString(CultureInfo.InvariantCulture), (double)number)
    {
    }

    /// <summary>
    /// Creates a PluralContext from a double.
    /// </summary>
    public PluralContext(double number) : this(number.ToString(CultureInfo.InvariantCulture), number)
    {
    }

    /// <summary>
    /// Creates a PluralContext from a string representation of a number.
    /// </summary>
    public PluralContext(string number) : this(number, double.Parse(number, CultureInfo.InvariantCulture))
    {
    }

    private PluralContext(string number, double parsed)
    {
        Number = parsed;
        N = Math.Abs(parsed);
        // Use long to avoid overflow for large numbers (preserves sign)
        I = (long)parsed;

        var dotIndex = number.IndexOf('.');
        if (dotIndex == -1)
        {
            V = 0;
            W = 0;
            F = 0;
            T = 0;
            C = 0;
            E = 0;
        }
        else
        {
            var fractionPart = number.Substring(dotIndex + 1);
            var fractionPartTrimmed = fractionPart.TrimEnd('0');

            V = fractionPart.Length;
            W = fractionPartTrimmed.Length;
            // Use long.Parse to avoid overflow for long fraction strings
            F = fractionPart.Length > 0 ? long.Parse(fractionPart) : 0;
            T = fractionPartTrimmed.Length > 0 ? long.Parse(fractionPartTrimmed) : 0;
            C = 0;
            E = 0;
        }
    }

    /// <summary>
    /// The original number value.
    /// </summary>
    public double Number { get; }

    /// <summary>
    /// n - absolute value of the source number.
    /// </summary>
    public double N { get; }

    /// <summary>
    /// i - integer digits of n.
    /// </summary>
    public long I { get; }

    /// <summary>
    /// v - number of visible fraction digits in n, with trailing zeros.
    /// </summary>
    public int V { get; }

    /// <summary>
    /// w - number of visible fraction digits in n, without trailing zeros.
    /// </summary>
    public int W { get; }

    /// <summary>
    /// f - visible fraction digits in n, with trailing zeros.
    /// </summary>
    public long F { get; }

    /// <summary>
    /// t - visible fraction digits in n, without trailing zeros.
    /// </summary>
    public long T { get; }

    /// <summary>
    /// c/e - compact decimal exponent value (currently always 0).
    /// </summary>
    public int C { get; }

    /// <summary>
    /// e - synonym for c (compact decimal exponent).
    /// </summary>
    public int E { get; }
}
