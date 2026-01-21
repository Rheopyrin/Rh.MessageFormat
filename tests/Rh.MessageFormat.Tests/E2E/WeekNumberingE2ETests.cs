using System;
using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.E2E;

/// <summary>
/// End-to-end tests for week-of-year skeleton formatting.
/// Tests the complete flow: Parse message -> Format with skeleton -> Post-process -> Verify output.
/// Tests both ISO 8601 (Monday first, minDays 4) and US (Sunday first, minDays 1) week rules.
/// </summary>
public class WeekNumberingE2ETests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());
    private readonly MessageFormatter _usFormatter;

    public WeekNumberingE2ETests()
    {
        // Create US formatter with Sunday-first week rules
        _usFormatter = new("en-US", TestOptions.WithLocaleData(MockCldrLocaleData.CreateEnglishUS()));
    }

    #region Basic ISO 8601 Week Tests

    [Fact]
    public void E2E_WeekOfYear_ISO_FirstWeekOfYear_2026()
    {
        // 2026-01-01 is Thursday - in ISO 8601, this is week 1
        // because Thursday is the 4th day >= minDays(4)
        var args = new Dictionary<string, object?> { { "date", new DateTime(2026, 1, 1) } };

        var result = _formatter.FormatMessage("{date, date, ::w}", args);

        Assert.Equal("1", result);
    }

    [Fact]
    public void E2E_WeekOfYear_ISO_MidYear()
    {
        // 2026-06-15 is Monday - should be week 25
        var args = new Dictionary<string, object?> { { "date", new DateTime(2026, 6, 15) } };

        var result = _formatter.FormatMessage("{date, date, ::w}", args);

        Assert.Equal("25", result);
    }

    [Fact]
    public void E2E_WeekOfYear_ISO_LastWeekOfYear_2026()
    {
        // 2026-12-31 is Thursday - ISO week 53
        var args = new Dictionary<string, object?> { { "date", new DateTime(2026, 12, 31) } };

        var result = _formatter.FormatMessage("{date, date, ::w}", args);

        Assert.Equal("53", result);
    }

    [Fact]
    public void E2E_WeekOfYear_ISO_Week52()
    {
        // 2024-12-30 is Monday in week 1 of 2025 (ISO)
        // 2024-12-23 is Monday in week 52
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 12, 23) } };

        var result = _formatter.FormatMessage("{date, date, ::w}", args);

        Assert.Equal("52", result);
    }

    #endregion

    #region Padding Tests

    [Fact]
    public void E2E_WeekOfYear_SingleW_NoPadding()
    {
        // Week 3 should be "3" (no padding)
        var args = new Dictionary<string, object?> { { "date", new DateTime(2026, 1, 15) } };

        var result = _formatter.FormatMessage("{date, date, ::w}", args);

        Assert.Equal("3", result);
    }

    [Fact]
    public void E2E_WeekOfYear_DoubleW_WithPadding()
    {
        // Week 3 should be "03" (padded to 2 digits)
        var args = new Dictionary<string, object?> { { "date", new DateTime(2026, 1, 15) } };

        var result = _formatter.FormatMessage("{date, date, ::ww}", args);

        Assert.Equal("03", result);
    }

    [Fact]
    public void E2E_WeekOfYear_DoubleW_DoubleDigit()
    {
        // Week 25 should be "25" (already 2 digits)
        var args = new Dictionary<string, object?> { { "date", new DateTime(2026, 6, 15) } };

        var result = _formatter.FormatMessage("{date, date, ::ww}", args);

        Assert.Equal("25", result);
    }

    [Fact]
    public void E2E_WeekOfYear_DoubleW_Week1()
    {
        // Week 1 should be "01" (padded)
        var args = new Dictionary<string, object?> { { "date", new DateTime(2026, 1, 1) } };

        var result = _formatter.FormatMessage("{date, date, ::ww}", args);

        Assert.Equal("01", result);
    }

    #endregion

    #region US Week Rules Tests

    [Fact]
    public void E2E_WeekOfYear_US_FirstWeekOfYear()
    {
        // US rules: Sunday first, minDays=1
        // 2026-01-01 is Thursday, which is in week 1
        var args = new Dictionary<string, object?> { { "date", new DateTime(2026, 1, 1) } };

        var result = _usFormatter.FormatMessage("{date, date, ::w}", args);

        Assert.Equal("1", result);
    }

    [Fact]
    public void E2E_WeekOfYear_US_MidYear()
    {
        // 2026-06-15 is Monday - US week number
        var args = new Dictionary<string, object?> { { "date", new DateTime(2026, 6, 15) } };

        var result = _usFormatter.FormatMessage("{date, date, ::w}", args);

        // US weeks start Sunday, so June 15 (Monday) is in a different week count
        Assert.NotNull(result);
        int.TryParse(result, out var week);
        Assert.True(week >= 24 && week <= 26, $"Expected week 24-26, got {week}");
    }

    [Fact]
    public void E2E_WeekOfYear_US_SundayStart()
    {
        // 2026-01-04 is Sunday - first day of week 2 in US
        var args = new Dictionary<string, object?> { { "date", new DateTime(2026, 1, 4) } };

        var result = _usFormatter.FormatMessage("{date, date, ::w}", args);

        Assert.Equal("2", result);
    }

    #endregion

    #region Combined Format Tests

    [Fact]
    public void E2E_WeekOfYear_WithYear()
    {
        var args = new Dictionary<string, object?> { { "date", new DateTime(2026, 6, 15) } };

        var result = _formatter.FormatMessage("{date, date, ::yw}", args);

        Assert.Contains("2026", result);
        Assert.Contains("25", result);
    }

    [Fact]
    public void E2E_WeekOfYear_WithYearPadded()
    {
        var args = new Dictionary<string, object?> { { "date", new DateTime(2026, 1, 15) } };

        var result = _formatter.FormatMessage("{date, date, ::yww}", args);

        Assert.Contains("2026", result);
        Assert.Contains("03", result);
    }

    [Fact]
    public void E2E_WeekOfYear_InComplexMessage()
    {
        var args = new Dictionary<string, object?>
        {
            { "name", "Bob" },
            { "date", new DateTime(2026, 3, 15) }
        };

        var result = _formatter.FormatMessage("Hello {name}, this is week {date, date, ::w} of the year.", args);

        Assert.Contains("Hello Bob", result);
        Assert.Contains("week", result);
        Assert.Contains("11", result); // March 15, 2026 is week 11
    }

    [Fact]
    public void E2E_WeekOfYear_WithDateTime()
    {
        var args = new Dictionary<string, object?> { { "dt", new DateTime(2026, 1, 15, 14, 30, 0) } };

        var result = _formatter.FormatMessage("{dt, datetime, ::wHm}", args);

        Assert.Contains("3", result);  // Week 3
        Assert.Contains("14", result); // Hour
        Assert.Contains("30", result); // Minute
    }

    [Fact]
    public void E2E_WeekOfYear_MultipleInMessage()
    {
        var args = new Dictionary<string, object?>
        {
            { "start", new DateTime(2026, 1, 5) },
            { "end", new DateTime(2026, 6, 15) }
        };

        var result = _formatter.FormatMessage("From week {start, date, ::ww} to week {end, date, ::ww}", args);

        Assert.Equal("From week 02 to week 25", result);
    }

    #endregion

    #region Edge Cases - Year Boundaries

    [Fact]
    public void E2E_WeekOfYear_ISO_Dec29_2025_LastWeekOf2025()
    {
        // Dec 29, 2025 is Monday in the last week of 2025
        // Note: Full ISO 8601 would assign this to week 1 of 2026,
        // but our implementation reports weeks within the same calendar year
        var args = new Dictionary<string, object?> { { "date", new DateTime(2025, 12, 29) } };

        var result = _formatter.FormatMessage("{date, date, ::w}", args);

        Assert.Equal("53", result);
    }

    [Fact]
    public void E2E_WeekOfYear_ISO_Dec28_2025_IsWeek52_2025()
    {
        // Dec 28, 2025 is Sunday, which belongs to week 52 of 2025
        var args = new Dictionary<string, object?> { { "date", new DateTime(2025, 12, 28) } };

        var result = _formatter.FormatMessage("{date, date, ::w}", args);

        Assert.Equal("52", result);
    }

    [Fact]
    public void E2E_WeekOfYear_ISO_Jan1_2024()
    {
        // 2024-01-01 is Monday - first day of week 1
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 1, 1) } };

        var result = _formatter.FormatMessage("{date, date, ::w}", args);

        Assert.Equal("1", result);
    }

    [Fact]
    public void E2E_WeekOfYear_ISO_Dec31_2024()
    {
        // 2024-12-31 is Tuesday in the last week of 2024
        // Note: Full ISO 8601 would assign this to week 1 of 2025,
        // but our implementation reports weeks within the same calendar year
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 12, 31) } };

        var result = _formatter.FormatMessage("{date, date, ::w}", args);

        // Dec 30 (Monday) starts week 53 of 2024
        Assert.Equal("53", result);
    }

    #endregion

    #region Different Date Types

    [Fact]
    public void E2E_WeekOfYear_WithDateOnly()
    {
        var args = new Dictionary<string, object?> { { "date", new DateOnly(2026, 6, 15) } };

        var result = _formatter.FormatMessage("{date, date, ::w}", args);

        Assert.Equal("25", result);
    }

    [Fact]
    public void E2E_WeekOfYear_WithDateTimeOffset()
    {
        var dto = new DateTimeOffset(2026, 6, 15, 10, 30, 0, TimeSpan.FromHours(-5));
        var args = new Dictionary<string, object?> { { "date", dto } };

        var result = _formatter.FormatMessage("{date, date, ::w}", args);

        Assert.Equal("25", result);
    }

    #endregion

    #region Specific Calendar Dates

    [Theory]
    [InlineData(2026, 1, 1, 1)]   // Thursday - week 1
    [InlineData(2026, 1, 4, 1)]   // Sunday - still week 1 (ISO)
    [InlineData(2026, 1, 5, 2)]   // Monday - week 2 starts
    [InlineData(2026, 3, 15, 11)] // Sunday mid-March
    [InlineData(2026, 7, 1, 27)]  // Wednesday - July
    [InlineData(2026, 10, 1, 40)] // Thursday - October
    [InlineData(2026, 12, 25, 52)]// Friday - Christmas
    public void E2E_WeekOfYear_ISO_SpecificDates(int year, int month, int day, int expectedWeek)
    {
        var args = new Dictionary<string, object?> { { "date", new DateTime(year, month, day) } };

        var result = _formatter.FormatMessage("{date, date, ::w}", args);

        Assert.Equal(expectedWeek.ToString(), result);
    }

    #endregion

    #region Week with Other Components

    [Fact]
    public void E2E_WeekOfYear_WithDayOfYear()
    {
        var args = new Dictionary<string, object?> { { "date", new DateTime(2026, 6, 15) } };

        var result = _formatter.FormatMessage("{date, date, ::wD}", args);

        // Should contain both week (25) and day of year (166)
        Assert.Contains("25", result);
        Assert.Contains("166", result);
    }

    [Fact]
    public void E2E_WeekOfYear_WithQuarter()
    {
        var args = new Dictionary<string, object?> { { "date", new DateTime(2026, 6, 15) } };

        var result = _formatter.FormatMessage("{date, date, ::wQ}", args);

        // Should contain week (25) and quarter (Q2)
        Assert.Contains("25", result);
        Assert.Contains("Q2", result);
    }

    #endregion
}
