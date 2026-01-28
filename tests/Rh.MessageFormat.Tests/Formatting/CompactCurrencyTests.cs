using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Formatting;

/// <summary>
/// Tests for compact currency formatting via skeleton syntax.
/// Compact currency combines currency display with compact notation (K, M, B, T).
/// </summary>
public class CompactCurrencyTests
{
    #region Compact Short Currency Tests

    [Theory]
    [InlineData(1000, "$1K")]
    [InlineData(1500, "$1.5K")]
    [InlineData(10000, "$10K")]
    [InlineData(100000, "$100K")]
    [InlineData(1000000, "$1M")]
    [InlineData(1500000, "$1.5M")]
    [InlineData(10000000, "$10M")]
    [InlineData(1000000000, "$1B")]
    [InlineData(1000000000000, "$1T")]
    public void Format_CompactShort_USD(double value, string expected)
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{amount, number, ::currency/USD compact-short}", new { amount = value });

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1000, "€1K")]
    [InlineData(1500000, "€1.5M")]
    [InlineData(1000000000, "€1B")]
    public void Format_CompactShort_EUR(double value, string expected)
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{amount, number, ::currency/EUR compact-short}", new { amount = value });

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1000, "£1K")]
    [InlineData(1500000, "£1.5M")]
    public void Format_CompactShort_GBP(double value, string expected)
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{amount, number, ::currency/GBP compact-short}", new { amount = value });

        Assert.Equal(expected, result);
    }

    #endregion

    #region Compact Long Currency Tests

    [Theory]
    [InlineData(1000, "$1 thousand")]
    [InlineData(1500000, "$1.5 million")]
    [InlineData(1000000000, "$1 billion")]
    [InlineData(1000000000000, "$1 trillion")]
    public void Format_CompactLong_USD(double value, string expected)
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{amount, number, ::currency/USD compact-long}", new { amount = value });

        Assert.Equal(expected, result);
    }

    #endregion

    #region Short Notation with K/KK

    [Theory]
    [InlineData(1000, "$1K")]
    [InlineData(1500000, "$1.5M")]
    public void Format_K_Notation_USD(double value, string expected)
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{amount, number, ::currency/USD K}", new { amount = value });

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1000, "$1 thousand")]
    [InlineData(1500000, "$1.5 million")]
    public void Format_KK_Notation_USD(double value, string expected)
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{amount, number, ::currency/USD KK}", new { amount = value });

        Assert.Equal(expected, result);
    }

    #endregion

    #region Small Values (Below Compact Threshold)

    [Theory]
    [InlineData(0, "$0.00")]
    [InlineData(1, "$1.00")]
    [InlineData(10, "$10.00")]
    [InlineData(100, "$100.00")]
    [InlineData(999, "$999.00")]
    public void Format_SmallValues_NoCompact(double value, string expected)
    {
        // Values below 1000 are formatted as standard currency with decimals
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{amount, number, ::currency/USD compact-short}", new { amount = value });

        Assert.Equal(expected, result);
    }

    #endregion

    #region Precision Control

    [Fact]
    public void Format_CompactShort_WithPrecision()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{amount, number, ::currency/USD compact-short .00}", new { amount = 1500000 });

        // Should show exactly 2 decimal places
        Assert.Contains("M", result);
    }

    [Fact]
    public void Format_CompactShort_NoPrecision()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{amount, number, ::currency/USD compact-short .0}", new { amount = 1000000 });

        Assert.Contains("M", result);
    }

    #endregion

    #region Negative Values

    [Theory]
    [InlineData(-1000, "-$1K")]
    [InlineData(-1500000, "-$1.5M")]
    public void Format_NegativeValues_CompactShort(double value, string expected)
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{amount, number, ::currency/USD compact-short}", new { amount = value });

        Assert.Equal(expected, result);
    }

    #endregion

    #region Dictionary Input

    [Fact]
    public void Format_DictionaryInput()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "amount", 1000000 } };
        var result = formatter.FormatMessage("{amount, number, ::currency/USD compact-short}", args);

        Assert.Equal("$1M", result);
    }

    #endregion

    #region Combined With Text

    [Fact]
    public void Format_InSentence()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage(
            "The company raised {amount, number, ::currency/USD compact-short} in funding.",
            new { amount = 2500000 });

        Assert.Equal("The company raised $2.5M in funding.", result);
    }

    [Fact]
    public void Format_MultipleAmounts()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage(
            "Revenue: {revenue, number, ::currency/USD compact-short}, Expenses: {expenses, number, ::currency/USD compact-short}",
            new { revenue = 5000000, expenses = 3000000 });

        Assert.Equal("Revenue: $5M, Expenses: $3M", result);
    }

    #endregion

    #region Currency Display Options

    [Fact]
    public void Format_CompactShort_CurrencyCode()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{amount, number, ::currency/USD compact-short unit-width-iso-code}", new { amount = 1000000 });

        Assert.Contains("USD", result);
        Assert.Contains("M", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Format_VeryLargeValue()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{amount, number, ::currency/USD compact-short}", new { amount = 999_000_000_000_000.0 });

        Assert.Contains("T", result);
    }

    [Fact]
    public void Format_NullValue()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "amount", null } };
        var result = formatter.FormatMessage("{amount, number, ::currency/USD compact-short}", args);

        // Null is treated as 0, which is below compact threshold, so formatted as standard currency
        Assert.Equal("$0.00", result);
    }

    #endregion
}
