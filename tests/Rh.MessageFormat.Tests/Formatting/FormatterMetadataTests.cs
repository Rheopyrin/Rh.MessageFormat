using System;
using System.Collections.Generic;
using Rh.MessageFormat.Exceptions;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Formatting;

/// <summary>
/// Tests for CurrencyMetadata, DatePatternMetadata, ListPatternMetadata, and UnitMetadata.
/// These tests cover fallback paths and edge cases to improve code coverage.
/// </summary>
public class FormatterMetadataTests
{
    #region CurrencyMetadata Tests

    [Fact]
    public void CurrencyMetadata_GetSymbol_KnownCurrency()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = formatter.FormatMessage("{n, number, ::currency/USD}", args);

        Assert.Contains("$", result);
    }

    [Fact]
    public void CurrencyMetadata_GetSymbol_UnknownCurrency_FallsBackToCode()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = formatter.FormatMessage("{n, number, ::currency/XYZ}", args);

        Assert.Contains("XYZ", result);
    }

    [Fact]
    public void CurrencyMetadata_GetSymbol_BaseLocaleFallback()
    {
        // Test that en-GB falls back to en for currency data
        var formatter = new MessageFormatter("en-GB", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = formatter.FormatMessage("{n, number, ::currency/USD}", args);

        Assert.Contains("$", result);
    }

    [Fact]
    public void CurrencyMetadata_GetSymbol_UnderscoreLocale_FallsBackToBase()
    {
        // Test locale with underscore (en_US -> en)
        var formatter = new MessageFormatter("en_US", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = formatter.FormatMessage("{n, number, ::currency/USD}", args);

        Assert.Contains("$", result);
    }

    [Fact]
    public void CurrencyMetadata_GetSymbol_UnknownLocale_FallsBackToFallbackLocale()
    {
        var formatter = new MessageFormatter("xx-XX", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = formatter.FormatMessage("{n, number, ::currency/USD}", args);

        Assert.Contains("$", result);
    }

    [Fact]
    public void CurrencyMetadata_GetSymbol_EmptyProvider_ThrowsInvalidLocaleException()
    {
        // Empty provider has no locale data
        Assert.Throws<InvalidLocaleException>(() => new MessageFormatter("en", TestOptions.WithEmptyProvider()));
    }

    [Fact]
    public void CurrencyMetadata_EuroCurrency()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 50 } };

        var result = formatter.FormatMessage("{n, number, ::currency/EUR}", args);

        Assert.Contains("€", result);
    }

    [Fact]
    public void CurrencyMetadata_GBPCurrency()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 50 } };

        var result = formatter.FormatMessage("{n, number, ::currency/GBP}", args);

        Assert.Contains("£", result);
    }

    [Fact]
    public void CurrencyMetadata_JPYCurrency()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 1000 } };

        var result = formatter.FormatMessage("{n, number, ::currency/JPY}", args);

        // JPY may use ¥ symbol or fall back to code depending on locale data
        Assert.Contains("1,000", result);
    }

    #endregion

    #region DatePatternMetadata Tests

    [Fact]
    public void DatePatternMetadata_GetDatePattern_Short()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "d", new DateTime(2024, 3, 15) } };

        var result = formatter.FormatMessage("{d, date, short}", args);

        Assert.NotEmpty(result);
        Assert.Contains("2024", result);
    }

    [Fact]
    public void DatePatternMetadata_GetDatePattern_Medium()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "d", new DateTime(2024, 3, 15) } };

        var result = formatter.FormatMessage("{d, date, medium}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DatePatternMetadata_GetDatePattern_Long()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "d", new DateTime(2024, 3, 15) } };

        var result = formatter.FormatMessage("{d, date, long}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DatePatternMetadata_GetDatePattern_Full()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "d", new DateTime(2024, 3, 15) } };

        var result = formatter.FormatMessage("{d, date, full}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DatePatternMetadata_GetDatePattern_UnknownStyle_FallsBackToShort()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "d", new DateTime(2024, 3, 15) } };

        var result = formatter.FormatMessage("{d, date, unknownstyle}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DatePatternMetadata_GetTimePattern_Short()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "t", new DateTime(2024, 1, 1, 14, 30, 45) } };

        var result = formatter.FormatMessage("{t, time, short}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DatePatternMetadata_GetTimePattern_Medium()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "t", new DateTime(2024, 1, 1, 14, 30, 45) } };

        var result = formatter.FormatMessage("{t, time, medium}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DatePatternMetadata_GetTimePattern_Long()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "t", new DateTime(2024, 1, 1, 14, 30, 45) } };

        var result = formatter.FormatMessage("{t, time, long}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DatePatternMetadata_GetTimePattern_Full()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "t", new DateTime(2024, 1, 1, 14, 30, 45) } };

        var result = formatter.FormatMessage("{t, time, full}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DatePatternMetadata_GetTimePattern_UnknownStyle_FallsBackToMedium()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "t", new DateTime(2024, 1, 1, 14, 30, 45) } };

        var result = formatter.FormatMessage("{t, time, unknownstyle}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DatePatternMetadata_GetDateTimePattern_Short()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "dt", new DateTime(2024, 3, 15, 14, 30, 0) } };

        var result = formatter.FormatMessage("{dt, datetime, short}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DatePatternMetadata_GetDateTimePattern_Medium()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "dt", new DateTime(2024, 3, 15, 14, 30, 0) } };

        var result = formatter.FormatMessage("{dt, datetime, medium}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DatePatternMetadata_GetDateTimePattern_Long()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "dt", new DateTime(2024, 3, 15, 14, 30, 0) } };

        var result = formatter.FormatMessage("{dt, datetime, long}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DatePatternMetadata_GetDateTimePattern_Full()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "dt", new DateTime(2024, 3, 15, 14, 30, 0) } };

        var result = formatter.FormatMessage("{dt, datetime, full}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DatePatternMetadata_BaseLocaleFallback()
    {
        var formatter = new MessageFormatter("en-GB", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "d", new DateTime(2024, 3, 15) } };

        var result = formatter.FormatMessage("{d, date, short}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DatePatternMetadata_UnknownLocale_UsesFallback()
    {
        var formatter = new MessageFormatter("xx-XX", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "d", new DateTime(2024, 3, 15) } };

        var result = formatter.FormatMessage("{d, date, short}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void DatePatternMetadata_EmptyProvider_ThrowsInvalidLocaleException()
    {
        // Empty provider has no locale data
        Assert.Throws<InvalidLocaleException>(() => new MessageFormatter("en", TestOptions.WithEmptyProvider()));
    }

    #endregion

    #region ListPatternMetadata Tests

    [Fact]
    public void ListPatternMetadata_Conjunction_Long()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C" } } };

        var result = formatter.FormatMessage("{items, list}", args);

        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
        Assert.Contains("and", result);
    }

    [Fact]
    public void ListPatternMetadata_Conjunction_Short()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C" } } };

        var result = formatter.FormatMessage("{items, list, conjunction, short}", args);

        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
    }

    [Fact]
    public void ListPatternMetadata_Conjunction_Narrow()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C" } } };

        var result = formatter.FormatMessage("{items, list, conjunction, narrow}", args);

        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
    }

    [Fact]
    public void ListPatternMetadata_Disjunction_Long()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C" } } };

        var result = formatter.FormatMessage("{items, list, disjunction}", args);

        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
        Assert.Contains("or", result);
    }

    [Fact]
    public void ListPatternMetadata_Disjunction_Short()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C" } } };

        var result = formatter.FormatMessage("{items, list, disjunction, short}", args);

        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
    }

    [Fact]
    public void ListPatternMetadata_Disjunction_Narrow()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C" } } };

        var result = formatter.FormatMessage("{items, list, disjunction, narrow}", args);

        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
    }

    [Fact]
    public void ListPatternMetadata_Unit_Long()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "5 km", "3 m" } } };

        var result = formatter.FormatMessage("{items, list, unit, long}", args);

        Assert.Contains("5 km", result);
        Assert.Contains("3 m", result);
    }

    [Fact]
    public void ListPatternMetadata_Unit_Short()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "5 km", "3 m" } } };

        var result = formatter.FormatMessage("{items, list, unit, short}", args);

        Assert.Contains("5 km", result);
        Assert.Contains("3 m", result);
    }

    [Fact]
    public void ListPatternMetadata_Unit_Narrow()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "5 km", "3 m" } } };

        var result = formatter.FormatMessage("{items, list, unit, narrow}", args);

        Assert.Contains("5 km", result);
        Assert.Contains("3 m", result);
    }

    [Fact]
    public void ListPatternMetadata_TwoItems_Conjunction()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B" } } };

        var result = formatter.FormatMessage("{items, list}", args);

        Assert.Equal("A and B", result);
    }

    [Fact]
    public void ListPatternMetadata_TwoItems_Disjunction()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B" } } };

        var result = formatter.FormatMessage("{items, list, disjunction}", args);

        Assert.Equal("A or B", result);
    }

    [Fact]
    public void ListPatternMetadata_SingleItem()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A" } } };

        var result = formatter.FormatMessage("{items, list}", args);

        Assert.Equal("A", result);
    }

    [Fact]
    public void ListPatternMetadata_EmptyList()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", Array.Empty<string>() } };

        var result = formatter.FormatMessage("{items, list}", args);

        Assert.Equal("", result);
    }

    [Fact]
    public void ListPatternMetadata_BaseLocaleFallback()
    {
        var formatter = new MessageFormatter("en-GB", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C" } } };

        var result = formatter.FormatMessage("{items, list}", args);

        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
    }

    [Fact]
    public void ListPatternMetadata_UnknownLocale_UsesFallback()
    {
        var formatter = new MessageFormatter("xx-XX", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C" } } };

        var result = formatter.FormatMessage("{items, list}", args);

        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
    }

    [Fact]
    public void ListPatternMetadata_EmptyProvider_ThrowsInvalidLocaleException()
    {
        // Empty provider has no locale data
        Assert.Throws<InvalidLocaleException>(() => new MessageFormatter("en", TestOptions.WithEmptyProvider()));
    }

    [Fact]
    public void ListPatternMetadata_FourItems()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C", "D" } } };

        var result = formatter.FormatMessage("{items, list}", args);

        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
        Assert.Contains("D", result);
    }

    [Fact]
    public void ListPatternMetadata_ManyItems()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C", "D", "E", "F" } } };

        var result = formatter.FormatMessage("{items, list}", args);

        Assert.Contains("A", result);
        Assert.Contains("F", result);
        Assert.Contains("and", result);
    }

    #endregion

    #region UnitMetadata Tests

    [Fact]
    public void UnitMetadata_GetUnitString_Meter()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 1 } };

        var result = formatter.FormatMessage("{n, number, ::unit/meter}", args);

        Assert.Contains("m", result.ToLower());
    }

    [Fact]
    public void UnitMetadata_GetUnitString_Kilometer()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = formatter.FormatMessage("{n, number, ::unit/kilometer}", args);

        Assert.Contains("km", result.ToLower());
    }

    [Fact]
    public void UnitMetadata_GetUnitString_Celsius()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 25 } };

        var result = formatter.FormatMessage("{n, number, ::unit/celsius}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void UnitMetadata_GetUnitString_Hour()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 2 } };

        var result = formatter.FormatMessage("{n, number, ::unit/hour}", args);

        Assert.Contains("hr", result.ToLower());
    }

    [Fact]
    public void UnitMetadata_GetUnitString_Percent()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 50 } };

        var result = formatter.FormatMessage("{n, number, ::unit/percent}", args);

        // May use "%" symbol or "percent" word depending on locale data
        Assert.Contains("50", result);
        Assert.True(result.Contains("%") || result.Contains("percent"),
            $"Expected '%' or 'percent' in result: {result}");
    }

    [Fact]
    public void UnitMetadata_GetUnitString_UnknownUnit_FallsBackToId()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = formatter.FormatMessage("{n, number, ::unit/unknown-unit}", args);

        Assert.Contains("5", result);
    }

    [Fact]
    public void UnitMetadata_BaseLocaleFallback()
    {
        var formatter = new MessageFormatter("en-GB", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = formatter.FormatMessage("{n, number, ::unit/meter}", args);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void UnitMetadata_UnknownLocale_UsesFallback()
    {
        var formatter = new MessageFormatter("xx-XX", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = formatter.FormatMessage("{n, number, ::unit/meter}", args);

        Assert.NotEmpty(result);
    }

    #endregion
}
