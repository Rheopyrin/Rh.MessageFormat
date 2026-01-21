using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Formatting;

/// <summary>
/// Tests for metadata fallback behavior in CurrencyMetadata, DatePatternMetadata, ListPatternMetadata, and UnitMetadata.
/// These tests focus on exercising fallback paths when locale data is not available.
/// </summary>
public class MetadataFallbackTests
{
    #region Date Pattern Fallback Tests

    [Fact]
    public void DatePattern_UnknownLocale_UsesFallback()
    {
        var formatter = new MessageFormatter("xx-XX", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "d", new System.DateTime(2024, 3, 15) } };

        // Use an unknown locale that will trigger fallback
        var result = formatter.FormatMessage("{d, date, short}", args);

        // Should still format the date using fallback patterns
        Assert.Contains("2024", result);
    }

    [Fact]
    public void DatePattern_Short_FallbackPattern()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "d", new System.DateTime(2024, 3, 15) } };

        var result = formatter.FormatMessage("{d, date, short}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DatePattern_Medium_FallbackPattern()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "d", new System.DateTime(2024, 3, 15) } };

        var result = formatter.FormatMessage("{d, date, medium}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DatePattern_Long_FallbackPattern()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "d", new System.DateTime(2024, 3, 15) } };

        var result = formatter.FormatMessage("{d, date, long}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DatePattern_Full_FallbackPattern()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "d", new System.DateTime(2024, 3, 15) } };

        var result = formatter.FormatMessage("{d, date, full}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DatePattern_UnknownStyle_UsesFallback()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "d", new System.DateTime(2024, 3, 15) } };

        // Unknown style should fall back to default (short)
        var result = formatter.FormatMessage("{d, date, unknownstyle}", args);

        Assert.NotEmpty(result);
    }

    #endregion

    #region Time Pattern Fallback Tests

    [Fact]
    public void TimePattern_Short_FallbackPattern()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "t", new System.DateTime(2024, 1, 1, 14, 30, 45) } };

        var result = formatter.FormatMessage("{t, time, short}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void TimePattern_Medium_FallbackPattern()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "t", new System.DateTime(2024, 1, 1, 14, 30, 45) } };

        var result = formatter.FormatMessage("{t, time, medium}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void TimePattern_Long_FallbackPattern()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "t", new System.DateTime(2024, 1, 1, 14, 30, 45) } };

        var result = formatter.FormatMessage("{t, time, long}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void TimePattern_Full_FallbackPattern()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "t", new System.DateTime(2024, 1, 1, 14, 30, 45) } };

        var result = formatter.FormatMessage("{t, time, full}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void TimePattern_UnknownStyle_UsesFallback()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "t", new System.DateTime(2024, 1, 1, 14, 30, 45) } };

        // Unknown style should fall back to default (medium)
        var result = formatter.FormatMessage("{t, time, unknownstyle}", args);

        Assert.NotEmpty(result);
    }

    #endregion

    #region DateTime Pattern Fallback Tests

    [Fact]
    public void DateTimePattern_Short_FallbackPattern()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "dt", new System.DateTime(2024, 3, 15, 14, 30, 0) } };

        var result = formatter.FormatMessage("{dt, datetime, short}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DateTimePattern_Medium_FallbackPattern()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "dt", new System.DateTime(2024, 3, 15, 14, 30, 0) } };

        var result = formatter.FormatMessage("{dt, datetime, medium}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DateTimePattern_Long_FallbackPattern()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "dt", new System.DateTime(2024, 3, 15, 14, 30, 0) } };

        var result = formatter.FormatMessage("{dt, datetime, long}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DateTimePattern_Full_FallbackPattern()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "dt", new System.DateTime(2024, 3, 15, 14, 30, 0) } };

        var result = formatter.FormatMessage("{dt, datetime, full}", args);

        Assert.NotEmpty(result);
    }

    #endregion

    #region List Pattern Style Tests

    [Fact]
    public void ListPattern_Disjunction_Short()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C" } } };

        var result = formatter.FormatMessage("{items, list, or, short}", args);

        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
    }

    [Fact]
    public void ListPattern_Disjunction_Narrow()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C" } } };

        var result = formatter.FormatMessage("{items, list, or, narrow}", args);

        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
    }

    [Fact]
    public void ListPattern_Unit_Long()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C" } } };

        var result = formatter.FormatMessage("{items, list, unit, long}", args);

        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
    }

    [Fact]
    public void ListPattern_Unit_Short()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C" } } };

        var result = formatter.FormatMessage("{items, list, unit, short}", args);

        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
    }

    [Fact]
    public void ListPattern_Unit_Narrow()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C" } } };

        var result = formatter.FormatMessage("{items, list, unit, narrow}", args);

        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
    }

    [Fact]
    public void ListPattern_Conjunction_Short()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C" } } };

        var result = formatter.FormatMessage("{items, list, conjunction, short}", args);

        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
    }

    [Fact]
    public void ListPattern_Conjunction_Narrow()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C" } } };

        var result = formatter.FormatMessage("{items, list, conjunction, narrow}", args);

        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
    }

    [Fact]
    public void ListPattern_TwoItems()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B" } } };

        var result = formatter.FormatMessage("{items, list}", args);

        Assert.Equal("A and B", result);
    }

    [Fact]
    public void ListPattern_TwoItems_Disjunction()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B" } } };

        // Use disjunction type for "or" list style
        var result = formatter.FormatMessage("{items, list, disjunction}", args);

        Assert.Equal("A or B", result);
    }

    #endregion

    #region Currency Fallback Tests

    [Fact]
    public void Currency_UnknownCode_FallsBackToCode()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = formatter.FormatMessage("{n, number, ::currency/XYZ}", args);

        Assert.Contains("XYZ", result);
    }

    [Fact]
    public void Currency_UnknownLocale_StillWorks()
    {
        var formatter = new MessageFormatter("xx-XX", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = formatter.FormatMessage("{n, number, ::currency/USD}", args);

        Assert.Contains("100", result);
    }

    #endregion

    #region Base Locale Fallback Tests

    [Fact]
    public void Date_BaseLocale_Fallback()
    {
        // Test that en-GB falls back to en if en-GB data is not complete
        var formatter = new MessageFormatter("en-GB", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "d", new System.DateTime(2024, 3, 15) } };

        var result = formatter.FormatMessage("{d, date, short}", args);

        Assert.Contains("2024", result);
    }

    [Fact]
    public void Time_BaseLocale_Fallback()
    {
        var formatter = new MessageFormatter("en-GB", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "t", new System.DateTime(2024, 1, 1, 14, 30, 45) } };

        var result = formatter.FormatMessage("{t, time, short}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void List_BaseLocale_Fallback()
    {
        var formatter = new MessageFormatter("en-GB", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C" } } };

        var result = formatter.FormatMessage("{items, list}", args);

        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
    }

    #endregion
}
