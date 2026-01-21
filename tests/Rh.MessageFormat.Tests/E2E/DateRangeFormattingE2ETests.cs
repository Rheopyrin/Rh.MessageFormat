using System;
using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.E2E;

/// <summary>
/// End-to-end tests for date range (interval) formatting.
/// Tests the complete flow: Parse message -> Format date range -> Verify output.
/// </summary>
public class DateRangeFormattingE2ETests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region Basic Date Range Tests

    [Fact]
    public void E2E_DateRange_SameDay()
    {
        // When start and end are the same, should output single date
        var args = new Dictionary<string, object?>
        {
            { "start", new DateTime(2026, 6, 15) },
            { "end", new DateTime(2026, 6, 15) }
        };

        var result = _formatter.FormatMessage("{start, daterange, end}", args);

        // Should just be a single date (medium format by default)
        Assert.Contains("6", result);
        Assert.Contains("15", result);
        Assert.Contains("2026", result);
    }

    [Fact]
    public void E2E_DateRange_DifferentDays_SameMonth()
    {
        var args = new Dictionary<string, object?>
        {
            { "start", new DateTime(2026, 6, 15) },
            { "end", new DateTime(2026, 6, 20) }
        };

        var result = _formatter.FormatMessage("{start, daterange, end}", args);

        // Should contain both dates with a separator
        Assert.Contains("–", result); // En dash separator
        Assert.Contains("15", result);
        Assert.Contains("20", result);
    }

    [Fact]
    public void E2E_DateRange_DifferentMonths()
    {
        var args = new Dictionary<string, object?>
        {
            { "start", new DateTime(2026, 1, 15) },
            { "end", new DateTime(2026, 3, 20) }
        };

        var result = _formatter.FormatMessage("{start, daterange, end}", args);

        // Should contain range separator
        Assert.Contains("–", result);
    }

    [Fact]
    public void E2E_DateRange_DifferentYears()
    {
        var args = new Dictionary<string, object?>
        {
            { "start", new DateTime(2025, 12, 25) },
            { "end", new DateTime(2026, 1, 5) }
        };

        var result = _formatter.FormatMessage("{start, daterange, end}", args);

        // Should contain both years
        Assert.Contains("2025", result);
        Assert.Contains("2026", result);
        Assert.Contains("–", result);
    }

    #endregion

    #region Style Tests

    [Fact]
    public void E2E_DateRange_ShortStyle()
    {
        var args = new Dictionary<string, object?>
        {
            { "start", new DateTime(2026, 6, 15) },
            { "end", new DateTime(2026, 6, 20) }
        };

        var result = _formatter.FormatMessage("{start, daterange, end, short}", args);

        // Short format should be more compact
        Assert.Contains("–", result);
    }

    [Fact]
    public void E2E_DateRange_LongStyle()
    {
        var args = new Dictionary<string, object?>
        {
            { "start", new DateTime(2026, 6, 15) },
            { "end", new DateTime(2026, 6, 20) }
        };

        var result = _formatter.FormatMessage("{start, daterange, end, long}", args);

        // Long format should include month name
        Assert.Contains("–", result);
    }

    #endregion

    #region Skeleton Tests

    [Fact]
    public void E2E_DateRange_WithSkeleton_YearMonthDay()
    {
        var args = new Dictionary<string, object?>
        {
            { "start", new DateTime(2026, 6, 15) },
            { "end", new DateTime(2026, 6, 20) }
        };

        var result = _formatter.FormatMessage("{start, daterange, end, ::yMd}", args);

        // Should format with specified skeleton
        Assert.Contains("2026", result);
        Assert.Contains("6", result);
    }

    [Fact]
    public void E2E_DateRange_WithSkeleton_MonthDayOnly()
    {
        var args = new Dictionary<string, object?>
        {
            { "start", new DateTime(2026, 6, 15) },
            { "end", new DateTime(2026, 6, 20) }
        };

        var result = _formatter.FormatMessage("{start, daterange, end, ::Md}", args);

        // Should have month and day
        Assert.Contains("6", result);
        Assert.Contains("15", result);
        Assert.Contains("20", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void E2E_DateRange_SwappedDates()
    {
        // When end is before start, should swap them
        var args = new Dictionary<string, object?>
        {
            { "start", new DateTime(2026, 6, 20) },
            { "end", new DateTime(2026, 6, 15) }
        };

        var result = _formatter.FormatMessage("{start, daterange, end}", args);

        // Should still format correctly (implementation swaps dates)
        Assert.Contains("–", result);
    }

    [Fact]
    public void E2E_DateRange_WithDateTimeOffset()
    {
        var args = new Dictionary<string, object?>
        {
            { "start", new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.FromHours(-5)) },
            { "end", new DateTimeOffset(2026, 6, 20, 14, 0, 0, TimeSpan.FromHours(-5)) }
        };

        var result = _formatter.FormatMessage("{start, daterange, end}", args);

        Assert.Contains("–", result);
    }

    [Fact]
    public void E2E_DateRange_WithDateOnly()
    {
        var args = new Dictionary<string, object?>
        {
            { "start", new DateOnly(2026, 6, 15) },
            { "end", new DateOnly(2026, 6, 20) }
        };

        var result = _formatter.FormatMessage("{start, daterange, end}", args);

        Assert.Contains("–", result);
    }

    [Fact]
    public void E2E_DateRange_InComplexMessage()
    {
        var args = new Dictionary<string, object?>
        {
            { "event", "Conference" },
            { "start", new DateTime(2026, 6, 15) },
            { "end", new DateTime(2026, 6, 18) }
        };

        var result = _formatter.FormatMessage("The {event} runs from {start, daterange, end}.", args);

        Assert.StartsWith("The Conference runs from", result);
        Assert.Contains("–", result);
        Assert.EndsWith(".", result);
    }

    #endregion

    #region Different Date Types in Same Range

    [Fact]
    public void E2E_DateRange_MixedDateTimeTypes()
    {
        // Using DateTime for start and DateOnly for end
        var args = new Dictionary<string, object?>
        {
            { "start", new DateTime(2026, 6, 15, 9, 0, 0) },
            { "end", new DateOnly(2026, 6, 20) }
        };

        var result = _formatter.FormatMessage("{start, daterange, end}", args);

        Assert.Contains("–", result);
    }

    #endregion

    #region Time Range Tests (Same Day)

    [Fact]
    public void E2E_DateRange_DifferentHours_SameDay()
    {
        var args = new Dictionary<string, object?>
        {
            { "start", new DateTime(2026, 6, 15, 9, 0, 0) },
            { "end", new DateTime(2026, 6, 15, 17, 0, 0) }
        };

        var result = _formatter.FormatMessage("{start, daterange, end}", args);

        // Should show the date range with times
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    [Fact]
    public void E2E_DateRange_DifferentMinutes_SameHour()
    {
        var args = new Dictionary<string, object?>
        {
            { "start", new DateTime(2026, 6, 15, 9, 0, 0) },
            { "end", new DateTime(2026, 6, 15, 9, 30, 0) }
        };

        var result = _formatter.FormatMessage("{start, daterange, end}", args);

        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    #endregion

    #region Multiple Date Ranges in Message

    [Fact]
    public void E2E_DateRange_MultiplePeriods()
    {
        var args = new Dictionary<string, object?>
        {
            { "phase1Start", new DateTime(2026, 1, 1) },
            { "phase1End", new DateTime(2026, 3, 31) },
            { "phase2Start", new DateTime(2026, 4, 1) },
            { "phase2End", new DateTime(2026, 6, 30) }
        };

        var result = _formatter.FormatMessage(
            "Phase 1: {phase1Start, daterange, phase1End} | Phase 2: {phase2Start, daterange, phase2End}",
            args);

        Assert.Contains("Phase 1:", result);
        Assert.Contains("Phase 2:", result);
        Assert.Contains("|", result);
    }

    #endregion
}
