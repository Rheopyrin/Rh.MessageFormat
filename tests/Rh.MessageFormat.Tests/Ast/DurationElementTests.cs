using System;
using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Tests for the DurationElement - first-class duration formatting in message patterns.
/// </summary>
public class DurationElementTests
{
    #region Basic Formatting Tests

    [Fact]
    public void Format_DefaultStyle_SecondsInput()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{d, duration}", new { d = 3661 });

        // Default is long style: 1 hour 1 minute 1 second
        Assert.Contains("hour", result);
        Assert.Contains("minute", result);
        Assert.Contains("second", result);
    }

    [Fact]
    public void Format_LongStyle_ExplicitSeconds()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{d, duration, long}", new { d = 3600 });

        Assert.Contains("hour", result);
    }

    [Fact]
    public void Format_ShortStyle()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{d, duration, short}", new { d = 3600 });

        Assert.Contains("hr", result);
    }

    [Fact]
    public void Format_NarrowStyle()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{d, duration, narrow}", new { d = 3600 });

        Assert.Contains("h", result);
    }

    [Fact]
    public void Format_TimerStyle()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{d, duration, timer}", new { d = 3661 });

        // Timer style: "1:01:01"
        Assert.Equal("1:01:01", result);
    }

    [Fact]
    public void Format_TimerStyle_ZeroSeconds()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{d, duration, timer}", new { d = 0 });

        Assert.Equal("0:00:00", result);
    }

    #endregion

    #region TimeSpan Input Tests

    [Fact]
    public void Format_TimeSpanInput_Long()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var timeSpan = TimeSpan.FromHours(1) + TimeSpan.FromMinutes(30);
        var result = formatter.FormatMessage("{d, duration, long}", new { d = timeSpan });

        Assert.Contains("hour", result);
        Assert.Contains("minute", result);
    }

    [Fact]
    public void Format_TimeSpanInput_Timer()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var timeSpan = TimeSpan.FromHours(1) + TimeSpan.FromMinutes(30) + TimeSpan.FromSeconds(45);
        var result = formatter.FormatMessage("{d, duration, timer}", new { d = timeSpan });

        Assert.Equal("1:30:45", result);
    }

    [Fact]
    public void Format_TimeSpanInput_Days()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var timeSpan = TimeSpan.FromDays(2) + TimeSpan.FromHours(3);
        var result = formatter.FormatMessage("{d, duration, long}", new { d = timeSpan });

        Assert.Contains("day", result);
        Assert.Contains("hour", result);
    }

    #endregion

    #region ISO 8601 String Input Tests

    [Fact]
    public void Format_ISOStringInput_Simple()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{d, duration, timer}", new { d = "PT1H30M" });

        Assert.Equal("1:30:00", result);
    }

    [Fact]
    public void Format_ISOStringInput_Complex()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{d, duration, timer}", new { d = "PT1H30M45S" });

        Assert.Equal("1:30:45", result);
    }

    [Fact]
    public void Format_ISOStringInput_Long()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{d, duration, long}", new { d = "PT2H" });

        Assert.Contains("hour", result);
    }

    [Fact]
    public void Format_ISOStringInput_Days()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{d, duration, long}", new { d = "P1DT2H" });

        Assert.Contains("day", result);
        Assert.Contains("hour", result);
    }

    #endregion

    #region Custom Format Tests

    [Fact]
    public void Format_CustomFormat_HoursMinutes()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{d, duration, {hours}:{minutes}}", new { d = 5400 }); // 1.5 hours

        // Custom format should use the provided template
        Assert.Contains(":", result);
    }

    #endregion

    #region Dictionary Input Tests

    [Fact]
    public void Format_DictionaryInput()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "d", 3661 } };
        var result = formatter.FormatMessage("{d, duration, timer}", args);

        Assert.Equal("1:01:01", result);
    }

    #endregion

    #region Numeric Type Input Tests

    [Theory]
    [InlineData(3600)]
    [InlineData(3600L)]
    [InlineData(3600.0)]
    [InlineData(3600.0f)]
    public void Format_NumericTypes(object value)
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "d", value } };
        var result = formatter.FormatMessage("{d, duration, timer}", args);

        Assert.Equal("1:00:00", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Format_NullValue()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "d", null } };
        var result = formatter.FormatMessage("{d, duration, timer}", args);

        Assert.Equal("0:00:00", result);
    }

    [Fact]
    public void Format_NegativeValue_Timer()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{d, duration, timer}", new { d = -3600 });

        // Negative duration handling - should format the absolute value
        Assert.Contains(":", result);
    }

    [Fact]
    public void Format_LargeValue()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{d, duration, long}", new { d = 86400 * 365 }); // 1 year in seconds

        Assert.Contains("year", result);
    }

    [Fact]
    public void Format_InvalidISOString_FallsBackToZero()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{d, duration, timer}", new { d = "not-a-duration" });

        // Invalid string should result in 0
        Assert.Equal("0:00:00", result);
    }

    [Fact]
    public void Format_NumericString()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("{d, duration, timer}", new { d = "3661" });

        Assert.Equal("1:01:01", result);
    }

    #endregion

    #region Combined Message Tests

    [Fact]
    public void Format_CombinedWithText()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage("Video length: {d, duration, timer}", new { d = 3661 });

        Assert.Equal("Video length: 1:01:01", result);
    }

    [Fact]
    public void Format_MultipleDurations()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage(
            "Start: {start, duration, timer}, End: {end, duration, timer}",
            new { start = 60, end = 120 });

        Assert.Contains("1:00", result);
        Assert.Contains("2:00", result);
    }

    #endregion

    #region Style Case Insensitivity Tests

    [Theory]
    [InlineData("LONG")]
    [InlineData("Long")]
    [InlineData("long")]
    public void Format_StyleCaseInsensitive_Long(string style)
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage($"{{d, duration, {style}}}", new { d = 3600 });

        Assert.Contains("hour", result);
    }

    [Theory]
    [InlineData("TIMER")]
    [InlineData("Timer")]
    [InlineData("timer")]
    public void Format_StyleCaseInsensitive_Timer(string style)
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var result = formatter.FormatMessage($"{{d, duration, {style}}}", new { d = 3661 });

        Assert.Equal("1:01:01", result);
    }

    #endregion
}
