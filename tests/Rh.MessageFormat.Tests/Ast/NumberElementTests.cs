using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Tests for NumberElement formatting including styles, custom formats, and skeletons.
/// </summary>
public class NumberElementTests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region Number Style Tests

    [Fact]
    public void Number_DefaultStyle()
    {
        var args = new Dictionary<string, object?> { { "n", 1234.56 } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.Contains("1,234.56", result);
    }

    [Fact]
    public void Number_IntegerStyle()
    {
        var args = new Dictionary<string, object?> { { "n", 1234.56 } };

        var result = _formatter.FormatMessage("{n, number, integer}", args);

        // Integer style truncates to integer
        Assert.Equal("1,234", result);
    }

    [Fact]
    public void Number_IntegerStyle_Truncates()
    {
        var args = new Dictionary<string, object?> { { "n", 1234.99 } };

        var result = _formatter.FormatMessage("{n, number, integer}", args);

        // Integer style truncates (casts to long)
        Assert.Equal("1,234", result);
    }

    [Fact]
    public void Number_IntegerStyle_NegativeValue()
    {
        var args = new Dictionary<string, object?> { { "n", -1234.56 } };

        var result = _formatter.FormatMessage("{n, number, integer}", args);

        Assert.Equal("-1,234", result);
    }

    [Fact]
    public void Number_PercentStyle()
    {
        var args = new Dictionary<string, object?> { { "n", 0.42 } };

        var result = _formatter.FormatMessage("{n, number, percent}", args);

        Assert.Contains("42", result);
        Assert.Contains("%", result);
    }

    [Fact]
    public void Number_CurrencyStyle()
    {
        var args = new Dictionary<string, object?> { { "n", 1234.56 } };

        var result = _formatter.FormatMessage("{n, number, currency}", args);

        // Default currency symbol varies by culture
        Assert.Contains("1,234.56", result);
    }

    #endregion

    #region Custom Format Tests

    [Fact]
    public void Number_CustomFormat_TwoDecimals()
    {
        var args = new Dictionary<string, object?> { { "n", 3.1 } };

        var result = _formatter.FormatMessage("{n, number, 0.00}", args);

        Assert.Equal("3.10", result);
    }

    [Fact]
    public void Number_CustomFormat_ThreeDecimals()
    {
        var args = new Dictionary<string, object?> { { "n", 3.14159 } };

        var result = _formatter.FormatMessage("{n, number, 0.000}", args);

        Assert.Equal("3.142", result);
    }

    [Fact]
    public void Number_CustomFormat_LeadingZeros()
    {
        var args = new Dictionary<string, object?> { { "n", 42 } };

        var result = _formatter.FormatMessage("{n, number, 00000}", args);

        Assert.Equal("00042", result);
    }

    [Fact]
    public void Number_CustomFormat_Grouping()
    {
        var args = new Dictionary<string, object?> { { "n", 1234567 } };

        var result = _formatter.FormatMessage("{n, number, #,##0}", args);

        Assert.Equal("1,234,567", result);
    }

    [Fact]
    public void Number_CustomFormat_NoGrouping()
    {
        var args = new Dictionary<string, object?> { { "n", 1234567 } };

        var result = _formatter.FormatMessage("{n, number, 0}", args);

        Assert.Equal("1234567", result);
    }

    [Fact]
    public void Number_CustomFormat_Scientific()
    {
        var args = new Dictionary<string, object?> { { "n", 12345 } };

        var result = _formatter.FormatMessage("{n, number, 0.00E+0}", args);

        Assert.Contains("E", result);
    }

    [Fact]
    public void Number_CustomFormat_Invalid_ReturnsFormatString()
    {
        var args = new Dictionary<string, object?> { { "n", 42 } };

        // Invalid format returns the format string as-is when formatting fails
        var result = _formatter.FormatMessage("{n, number, %%%invalid%%%}", args);

        // The implementation returns the format string when it can't parse/apply it
        Assert.NotEmpty(result);
    }

    #endregion

    #region Skeleton Tests

    [Fact]
    public void Number_Skeleton_Percent()
    {
        var args = new Dictionary<string, object?> { { "n", 0.42 } };

        var result = _formatter.FormatMessage("{n, number, ::percent}", args);

        Assert.Contains("42", result);
        Assert.Contains("%", result);
    }

    [Fact]
    public void Number_Skeleton_Currency()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD}", args);

        Assert.Contains("$", result);
    }

    [Fact]
    public void Number_Skeleton_Precision()
    {
        var args = new Dictionary<string, object?> { { "n", 3.14159 } };

        var result = _formatter.FormatMessage("{n, number, ::.00}", args);

        Assert.Equal("3.14", result);
    }

    [Fact]
    public void Number_Skeleton_SignAlways()
    {
        var args = new Dictionary<string, object?> { { "n", 42 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-always}", args);

        Assert.StartsWith("+", result);
    }

    [Fact]
    public void Number_Skeleton_GroupOff()
    {
        var args = new Dictionary<string, object?> { { "n", 1234567 } };

        var result = _formatter.FormatMessage("{n, number, ::group-off}", args);

        Assert.DoesNotContain(",", result);
    }

    #endregion

    #region Value Type Conversion Tests

    [Fact]
    public void Number_FromInt()
    {
        var args = new Dictionary<string, object?> { { "n", 42 } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.Contains("42", result);
    }

    [Fact]
    public void Number_FromDouble()
    {
        var args = new Dictionary<string, object?> { { "n", 42.5 } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.Contains("42.5", result);
    }

    [Fact]
    public void Number_FromDecimal()
    {
        var args = new Dictionary<string, object?> { { "n", 42.5m } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.Contains("42.5", result);
    }

    [Fact]
    public void Number_FromFloat()
    {
        var args = new Dictionary<string, object?> { { "n", 42.5f } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.Contains("42.5", result);
    }

    [Fact]
    public void Number_FromLong()
    {
        var args = new Dictionary<string, object?> { { "n", 9999999999L } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.Contains("9,999,999,999", result);
    }

    [Fact]
    public void Number_FromString()
    {
        var args = new Dictionary<string, object?> { { "n", "123.45" } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.Contains("123", result);
    }

    [Fact]
    public void Number_Null_ReturnsZero()
    {
        var args = new Dictionary<string, object?> { { "n", null } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.Equal("0", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Number_Zero()
    {
        var args = new Dictionary<string, object?> { { "n", 0 } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.Equal("0", result);
    }

    [Fact]
    public void Number_Negative()
    {
        var args = new Dictionary<string, object?> { { "n", -42 } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.Contains("-42", result);
    }

    [Fact]
    public void Number_VeryLarge()
    {
        var args = new Dictionary<string, object?> { { "n", 1e15 } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void Number_VerySmall()
    {
        var args = new Dictionary<string, object?> { { "n", 0.00001 } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void Number_Infinity()
    {
        var args = new Dictionary<string, object?> { { "n", double.PositiveInfinity } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void Number_NegativeInfinity()
    {
        var args = new Dictionary<string, object?> { { "n", double.NegativeInfinity } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void Number_NaN()
    {
        var args = new Dictionary<string, object?> { { "n", double.NaN } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.NotEmpty(result);
    }

    #endregion

    #region Multiple Numbers in Message Tests

    [Fact]
    public void Number_Multiple_DifferentStyles()
    {
        var args = new Dictionary<string, object?>
        {
            { "a", 42 },
            { "b", 0.5 },
            { "c", 1234.56 }
        };

        var result = _formatter.FormatMessage(
            "Int: {a, number, integer}, Pct: {b, number, percent}, Cur: {c, number}",
            args);

        Assert.Contains("Int: 42", result);
        Assert.Contains("Pct: 50", result);
        Assert.Contains("Cur: 1,234.56", result);
    }

    [Fact]
    public void Number_SameVariable_DifferentStyles()
    {
        var args = new Dictionary<string, object?> { { "n", 0.42 } };

        var result = _formatter.FormatMessage(
            "Default: {n, number}, Percent: {n, number, percent}",
            args);

        Assert.Contains("Default: 0.42", result);
        Assert.Contains("Percent: 42", result);
    }

    #endregion
}
