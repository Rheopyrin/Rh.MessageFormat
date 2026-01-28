using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Tests for the NumberRangeElement - formatting number ranges with locale-appropriate separators.
/// </summary>
public class NumberRangeElementTests
{
    #region Basic Formatting Tests

    [Fact]
    public void Format_BasicRange()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{min, numberRange, max}", new { min = 1, max = 10 });

        // Should contain both numbers and a separator
        Assert.Contains("1", result);
        Assert.Contains("10", result);
        Assert.Contains("\u2013", result); // en-dash
    }

    [Fact]
    public void Format_LargeNumbers()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{min, numberRange, max}", new { min = 1000, max = 5000 });

        Assert.Contains("1,000", result);
        Assert.Contains("5,000", result);
    }

    [Fact]
    public void Format_DecimalNumbers()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{min, numberRange, max, ::.00}", new { min = 1.5, max = 3.75 });

        Assert.Contains("1.50", result);
        Assert.Contains("3.75", result);
    }

    #endregion

    #region Skeleton Formatting

    [Fact]
    public void Format_WithCurrencySkeleton()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{min, numberRange, max, ::currency/USD}", new { min = 100, max = 500 });

        Assert.Contains("$", result);
        Assert.Contains("100", result);
        Assert.Contains("500", result);
    }

    [Fact]
    public void Format_WithPercentSkeleton()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{min, numberRange, max, ::%}", new { min = 0.1, max = 0.5 });

        Assert.Contains("%", result);
    }

    [Fact]
    public void Format_WithCompactSkeleton()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{min, numberRange, max, ::compact-short}", new { min = 1000, max = 5000 });

        Assert.Contains("K", result);
    }

    #endregion

    #region Dictionary Input

    [Fact]
    public void Format_DictionaryInput()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?>
        {
            { "min", 10 },
            { "max", 20 }
        };
        var result = formatter.FormatMessage("{min, numberRange, max}", args);

        Assert.Contains("10", result);
        Assert.Contains("20", result);
    }

    #endregion

    #region Combined With Text

    [Fact]
    public void Format_InSentence()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage(
            "Price range: {min, numberRange, max, ::currency/USD}",
            new { min = 100, max = 500 });

        Assert.StartsWith("Price range:", result);
        Assert.Contains("$", result);
    }

    [Fact]
    public void Format_MultipleRanges()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage(
            "Width: {minWidth, numberRange, maxWidth}, Height: {minHeight, numberRange, maxHeight}",
            new { minWidth = 100, maxWidth = 200, minHeight = 50, maxHeight = 100 });

        Assert.Contains("Width:", result);
        Assert.Contains("Height:", result);
    }

    #endregion

    #region Numeric Types

    [Theory]
    [InlineData(1, 10)]
    [InlineData(1L, 10L)]
    [InlineData(1.0, 10.0)]
    [InlineData(1.0f, 10.0f)]
    public void Format_NumericTypes(object min, object max)
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?>
        {
            { "min", min },
            { "max", max }
        };
        var result = formatter.FormatMessage("{min, numberRange, max}", args);

        Assert.Contains("1", result);
        Assert.Contains("10", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Format_SameValues()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{min, numberRange, max}", new { min = 5, max = 5 });

        // Should still format with the separator
        Assert.Contains("5", result);
        Assert.Contains("\u2013", result); // en-dash
    }

    [Fact]
    public void Format_NegativeValues()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{min, numberRange, max}", new { min = -10, max = 10 });

        Assert.Contains("-10", result);
        Assert.Contains("10", result);
    }

    [Fact]
    public void Format_ZeroValues()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{min, numberRange, max}", new { min = 0, max = 100 });

        Assert.Contains("0", result);
        Assert.Contains("100", result);
    }

    [Fact]
    public void Format_NullValues()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?>
        {
            { "min", null },
            { "max", 10 }
        };
        var result = formatter.FormatMessage("{min, numberRange, max}", args);

        // Null should be treated as 0
        Assert.Contains("0", result);
        Assert.Contains("10", result);
    }

    #endregion

    #region Locale-Specific Separators

    [Fact]
    public void Format_EnglishLocale()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{min, numberRange, max}", new { min = 1, max = 10 });

        // English uses thin space + en-dash + thin space
        Assert.Contains("\u2013", result); // en-dash
    }

    [Fact]
    public void Format_GermanLocale()
    {
        var formatter = new MessageFormatter("de-DE", TestOptions.WithGerman());
        var result = formatter.FormatMessage("{min, numberRange, max}", new { min = 1, max = 10 });

        // German also uses en-dash
        Assert.Contains("\u2013", result);
    }

    #endregion

    #region Error Handling

    [Fact]
    public void Parse_MissingEndVariable_ThrowsException()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());

        var ex = Assert.Throws<Rh.MessageFormat.Exceptions.MessageFormatterException>(() =>
            formatter.FormatMessage("{min, numberRange}", new { min = 1 }));

        Assert.Contains("end variable", ex.Message);
    }

    #endregion
}
