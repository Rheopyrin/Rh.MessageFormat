using System;
using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Tests for date and time type conversions and edge cases.
/// </summary>
public class DateTimeTypeConversionTests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region DateTime Type Conversion Tests

    [Fact]
    public void Date_FromDateTime_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "d", new DateTime(2024, 3, 15) } };

        var result = _formatter.FormatMessage("{d, date, short}", args);

        Assert.Contains("2024", result);
        Assert.Contains("3", result);
        Assert.Contains("15", result);
    }

    [Fact]
    public void Date_FromDateTimeOffset_FormatsCorrectly()
    {
        var dto = new DateTimeOffset(2024, 3, 15, 10, 30, 0, TimeSpan.FromHours(2));
        var args = new Dictionary<string, object?> { { "d", dto } };

        var result = _formatter.FormatMessage("{d, date, short}", args);

        Assert.Contains("2024", result);
        Assert.Contains("3", result);
        Assert.Contains("15", result);
    }

    [Fact]
    public void Date_FromDateOnly_FormatsCorrectly()
    {
        var dateOnly = new DateOnly(2024, 3, 15);
        var args = new Dictionary<string, object?> { { "d", dateOnly } };

        var result = _formatter.FormatMessage("{d, date, short}", args);

        Assert.Contains("2024", result);
        Assert.Contains("3", result);
        Assert.Contains("15", result);
    }

    [Fact]
    public void Date_FromString_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "d", "2024-03-15" } };

        var result = _formatter.FormatMessage("{d, date, short}", args);

        Assert.Contains("2024", result);
        Assert.Contains("3", result);
        Assert.Contains("15", result);
    }

    [Fact]
    public void Date_NullValue_ReturnsEmpty()
    {
        var args = new Dictionary<string, object?> { { "d", null } };

        var result = _formatter.FormatMessage("Date: {d, date, short}!", args);

        Assert.Equal("Date: !", result);
    }

    [Fact]
    public void Date_FromUnixMilliseconds_Long_FormatsCorrectly()
    {
        // 1704067200000 = January 1, 2024 00:00:00 UTC
        var args = new Dictionary<string, object?> { { "d", 1704067200000L } };

        var result = _formatter.FormatMessage("{d, date, short}", args);

        Assert.Contains("2024", result);
        Assert.Contains("1", result); // January
    }

    [Fact]
    public void Date_FromUnixMilliseconds_Int_FormatsCorrectly()
    {
        // 86400000 = January 2, 1970 00:00:00 UTC (1 day after epoch)
        var args = new Dictionary<string, object?> { { "d", 86400000 } };

        var result = _formatter.FormatMessage("{d, date, short}", args);

        Assert.Contains("1970", result);
        Assert.Contains("1", result); // January
        Assert.Contains("2", result); // Day 2
    }

    [Fact]
    public void Date_FromUnixMilliseconds_Double_FormatsCorrectly()
    {
        // 1704067200000.0 = January 1, 2024 00:00:00 UTC
        var args = new Dictionary<string, object?> { { "d", 1704067200000.0 } };

        var result = _formatter.FormatMessage("{d, date, short}", args);

        Assert.Contains("2024", result);
        Assert.Contains("1", result); // January
    }

    [Fact]
    public void Date_FromUnixMilliseconds_String_FormatsCorrectly()
    {
        // "1704067200000" = January 1, 2024 00:00:00 UTC as string
        var args = new Dictionary<string, object?> { { "d", "1704067200000" } };

        var result = _formatter.FormatMessage("{d, date, short}", args);

        Assert.Contains("2024", result);
        Assert.Contains("1", result); // January
    }

    [Fact]
    public void Date_FromUnixMilliseconds_NoExtraZeroInYear()
    {
        // Verify that formatting doesn't add extra zeros before the year
        // 1704067200000 = January 1, 2024 00:00:00 UTC
        var args = new Dictionary<string, object?> { { "d", 1704067200000L } };

        var result = _formatter.FormatMessage("{d, date, short}", args);

        // Should be like "1/1/24" or "1/1/2024", NOT "1/1/02024"
        Assert.DoesNotContain("02024", result);
        Assert.DoesNotContain("002024", result);
    }

    [Fact]
    public void Date_FromUnixMilliseconds_UkrainianLocale_NoExtraZeroInYear()
    {
        // Bug fix: Ukrainian locale was generating "dd.MM.yyyyy" instead of "dd.MM.yy"
        // This caused dates like "01.01.02024" instead of "01.01.24"
        var ukFormatter = new MessageFormatter("uk", TestOptions.WithCommonLocales());
        var args = new Dictionary<string, object?> { { "expiringDate", "1704067200000" } };

        var result = ukFormatter.FormatMessage("{expiringDate, date, short}", args);

        // Should be "01.01.24", NOT "01.01.02024"
        Assert.DoesNotContain("02024", result);
        Assert.Contains("24", result); // 2-digit year
    }

    [Fact]
    public void Date_FromDateString_StillWorks()
    {
        // Regular date string should still work
        var args = new Dictionary<string, object?> { { "d", "2024-06-15" } };

        var result = _formatter.FormatMessage("{d, date, short}", args);

        Assert.Contains("2024", result);
        Assert.Contains("6", result); // June
        Assert.Contains("15", result);
    }

    #endregion

    #region Time Type Conversion Tests

    [Fact]
    public void Time_FromDateTime_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "t", new DateTime(2024, 1, 1, 14, 30, 45) } };

        var result = _formatter.FormatMessage("{t, time, short}", args);

        Assert.Contains("2", result); // Should contain hour
        Assert.Contains("30", result); // Should contain minutes
    }

    [Fact]
    public void Time_FromDateTimeOffset_FormatsCorrectly()
    {
        var dto = new DateTimeOffset(2024, 1, 1, 14, 30, 45, TimeSpan.FromHours(0));
        var args = new Dictionary<string, object?> { { "t", dto } };

        var result = _formatter.FormatMessage("{t, time, short}", args);

        Assert.Contains("30", result);
    }

    [Fact]
    public void Time_FromTimeOnly_FormatsCorrectly()
    {
        var timeOnly = new TimeOnly(14, 30, 45);
        var args = new Dictionary<string, object?> { { "t", timeOnly } };

        var result = _formatter.FormatMessage("{t, time, short}", args);

        Assert.Contains("30", result);
    }

    [Fact]
    public void Time_FromString_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "t", "2024-01-01T14:30:45" } };

        var result = _formatter.FormatMessage("{t, time, short}", args);

        Assert.Contains("30", result);
    }

    [Fact]
    public void Time_NullValue_ReturnsEmpty()
    {
        var args = new Dictionary<string, object?> { { "t", null } };

        var result = _formatter.FormatMessage("Time: {t, time, short}!", args);

        Assert.Equal("Time: !", result);
    }

    #endregion

    #region Date Style Tests

    [Fact]
    public void Date_ShortStyle_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "d", new DateTime(2024, 12, 25) } };

        var result = _formatter.FormatMessage("{d, date, short}", args);

        Assert.Contains("2024", result);
        Assert.Contains("12", result);
        Assert.Contains("25", result);
    }

    [Fact]
    public void Date_MediumStyle_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "d", new DateTime(2024, 12, 25) } };

        var result = _formatter.FormatMessage("{d, date, medium}", args);

        Assert.Contains("2024", result);
    }

    [Fact]
    public void Date_LongStyle_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "d", new DateTime(2024, 12, 25) } };

        var result = _formatter.FormatMessage("{d, date, long}", args);

        Assert.Contains("2024", result);
    }

    [Fact]
    public void Date_FullStyle_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "d", new DateTime(2024, 12, 25) } };

        var result = _formatter.FormatMessage("{d, date, full}", args);

        Assert.Contains("2024", result);
    }

    [Fact]
    public void Date_DefaultStyle_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "d", new DateTime(2024, 12, 25) } };

        var result = _formatter.FormatMessage("{d, date}", args);

        Assert.Contains("2024", result);
    }

    #endregion

    #region Time Style Tests

    [Fact]
    public void Time_ShortStyle_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "t", new DateTime(2024, 1, 1, 14, 30, 45) } };

        var result = _formatter.FormatMessage("{t, time, short}", args);

        // Short time should have hours and minutes
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Time_MediumStyle_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "t", new DateTime(2024, 1, 1, 14, 30, 45) } };

        var result = _formatter.FormatMessage("{t, time, medium}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void Time_LongStyle_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "t", new DateTime(2024, 1, 1, 14, 30, 45) } };

        var result = _formatter.FormatMessage("{t, time, long}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void Time_FullStyle_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "t", new DateTime(2024, 1, 1, 14, 30, 45) } };

        var result = _formatter.FormatMessage("{t, time, full}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void Time_DefaultStyle_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "t", new DateTime(2024, 1, 1, 14, 30, 45) } };

        var result = _formatter.FormatMessage("{t, time}", args);

        Assert.NotEmpty(result);
    }

    #endregion

    #region Custom Format Tests

    [Fact]
    public void Date_CustomFormat_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "d", new DateTime(2024, 3, 15) } };

        var result = _formatter.FormatMessage("{d, date, yyyy-MM-dd}", args);

        Assert.Equal("2024-03-15", result);
    }

    [Fact]
    public void Time_CustomFormat_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "t", new DateTime(2024, 1, 1, 14, 30, 45) } };

        var result = _formatter.FormatMessage("{t, time, HH:mm:ss}", args);

        Assert.Equal("14:30:45", result);
    }

    #endregion

    #region DateTime Combined Tests

    [Fact]
    public void DateTime_ShortStyle_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "dt", new DateTime(2024, 3, 15, 14, 30, 0) } };

        var result = _formatter.FormatMessage("{dt, datetime, short}", args);

        Assert.Contains("2024", result);
    }

    [Fact]
    public void DateTime_MediumStyle_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "dt", new DateTime(2024, 3, 15, 14, 30, 0) } };

        var result = _formatter.FormatMessage("{dt, datetime, medium}", args);

        Assert.Contains("2024", result);
    }

    [Fact]
    public void DateTime_NullValue_ReturnsEmpty()
    {
        var args = new Dictionary<string, object?> { { "dt", null } };

        var result = _formatter.FormatMessage("DateTime: {dt, datetime, short}!", args);

        Assert.Equal("DateTime: !", result);
    }

    #endregion

    #region Skeleton Tests

    [Fact]
    public void Date_Skeleton_YearMonthDay()
    {
        var args = new Dictionary<string, object?> { { "d", new DateTime(2024, 3, 15) } };

        var result = _formatter.FormatMessage("{d, date, ::yMd}", args);

        Assert.Contains("2024", result);
    }

    [Fact]
    public void Date_Skeleton_MonthDay()
    {
        var args = new Dictionary<string, object?> { { "d", new DateTime(2024, 3, 15) } };

        var result = _formatter.FormatMessage("{d, date, ::Md}", args);

        Assert.Contains("3", result);
        Assert.Contains("15", result);
    }

    [Fact]
    public void Time_Skeleton_HourMinute()
    {
        var args = new Dictionary<string, object?> { { "t", new DateTime(2024, 1, 1, 14, 30, 0) } };

        var result = _formatter.FormatMessage("{t, time, ::Hm}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void Time_Skeleton_HourMinuteSecond()
    {
        var args = new Dictionary<string, object?> { { "t", new DateTime(2024, 1, 1, 14, 30, 45) } };

        var result = _formatter.FormatMessage("{t, time, ::Hms}", args);

        Assert.Contains("45", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Date_LeapYear_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "d", new DateTime(2024, 2, 29) } };

        var result = _formatter.FormatMessage("{d, date, short}", args);

        Assert.Contains("29", result);
    }

    [Fact]
    public void Date_MinValue_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "d", DateTime.MinValue } };

        var result = _formatter.FormatMessage("{d, date, short}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void Date_MaxValue_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "d", DateTime.MaxValue } };

        var result = _formatter.FormatMessage("{d, date, short}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void Time_Midnight_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "t", new DateTime(2024, 1, 1, 0, 0, 0) } };

        var result = _formatter.FormatMessage("{t, time, short}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void Time_EndOfDay_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "t", new DateTime(2024, 1, 1, 23, 59, 59) } };

        var result = _formatter.FormatMessage("{t, time, short}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DateTime_InMessage_FormatsCorrectly()
    {
        var date = new DateTime(2024, 12, 25, 10, 30, 0);
        var args = new Dictionary<string, object?> { { "event", date } };

        var result = _formatter.FormatMessage(
            "The event is on {event, date, short} at {event, time, short}",
            args);

        Assert.Contains("2024", result);
    }

    #endregion
}
