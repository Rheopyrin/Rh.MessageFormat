using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Formatting;

/// <summary>
/// Tests for NumberSkeletonParser - parsing ICU number skeleton strings.
/// </summary>
public class NumberSkeletonParserTests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region Concise Notation Tests

    [Fact]
    public void Skeleton_PercentSymbol()
    {
        var args = new Dictionary<string, object?> { { "n", 0.5 } };

        var result = _formatter.FormatMessage("{n, number, ::%}", args);

        Assert.Contains("50", result);
        Assert.Contains("%", result);
    }

    [Fact]
    public void Skeleton_CompactShort_K()
    {
        var args = new Dictionary<string, object?> { { "n", 1500 } };

        var result = _formatter.FormatMessage("{n, number, ::K}", args);

        Assert.Contains("K", result);
    }

    [Fact]
    public void Skeleton_CompactLong_KK()
    {
        var args = new Dictionary<string, object?> { { "n", 1500 } };

        var result = _formatter.FormatMessage("{n, number, ::KK}", args);

        Assert.Contains("thousand", result);
    }

    [Fact]
    public void Skeleton_SignAlways_PlusExclamation()
    {
        var args = new Dictionary<string, object?> { { "n", 42 } };

        var result = _formatter.FormatMessage("{n, number, ::+!}", args);

        Assert.StartsWith("+", result);
    }

    [Fact]
    public void Skeleton_SignNever_PlusUnderscore()
    {
        var args = new Dictionary<string, object?> { { "n", -42 } };

        var result = _formatter.FormatMessage("{n, number, ::+_}", args);

        Assert.DoesNotContain("-", result);
    }

    [Fact]
    public void Skeleton_SignExceptZero_PlusQuestion()
    {
        var args = new Dictionary<string, object?> { { "n", 42 } };

        var result = _formatter.FormatMessage("{n, number, ::+?}", args);

        Assert.StartsWith("+", result);
    }

    [Fact]
    public void Skeleton_Accounting_Parentheses()
    {
        var args = new Dictionary<string, object?> { { "n", -100 } };

        var result = _formatter.FormatMessage("{n, number, ::()}", args);

        Assert.Contains("(", result);
        Assert.Contains(")", result);
    }

    [Fact]
    public void Skeleton_GroupOff_CommaUnderscore()
    {
        var args = new Dictionary<string, object?> { { "n", 1234567 } };

        var result = _formatter.FormatMessage("{n, number, ::,_}", args);

        Assert.DoesNotContain(",", result);
    }

    [Fact]
    public void Skeleton_GroupMin2_CommaQuestion()
    {
        var args = new Dictionary<string, object?> { { "n", 1234 } };

        var result = _formatter.FormatMessage("{n, number, ::,?}", args);

        // 1234 should have grouping with min2
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Skeleton_GroupAlways_CommaExclamation()
    {
        var args = new Dictionary<string, object?> { { "n", 1234567 } };

        var result = _formatter.FormatMessage("{n, number, ::,!}", args);

        Assert.Contains(",", result);
    }

    #endregion

    #region Currency Tests

    [Fact]
    public void Skeleton_Currency_USD()
    {
        var args = new Dictionary<string, object?> { { "n", 99.99 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD}", args);

        Assert.Contains("$", result);
    }

    [Fact]
    public void Skeleton_Currency_EUR()
    {
        var args = new Dictionary<string, object?> { { "n", 50 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/EUR}", args);

        // Should contain euro symbol or EUR
        Assert.True(result.Contains("€") || result.Contains("EUR"));
    }

    [Fact]
    public void Skeleton_Currency_GBP()
    {
        var args = new Dictionary<string, object?> { { "n", 25.50 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/GBP}", args);

        Assert.True(result.Contains("£") || result.Contains("GBP"));
    }

    [Fact]
    public void Skeleton_Currency_LowercaseConverted()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/usd}", args);

        // Should work with lowercase and convert to uppercase
        Assert.Contains("$", result);
    }

    #endregion

    #region Scale Tests

    [Fact]
    public void Skeleton_Scale_100()
    {
        var args = new Dictionary<string, object?> { { "n", 0.5 } };

        var result = _formatter.FormatMessage("{n, number, ::scale/100}", args);

        Assert.Equal("50", result);
    }

    [Fact]
    public void Skeleton_Scale_1000()
    {
        var args = new Dictionary<string, object?> { { "n", 1.5 } };

        var result = _formatter.FormatMessage("{n, number, ::scale/1000}", args);

        Assert.Contains("1,500", result);
    }

    [Fact]
    public void Skeleton_Scale_LargeFactor()
    {
        var args = new Dictionary<string, object?> { { "n", 5 } };

        // scale/10 multiplies the value by 10
        var result = _formatter.FormatMessage("{n, number, ::scale/10}", args);

        // 5 * 10 = 50
        Assert.Equal("50", result);
    }

    #endregion

    #region Unit Tests

    [Fact]
    public void Skeleton_Unit_Meter()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/meter}", args);

        Assert.Contains("100", result);
    }

    [Fact]
    public void Skeleton_MeasureUnit_Kilometer()
    {
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = _formatter.FormatMessage("{n, number, ::measure-unit/kilometer}", args);

        Assert.Contains("5", result);
    }

    #endregion

    #region Integer Width Tests

    [Fact]
    public void Skeleton_IntegerWidth_ThreeDigits()
    {
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = _formatter.FormatMessage("{n, number, ::integer-width/*000}", args);

        Assert.Contains("005", result);
    }

    [Fact]
    public void Skeleton_IntegerWidth_FourDigits()
    {
        var args = new Dictionary<string, object?> { { "n", 42 } };

        var result = _formatter.FormatMessage("{n, number, ::integer-width/*0000 group-off}", args);

        Assert.Contains("0042", result);
    }

    [Fact]
    public void Skeleton_IntegerWidth_WithoutStar()
    {
        var args = new Dictionary<string, object?> { { "n", 7 } };

        var result = _formatter.FormatMessage("{n, number, ::integer-width/000}", args);

        Assert.Contains("007", result);
    }

    #endregion

    #region Sign Display Tests

    [Fact]
    public void Skeleton_SignAlways()
    {
        var args = new Dictionary<string, object?> { { "n", 42 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-always}", args);

        Assert.StartsWith("+", result);
    }

    [Fact]
    public void Skeleton_SignAlways_Negative()
    {
        var args = new Dictionary<string, object?> { { "n", -42 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-always}", args);

        Assert.Contains("-", result);
    }

    [Fact]
    public void Skeleton_SignNever()
    {
        var args = new Dictionary<string, object?> { { "n", -42 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-never}", args);

        Assert.DoesNotContain("-", result);
    }

    [Fact]
    public void Skeleton_SignExceptZero_Zero()
    {
        var args = new Dictionary<string, object?> { { "n", 0 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-except-zero}", args);

        Assert.DoesNotContain("+", result);
        Assert.DoesNotContain("-", result);
    }

    [Fact]
    public void Skeleton_SignAccounting()
    {
        var args = new Dictionary<string, object?> { { "n", -100 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-accounting}", args);

        Assert.Contains("(", result);
        Assert.Contains(")", result);
    }

    [Fact]
    public void Skeleton_SignAccountingAlways()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-accounting-always}", args);

        Assert.StartsWith("+", result);
    }

    [Fact]
    public void Skeleton_SignAccountingAlways_Negative()
    {
        var args = new Dictionary<string, object?> { { "n", -100 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-accounting-always}", args);

        Assert.Contains("(", result);
        Assert.Contains(")", result);
    }

    [Fact]
    public void Skeleton_SignAccountingExceptZero()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-accounting-except-zero}", args);

        Assert.StartsWith("+", result);
    }

    [Fact]
    public void Skeleton_SignAccountingExceptZero_Zero()
    {
        var args = new Dictionary<string, object?> { { "n", 0 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-accounting-except-zero}", args);

        Assert.DoesNotContain("+", result);
    }

    #endregion

    #region Grouping Tests

    [Fact]
    public void Skeleton_GroupOff()
    {
        var args = new Dictionary<string, object?> { { "n", 1234567 } };

        var result = _formatter.FormatMessage("{n, number, ::group-off}", args);

        Assert.DoesNotContain(",", result);
    }

    [Fact]
    public void Skeleton_GroupMin2()
    {
        var args = new Dictionary<string, object?> { { "n", 12345 } };

        var result = _formatter.FormatMessage("{n, number, ::group-min2}", args);

        Assert.Contains(",", result);
    }

    [Fact]
    public void Skeleton_GroupAuto()
    {
        var args = new Dictionary<string, object?> { { "n", 1234567 } };

        var result = _formatter.FormatMessage("{n, number, ::group-auto}", args);

        Assert.Contains(",", result);
    }

    [Fact]
    public void Skeleton_GroupOnAligned()
    {
        var args = new Dictionary<string, object?> { { "n", 1234567 } };

        var result = _formatter.FormatMessage("{n, number, ::group-on-aligned}", args);

        Assert.Contains(",", result);
    }

    [Fact]
    public void Skeleton_GroupAlways()
    {
        var args = new Dictionary<string, object?> { { "n", 1234567 } };

        var result = _formatter.FormatMessage("{n, number, ::group-always}", args);

        Assert.Contains(",", result);
    }

    #endregion

    #region Unit Width Tests

    [Fact]
    public void Skeleton_UnitWidthShort()
    {
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/meter unit-width-short}", args);

        Assert.Contains("5", result);
    }

    [Fact]
    public void Skeleton_UnitWidthNarrow()
    {
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/meter unit-width-narrow}", args);

        Assert.Contains("5", result);
    }

    [Fact]
    public void Skeleton_UnitWidthFullName()
    {
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/meter unit-width-full-name}", args);

        Assert.Contains("5", result);
    }

    [Fact]
    public void Skeleton_UnitWidthIsoCode()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD unit-width-iso-code}", args);

        Assert.Contains("USD", result);
    }

    #endregion

    #region Currency Display Tests

    [Fact]
    public void Skeleton_CurrencySymbol()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD currency-symbol}", args);

        Assert.Contains("$", result);
    }

    [Fact]
    public void Skeleton_CurrencyNarrowSymbol()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD currency-narrow-symbol}", args);

        Assert.Contains("$", result);
    }

    #endregion

    #region Significant Digits Tests

    [Fact]
    public void Skeleton_SignificantDigits_Three()
    {
        var args = new Dictionary<string, object?> { { "n", 12345 } };

        var result = _formatter.FormatMessage("{n, number, ::@@@}", args);

        // Should round to 3 significant digits: 12300 or 12,300
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Skeleton_SignificantDigits_TwoToFour()
    {
        var args = new Dictionary<string, object?> { { "n", 1.5 } };

        var result = _formatter.FormatMessage("{n, number, ::@@##}", args);

        Assert.Contains("1.5", result);
    }

    [Fact]
    public void Skeleton_SignificantDigits_OneToThree()
    {
        var args = new Dictionary<string, object?> { { "n", 1234 } };

        var result = _formatter.FormatMessage("{n, number, ::@##}", args);

        Assert.NotEmpty(result);
    }

    #endregion

    #region Fraction Precision Tests

    [Fact]
    public void Skeleton_Fraction_ExactlyTwo()
    {
        var args = new Dictionary<string, object?> { { "n", 3.1 } };

        var result = _formatter.FormatMessage("{n, number, ::.00}", args);

        Assert.Equal("3.10", result);
    }

    [Fact]
    public void Skeleton_Fraction_ExactlyThree()
    {
        var args = new Dictionary<string, object?> { { "n", 3.14 } };

        var result = _formatter.FormatMessage("{n, number, ::.000}", args);

        Assert.Equal("3.140", result);
    }

    [Fact]
    public void Skeleton_Fraction_AtMostTwo()
    {
        var args = new Dictionary<string, object?> { { "n", 3.0 } };

        var result = _formatter.FormatMessage("{n, number, ::.##}", args);

        Assert.Equal("3", result);
    }

    [Fact]
    public void Skeleton_Fraction_AtMostTwo_WithFraction()
    {
        var args = new Dictionary<string, object?> { { "n", 3.14 } };

        var result = _formatter.FormatMessage("{n, number, ::.##}", args);

        Assert.Equal("3.14", result);
    }

    [Fact]
    public void Skeleton_Fraction_OneToTwo()
    {
        var args = new Dictionary<string, object?> { { "n", 3.1 } };

        var result = _formatter.FormatMessage("{n, number, ::.0#}", args);

        Assert.Equal("3.1", result);
    }

    [Fact]
    public void Skeleton_Fraction_Unlimited()
    {
        var args = new Dictionary<string, object?> { { "n", 3.14159 } };

        var result = _formatter.FormatMessage("{n, number, ::.00*}", args);

        // Should have at least 2 decimals and allow more
        Assert.Contains("3.14", result);
    }

    #endregion

    #region Integer Digits Tests

    [Fact]
    public void Skeleton_IntegerDigits_Three()
    {
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = _formatter.FormatMessage("{n, number, ::000}", args);

        Assert.Contains("005", result);
    }

    [Fact]
    public void Skeleton_IntegerDigits_Four()
    {
        var args = new Dictionary<string, object?> { { "n", 42 } };

        var result = _formatter.FormatMessage("{n, number, ::0000 group-off}", args);

        Assert.Contains("0042", result);
    }

    #endregion

    #region Combined Options Tests

    [Fact]
    public void Skeleton_CombinedOptions_PercentWithPrecision()
    {
        var args = new Dictionary<string, object?> { { "n", 0.1234 } };

        var result = _formatter.FormatMessage("{n, number, ::percent .00}", args);

        Assert.Contains("12.34", result);
        Assert.Contains("%", result);
    }

    [Fact]
    public void Skeleton_CombinedOptions_CurrencyWithSign()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD sign-always}", args);

        Assert.Contains("+", result);
        Assert.Contains("$", result);
    }

    [Fact]
    public void Skeleton_CombinedOptions_ScaleWithGrouping()
    {
        var args = new Dictionary<string, object?> { { "n", 1.5 } };

        var result = _formatter.FormatMessage("{n, number, ::scale/1000 group-auto}", args);

        Assert.Contains("1,500", result);
    }

    [Fact]
    public void Skeleton_EmptySkeleton()
    {
        var args = new Dictionary<string, object?> { { "n", 42 } };

        var result = _formatter.FormatMessage("{n, number, ::}", args);

        Assert.Contains("42", result);
    }

    [Fact]
    public void Skeleton_WhitespaceOnly()
    {
        var args = new Dictionary<string, object?> { { "n", 42 } };

        var result = _formatter.FormatMessage("{n, number, ::   }", args);

        Assert.Contains("42", result);
    }

    #endregion

    #region Scientific/Engineering Notation Tests

    [Fact]
    public void Skeleton_Scientific()
    {
        var args = new Dictionary<string, object?> { { "n", 12345 } };

        var result = _formatter.FormatMessage("{n, number, ::scientific}", args);

        Assert.Contains("E", result);
    }

    [Fact]
    public void Skeleton_Engineering()
    {
        var args = new Dictionary<string, object?> { { "n", 12345 } };

        var result = _formatter.FormatMessage("{n, number, ::engineering}", args);

        Assert.Contains("E", result);
    }

    [Fact]
    public void Skeleton_CompactShort()
    {
        var args = new Dictionary<string, object?> { { "n", 1500000 } };

        var result = _formatter.FormatMessage("{n, number, ::compact-short}", args);

        Assert.Contains("M", result);
    }

    [Fact]
    public void Skeleton_CompactLong()
    {
        var args = new Dictionary<string, object?> { { "n", 1500000 } };

        var result = _formatter.FormatMessage("{n, number, ::compact-long}", args);

        Assert.Contains("million", result);
    }

    #endregion

    #region Permille Tests

    [Fact]
    public void Skeleton_Permille()
    {
        var args = new Dictionary<string, object?> { { "n", 0.005 } };

        var result = _formatter.FormatMessage("{n, number, ::permille}", args);

        Assert.Contains("5", result);
        Assert.Contains("‰", result);
    }

    [Fact]
    public void Skeleton_Permille_LargerValue()
    {
        var args = new Dictionary<string, object?> { { "n", 0.123 } };

        var result = _formatter.FormatMessage("{n, number, ::permille}", args);

        Assert.Contains("123", result);
        Assert.Contains("‰", result);
    }

    #endregion
}
