using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Tests for relative time formatting.
/// </summary>
public class RelativeTimeFormattingTests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region Basic Day Formatting Tests

    [Fact]
    public void RelativeTime_Day_FuturePlural()
    {
        var args = new Dictionary<string, object?> { { "days", 5 } };

        var result = _formatter.FormatMessage("{days, relativeTime, day}", args);

        Assert.Equal("in 5 days", result);
    }

    [Fact]
    public void RelativeTime_Day_FutureSingular()
    {
        var args = new Dictionary<string, object?> { { "days", 1 } };

        var result = _formatter.FormatMessage("{days, relativeTime, day}", args);

        Assert.Equal("in 1 day", result);
    }

    [Fact]
    public void RelativeTime_Day_PastPlural()
    {
        var args = new Dictionary<string, object?> { { "days", -3 } };

        var result = _formatter.FormatMessage("{days, relativeTime, day}", args);

        Assert.Equal("3 days ago", result);
    }

    [Fact]
    public void RelativeTime_Day_PastSingular()
    {
        var args = new Dictionary<string, object?> { { "days", -1 } };

        var result = _formatter.FormatMessage("{days, relativeTime, day}", args);

        Assert.Equal("1 day ago", result);
    }

    #endregion

    #region Auto Numeric Mode Tests

    [Fact]
    public void RelativeTime_Day_Auto_Yesterday()
    {
        var args = new Dictionary<string, object?> { { "days", -1 } };

        var result = _formatter.FormatMessage("{days, relativeTime, day long auto}", args);

        Assert.Equal("yesterday", result);
    }

    [Fact]
    public void RelativeTime_Day_Auto_Today()
    {
        var args = new Dictionary<string, object?> { { "days", 0 } };

        var result = _formatter.FormatMessage("{days, relativeTime, day long auto}", args);

        Assert.Equal("today", result);
    }

    [Fact]
    public void RelativeTime_Day_Auto_Tomorrow()
    {
        var args = new Dictionary<string, object?> { { "days", 1 } };

        var result = _formatter.FormatMessage("{days, relativeTime, day long auto}", args);

        Assert.Equal("tomorrow", result);
    }

    [Fact]
    public void RelativeTime_Day_Auto_FallbackToNumeric()
    {
        var args = new Dictionary<string, object?> { { "days", 5 } };

        var result = _formatter.FormatMessage("{days, relativeTime, day long auto}", args);

        Assert.Equal("in 5 days", result);
    }

    #endregion

    #region Style Tests

    [Fact]
    public void RelativeTime_Day_Long()
    {
        var args = new Dictionary<string, object?> { { "days", 2 } };

        var result = _formatter.FormatMessage("{days, relativeTime, day long}", args);

        Assert.Equal("in 2 days", result);
    }

    [Fact]
    public void RelativeTime_Day_Narrow()
    {
        var args = new Dictionary<string, object?> { { "days", 2 } };

        var result = _formatter.FormatMessage("{days, relativeTime, day narrow}", args);

        Assert.Equal("in 2d", result);
    }

    [Fact]
    public void RelativeTime_Day_Narrow_Past()
    {
        var args = new Dictionary<string, object?> { { "days", -3 } };

        var result = _formatter.FormatMessage("{days, relativeTime, day narrow}", args);

        Assert.Equal("3d ago", result);
    }

    #endregion

    #region Different Field Tests

    [Fact]
    public void RelativeTime_Year_Future()
    {
        var args = new Dictionary<string, object?> { { "years", 2 } };

        var result = _formatter.FormatMessage("{years, relativeTime, year}", args);

        Assert.Equal("in 2 years", result);
    }

    [Fact]
    public void RelativeTime_Year_Past()
    {
        var args = new Dictionary<string, object?> { { "years", -1 } };

        var result = _formatter.FormatMessage("{years, relativeTime, year}", args);

        Assert.Equal("1 year ago", result);
    }

    [Fact]
    public void RelativeTime_Year_Auto_LastYear()
    {
        var args = new Dictionary<string, object?> { { "years", -1 } };

        var result = _formatter.FormatMessage("{years, relativeTime, year long auto}", args);

        Assert.Equal("last year", result);
    }

    [Fact]
    public void RelativeTime_Month_Future()
    {
        var args = new Dictionary<string, object?> { { "months", 3 } };

        var result = _formatter.FormatMessage("{months, relativeTime, month}", args);

        Assert.Equal("in 3 months", result);
    }

    [Fact]
    public void RelativeTime_Week_Future()
    {
        var args = new Dictionary<string, object?> { { "weeks", 2 } };

        var result = _formatter.FormatMessage("{weeks, relativeTime, week}", args);

        Assert.Equal("in 2 weeks", result);
    }

    [Fact]
    public void RelativeTime_Hour_Future()
    {
        var args = new Dictionary<string, object?> { { "hours", 5 } };

        var result = _formatter.FormatMessage("{hours, relativeTime, hour}", args);

        Assert.Equal("in 5 hours", result);
    }

    [Fact]
    public void RelativeTime_Minute_Past()
    {
        var args = new Dictionary<string, object?> { { "minutes", -10 } };

        var result = _formatter.FormatMessage("{minutes, relativeTime, minute}", args);

        Assert.Equal("10 minutes ago", result);
    }

    [Fact]
    public void RelativeTime_Second_Now()
    {
        var args = new Dictionary<string, object?> { { "seconds", 0 } };

        var result = _formatter.FormatMessage("{seconds, relativeTime, second long auto}", args);

        Assert.Equal("now", result);
    }

    #endregion

    #region Different Value Types

    [Fact]
    public void RelativeTime_IntValue()
    {
        var args = new Dictionary<string, object?> { { "value", 3 } };

        var result = _formatter.FormatMessage("{value, relativeTime, day}", args);

        Assert.Equal("in 3 days", result);
    }

    [Fact]
    public void RelativeTime_DoubleValue()
    {
        var args = new Dictionary<string, object?> { { "value", 2.0 } };

        var result = _formatter.FormatMessage("{value, relativeTime, day}", args);

        Assert.Equal("in 2 days", result);
    }

    [Fact]
    public void RelativeTime_LongValue()
    {
        var args = new Dictionary<string, object?> { { "value", -5L } };

        var result = _formatter.FormatMessage("{value, relativeTime, day}", args);

        Assert.Equal("5 days ago", result);
    }

    [Fact]
    public void RelativeTime_StringValue()
    {
        var args = new Dictionary<string, object?> { { "value", "3" } };

        var result = _formatter.FormatMessage("{value, relativeTime, day}", args);

        Assert.Equal("in 3 days", result);
    }

    [Fact]
    public void RelativeTime_NullValue()
    {
        var args = new Dictionary<string, object?> { { "value", null } };

        var result = _formatter.FormatMessage("Time: {value, relativeTime, day long auto}", args);

        // null converts to 0, and with auto it should show "today"
        Assert.Equal("Time: today", result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void RelativeTime_InComplexMessage()
    {
        var args = new Dictionary<string, object?>
        {
            { "name", "Alice" },
            { "days", -2 }
        };

        var result = _formatter.FormatMessage("{name} visited {days, relativeTime, day}.", args);

        Assert.Equal("Alice visited 2 days ago.", result);
    }

    [Fact]
    public void RelativeTime_WithOtherFormatters()
    {
        var args = new Dictionary<string, object?>
        {
            { "count", 3 },
            { "days", 5 },
            { "items", new[] { "book", "pen", "notebook" } }
        };

        var result = _formatter.FormatMessage("Order {count, number} items: {items, list}. Delivery: {days, relativeTime, day}.", args);

        Assert.Equal("Order 3 items: book, pen, and notebook. Delivery: in 5 days.", result);
    }

    [Fact]
    public void RelativeTime_DefaultsToDay_WhenNoFieldSpecified()
    {
        var args = new Dictionary<string, object?> { { "value", 3 } };

        var result = _formatter.FormatMessage("{value, relativeTime}", args);

        Assert.Equal("in 3 days", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void RelativeTime_ZeroValue_Future()
    {
        var args = new Dictionary<string, object?> { { "value", 0 } };

        var result = _formatter.FormatMessage("{value, relativeTime, day}", args);

        // 0 is treated as future, so "in 0 days"
        Assert.Equal("in 0 days", result);
    }

    [Fact]
    public void RelativeTime_InvalidField_UsesFallback()
    {
        var args = new Dictionary<string, object?> { { "value", 3 } };

        var result = _formatter.FormatMessage("{value, relativeTime, invalid}", args);

        // Should use fallback format
        Assert.Equal("in 3 invalid", result);
    }

    #endregion
}
