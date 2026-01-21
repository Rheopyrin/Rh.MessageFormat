using System;
using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Additional tests for date/time skeleton parsing and formatting edge cases.
/// </summary>
public class DateTimeSkeletonEdgeCaseTests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region Date Skeleton Field Tests

    [Fact]
    public void DateSkeleton_YearOnly()
    {
        var date = new DateTime(2026, 6, 15);
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, ::yyyy}", args);

        Assert.Equal("2026", result);
    }

    [Fact]
    public void DateSkeleton_YearShort()
    {
        var date = new DateTime(2026, 6, 15);
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, ::yy}", args);

        Assert.Equal("26", result);
    }

    [Fact]
    public void DateSkeleton_MonthOnly()
    {
        var date = new DateTime(2026, 6, 15);
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, ::MMMM}", args);

        Assert.Equal("June", result);
    }

    [Fact]
    public void DateSkeleton_MonthShort()
    {
        var date = new DateTime(2026, 6, 15);
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, ::MMM}", args);

        Assert.Equal("Jun", result);
    }

    [Fact]
    public void DateSkeleton_MonthNumeric()
    {
        var date = new DateTime(2026, 6, 15);
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, ::MM}", args);

        Assert.Equal("06", result);
    }

    [Fact]
    public void DateSkeleton_DayOnly()
    {
        var date = new DateTime(2026, 6, 5);
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, ::dd}", args);

        Assert.Equal("05", result);
    }

    [Fact]
    public void DateSkeleton_DayOfWeekFull()
    {
        var date = new DateTime(2026, 6, 15); // Monday
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, ::EEEE}", args);

        Assert.Equal("Monday", result);
    }

    [Fact]
    public void DateSkeleton_DayOfWeekShort()
    {
        var date = new DateTime(2026, 6, 15); // Monday
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, ::EEE}", args);

        Assert.Equal("Mon", result);
    }

    #endregion

    #region Time Skeleton Field Tests

    [Fact]
    public void TimeSkeleton_Hour24()
    {
        var time = new DateTime(2026, 1, 1, 14, 30, 0);
        var args = new Dictionary<string, object?> { { "t", time } };

        var result = _formatter.FormatMessage("{t, time, ::HH}", args);

        Assert.Equal("14", result);
    }

    [Fact]
    public void TimeSkeleton_Hour12()
    {
        var time = new DateTime(2026, 1, 1, 14, 30, 0);
        var args = new Dictionary<string, object?> { { "t", time } };

        var result = _formatter.FormatMessage("{t, time, ::hh}", args);

        Assert.Equal("02", result);
    }

    [Fact]
    public void TimeSkeleton_MinutesOnly()
    {
        var time = new DateTime(2026, 1, 1, 14, 5, 0);
        var args = new Dictionary<string, object?> { { "t", time } };

        var result = _formatter.FormatMessage("{t, time, ::mm}", args);

        Assert.Equal("05", result);
    }

    [Fact]
    public void TimeSkeleton_SecondsOnly()
    {
        var time = new DateTime(2026, 1, 1, 14, 30, 45);
        var args = new Dictionary<string, object?> { { "t", time } };

        var result = _formatter.FormatMessage("{t, time, ::ss}", args);

        Assert.Equal("45", result);
    }

    [Fact]
    public void TimeSkeleton_HourMinute()
    {
        var time = new DateTime(2026, 1, 1, 14, 30, 0);
        var args = new Dictionary<string, object?> { { "t", time } };

        var result = _formatter.FormatMessage("{t, time, ::HHmm}", args);

        Assert.Contains("14", result);
        Assert.Contains("30", result);
    }

    #endregion

    #region Combined Date/Time Skeleton Tests

    [Fact]
    public void DateTimeSkeleton_YearMonthDayHourMinute()
    {
        var dt = new DateTime(2026, 6, 15, 14, 30, 0);
        var args = new Dictionary<string, object?> { { "dt", dt } };

        var result = _formatter.FormatMessage("{dt, datetime, ::yMdHm}", args);

        Assert.Contains("2026", result);
        Assert.Contains("6", result);
        Assert.Contains("15", result);
        Assert.Contains("14", result);
        Assert.Contains("30", result);
    }

    [Fact]
    public void DateTimeSkeleton_FullDateTime()
    {
        var dt = new DateTime(2026, 6, 15, 14, 30, 45);
        var args = new Dictionary<string, object?> { { "dt", dt } };

        var result = _formatter.FormatMessage("{dt, datetime, ::EEEEyMMMMdHmmss}", args);

        Assert.Contains("Monday", result);
        Assert.Contains("June", result);
        Assert.Contains("15", result);
        Assert.Contains("14", result);
        Assert.Contains("30", result);
        Assert.Contains("45", result);
    }

    #endregion

    #region Different Date/Time Types

    [Fact]
    public void Date_WithDateOnly()
    {
        var date = new DateOnly(2026, 6, 15);
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, ::yMd}", args);

        Assert.Contains("2026", result);
        Assert.Contains("6", result);
        Assert.Contains("15", result);
    }

    [Fact]
    public void Time_WithTimeOnly()
    {
        var time = new TimeOnly(14, 30, 45);
        var args = new Dictionary<string, object?> { { "t", time } };

        var result = _formatter.FormatMessage("{t, time, ::Hms}", args);

        Assert.Contains("14", result);
        Assert.Contains("30", result);
        Assert.Contains("45", result);
    }

    [Fact]
    public void DateTime_WithDateTimeOffset()
    {
        var dt = new DateTimeOffset(2026, 6, 15, 14, 30, 45, TimeSpan.FromHours(2));
        var args = new Dictionary<string, object?> { { "dt", dt } };

        var result = _formatter.FormatMessage("{dt, datetime, ::yMdHms}", args);

        Assert.Contains("2026", result);
        Assert.Contains("14", result);
    }

    #endregion

    #region Style Tests

    [Fact]
    public void Date_LongStyle_WithDateTimeOffset()
    {
        var dt = new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero);
        var args = new Dictionary<string, object?> { { "d", dt } };

        var result = _formatter.FormatMessage("{d, date, long}", args);

        Assert.Contains("June", result);
        Assert.Contains("15", result);
        Assert.Contains("2026", result);
    }

    [Fact]
    public void Time_LongStyle()
    {
        var time = new DateTime(2026, 1, 1, 14, 30, 45);
        var args = new Dictionary<string, object?> { { "t", time } };

        var result = _formatter.FormatMessage("{t, time, long}", args);

        Assert.Contains("30", result);
        Assert.Contains("45", result);
    }

    [Fact]
    public void DateTime_MediumStyle()
    {
        var dt = new DateTime(2026, 6, 15, 14, 30, 0);
        var args = new Dictionary<string, object?> { { "dt", dt } };

        var result = _formatter.FormatMessage("{dt, datetime, medium}", args);

        Assert.Contains("Jun", result);
        Assert.Contains("15", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Date_NullValue_ReturnsEmpty()
    {
        var args = new Dictionary<string, object?> { { "d", null } };

        var result = _formatter.FormatMessage("Date: {d, date}", args);

        Assert.Equal("Date: ", result);
    }

    [Fact]
    public void Time_NullValue_ReturnsEmpty()
    {
        var args = new Dictionary<string, object?> { { "t", null } };

        var result = _formatter.FormatMessage("Time: {t, time}", args);

        Assert.Equal("Time: ", result);
    }

    [Fact]
    public void DateTime_NullValue_ReturnsEmpty()
    {
        var args = new Dictionary<string, object?> { { "dt", null } };

        var result = _formatter.FormatMessage("DateTime: {dt, datetime}", args);

        Assert.Equal("DateTime: ", result);
    }

    [Fact]
    public void Date_MinValue()
    {
        var date = DateTime.MinValue;
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, ::yyyy}", args);

        Assert.Equal("0001", result);
    }

    [Fact]
    public void Date_MaxValue()
    {
        var date = DateTime.MaxValue;
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, ::yyyy}", args);

        Assert.Equal("9999", result);
    }

    [Fact]
    public void Date_LeapYear()
    {
        var date = new DateTime(2024, 2, 29);
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, ::yMd}", args);

        Assert.Contains("2", result);
        Assert.Contains("29", result);
    }

    [Fact]
    public void Time_Midnight()
    {
        var time = new DateTime(2026, 1, 1, 0, 0, 0);
        var args = new Dictionary<string, object?> { { "t", time } };

        var result = _formatter.FormatMessage("{t, time, ::HHmmss}", args);

        Assert.Contains("00", result);
    }

    [Fact]
    public void Time_Noon()
    {
        var time = new DateTime(2026, 1, 1, 12, 0, 0);
        var args = new Dictionary<string, object?> { { "t", time } };

        var result = _formatter.FormatMessage("{t, time, ::HHmm}", args);

        Assert.Contains("12", result);
        Assert.Contains("00", result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void DateTime_InComplexMessage()
    {
        var dt = new DateTime(2026, 6, 15, 14, 30, 0);
        var args = new Dictionary<string, object?>
        {
            { "name", "Alice" },
            { "event", "meeting" },
            { "dt", dt }
        };

        var result = _formatter.FormatMessage(
            "Hello {name}, your {event} is on {dt, date, ::EEEEMMMMd} at {dt, time, ::hmm}.",
            args);

        Assert.Contains("Alice", result);
        Assert.Contains("meeting", result);
        Assert.Contains("Monday", result);
        Assert.Contains("June", result);
        Assert.Contains("15", result);
    }

    [Fact]
    public void DateTime_MultipleFormatsInMessage()
    {
        var dt = new DateTime(2026, 6, 15, 14, 30, 0);
        var args = new Dictionary<string, object?> { { "dt", dt } };

        var result = _formatter.FormatMessage(
            "Short: {dt, datetime, short} | Full: {dt, datetime, full}",
            args);

        Assert.Contains("Short:", result);
        Assert.Contains("Full:", result);
        Assert.Contains("Monday", result); // From full format
    }

    #endregion
}
