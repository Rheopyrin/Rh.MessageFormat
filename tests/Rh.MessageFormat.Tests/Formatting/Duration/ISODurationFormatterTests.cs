using System;
using System.Collections.Generic;
using System.Globalization;
using Rh.MessageFormat.Formatting.Duration;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Formatting.Duration;

/// <summary>
/// Tests for ISODurationFormatter - parsing and formatting ISO 8601 durations.
/// </summary>
public class ISODurationFormatterTests
{
    private readonly ISODurationFormatter _formatter;
    private readonly CultureInfo _enCulture = CultureInfo.GetCultureInfo("en-US");

    public ISODurationFormatterTests()
    {
        _formatter = new ISODurationFormatter("en");
    }

    #region Parse Tests - Basic Units

    [Fact]
    public void Parse_Years_ParsesCorrectly()
    {
        var result = ISODurationFormatter.Parse("P1Y");

        Assert.Equal(1, result.Years);
        Assert.Equal(0, result.Months);
        Assert.Equal(0, result.Days);
    }

    [Fact]
    public void Parse_Months_ParsesCorrectly()
    {
        var result = ISODurationFormatter.Parse("P2M");

        Assert.Equal(2, result.Months);
    }

    [Fact]
    public void Parse_Weeks_ParsesCorrectly()
    {
        var result = ISODurationFormatter.Parse("P3W");

        Assert.Equal(3, result.Weeks);
    }

    [Fact]
    public void Parse_Days_ParsesCorrectly()
    {
        var result = ISODurationFormatter.Parse("P4D");

        Assert.Equal(4, result.Days);
    }

    [Fact]
    public void Parse_Hours_ParsesCorrectly()
    {
        var result = ISODurationFormatter.Parse("PT5H");

        Assert.Equal(5, result.Hours);
    }

    [Fact]
    public void Parse_Minutes_ParsesCorrectly()
    {
        var result = ISODurationFormatter.Parse("PT6M");

        Assert.Equal(6, result.Minutes);
    }

    [Fact]
    public void Parse_Seconds_ParsesCorrectly()
    {
        var result = ISODurationFormatter.Parse("PT7S");

        Assert.Equal(7, result.Seconds);
    }

    #endregion

    #region Parse Tests - Combined Units

    [Fact]
    public void Parse_FullDuration_ParsesAllUnits()
    {
        var result = ISODurationFormatter.Parse("P1Y2M3DT4H5M6S");

        Assert.Equal(1, result.Years);
        Assert.Equal(2, result.Months);
        Assert.Equal(3, result.Days);
        Assert.Equal(4, result.Hours);
        Assert.Equal(5, result.Minutes);
        Assert.Equal(6, result.Seconds);
    }

    [Fact]
    public void Parse_DateOnly_ParsesCorrectly()
    {
        var result = ISODurationFormatter.Parse("P1Y2M3D");

        Assert.Equal(1, result.Years);
        Assert.Equal(2, result.Months);
        Assert.Equal(3, result.Days);
        Assert.Equal(0, result.Hours);
    }

    [Fact]
    public void Parse_TimeOnly_ParsesCorrectly()
    {
        var result = ISODurationFormatter.Parse("PT1H2M3S");

        Assert.Equal(0, result.Years);
        Assert.Equal(1, result.Hours);
        Assert.Equal(2, result.Minutes);
        Assert.Equal(3, result.Seconds);
    }

    #endregion

    #region Parse Tests - Fractional Values

    [Fact]
    public void Parse_FractionalYears_ParsesCorrectly()
    {
        var result = ISODurationFormatter.Parse("P1.5Y");

        Assert.Equal(1.5, result.Years);
    }

    [Fact]
    public void Parse_FractionalSeconds_ParsesCorrectly()
    {
        var result = ISODurationFormatter.Parse("PT1.5S");

        Assert.Equal(1.5, result.Seconds);
    }

    [Fact]
    public void Parse_FractionalHours_ParsesCorrectly()
    {
        var result = ISODurationFormatter.Parse("PT2.5H");

        Assert.Equal(2.5, result.Hours);
    }

    #endregion

    #region Parse Tests - Negative Duration

    [Fact]
    public void Parse_NegativeDuration_SetsIsNegative()
    {
        var result = ISODurationFormatter.Parse("-P1D");

        Assert.True(result.IsNegative);
        Assert.Equal(1, result.Days);
    }

    [Fact]
    public void Parse_NegativeComplex_ParsesCorrectly()
    {
        var result = ISODurationFormatter.Parse("-PT1H30M");

        Assert.True(result.IsNegative);
        Assert.Equal(1, result.Hours);
        Assert.Equal(30, result.Minutes);
    }

    #endregion

    #region Parse Tests - Invalid Input

    [Fact]
    public void Parse_EmptyString_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => ISODurationFormatter.Parse(""));
    }

    [Fact]
    public void Parse_NullString_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => ISODurationFormatter.Parse(null!));
    }

    [Fact]
    public void Parse_InvalidFormat_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => ISODurationFormatter.Parse("invalid"));
    }

    [Fact]
    public void Parse_MissingP_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => ISODurationFormatter.Parse("1Y"));
    }

    #endregion

    #region TryParse Tests

    [Fact]
    public void TryParse_ValidDuration_ReturnsTrue()
    {
        var success = ISODurationFormatter.TryParse("P1D", out var result);

        Assert.True(success);
        Assert.Equal(1, result.Days);
    }

    [Fact]
    public void TryParse_InvalidDuration_ReturnsFalse()
    {
        var success = ISODurationFormatter.TryParse("invalid", out _);

        Assert.False(success);
    }

    [Fact]
    public void TryParse_EmptyString_ReturnsFalse()
    {
        var success = ISODurationFormatter.TryParse("", out _);

        Assert.False(success);
    }

    [Fact]
    public void TryParse_NullString_ReturnsFalse()
    {
        var success = ISODurationFormatter.TryParse(null!, out _);

        Assert.False(success);
    }

    #endregion

    #region ToSeconds Tests

    [Fact]
    public void ToSeconds_OneHour_Returns3600()
    {
        var seconds = ISODurationFormatter.ToSeconds("PT1H");

        Assert.Equal(3600, seconds);
    }

    [Fact]
    public void ToSeconds_OneDay_Returns86400()
    {
        var seconds = ISODurationFormatter.ToSeconds("P1D");

        Assert.Equal(86400, seconds);
    }

    [Fact]
    public void ToSeconds_OneWeek_Returns604800()
    {
        var seconds = ISODurationFormatter.ToSeconds("P1W");

        Assert.Equal(604800, seconds);
    }

    [Fact]
    public void ToSeconds_Complex_ReturnsCorrectSum()
    {
        // 1 hour + 30 minutes + 15 seconds = 3600 + 1800 + 15 = 5415
        var seconds = ISODurationFormatter.ToSeconds("PT1H30M15S");

        Assert.Equal(5415, seconds);
    }

    [Fact]
    public void ToSeconds_Negative_ReturnsNegativeValue()
    {
        var seconds = ISODurationFormatter.ToSeconds("-PT1H");

        Assert.Equal(-3600, seconds);
    }

    #endregion

    #region TryParseToSeconds Tests

    [Fact]
    public void TryParseToSeconds_ValidDuration_ReturnsTrue()
    {
        var success = ISODurationFormatter.TryParseToSeconds("PT1H", out var seconds);

        Assert.True(success);
        Assert.Equal(3600, seconds);
    }

    [Fact]
    public void TryParseToSeconds_InvalidDuration_ReturnsFalse()
    {
        var success = ISODurationFormatter.TryParseToSeconds("invalid", out var seconds);

        Assert.False(success);
        Assert.Equal(0, seconds);
    }

    #endregion

    #region ToTimeSpan Tests

    [Fact]
    public void ToTimeSpan_OneHour_ReturnsCorrectTimeSpan()
    {
        var timeSpan = ISODurationFormatter.ToTimeSpan("PT1H");

        Assert.Equal(TimeSpan.FromHours(1), timeSpan);
    }

    [Fact]
    public void ToTimeSpan_Complex_ReturnsCorrectTimeSpan()
    {
        var timeSpan = ISODurationFormatter.ToTimeSpan("P1DT2H30M");

        var expected = TimeSpan.FromDays(1) + TimeSpan.FromHours(2) + TimeSpan.FromMinutes(30);
        Assert.Equal(expected, timeSpan);
    }

    [Fact]
    public void ToTimeSpan_Negative_ReturnsNegativeTimeSpan()
    {
        var timeSpan = ISODurationFormatter.ToTimeSpan("-PT1H");

        Assert.Equal(TimeSpan.FromHours(-1), timeSpan);
    }

    #endregion

    #region ToISOString Tests

    [Fact]
    public void ToISOString_OneHour_ReturnsCorrectFormat()
    {
        var result = ISODurationFormatter.ToISOString(3600);

        Assert.Equal("PT1H", result);
    }

    [Fact]
    public void ToISOString_OneDay_ReturnsCorrectFormat()
    {
        var result = ISODurationFormatter.ToISOString(86400);

        Assert.Equal("P1D", result);
    }

    [Fact]
    public void ToISOString_Complex_ReturnsCorrectFormat()
    {
        // 1 day, 2 hours, 30 minutes, 15 seconds
        var result = ISODurationFormatter.ToISOString(86400 + 7200 + 1800 + 15);

        Assert.Contains("D", result);
        Assert.Contains("H", result);
        Assert.Contains("M", result);
        Assert.Contains("S", result);
    }

    [Fact]
    public void ToISOString_Zero_ReturnsZeroDuration()
    {
        var result = ISODurationFormatter.ToISOString(0);

        Assert.Equal("PT0S", result);
    }

    [Fact]
    public void ToISOString_Negative_IncludesMinusSign()
    {
        var result = ISODurationFormatter.ToISOString(-3600);

        Assert.StartsWith("-P", result);
    }

    [Fact]
    public void ToISOString_TimeSpan_ReturnsCorrectFormat()
    {
        var result = ISODurationFormatter.ToISOString(TimeSpan.FromHours(2.5));

        Assert.Contains("H", result);
        Assert.Contains("M", result);
    }

    #endregion

    #region ParsedISODuration Tests

    [Fact]
    public void ParsedISODuration_ToTotalSeconds_CalculatesCorrectly()
    {
        var parsed = new ParsedISODuration(0, 0, 0, 1, 2, 30, 15, false);

        // 1 day + 2 hours + 30 minutes + 15 seconds
        var expected = 86400 + 7200 + 1800 + 15;
        Assert.Equal(expected, parsed.ToTotalSeconds());
    }

    [Fact]
    public void ParsedISODuration_ToTotalSeconds_NegativeIsNegative()
    {
        var parsed = new ParsedISODuration(0, 0, 0, 0, 1, 0, 0, true);

        Assert.Equal(-3600, parsed.ToTotalSeconds());
    }

    [Fact]
    public void ParsedISODuration_ToTimeSpan_ReturnsCorrectValue()
    {
        var parsed = new ParsedISODuration(0, 0, 0, 0, 1, 30, 0, false);

        var expected = TimeSpan.FromMinutes(90);
        Assert.Equal(expected, parsed.ToTimeSpan());
    }

    [Fact]
    public void ParsedISODuration_ToString_ReturnsISOFormat()
    {
        var parsed = new ParsedISODuration(1, 2, 0, 3, 4, 5, 6, false);

        var result = parsed.ToString();

        Assert.StartsWith("P", result);
        Assert.Contains("Y", result);
        Assert.Contains("M", result);
        Assert.Contains("D", result);
        Assert.Contains("H", result);
    }

    [Fact]
    public void ParsedISODuration_ToString_ZeroDuration()
    {
        var parsed = new ParsedISODuration(0, 0, 0, 0, 0, 0, 0, false);

        var result = parsed.ToString();

        Assert.Equal("PT0S", result);
    }

    #endregion

    #region Format Tests - CustomFormatterDelegate

    [Fact]
    public void Format_ISOString_FormatsCorrectly()
    {
        var result = _formatter.Format("PT1H30M", "long", "en", _enCulture);

        // Should contain hours and minutes
        Assert.Contains("hour", result);
    }

    [Fact]
    public void Format_ISOString_TimerStyle()
    {
        var result = _formatter.Format("PT1H30M15S", "timer", "en", _enCulture);

        Assert.Equal("1:30:15", result);
    }

    [Fact]
    public void Format_NumericValue_PassesThroughCorrectly()
    {
        var result = _formatter.Format(3600.0, "timer", "en", _enCulture);

        Assert.Equal("1:00:00", result);
    }

    [Fact]
    public void Format_NumericString_ParsesAsSeconds()
    {
        var result = _formatter.Format("3600", "timer", "en", _enCulture);

        Assert.Equal("1:00:00", result);
    }

    [Fact]
    public void Format_InvalidISOString_ReturnsOriginal()
    {
        var result = _formatter.Format("invalid", "long", "en", _enCulture);

        Assert.Equal("invalid", result);
    }

    [Fact]
    public void Format_NullValue_ReturnsEmpty()
    {
        var result = _formatter.Format(null, "long", "en", _enCulture);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Format_EmptyString_ReturnsEmpty()
    {
        var result = _formatter.Format("", "long", "en", _enCulture);

        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region Integration with MessageFormatter Tests

    [Fact]
    public void Integration_RegisterAsCustomFormatter()
    {
        var options = TestOptions.WithEnglish();
        var isoFormatter = new ISODurationFormatter("en");
        options.CustomFormatters["isoduration"] = isoFormatter.Format;

        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "duration", "PT1H30M" } };
        var result = formatter.FormatMessage("Duration: {duration, isoduration, timer}", args);

        Assert.Equal("Duration: 1:30:00", result);
    }

    [Fact]
    public void Integration_LongStyleInMessage()
    {
        var options = TestOptions.WithEnglish();
        var isoFormatter = new ISODurationFormatter("en");
        options.CustomFormatters["isoduration"] = isoFormatter.Format;

        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "time", "P1DT2H" } };
        var result = formatter.FormatMessage("Elapsed: {time, isoduration, long}", args);

        Assert.Contains("day", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Parse_WhitespaceString_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => ISODurationFormatter.Parse("   "));
    }

    [Fact]
    public void Parse_POnly_ParsesAsZeroDuration()
    {
        var success = ISODurationFormatter.TryParse("P", out var result);

        Assert.True(success);
        Assert.Equal(0, result.ToTotalSeconds());
    }

    [Fact]
    public void Parse_LargeValues_HandlesCorrectly()
    {
        var result = ISODurationFormatter.Parse("P100Y");

        Assert.Equal(100, result.Years);
    }

    [Fact]
    public void ToSeconds_VeryLargeDuration_HandlesCorrectly()
    {
        // 100 years in seconds
        var seconds = ISODurationFormatter.ToSeconds("P100Y");

        Assert.True(seconds > 3_000_000_000); // > 3 billion seconds
    }

    #endregion
}
