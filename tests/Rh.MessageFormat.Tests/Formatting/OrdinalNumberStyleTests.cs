using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Formatting;

/// <summary>
/// Tests for ordinal number formatting via skeleton syntax: {n, number, ::ordinal}
/// </summary>
public class OrdinalNumberStyleTests
{
    #region Basic Ordinal Tests (English)

    [Theory]
    [InlineData(1, "1st")]
    [InlineData(2, "2nd")]
    [InlineData(3, "3rd")]
    [InlineData(4, "4th")]
    [InlineData(5, "5th")]
    [InlineData(10, "10th")]
    [InlineData(11, "11th")]
    [InlineData(12, "12th")]
    [InlineData(13, "13th")]
    [InlineData(14, "14th")]
    [InlineData(20, "20th")]
    [InlineData(21, "21st")]
    [InlineData(22, "22nd")]
    [InlineData(23, "23rd")]
    [InlineData(24, "24th")]
    [InlineData(100, "100th")]
    [InlineData(101, "101st")]
    [InlineData(102, "102nd")]
    [InlineData(103, "103rd")]
    [InlineData(111, "111th")]
    [InlineData(112, "112th")]
    [InlineData(113, "113th")]
    public void Format_English_BasicOrdinals(int number, string expected)
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{n, number, ::ordinal}", new { n = number });

        Assert.Equal(expected, result);
    }

    #endregion

    #region Negative Numbers

    [Fact]
    public void Format_NegativeNumber()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{n, number, ::ordinal}", new { n = -1 });

        // Negative ordinals should still work, suffix based on absolute value
        Assert.Equal("-1st", result);
    }

    #endregion

    #region Zero

    [Fact]
    public void Format_Zero()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{n, number, ::ordinal}", new { n = 0 });

        Assert.Equal("0th", result);
    }

    #endregion

    #region Floating Point Numbers

    [Fact]
    public void Format_FloatingPoint_RoundsToInteger()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{n, number, ::ordinal}", new { n = 1.7 });

        // Should round to nearest integer (2) and format as ordinal
        Assert.Equal("2nd", result);
    }

    [Fact]
    public void Format_FloatingPoint_RoundsDown()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{n, number, ::ordinal}", new { n = 2.3 });

        Assert.Equal("2nd", result);
    }

    #endregion

    #region Large Numbers

    [Theory]
    [InlineData(1000, "1000th")]
    [InlineData(1001, "1001st")]
    [InlineData(10000, "10000th")]
    [InlineData(1000000, "1000000th")]
    public void Format_LargeNumbers(int number, string expected)
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{n, number, ::ordinal}", new { n = number });

        Assert.Equal(expected, result);
    }

    #endregion

    #region Combined with Text

    [Fact]
    public void Format_InSentence()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("You finished in {place, number, ::ordinal} place!", new { place = 1 });

        Assert.Equal("You finished in 1st place!", result);
    }

    [Fact]
    public void Format_MultipleOrdinals()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage(
            "{a, number, ::ordinal}, {b, number, ::ordinal}, and {c, number, ::ordinal}",
            new { a = 1, b = 2, c = 3 });

        Assert.Equal("1st, 2nd, and 3rd", result);
    }

    #endregion

    #region Dictionary Input

    [Fact]
    public void Format_DictionaryInput()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 42 } };
        var result = formatter.FormatMessage("{n, number, ::ordinal}", args);

        Assert.Equal("42nd", result);
    }

    #endregion

    #region Different Numeric Types

    [Theory]
    [InlineData(1)]
    [InlineData(1L)]
    [InlineData(1.0)]
    [InlineData(1.0f)]
    public void Format_NumericTypes(object value)
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", value } };
        var result = formatter.FormatMessage("{n, number, ::ordinal}", args);

        Assert.Equal("1st", result);
    }

    #endregion

    #region German Locale Tests

    [Theory]
    [InlineData(1, "1.")]
    [InlineData(2, "2.")]
    [InlineData(3, "3.")]
    [InlineData(21, "21.")]
    public void Format_German_OrdinalWithPeriod(int number, string expected)
    {
        var formatter = new MessageFormatter("de-DE", TestOptions.WithGerman());
        var result = formatter.FormatMessage("{n, number, ::ordinal}", new { n = number });

        Assert.Equal(expected, result);
    }

    #endregion

    #region French Locale Tests

    [Theory]
    [InlineData(1, "1er")]
    [InlineData(2, "2e")]
    [InlineData(3, "3e")]
    [InlineData(21, "21e")]  // French: only 1 uses "er", all others use "e"
    public void Format_French_Ordinals(int number, string expected)
    {
        var formatter = new MessageFormatter("fr-FR", TestOptions.WithFrench());
        var result = formatter.FormatMessage("{n, number, ::ordinal}", new { n = number });

        Assert.Equal(expected, result);
    }

    #endregion

    #region Skeleton Parser Tests

    [Fact]
    public void Parser_RecognizesOrdinalToken()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());

        // Should not throw
        var result = formatter.FormatMessage("{n, number, ::ordinal}", new { n = 5 });
        Assert.Equal("5th", result);
    }

    [Fact]
    public void Parser_OrdinalWithSpaces()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{n, number, ::  ordinal  }", new { n = 1 });

        Assert.Equal("1st", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Format_NullValue()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", null } };
        var result = formatter.FormatMessage("{n, number, ::ordinal}", args);

        // Null should be treated as 0
        Assert.Equal("0th", result);
    }

    [Fact]
    public void Format_StringNumber()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{n, number, ::ordinal}", new { n = "42" });

        Assert.Equal("42nd", result);
    }

    #endregion
}
