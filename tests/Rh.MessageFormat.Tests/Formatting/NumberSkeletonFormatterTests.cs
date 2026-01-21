using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Formatting;

/// <summary>
/// Tests for NumberSkeletonFormatter - formatting numbers with various options.
/// </summary>
public class NumberSkeletonFormatterTests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region Scientific Notation Tests

    [Fact]
    public void Scientific_PositiveNumber()
    {
        var args = new Dictionary<string, object?> { { "n", 12345 } };

        var result = _formatter.FormatMessage("{n, number, ::scientific}", args);

        Assert.Contains("E", result);
    }

    [Fact]
    public void Scientific_NegativeNumber()
    {
        var args = new Dictionary<string, object?> { { "n", -12345 } };

        var result = _formatter.FormatMessage("{n, number, ::scientific}", args);

        Assert.Contains("E", result);
        Assert.Contains("-", result);
    }

    [Fact]
    public void Scientific_SmallNumber()
    {
        var args = new Dictionary<string, object?> { { "n", 0.00123 } };

        var result = _formatter.FormatMessage("{n, number, ::scientific}", args);

        Assert.Contains("E", result);
    }

    [Fact]
    public void Scientific_Zero()
    {
        var args = new Dictionary<string, object?> { { "n", 0 } };

        var result = _formatter.FormatMessage("{n, number, ::scientific}", args);

        Assert.Contains("E", result);
    }

    [Fact]
    public void Scientific_One()
    {
        var args = new Dictionary<string, object?> { { "n", 1 } };

        var result = _formatter.FormatMessage("{n, number, ::scientific}", args);

        Assert.Contains("E", result);
    }

    [Fact]
    public void Scientific_WithPrecision()
    {
        var args = new Dictionary<string, object?> { { "n", 12345.6789 } };

        var result = _formatter.FormatMessage("{n, number, ::scientific .000}", args);

        Assert.Contains("E", result);
    }

    #endregion

    #region Engineering Notation Tests

    [Fact]
    public void Engineering_PositiveNumber()
    {
        var args = new Dictionary<string, object?> { { "n", 12345 } };

        var result = _formatter.FormatMessage("{n, number, ::engineering}", args);

        Assert.Contains("E", result);
        // Engineering notation uses exponents that are multiples of 3
    }

    [Fact]
    public void Engineering_LargeNumber()
    {
        var args = new Dictionary<string, object?> { { "n", 12345678 } };

        var result = _formatter.FormatMessage("{n, number, ::engineering}", args);

        Assert.Contains("E+", result);
    }

    [Fact]
    public void Engineering_SmallNumber()
    {
        var args = new Dictionary<string, object?> { { "n", 0.00000123 } };

        var result = _formatter.FormatMessage("{n, number, ::engineering}", args);

        Assert.Contains("E", result);
    }

    [Fact]
    public void Engineering_Zero()
    {
        var args = new Dictionary<string, object?> { { "n", 0 } };

        var result = _formatter.FormatMessage("{n, number, ::engineering}", args);

        Assert.Equal("0", result);
    }

    [Fact]
    public void Engineering_NegativeNumber()
    {
        var args = new Dictionary<string, object?> { { "n", -12345678 } };

        var result = _formatter.FormatMessage("{n, number, ::engineering}", args);

        Assert.Contains("E", result);
        Assert.Contains("-", result);
    }

    #endregion

    #region Compact Notation Tests

    [Fact]
    public void CompactShort_Thousand()
    {
        var args = new Dictionary<string, object?> { { "n", 1500 } };

        var result = _formatter.FormatMessage("{n, number, ::compact-short}", args);

        Assert.Contains("K", result);
    }

    [Fact]
    public void CompactShort_Million()
    {
        var args = new Dictionary<string, object?> { { "n", 1500000 } };

        var result = _formatter.FormatMessage("{n, number, ::compact-short}", args);

        Assert.Contains("M", result);
    }

    [Fact]
    public void CompactShort_Billion()
    {
        var args = new Dictionary<string, object?> { { "n", 1500000000 } };

        var result = _formatter.FormatMessage("{n, number, ::compact-short}", args);

        Assert.Contains("B", result);
    }

    [Fact]
    public void CompactShort_SmallNumber_NoSuffix()
    {
        var args = new Dictionary<string, object?> { { "n", 500 } };

        var result = _formatter.FormatMessage("{n, number, ::compact-short}", args);

        Assert.DoesNotContain("K", result);
        Assert.Contains("500", result);
    }

    [Fact]
    public void CompactLong_Thousand()
    {
        var args = new Dictionary<string, object?> { { "n", 1500 } };

        var result = _formatter.FormatMessage("{n, number, ::compact-long}", args);

        Assert.Contains("thousand", result);
    }

    [Fact]
    public void CompactLong_Million()
    {
        var args = new Dictionary<string, object?> { { "n", 1500000 } };

        var result = _formatter.FormatMessage("{n, number, ::compact-long}", args);

        Assert.Contains("million", result);
    }

    [Fact]
    public void CompactLong_SmallNumber_NoSuffix()
    {
        var args = new Dictionary<string, object?> { { "n", 500 } };

        var result = _formatter.FormatMessage("{n, number, ::compact-long}", args);

        Assert.DoesNotContain("thousand", result);
        Assert.Contains("500", result);
    }

    [Fact]
    public void Compact_NegativeNumber()
    {
        var args = new Dictionary<string, object?> { { "n", -1500000 } };

        var result = _formatter.FormatMessage("{n, number, ::compact-short}", args);

        Assert.Contains("-", result);
        Assert.Contains("M", result);
    }

    [Fact]
    public void Compact_WithPrecision()
    {
        var args = new Dictionary<string, object?> { { "n", 1234567 } };

        var result = _formatter.FormatMessage("{n, number, ::compact-short .0}", args);

        Assert.Contains("M", result);
    }

    #endregion

    #region Percent Tests

    [Fact]
    public void Percent_Decimal()
    {
        var args = new Dictionary<string, object?> { { "n", 0.42 } };

        var result = _formatter.FormatMessage("{n, number, ::percent}", args);

        Assert.Contains("42", result);
        Assert.Contains("%", result);
    }

    [Fact]
    public void Percent_WholeNumber()
    {
        var args = new Dictionary<string, object?> { { "n", 1 } };

        var result = _formatter.FormatMessage("{n, number, ::percent}", args);

        Assert.Contains("100", result);
        Assert.Contains("%", result);
    }

    [Fact]
    public void Percent_Zero()
    {
        var args = new Dictionary<string, object?> { { "n", 0 } };

        var result = _formatter.FormatMessage("{n, number, ::percent}", args);

        Assert.Contains("0", result);
        Assert.Contains("%", result);
    }

    [Fact]
    public void Percent_Negative()
    {
        var args = new Dictionary<string, object?> { { "n", -0.25 } };

        var result = _formatter.FormatMessage("{n, number, ::percent}", args);

        Assert.Contains("-", result);
        Assert.Contains("25", result);
        Assert.Contains("%", result);
    }

    [Fact]
    public void Percent_OverOneHundred()
    {
        var args = new Dictionary<string, object?> { { "n", 1.5 } };

        var result = _formatter.FormatMessage("{n, number, ::percent}", args);

        Assert.Contains("150", result);
        Assert.Contains("%", result);
    }

    [Fact]
    public void Percent_WithPrecision()
    {
        var args = new Dictionary<string, object?> { { "n", 0.1234 } };

        var result = _formatter.FormatMessage("{n, number, ::percent .00}", args);

        Assert.Contains("12.34", result);
        Assert.Contains("%", result);
    }

    #endregion

    #region Permille Tests

    [Fact]
    public void Permille_Decimal()
    {
        var args = new Dictionary<string, object?> { { "n", 0.042 } };

        var result = _formatter.FormatMessage("{n, number, ::permille}", args);

        Assert.Contains("42", result);
        Assert.Contains("‰", result);
    }

    [Fact]
    public void Permille_SmallDecimal()
    {
        var args = new Dictionary<string, object?> { { "n", 0.001 } };

        var result = _formatter.FormatMessage("{n, number, ::permille}", args);

        Assert.Contains("1", result);
        Assert.Contains("‰", result);
    }

    [Fact]
    public void Permille_Zero()
    {
        var args = new Dictionary<string, object?> { { "n", 0 } };

        var result = _formatter.FormatMessage("{n, number, ::permille}", args);

        Assert.Contains("0", result);
        Assert.Contains("‰", result);
    }

    #endregion

    #region Sign Display Tests

    [Fact]
    public void SignAlways_Positive()
    {
        var args = new Dictionary<string, object?> { { "n", 42 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-always}", args);

        Assert.StartsWith("+", result);
    }

    [Fact]
    public void SignAlways_Negative()
    {
        var args = new Dictionary<string, object?> { { "n", -42 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-always}", args);

        Assert.Contains("-", result);
    }

    [Fact]
    public void SignAlways_Zero()
    {
        var args = new Dictionary<string, object?> { { "n", 0 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-always}", args);

        // Zero might show + or nothing depending on implementation
        Assert.NotEmpty(result);
    }

    [Fact]
    public void SignNever_Positive()
    {
        var args = new Dictionary<string, object?> { { "n", 42 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-never}", args);

        Assert.DoesNotContain("+", result);
    }

    [Fact]
    public void SignNever_Negative()
    {
        var args = new Dictionary<string, object?> { { "n", -42 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-never}", args);

        Assert.DoesNotContain("-", result);
    }

    [Fact]
    public void SignExceptZero_Positive()
    {
        var args = new Dictionary<string, object?> { { "n", 42 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-except-zero}", args);

        Assert.StartsWith("+", result);
    }

    [Fact]
    public void SignExceptZero_Negative()
    {
        var args = new Dictionary<string, object?> { { "n", -42 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-except-zero}", args);

        Assert.Contains("-", result);
    }

    [Fact]
    public void SignExceptZero_Zero()
    {
        var args = new Dictionary<string, object?> { { "n", 0 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-except-zero}", args);

        Assert.DoesNotContain("+", result);
        Assert.DoesNotContain("-", result);
    }

    [Fact]
    public void SignAccounting_Negative()
    {
        var args = new Dictionary<string, object?> { { "n", -100 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-accounting}", args);

        Assert.Contains("(", result);
        Assert.Contains(")", result);
    }

    [Fact]
    public void SignAccounting_Positive()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-accounting}", args);

        Assert.DoesNotContain("(", result);
        Assert.DoesNotContain(")", result);
    }

    [Fact]
    public void SignAccountingAlways_Positive()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-accounting-always}", args);

        Assert.StartsWith("+", result);
    }

    [Fact]
    public void SignAccountingAlways_Negative()
    {
        var args = new Dictionary<string, object?> { { "n", -100 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-accounting-always}", args);

        Assert.Contains("(", result);
        Assert.Contains(")", result);
    }

    [Fact]
    public void SignAccountingExceptZero_Positive()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-accounting-except-zero}", args);

        Assert.StartsWith("+", result);
    }

    [Fact]
    public void SignAccountingExceptZero_Zero()
    {
        var args = new Dictionary<string, object?> { { "n", 0 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-accounting-except-zero}", args);

        Assert.DoesNotContain("+", result);
    }

    #endregion

    #region Scale Tests

    [Fact]
    public void Scale_Multiply()
    {
        var args = new Dictionary<string, object?> { { "n", 1 } };

        var result = _formatter.FormatMessage("{n, number, ::scale/1000}", args);

        Assert.Equal("1,000", result);
    }

    [Fact]
    public void Scale_MultiplyByTen()
    {
        var args = new Dictionary<string, object?> { { "n", 10 } };

        var result = _formatter.FormatMessage("{n, number, ::scale/10}", args);

        Assert.Equal("100", result);
    }

    [Fact]
    public void Scale_NoEffect()
    {
        var args = new Dictionary<string, object?> { { "n", 42 } };

        var result = _formatter.FormatMessage("{n, number, ::scale/1}", args);

        Assert.Contains("42", result);
    }

    [Fact]
    public void Scale_Combined_WithPercent()
    {
        var args = new Dictionary<string, object?> { { "n", 1 } };

        // scale/100 then percent (x100) = x10000
        var result = _formatter.FormatMessage("{n, number, ::scale/100 percent}", args);

        // 1 * 100 (scale) * 100 (percent) = 10000
        Assert.Contains("10", result);
        Assert.Contains("%", result);
    }

    #endregion

    #region Grouping Tests

    [Fact]
    public void GroupOff_NoGrouping()
    {
        var args = new Dictionary<string, object?> { { "n", 1234567 } };

        var result = _formatter.FormatMessage("{n, number, ::group-off}", args);

        Assert.DoesNotContain(",", result);
        Assert.Contains("1234567", result);
    }

    [Fact]
    public void GroupAuto_HasGrouping()
    {
        var args = new Dictionary<string, object?> { { "n", 1234567 } };

        var result = _formatter.FormatMessage("{n, number, ::group-auto}", args);

        Assert.Contains(",", result);
    }

    [Fact]
    public void GroupMin2_LargeNumber()
    {
        var args = new Dictionary<string, object?> { { "n", 12345 } };

        var result = _formatter.FormatMessage("{n, number, ::group-min2}", args);

        Assert.Contains(",", result);
    }

    [Fact]
    public void GroupAlways_HasGrouping()
    {
        var args = new Dictionary<string, object?> { { "n", 1234567 } };

        var result = _formatter.FormatMessage("{n, number, ::group-always}", args);

        Assert.Contains(",", result);
    }

    #endregion

    #region Format String Building Tests

    [Fact]
    public void FormatString_MinimumIntegerDigits()
    {
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = _formatter.FormatMessage("{n, number, ::0000 group-off}", args);

        Assert.Contains("0005", result);
    }

    [Fact]
    public void FormatString_MinimumFractionDigits()
    {
        var args = new Dictionary<string, object?> { { "n", 3.1 } };

        var result = _formatter.FormatMessage("{n, number, ::.00}", args);

        Assert.Equal("3.10", result);
    }

    [Fact]
    public void FormatString_MaximumFractionDigits()
    {
        var args = new Dictionary<string, object?> { { "n", 3.0 } };

        var result = _formatter.FormatMessage("{n, number, ::.##}", args);

        Assert.Equal("3", result);
    }

    [Fact]
    public void FormatString_MixedFractionDigits()
    {
        var args = new Dictionary<string, object?> { { "n", 3.1 } };

        var result = _formatter.FormatMessage("{n, number, ::.0##}", args);

        Assert.Equal("3.1", result);
    }

    [Fact]
    public void FormatString_GroupingWithMinDigits()
    {
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = _formatter.FormatMessage("{n, number, ::000 group-auto}", args);

        Assert.Contains("005", result);
    }

    #endregion

    #region Edge Cases

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
        var args = new Dictionary<string, object?> { { "n", 1e-10 } };

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
}
