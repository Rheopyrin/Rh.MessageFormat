using System;
using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.E2E;

/// <summary>
/// End-to-end tests for quarter skeleton formatting.
/// Tests the complete flow: Parse message -> Format with skeleton -> Post-process -> Verify output.
/// </summary>
public class QuarterFormattingE2ETests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region Basic Quarter Tests

    [Theory]
    [InlineData(1, "Q1")]
    [InlineData(2, "Q1")]
    [InlineData(3, "Q1")]
    [InlineData(4, "Q2")]
    [InlineData(5, "Q2")]
    [InlineData(6, "Q2")]
    [InlineData(7, "Q3")]
    [InlineData(8, "Q3")]
    [InlineData(9, "Q3")]
    [InlineData(10, "Q4")]
    [InlineData(11, "Q4")]
    [InlineData(12, "Q4")]
    public void E2E_Quarter_AllMonthsAbbreviated(int month, string expectedQuarter)
    {
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, month, 15) } };

        var result = _formatter.FormatMessage("{date, date, ::Q}", args);

        Assert.Equal(expectedQuarter, result);
    }

    [Theory]
    [InlineData(1, "1st quarter")]
    [InlineData(4, "2nd quarter")]
    [InlineData(7, "3rd quarter")]
    [InlineData(10, "4th quarter")]
    public void E2E_Quarter_AllQuartersWide(int month, string expectedQuarter)
    {
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, month, 15) } };

        var result = _formatter.FormatMessage("{date, date, ::QQQQ}", args);

        Assert.Equal(expectedQuarter, result);
    }

    [Theory]
    [InlineData(1, "1")]
    [InlineData(4, "2")]
    [InlineData(7, "3")]
    [InlineData(10, "4")]
    public void E2E_Quarter_AllQuartersNarrow(int month, string expectedQuarter)
    {
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, month, 15) } };

        var result = _formatter.FormatMessage("{date, date, ::QQQQQ}", args);

        Assert.Equal(expectedQuarter, result);
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public void E2E_Quarter_FirstDayOfYear()
    {
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 1, 1) } };

        var result = _formatter.FormatMessage("{date, date, ::Q}", args);

        Assert.Equal("Q1", result);
    }

    [Fact]
    public void E2E_Quarter_LastDayOfYear()
    {
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 12, 31) } };

        var result = _formatter.FormatMessage("{date, date, ::Q}", args);

        Assert.Equal("Q4", result);
    }

    [Fact]
    public void E2E_Quarter_Q1ToQ2Boundary()
    {
        var march31 = new DateTime(2024, 3, 31);
        var april1 = new DateTime(2024, 4, 1);

        var argsQ1 = new Dictionary<string, object?> { { "date", march31 } };
        var argsQ2 = new Dictionary<string, object?> { { "date", april1 } };

        var resultQ1 = _formatter.FormatMessage("{date, date, ::Q}", argsQ1);
        var resultQ2 = _formatter.FormatMessage("{date, date, ::Q}", argsQ2);

        Assert.Equal("Q1", resultQ1);
        Assert.Equal("Q2", resultQ2);
    }

    [Fact]
    public void E2E_Quarter_Q2ToQ3Boundary()
    {
        var june30 = new DateTime(2024, 6, 30);
        var july1 = new DateTime(2024, 7, 1);

        var argsQ2 = new Dictionary<string, object?> { { "date", june30 } };
        var argsQ3 = new Dictionary<string, object?> { { "date", july1 } };

        var resultQ2 = _formatter.FormatMessage("{date, date, ::Q}", argsQ2);
        var resultQ3 = _formatter.FormatMessage("{date, date, ::Q}", argsQ3);

        Assert.Equal("Q2", resultQ2);
        Assert.Equal("Q3", resultQ3);
    }

    [Fact]
    public void E2E_Quarter_Q3ToQ4Boundary()
    {
        var sept30 = new DateTime(2024, 9, 30);
        var oct1 = new DateTime(2024, 10, 1);

        var argsQ3 = new Dictionary<string, object?> { { "date", sept30 } };
        var argsQ4 = new Dictionary<string, object?> { { "date", oct1 } };

        var resultQ3 = _formatter.FormatMessage("{date, date, ::Q}", argsQ3);
        var resultQ4 = _formatter.FormatMessage("{date, date, ::Q}", argsQ4);

        Assert.Equal("Q3", resultQ3);
        Assert.Equal("Q4", resultQ4);
    }

    #endregion

    #region Combined Format Tests

    [Fact]
    public void E2E_Quarter_WithYear()
    {
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 6, 15) } };

        var result = _formatter.FormatMessage("{date, date, ::yQ}", args);

        Assert.Contains("2024", result);
        Assert.Contains("Q2", result);
    }

    [Fact]
    public void E2E_Quarter_InComplexMessage()
    {
        var args = new Dictionary<string, object?>
        {
            { "name", "Alice" },
            { "date", new DateTime(2024, 7, 15) }
        };

        var result = _formatter.FormatMessage("Hello {name}, the report for {date, date, ::QQQQ} is ready.", args);

        Assert.Equal("Hello Alice, the report for 3rd quarter is ready.", result);
    }

    [Fact]
    public void E2E_Quarter_MultipleInMessage()
    {
        var args = new Dictionary<string, object?>
        {
            { "start", new DateTime(2024, 1, 1) },
            { "end", new DateTime(2024, 12, 31) }
        };

        var result = _formatter.FormatMessage("From {start, date, ::Q} to {end, date, ::Q}", args);

        Assert.Equal("From Q1 to Q4", result);
    }

    #endregion

    #region Standalone Quarter Tests

    [Fact]
    public void E2E_StandaloneQuarter_Abbreviated()
    {
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 7, 15) } };

        var result = _formatter.FormatMessage("{date, date, ::q}", args);

        Assert.Equal("Q3", result);
    }

    [Fact]
    public void E2E_StandaloneQuarter_Wide()
    {
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 4, 15) } };

        var result = _formatter.FormatMessage("{date, date, ::qqqq}", args);

        Assert.Equal("2nd quarter", result);
    }

    [Fact]
    public void E2E_StandaloneQuarter_Narrow()
    {
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 10, 15) } };

        var result = _formatter.FormatMessage("{date, date, ::qqqqq}", args);

        Assert.Equal("4", result);
    }

    #endregion

    #region Multi-Locale Tests

    [Fact]
    public void E2E_Quarter_GermanLocale()
    {
        var formatter = new MessageFormatter("de-DE", TestOptions.WithGerman());
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 7, 15) } };

        var result = formatter.FormatMessage("{date, date, ::QQQQ}", args);

        Assert.Equal("3. Quartal", result);
    }

    [Fact]
    public void E2E_Quarter_FrenchLocale()
    {
        var formatter = new MessageFormatter("fr-FR", TestOptions.WithFrench());
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 1, 15) } };

        var abbreviated = formatter.FormatMessage("{date, date, ::Q}", args);
        var wide = formatter.FormatMessage("{date, date, ::QQQQ}", args);

        Assert.Equal("T1", abbreviated);
        Assert.Equal("1er trimestre", wide);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void E2E_Quarter_WithDateOnly()
    {
        var args = new Dictionary<string, object?> { { "date", new DateOnly(2024, 7, 15) } };

        var result = _formatter.FormatMessage("{date, date, ::Q}", args);

        Assert.Equal("Q3", result);
    }

    [Fact]
    public void E2E_Quarter_WithDateTimeOffset()
    {
        var dto = new DateTimeOffset(2024, 10, 15, 10, 30, 0, TimeSpan.FromHours(-5));
        var args = new Dictionary<string, object?> { { "date", dto } };

        var result = _formatter.FormatMessage("{date, date, ::Q}", args);

        Assert.Equal("Q4", result);
    }

    [Fact]
    public void E2E_Quarter_LeapYearFeb29()
    {
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 2, 29) } };

        var result = _formatter.FormatMessage("{date, date, ::Q}", args);

        Assert.Equal("Q1", result);
    }

    #endregion
}
