using System;
using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.E2E;

/// <summary>
/// End-to-end tests for day-of-year skeleton formatting.
/// Tests the complete flow: Parse message -> Format with skeleton -> Post-process -> Verify output.
/// </summary>
public class DayOfYearE2ETests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region Basic Day of Year Tests

    [Fact]
    public void E2E_DayOfYear_FirstDayOfYear()
    {
        // Arrange
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 1, 1) } };

        // Act
        var result = _formatter.FormatMessage("{date, date, ::D}", args);

        // Assert
        Assert.Equal("1", result);
    }

    [Fact]
    public void E2E_DayOfYear_LastDayOfRegularYear()
    {
        // Arrange - 2026 is not a leap year
        var args = new Dictionary<string, object?> { { "date", new DateTime(2026, 12, 31) } };

        // Act
        var result = _formatter.FormatMessage("{date, date, ::D}", args);

        // Assert
        Assert.Equal("365", result);
    }

    [Fact]
    public void E2E_DayOfYear_LastDayOfLeapYear()
    {
        // Arrange - 2024 is a leap year
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 12, 31) } };

        // Act
        var result = _formatter.FormatMessage("{date, date, ::D}", args);

        // Assert
        Assert.Equal("366", result);
    }

    [Fact]
    public void E2E_DayOfYear_LeapDayFeb29()
    {
        // Arrange - Feb 29 is day 60 in leap year
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 2, 29) } };

        // Act
        var result = _formatter.FormatMessage("{date, date, ::D}", args);

        // Assert
        Assert.Equal("60", result);
    }

    [Fact]
    public void E2E_DayOfYear_MarchFirstInLeapYear()
    {
        // Arrange - March 1 is day 61 in leap year
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 3, 1) } };

        // Act
        var result = _formatter.FormatMessage("{date, date, ::D}", args);

        // Assert
        Assert.Equal("61", result);
    }

    [Fact]
    public void E2E_DayOfYear_MarchFirstInRegularYear()
    {
        // Arrange - March 1 is day 60 in non-leap year
        var args = new Dictionary<string, object?> { { "date", new DateTime(2026, 3, 1) } };

        // Act
        var result = _formatter.FormatMessage("{date, date, ::D}", args);

        // Assert
        Assert.Equal("60", result);
    }

    #endregion

    #region Padding Tests

    [Fact]
    public void E2E_DayOfYear_DoublePadding_SingleDigit()
    {
        // Arrange - Day 5 with DD padding should be "05"
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 1, 5) } };

        // Act
        var result = _formatter.FormatMessage("{date, date, ::DD}", args);

        // Assert
        Assert.Equal("05", result);
    }

    [Fact]
    public void E2E_DayOfYear_DoublePadding_DoubleDigit()
    {
        // Arrange - Day 50 with DD padding should be "50"
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 2, 19) } }; // Day 50

        // Act
        var result = _formatter.FormatMessage("{date, date, ::DD}", args);

        // Assert
        Assert.Equal("50", result);
    }

    [Fact]
    public void E2E_DayOfYear_DoublePadding_TripleDigit()
    {
        // Arrange - Day 100 with DD padding should be "100" (no extra padding needed)
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 4, 9) } }; // Day 100 in leap year

        // Act
        var result = _formatter.FormatMessage("{date, date, ::DD}", args);

        // Assert
        Assert.Equal("100", result);
    }

    [Fact]
    public void E2E_DayOfYear_TriplePadding_SingleDigit()
    {
        // Arrange - Day 5 with DDD padding should be "005"
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 1, 5) } };

        // Act
        var result = _formatter.FormatMessage("{date, date, ::DDD}", args);

        // Assert
        Assert.Equal("005", result);
    }

    [Fact]
    public void E2E_DayOfYear_TriplePadding_DoubleDigit()
    {
        // Arrange - Day 50 with DDD padding should be "050"
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 2, 19) } }; // Day 50

        // Act
        var result = _formatter.FormatMessage("{date, date, ::DDD}", args);

        // Assert
        Assert.Equal("050", result);
    }

    [Fact]
    public void E2E_DayOfYear_TriplePadding_TripleDigit()
    {
        // Arrange - Day 365 with DDD padding should be "365"
        var args = new Dictionary<string, object?> { { "date", new DateTime(2026, 12, 31) } };

        // Act
        var result = _formatter.FormatMessage("{date, date, ::DDD}", args);

        // Assert
        Assert.Equal("365", result);
    }

    #endregion

    #region Combined Format Tests

    [Fact]
    public void E2E_DayOfYear_WithYear()
    {
        // Arrange
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 6, 15) } }; // Day 167 in leap year

        // Act
        var result = _formatter.FormatMessage("{date, date, ::yD}", args);

        // Assert
        Assert.Contains("2024", result);
        Assert.Contains("167", result);
    }

    [Fact]
    public void E2E_DayOfYear_InComplexMessage()
    {
        // Arrange
        var args = new Dictionary<string, object?>
        {
            { "name", "Alice" },
            { "date", new DateTime(2024, 7, 4) } // Day 186 (Independence Day in leap year)
        };

        // Act
        var result = _formatter.FormatMessage("Hello {name}, today is day {date, date, ::D} of the year.", args);

        // Assert
        Assert.Equal("Hello Alice, today is day 186 of the year.", result);
    }

    [Fact]
    public void E2E_DayOfYear_WithDateTime()
    {
        // Arrange
        var args = new Dictionary<string, object?> { { "dt", new DateTime(2024, 1, 15, 14, 30, 0) } }; // Day 15

        // Act
        var result = _formatter.FormatMessage("{dt, datetime, ::DHm}", args);

        // Assert
        Assert.Contains("15", result);
        Assert.Contains("14", result);
        Assert.Contains("30", result);
    }

    [Fact]
    public void E2E_DayOfYear_MultipleInMessage()
    {
        // Arrange
        var args = new Dictionary<string, object?>
        {
            { "start", new DateTime(2024, 1, 1) },
            { "end", new DateTime(2024, 12, 31) }
        };

        // Act
        var result = _formatter.FormatMessage("From day {start, date, ::DDD} to day {end, date, ::DDD}", args);

        // Assert
        Assert.Equal("From day 001 to day 366", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void E2E_DayOfYear_WithDateOnly()
    {
        // Arrange - Using DateOnly type
        var args = new Dictionary<string, object?> { { "date", new DateOnly(2024, 1, 15) } };

        // Act
        var result = _formatter.FormatMessage("{date, date, ::D}", args);

        // Assert
        Assert.Equal("15", result);
    }

    [Fact]
    public void E2E_DayOfYear_WithDateTimeOffset()
    {
        // Arrange - Using DateTimeOffset type
        var dto = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.FromHours(-5));
        var args = new Dictionary<string, object?> { { "date", dto } };

        // Act
        var result = _formatter.FormatMessage("{date, date, ::D}", args);

        // Assert
        Assert.Equal("15", result);
    }

    [Fact]
    public void E2E_DayOfYear_MidYear()
    {
        // Arrange - July 1 in regular year is day 182
        var args = new Dictionary<string, object?> { { "date", new DateTime(2026, 7, 1) } };

        // Act
        var result = _formatter.FormatMessage("{date, date, ::D}", args);

        // Assert
        Assert.Equal("182", result);
    }

    #endregion
}
