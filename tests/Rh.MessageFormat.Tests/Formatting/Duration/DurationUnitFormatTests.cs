using System.Collections.Generic;
using Rh.MessageFormat.Formatting.Duration;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Formatting.Duration;

/// <summary>
/// Tests for DurationUnitFormat - 1-to-1 port of JS DurationUnitFormat.
/// </summary>
public class DurationUnitFormatTests
{
    #region Default Format Tests (seconds only)

    [Fact]
    public void Format_DefaultFormat_ZeroSeconds()
    {
        var formatter = new DurationUnitFormat("en");

        Assert.Equal("0 seconds", formatter.Format(0));
    }

    [Fact]
    public void Format_DefaultFormat_OneSecond()
    {
        var formatter = new DurationUnitFormat("en");

        Assert.Equal("1 second", formatter.Format(1));
    }

    [Fact]
    public void Format_DefaultFormat_ThirtySeconds()
    {
        var formatter = new DurationUnitFormat("en");

        Assert.Equal("30 seconds", formatter.Format(30));
    }

    [Fact]
    public void Format_DefaultFormat_SixtySeconds()
    {
        var formatter = new DurationUnitFormat("en");

        // Default format is {seconds}, so 60 seconds stays as seconds
        Assert.Equal("60 seconds", formatter.Format(60));
    }

    [Fact]
    public void Format_DefaultFormat_SixtyOneSeconds()
    {
        var formatter = new DurationUnitFormat("en");

        Assert.Equal("61 seconds", formatter.Format(61));
    }

    [Fact]
    public void Format_DefaultFormat_OneTwentySeconds()
    {
        var formatter = new DurationUnitFormat("en");

        Assert.Equal("120 seconds", formatter.Format(120));
    }

    #endregion

    #region Timer Style Tests

    [Fact]
    public void Format_TimerStyle_ZeroSeconds()
    {
        var formatter = new DurationUnitFormat("en", new DurationUnitFormatOptions
        {
            Style = DurationUnitFormat.Styles.TIMER
        });

        Assert.Equal("0:00", formatter.Format(0));
    }

    [Fact]
    public void Format_TimerStyle_ThirtySeconds()
    {
        var formatter = new DurationUnitFormat("en", new DurationUnitFormatOptions
        {
            Style = DurationUnitFormat.Styles.TIMER
        });

        Assert.Equal("0:30", formatter.Format(30));
    }

    [Fact]
    public void Format_TimerStyle_SixtySeconds()
    {
        var formatter = new DurationUnitFormat("en", new DurationUnitFormatOptions
        {
            Style = DurationUnitFormat.Styles.TIMER
        });

        Assert.Equal("1:00", formatter.Format(60));
    }

    [Fact]
    public void Format_TimerStyle_NinetySeconds()
    {
        var formatter = new DurationUnitFormat("en", new DurationUnitFormatOptions
        {
            Style = DurationUnitFormat.Styles.TIMER
        });

        Assert.Equal("1:30", formatter.Format(90));
    }

    [Fact]
    public void Format_TimerStyle_HoursMinutesSeconds()
    {
        var formatter = new DurationUnitFormat("en", new DurationUnitFormatOptions
        {
            Style = DurationUnitFormat.Styles.TIMER,
            Format = "{hours}:{minutes}:{seconds}"
        });

        // 1 hour, 1 minute, 1 second = 3661 seconds
        Assert.Equal("1:01:01", formatter.Format(3661));
    }

    [Fact]
    public void Format_TimerStyle_ZeroPadding()
    {
        var formatter = new DurationUnitFormat("en", new DurationUnitFormatOptions
        {
            Style = DurationUnitFormat.Styles.TIMER,
            Format = "{hours}:{minutes}:{seconds}"
        });

        // 61 seconds = 0:01:01
        Assert.Equal("0:01:01", formatter.Format(61));
    }

    #endregion

    #region Custom Format Tests

    [Fact]
    public void Format_CustomFormat_HoursMinutes()
    {
        var formatter = new DurationUnitFormat("en", new DurationUnitFormatOptions
        {
            Style = DurationUnitFormat.Styles.CUSTOM,
            Format = "{hours} {minutes}"
        });

        // 1 hour, 30 minutes = 5400 seconds
        Assert.Equal("1 hour 30 minutes", formatter.Format(5400));
    }

    [Fact]
    public void Format_CustomFormat_DaysHours()
    {
        var formatter = new DurationUnitFormat("en", new DurationUnitFormatOptions
        {
            Style = DurationUnitFormat.Styles.CUSTOM,
            Format = "{days} {hours}"
        });

        // 1 day, 2 hours = 86400 + 7200 = 93600 seconds
        Assert.Equal("1 day 2 hours", formatter.Format(93600));
    }

    [Fact]
    public void Format_CustomFormat_ZeroValueSkipped()
    {
        var formatter = new DurationUnitFormat("en", new DurationUnitFormatOptions
        {
            Style = DurationUnitFormat.Styles.CUSTOM,
            Format = "{hours} {minutes}"
        });

        // 1 hour = 3600 seconds (0 minutes should be skipped)
        Assert.Equal("1 hour", formatter.Format(3600));
    }

    #endregion

    #region Round Tests

    [Fact]
    public void Format_Round_TwentyNineSeconds_NoRound()
    {
        var formatter = new DurationUnitFormat("en", new DurationUnitFormatOptions
        {
            Format = "{hours} {minutes}",
            Round = true
        });

        // 29 seconds < 30 seconds (50% of minute), rounds to 0 minutes
        var parts = formatter.FormatToParts(29);
        Assert.Contains(parts, p => p.Type == DurationUnitFormat.Units.MINUTE && p.Value == "0");
    }

    [Fact]
    public void Format_Round_ThirtySeconds_RoundsUp()
    {
        var formatter = new DurationUnitFormat("en", new DurationUnitFormatOptions
        {
            Format = "{hours} {minutes}",
            Round = true
        });

        // 30 seconds >= 30 seconds (50% of minute), rounds to 1 minute
        var parts = formatter.FormatToParts(30);
        Assert.Contains(parts, p => p.Type == DurationUnitFormat.Units.MINUTE && p.Value == "1");
    }

    [Fact]
    public void Format_Round_59Minutes32Seconds_RoundsTo1Hour()
    {
        var formatter = new DurationUnitFormat("en", new DurationUnitFormatOptions
        {
            Format = "{hours} {minutes}",
            Round = true
        });

        // 59 minutes + 32 seconds = 3572 seconds, rounds up to 1 hour
        var parts = formatter.FormatToParts(59 * 60 + 32);
        Assert.Contains(parts, p => p.Type == DurationUnitFormat.Units.HOUR && p.Value == "1");
    }

    #endregion

    #region FormatToParts Tests

    [Fact]
    public void FormatToParts_ReturnsCorrectStructure()
    {
        var formatter = new DurationUnitFormat("en", new DurationUnitFormatOptions
        {
            Format = "{seconds}"
        });

        var parts = formatter.FormatToParts(5);

        Assert.Equal(3, parts.Length);
        Assert.Equal(DurationUnitFormat.Units.SECOND, parts[0].Type);
        Assert.Equal("5", parts[0].Value);
        Assert.Equal(DurationFormatPartType.Literal, parts[1].Type);
        Assert.Equal(" ", parts[1].Value);
        Assert.Equal(DurationFormatPartType.Unit, parts[2].Type);
        Assert.Equal("seconds", parts[2].Value); // 5 seconds is plural
    }

    [Fact]
    public void FormatToParts_Timer_ReturnsOnlyValues()
    {
        var formatter = new DurationUnitFormat("en", new DurationUnitFormatOptions
        {
            Style = DurationUnitFormat.Styles.TIMER,
            Format = "{minutes}:{seconds}"
        });

        var parts = formatter.FormatToParts(65);

        Assert.Equal(3, parts.Length);
        Assert.Equal(DurationUnitFormat.Units.MINUTE, parts[0].Type);
        Assert.Equal("1", parts[0].Value);
        Assert.Equal(DurationFormatPartType.Literal, parts[1].Type);
        Assert.Equal(":", parts[1].Value);
        Assert.Equal(DurationUnitFormat.Units.SECOND, parts[2].Type);
        Assert.Equal("05", parts[2].Value);
    }

    #endregion

    #region Custom FormatUnits Tests

    [Fact]
    public void Format_CustomFormatUnits_DailyWeekly()
    {
        var formatter = new DurationUnitFormat("en", new DurationUnitFormatOptions
        {
            Style = DurationUnitFormat.Styles.CUSTOM,
            Format = "{days}",
            FormatUnits = new Dictionary<string, string>
            {
                [DurationUnitFormat.Units.DAY] = "{value, plural, =1 {Daily} =7 {Weekly} other {# Days}}"
            }
        });

        Assert.Equal("Daily", formatter.Format(86400)); // 1 day
        Assert.Equal("Weekly", formatter.Format(86400 * 7)); // 7 days
        Assert.Equal("3 Days", formatter.Format(86400 * 3)); // 3 days
    }

    [Fact]
    public void Format_CustomFormatUnits_Hyphenated()
    {
        var formatter = new DurationUnitFormat("en", new DurationUnitFormatOptions
        {
            Style = DurationUnitFormat.Styles.CUSTOM,
            Format = "{days}",
            FormatDuration = "{value}-{unit}",
            FormatUnits = new Dictionary<string, string>
            {
                [DurationUnitFormat.Units.DAY] = "day"
            }
        });

        Assert.Equal("2-day", formatter.Format(86400 * 2));
    }

    #endregion

    #region Style Tests

    [Fact]
    public void Format_LongStyle_FullUnitNames()
    {
        var formatter = new DurationUnitFormat("en", new DurationUnitFormatOptions
        {
            Style = DurationUnitFormat.Styles.LONG,
            Format = "{hours}"
        });

        Assert.Equal("1 hour", formatter.Format(3600));
        Assert.Equal("2 hours", formatter.Format(7200));
    }

    [Fact]
    public void Format_ShortStyle_AbbreviatedUnits()
    {
        var formatter = new DurationUnitFormat("en", new DurationUnitFormatOptions
        {
            Style = DurationUnitFormat.Styles.SHORT,
            Format = "{hours}"
        });

        Assert.Equal("1 hr", formatter.Format(3600));
        Assert.Equal("2 hrs", formatter.Format(7200));
    }

    [Fact]
    public void Format_NarrowStyle_MinimalUnits()
    {
        var formatter = new DurationUnitFormat("en", new DurationUnitFormatOptions
        {
            Style = DurationUnitFormat.Styles.NARROW,
            Format = "{hours}"
        });

        Assert.Equal("1 h", formatter.Format(3600));
        Assert.Equal("2 h", formatter.Format(7200));
    }

    #endregion

    #region Static Constants Tests

    [Fact]
    public void StaticConstants_Units_AreCorrect()
    {
        Assert.Equal("year", DurationUnitFormat.Units.YEAR);
        Assert.Equal("month", DurationUnitFormat.Units.MONTH);
        Assert.Equal("day", DurationUnitFormat.Units.DAY);
        Assert.Equal("hour", DurationUnitFormat.Units.HOUR);
        Assert.Equal("minute", DurationUnitFormat.Units.MINUTE);
        Assert.Equal("second", DurationUnitFormat.Units.SECOND);
    }

    [Fact]
    public void StaticConstants_Styles_AreCorrect()
    {
        Assert.Equal("custom", DurationUnitFormat.Styles.CUSTOM);
        Assert.Equal("timer", DurationUnitFormat.Styles.TIMER);
        Assert.Equal("long", DurationUnitFormat.Styles.LONG);
        Assert.Equal("short", DurationUnitFormat.Styles.SHORT);
        Assert.Equal("narrow", DurationUnitFormat.Styles.NARROW);
    }

    [Fact]
    public void StaticConstants_SecondsIn_AreCorrect()
    {
        Assert.Equal(31536000, DurationUnitFormat.SecondsIn.YEAR);
        Assert.Equal(2592000, DurationUnitFormat.SecondsIn.MONTH);
        Assert.Equal(86400, DurationUnitFormat.SecondsIn.DAY);
        Assert.Equal(3600, DurationUnitFormat.SecondsIn.HOUR);
        Assert.Equal(60, DurationUnitFormat.SecondsIn.MINUTE);
        Assert.Equal(1, DurationUnitFormat.SecondsIn.SECOND);
    }

    #endregion

    #region Integration with MessageFormatter Tests

    [Fact]
    public void Integration_RegisterAsCustomFormatter()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["duration"] = (value, style, locale, culture) =>
        {
            var seconds = value switch
            {
                int i => i,
                double d => d,
                long l => l,
                _ => 0.0
            };

            var formatOptions = new DurationUnitFormatOptions
            {
                Style = style ?? DurationUnitFormat.Styles.TIMER,
                Format = style == DurationUnitFormat.Styles.TIMER ? "{hours}:{minutes}:{seconds}" : "{hours} {minutes}"
            };

            var formatter = new DurationUnitFormat(locale, formatOptions);
            return formatter.Format(seconds);
        };

        var messageFormatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "seconds", 3661 } };
        var result = messageFormatter.FormatMessage("Duration: {seconds, duration, timer}", args);

        Assert.Equal("Duration: 1:01:01", result);
    }

    #endregion
}
