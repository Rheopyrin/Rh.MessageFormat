using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Additional tests for number skeleton parsing and formatting edge cases.
/// </summary>
public class NumberSkeletonEdgeCaseTests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region Basic Number Formatting

    [Fact]
    public void Number_Default_FormatsInteger()
    {
        var args = new Dictionary<string, object?> { { "n", 42 } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.Equal("42", result);
    }

    [Fact]
    public void Number_Default_FormatsNegative()
    {
        var args = new Dictionary<string, object?> { { "n", -42.5 } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.Equal("-42.5", result);
    }

    [Fact]
    public void Number_Default_FormatsZero()
    {
        var args = new Dictionary<string, object?> { { "n", 0 } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.Equal("0", result);
    }

    [Fact]
    public void Number_Integer_RoundsDecimal()
    {
        var args = new Dictionary<string, object?> { { "n", 42.4 } };

        var result = _formatter.FormatMessage("{n, number, integer}", args);

        // Rounds to nearest integer
        Assert.Equal("42", result);
    }

    [Fact]
    public void Number_Percent_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "n", 0.42 } };

        var result = _formatter.FormatMessage("{n, number, percent}", args);

        Assert.Contains("42", result);
        Assert.Contains("%", result);
    }

    #endregion

    #region Skeleton Precision Tests

    [Fact]
    public void NumberSkeleton_ThreeDecimalPlaces()
    {
        var args = new Dictionary<string, object?> { { "n", 3.14159 } };

        var result = _formatter.FormatMessage("{n, number, ::.000}", args);

        Assert.Equal("3.142", result);
    }

    [Fact]
    public void NumberSkeleton_ZeroDecimalPlaces()
    {
        var args = new Dictionary<string, object?> { { "n", 3.14159 } };

        // Use integer style for zero decimal places
        var result = _formatter.FormatMessage("{n, number, integer}", args);

        Assert.Equal("3", result);
    }

    [Fact]
    public void NumberSkeleton_MinMaxFraction()
    {
        var args = new Dictionary<string, object?> { { "n", 3.1 } };

        var result = _formatter.FormatMessage("{n, number, ::.00##}", args);

        Assert.Equal("3.10", result);
    }

    [Fact]
    public void NumberSkeleton_OptionalFractionDigits()
    {
        var args1 = new Dictionary<string, object?> { { "n", 3.0 } };
        var args2 = new Dictionary<string, object?> { { "n", 3.14 } };

        var result1 = _formatter.FormatMessage("{n, number, ::.##}", args1);
        var result2 = _formatter.FormatMessage("{n, number, ::.##}", args2);

        Assert.Equal("3", result1);
        Assert.Equal("3.14", result2);
    }

    #endregion

    #region Skeleton Integer Digit Tests

    [Fact]
    public void NumberSkeleton_MinIntegerDigits()
    {
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = _formatter.FormatMessage("{n, number, ::0000}", args);

        // Result includes grouping, verify it has leading zeros
        Assert.Contains("005", result);
    }

    [Fact]
    public void NumberSkeleton_WithFixedFraction()
    {
        var args = new Dictionary<string, object?> { { "n", 5.5 } };

        var result = _formatter.FormatMessage("{n, number, ::.00}", args);

        Assert.Equal("5.50", result);
    }

    #endregion

    #region Sign Display Tests

    [Fact]
    public void NumberSkeleton_SignNever_PositiveNumber()
    {
        var args = new Dictionary<string, object?> { { "n", 42 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-never}", args);

        Assert.DoesNotContain("+", result);
        Assert.DoesNotContain("-", result);
    }

    [Fact]
    public void NumberSkeleton_SignNever_NegativeNumber()
    {
        var args = new Dictionary<string, object?> { { "n", -42 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-never}", args);

        Assert.DoesNotContain("-", result);
    }

    [Fact]
    public void NumberSkeleton_SignExceptZero_ZeroNumber()
    {
        var args = new Dictionary<string, object?> { { "n", 0 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-except-zero}", args);

        Assert.DoesNotContain("+", result);
        Assert.DoesNotContain("-", result);
    }

    [Fact]
    public void NumberSkeleton_SignExceptZero_PositiveNumber()
    {
        var args = new Dictionary<string, object?> { { "n", 42 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-except-zero}", args);

        Assert.StartsWith("+", result);
    }

    #endregion

    #region Scale Tests

    [Fact]
    public void NumberSkeleton_Scale1000()
    {
        var args = new Dictionary<string, object?> { { "n", 1.5 } };

        var result = _formatter.FormatMessage("{n, number, ::scale/1000}", args);

        Assert.Equal("1,500", result);
    }

    [Fact]
    public void NumberSkeleton_ScaleMultiply()
    {
        var args = new Dictionary<string, object?> { { "n", 0.5 } };

        // scale/100 multiplies the value by 100 (like percent but without symbol)
        var result = _formatter.FormatMessage("{n, number, ::scale/100}", args);

        Assert.Equal("50", result);
    }

    #endregion

    #region Grouping Tests

    [Fact]
    public void NumberSkeleton_GroupAuto()
    {
        var args = new Dictionary<string, object?> { { "n", 1234567 } };

        var result = _formatter.FormatMessage("{n, number, ::group-auto}", args);

        Assert.Contains(",", result);
    }

    [Fact]
    public void NumberSkeleton_GroupMin2()
    {
        var args = new Dictionary<string, object?> { { "n", 12345 } };

        var result = _formatter.FormatMessage("{n, number, ::group-min2}", args);

        // 12345 should have grouping (5 digits)
        Assert.Contains(",", result);
        Assert.Contains("12", result);
        Assert.Contains("345", result);
    }

    #endregion

    #region Scientific Notation Tests

    [Fact]
    public void NumberSkeleton_Scientific_SmallNumber()
    {
        var args = new Dictionary<string, object?> { { "n", 0.00123 } };

        var result = _formatter.FormatMessage("{n, number, ::scientific}", args);

        Assert.Contains("E", result);
    }

    [Fact]
    public void NumberSkeleton_Engineering_LargeNumber()
    {
        var args = new Dictionary<string, object?> { { "n", 12345678 } };

        var result = _formatter.FormatMessage("{n, number, ::engineering}", args);

        Assert.Contains("E", result);
    }

    #endregion

    #region Permille Tests

    [Fact]
    public void NumberSkeleton_Permille()
    {
        var args = new Dictionary<string, object?> { { "n", 0.005 } };

        var result = _formatter.FormatMessage("{n, number, ::permille}", args);

        Assert.Contains("5", result);
        Assert.Contains("‰", result);
    }

    #endregion

    #region Currency Display Tests

    [Fact]
    public void NumberSkeleton_CurrencySymbol()
    {
        var args = new Dictionary<string, object?> { { "n", 99.99 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD}", args);

        // Default currency display uses symbol
        Assert.Contains("$", result);
        Assert.Contains("99.99", result);
    }

    [Fact]
    public void NumberSkeleton_CurrencyNarrow()
    {
        var args = new Dictionary<string, object?> { { "n", 99.99 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD currency-narrow-symbol}", args);

        Assert.Contains("$", result);
    }

    #endregion

    #region Combined Options Tests

    [Fact]
    public void NumberSkeleton_MultipleOptions()
    {
        var args = new Dictionary<string, object?> { { "n", 0.1234 } };

        // Percent with 2 decimal places and always show sign
        var result = _formatter.FormatMessage("{n, number, ::percent .00 sign-always}", args);

        Assert.StartsWith("+", result);
        Assert.Contains("12.34", result);
        Assert.Contains("%", result);
    }

    [Fact]
    public void NumberSkeleton_CurrencyWithPrecision()
    {
        var args = new Dictionary<string, object?> { { "n", 1234.5 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD .00}", args);

        Assert.Contains("$", result);
        Assert.Contains("1,234.50", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Number_NullValue_ReturnsZero()
    {
        var args = new Dictionary<string, object?> { { "n", null } };

        var result = _formatter.FormatMessage("Value: {n, number}", args);

        // Null is treated as 0 for number formatting
        Assert.Equal("Value: 0", result);
    }

    [Fact]
    public void Number_StringValue_ParsesAndFormats()
    {
        var args = new Dictionary<string, object?> { { "n", "123.45" } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.Contains("123", result);
    }

    [Fact]
    public void Number_VeryLargeNumber()
    {
        var args = new Dictionary<string, object?> { { "n", 1234567890123456.0 } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void Number_VerySmallNumber()
    {
        var args = new Dictionary<string, object?> { { "n", 0.000000001 } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.NotEmpty(result);
    }

    #endregion
}
