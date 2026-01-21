using System;
using System.Collections.Generic;
using System.Globalization;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Tests for ICU skeleton support (number and datetime).
/// </summary>
public class SkeletonTests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region Number Skeleton Tests

    [Fact]
    public void NumberSkeleton_Percent()
    {
        var args = new Dictionary<string, object?> { { "n", 0.5 } };

        var result = _formatter.FormatMessage("{n, number, ::percent}", args);

        Assert.Equal("50%", result);
    }

    [Fact]
    public void NumberSkeleton_PercentWithConciseSyntax()
    {
        var args = new Dictionary<string, object?> { { "n", 0.75 } };

        var result = _formatter.FormatMessage("{n, number, ::%}", args);

        Assert.Equal("75%", result);
    }

    [Fact]
    public void NumberSkeleton_Currency()
    {
        var args = new Dictionary<string, object?> { { "n", 99.99 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD}", args);

        Assert.Contains("$", result);
        Assert.Contains("99.99", result);
    }

    [Fact]
    public void NumberSkeleton_CurrencyEUR()
    {
        var args = new Dictionary<string, object?> { { "n", 50.00 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/EUR}", args);

        Assert.Contains("\u20AC", result); // Euro sign
    }

    [Fact]
    public void NumberSkeleton_CompactShort()
    {
        var args = new Dictionary<string, object?> { { "n", 1500 } };

        var result = _formatter.FormatMessage("{n, number, ::compact-short}", args);

        Assert.Contains("K", result);
    }

    [Fact]
    public void NumberSkeleton_CompactShortConcise()
    {
        var args = new Dictionary<string, object?> { { "n", 2500000 } };

        var result = _formatter.FormatMessage("{n, number, ::K}", args);

        Assert.Contains("M", result);
    }

    [Fact]
    public void NumberSkeleton_CompactLong()
    {
        var args = new Dictionary<string, object?> { { "n", 1000000 } };

        var result = _formatter.FormatMessage("{n, number, ::compact-long}", args);

        Assert.Contains("million", result);
    }

    [Fact]
    public void NumberSkeleton_FractionDigits()
    {
        var args = new Dictionary<string, object?> { { "n", 3.14159 } };

        var result = _formatter.FormatMessage("{n, number, ::.00}", args);

        Assert.Equal("3.14", result);
    }

    [Fact]
    public void NumberSkeleton_FractionDigitsOptional()
    {
        var args = new Dictionary<string, object?> { { "n", 3.1 } };

        var result = _formatter.FormatMessage("{n, number, ::.0#}", args);

        Assert.Equal("3.1", result);

        args["n"] = 3.14;
        result = _formatter.FormatMessage("{n, number, ::.0#}", args);
        Assert.Equal("3.14", result);
    }

    [Fact]
    public void NumberSkeleton_Scale()
    {
        var args = new Dictionary<string, object?> { { "n", 0.5 } };

        var result = _formatter.FormatMessage("{n, number, ::scale/100}", args);

        Assert.Equal("50", result);
    }

    [Fact]
    public void NumberSkeleton_SignAlways()
    {
        var args = new Dictionary<string, object?> { { "n", 42 } };

        var result = _formatter.FormatMessage("{n, number, ::sign-always}", args);

        Assert.StartsWith("+", result);
    }

    [Fact]
    public void NumberSkeleton_SignAlwaysConcise()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::+!}", args);

        Assert.StartsWith("+", result);
    }

    [Fact]
    public void NumberSkeleton_GroupOff()
    {
        var args = new Dictionary<string, object?> { { "n", 1000000 } };

        var result = _formatter.FormatMessage("{n, number, ::group-off}", args);

        Assert.DoesNotContain(",", result);
    }

    [Fact]
    public void NumberSkeleton_Scientific()
    {
        var args = new Dictionary<string, object?> { { "n", 12345 } };

        var result = _formatter.FormatMessage("{n, number, ::scientific}", args);

        Assert.Contains("E", result);
    }

    [Fact]
    public void NumberSkeleton_CombinedOptions()
    {
        var args = new Dictionary<string, object?> { { "n", 0.1234 } };

        // Percent with 2 decimal places
        var result = _formatter.FormatMessage("{n, number, ::percent .00}", args);

        Assert.Equal("12.34%", result);
    }

    [Fact]
    public void NumberSkeleton_IntegerDigits()
    {
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = _formatter.FormatMessage("{n, number, ::000}", args);

        Assert.Equal("005", result);
    }

    #endregion

    #region DateTime Skeleton Tests

    [Fact]
    public void DateSkeleton_yMMMd()
    {
        var date = new DateTime(2026, 1, 15);
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, ::yMMMd}", args);

        Assert.Contains("Jan", result);
        Assert.Contains("15", result);
        Assert.Contains("2026", result);
    }

    [Fact]
    public void DateSkeleton_yMMMMd()
    {
        var date = new DateTime(2026, 1, 15);
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, ::yMMMMd}", args);

        Assert.Contains("January", result);
        Assert.Contains("15", result);
        Assert.Contains("2026", result);
    }

    [Fact]
    public void DateSkeleton_EEEE()
    {
        var date = new DateTime(2026, 1, 15); // Thursday
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, ::EEEE}", args);

        Assert.Equal("Thursday", result);
    }

    [Fact]
    public void DateSkeleton_EEE()
    {
        var date = new DateTime(2026, 1, 15); // Thursday
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, ::EEE}", args);

        Assert.Equal("Thu", result);
    }

    [Fact]
    public void TimeSkeleton_Hms()
    {
        var time = new DateTime(2026, 1, 15, 14, 30, 45);
        var args = new Dictionary<string, object?> { { "t", time } };

        var result = _formatter.FormatMessage("{t, time, ::Hms}", args);

        Assert.Equal("14:30:45", result);
    }

    [Fact]
    public void TimeSkeleton_hmmss()
    {
        var time = new DateTime(2026, 1, 15, 14, 30, 45);
        var args = new Dictionary<string, object?> { { "t", time } };

        var result = _formatter.FormatMessage("{t, time, ::hmmss}", args);

        Assert.Contains("2", result); // 2 PM
        Assert.Contains("30", result);
        Assert.Contains("45", result);
    }

    [Fact]
    public void TimeSkeleton_jmm()
    {
        var time = new DateTime(2026, 1, 15, 14, 30, 0);
        var args = new Dictionary<string, object?> { { "t", time } };

        // j is locale-aware (12h for en-US)
        var result = _formatter.FormatMessage("{t, time, ::jmm}", args);

        Assert.Contains("30", result);
    }

    [Fact]
    public void DateTimeSkeleton_yMdHms()
    {
        var dt = new DateTime(2026, 1, 15, 14, 30, 45);
        var args = new Dictionary<string, object?> { { "dt", dt } };

        var result = _formatter.FormatMessage("{dt, datetime, ::yMdHms}", args);

        Assert.Contains("2026", result);
        Assert.Contains("1", result);
        Assert.Contains("15", result);
        Assert.Contains("14", result);
        Assert.Contains("30", result);
        Assert.Contains("45", result);
    }

    [Fact]
    public void DateTimeSkeleton_FullDateWithTime()
    {
        var dt = new DateTime(2026, 1, 15, 14, 30, 0);
        var args = new Dictionary<string, object?> { { "dt", dt } };

        var result = _formatter.FormatMessage("{dt, datetime, ::EEEEyMMMMdHmm}", args);

        Assert.Contains("Thursday", result);
        Assert.Contains("January", result);
        Assert.Contains("14", result);
        Assert.Contains("30", result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Skeleton_InComplexMessage()
    {
        var dt = new DateTime(2026, 1, 15);
        var args = new Dictionary<string, object?>
        {
            { "name", "Alice" },
            { "amount", 1234.56 },
            { "date", dt }
        };

        var result = _formatter.FormatMessage(
            "Hello {name}, your balance is {amount, number, ::currency/USD} as of {date, date, ::yMMMd}.",
            args);

        Assert.Contains("Alice", result);
        Assert.Contains("$", result);
        Assert.Contains("Jan", result);
        Assert.Contains("15", result);
        Assert.Contains("2026", result);
    }

    [Fact]
    public void Skeleton_WithLocale()
    {
        var formatter = new MessageFormatter("de-DE", TestOptions.WithGerman());
        var args = new Dictionary<string, object?> { { "n", 1234.56 } };

        var result = formatter.FormatMessage("{n, number, ::currency/EUR}", args);

        Assert.Contains("\u20AC", result); // Euro sign
    }

    #endregion
}