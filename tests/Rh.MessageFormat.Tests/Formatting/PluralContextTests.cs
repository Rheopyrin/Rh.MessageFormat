using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Formatting;

/// <summary>
/// Tests for PluralContext struct and plural operand calculations.
/// PluralContext extracts CLDR plural operands (N, I, V, W, F, T, C, E) from numbers.
/// </summary>
public class PluralContextTests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region Integer Operand Tests

    [Theory]
    [InlineData(0, "zero")]
    [InlineData(1, "one")]
    [InlineData(2, "two")]
    [InlineData(5, "five")]
    [InlineData(10, "ten")]
    [InlineData(100, "hundred")]
    public void PluralContext_IntegerValues_CorrectOperands(int value, string expected)
    {
        var args = new Dictionary<string, object?> { { "n", value } };

        var result = _formatter.FormatMessage(
            "{n, plural, =0 {zero} =1 {one} =2 {two} =5 {five} =10 {ten} =100 {hundred} other {other}}",
            args);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void PluralContext_NegativeInteger_CorrectOperands()
    {
        var args = new Dictionary<string, object?> { { "n", -5 } };

        var result = _formatter.FormatMessage(
            "{n, plural, one {# item} other {# items}}",
            args);

        // N (absolute value) should be used for plural rules
        Assert.Contains("-5", result);
        Assert.Contains("items", result);
    }

    #endregion

    #region Decimal/Double Operand Tests

    [Fact]
    public void PluralContext_DecimalWithFraction_CorrectOperands()
    {
        var args = new Dictionary<string, object?> { { "n", 1.5m } };

        var result = _formatter.FormatMessage(
            "{n, plural, one {# item} other {# items}}",
            args);

        // 1.5 should use "other" in English (fractional numbers use other)
        Assert.Contains("items", result);
    }

    [Fact]
    public void PluralContext_DoubleWithFraction_CorrectOperands()
    {
        var args = new Dictionary<string, object?> { { "n", 2.5 } };

        var result = _formatter.FormatMessage(
            "{n, plural, one {# item} other {# items}}",
            args);

        Assert.Contains("items", result);
    }

    [Fact]
    public void PluralContext_DecimalWholeNumber_UsesOne()
    {
        var args = new Dictionary<string, object?> { { "n", 1.0m } };

        var result = _formatter.FormatMessage(
            "{n, plural, one {# item} other {# items}}",
            args);

        Assert.Equal("1 item", result);
    }

    [Fact]
    public void PluralContext_DoubleWholeNumber_UsesOne()
    {
        var args = new Dictionary<string, object?> { { "n", 1.0 } };

        var result = _formatter.FormatMessage(
            "{n, plural, one {# item} other {# items}}",
            args);

        Assert.Equal("1 item", result);
    }

    #endregion

    #region String Number Operand Tests

    [Fact]
    public void PluralContext_StringInteger_CorrectOperands()
    {
        var args = new Dictionary<string, object?> { { "n", "1" } };

        var result = _formatter.FormatMessage(
            "{n, plural, one {# item} other {# items}}",
            args);

        Assert.Equal("1 item", result);
    }

    [Fact]
    public void PluralContext_StringDecimal_CorrectOperands()
    {
        var args = new Dictionary<string, object?> { { "n", "1.5" } };

        var result = _formatter.FormatMessage(
            "{n, plural, one {# item} other {# items}}",
            args);

        Assert.Contains("items", result);
    }

    [Fact]
    public void PluralContext_StringWithTrailingZeros_CorrectOperands()
    {
        // In CLDR, "1.50" has V=2 (visible fraction digits), W=1 (without trailing zeros)
        var args = new Dictionary<string, object?> { { "n", "1.50" } };

        var result = _formatter.FormatMessage(
            "{n, plural, one {# item} other {# items}}",
            args);

        // 1.50 is not exactly 1, so should use "other"
        Assert.Contains("items", result);
    }

    #endregion

    #region Fraction Operand Tests (V, W, F, T)

    [Fact]
    public void PluralContext_VisibleFractionDigits_Calculated()
    {
        // V (visible fraction digits) affects some language plural rules
        // Testing with exact match to verify operands are calculated
        var args = new Dictionary<string, object?> { { "n", "1.23" } };

        var result = _formatter.FormatMessage(
            "{n, plural, one {one} other {other: #}}",
            args);

        // 1.23 has V=2, which means it's not "one" in most rules
        Assert.Contains("other", result);
    }

    [Fact]
    public void PluralContext_FractionWithoutTrailingZeros_Calculated()
    {
        // W (fraction digits without trailing zeros)
        var args = new Dictionary<string, object?> { { "n", "1.20" } };

        var result = _formatter.FormatMessage(
            "{n, plural, one {one} other {other}}",
            args);

        // 1.20 has W=1 (only "2" without trailing zero)
        Assert.Contains("other", result);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void PluralContext_Zero_UsesZeroOrOther()
    {
        var args = new Dictionary<string, object?> { { "n", 0 } };

        var result = _formatter.FormatMessage(
            "{n, plural, zero {zero} one {one} other {other}}",
            args);

        // English doesn't have "zero" category, falls to "other"
        Assert.Contains("other", result);
    }

    [Fact]
    public void PluralContext_ZeroWithExactMatch_UsesExactMatch()
    {
        var args = new Dictionary<string, object?> { { "n", 0 } };

        var result = _formatter.FormatMessage(
            "{n, plural, =0 {exactly zero} other {other}}",
            args);

        Assert.Equal("exactly zero", result);
    }

    [Fact]
    public void PluralContext_LargeNumber_UsesOther()
    {
        var args = new Dictionary<string, object?> { { "n", 1000000 } };

        var result = _formatter.FormatMessage(
            "{n, plural, one {# item} other {# items}}",
            args);

        Assert.Contains("1000000", result);
        Assert.Contains("items", result);
    }

    [Fact]
    public void PluralContext_VerySmallDecimal_UsesOther()
    {
        var args = new Dictionary<string, object?> { { "n", 0.001 } };

        var result = _formatter.FormatMessage(
            "{n, plural, one {# item} other {# items}}",
            args);

        Assert.Contains("items", result);
    }

    [Fact]
    public void PluralContext_NegativeDecimal_UsesOther()
    {
        var args = new Dictionary<string, object?> { { "n", -1.5 } };

        var result = _formatter.FormatMessage(
            "{n, plural, one {# item} other {# items}}",
            args);

        Assert.Contains("items", result);
    }

    #endregion

    #region SelectOrdinal Tests

    [Fact]
    public void PluralContext_Ordinal_CorrectCategory()
    {
        var args = new Dictionary<string, object?> { { "n", 1 } };

        var result = _formatter.FormatMessage(
            "{n, selectordinal, one {#st} two {#nd} few {#rd} other {#th}}",
            args);

        Assert.Equal("1st", result);
    }

    [Fact]
    public void PluralContext_Ordinal_Second()
    {
        var args = new Dictionary<string, object?> { { "n", 2 } };

        var result = _formatter.FormatMessage(
            "{n, selectordinal, one {#st} two {#nd} few {#rd} other {#th}}",
            args);

        Assert.Equal("2nd", result);
    }

    [Fact]
    public void PluralContext_Ordinal_Third()
    {
        var args = new Dictionary<string, object?> { { "n", 3 } };

        var result = _formatter.FormatMessage(
            "{n, selectordinal, one {#st} two {#nd} few {#rd} other {#th}}",
            args);

        Assert.Equal("3rd", result);
    }

    [Fact]
    public void PluralContext_Ordinal_Fourth()
    {
        var args = new Dictionary<string, object?> { { "n", 4 } };

        var result = _formatter.FormatMessage(
            "{n, selectordinal, one {#st} two {#nd} few {#rd} other {#th}}",
            args);

        Assert.Equal("4th", result);
    }

    [Fact]
    public void PluralContext_Ordinal_Eleventh()
    {
        var args = new Dictionary<string, object?> { { "n", 11 } };

        var result = _formatter.FormatMessage(
            "{n, selectordinal, one {#st} two {#nd} few {#rd} other {#th}}",
            args);

        // 11, 12, 13 all use "other" in English ordinal rules
        Assert.Equal("11th", result);
    }

    [Fact]
    public void PluralContext_Ordinal_TwentyFirst()
    {
        var args = new Dictionary<string, object?> { { "n", 21 } };

        var result = _formatter.FormatMessage(
            "{n, selectordinal, one {#st} two {#nd} few {#rd} other {#th}}",
            args);

        // 21 uses "one" in English ordinal rules
        Assert.Equal("21st", result);
    }

    #endregion

    #region Multiple Plural Operands in Complex Messages

    [Fact]
    public void PluralContext_NestedPlurals_BothEvaluated()
    {
        var args = new Dictionary<string, object?>
        {
            { "files", 1 },
            { "folders", 2 }
        };

        var result = _formatter.FormatMessage(
            "{files, plural, one {# file} other {# files}} in {folders, plural, one {# folder} other {# folders}}",
            args);

        Assert.Equal("1 file in 2 folders", result);
    }

    [Fact]
    public void PluralContext_MixedTypes_AllWork()
    {
        var args = new Dictionary<string, object?>
        {
            { "intVal", 1 },
            { "doubleVal", 2.0 },
            { "decimalVal", 3m },
            { "stringVal", "4" }
        };

        var result = _formatter.FormatMessage(
            "{intVal}: {intVal, plural, one {one} other {other}}, " +
            "{doubleVal}: {doubleVal, plural, one {one} other {other}}, " +
            "{decimalVal}: {decimalVal, plural, one {one} other {other}}, " +
            "{stringVal}: {stringVal, plural, one {one} other {other}}",
            args);

        Assert.Contains("1: one", result);
        Assert.Contains("2: other", result);
        Assert.Contains("3: other", result);
        Assert.Contains("4: other", result);
    }

    #endregion
}
