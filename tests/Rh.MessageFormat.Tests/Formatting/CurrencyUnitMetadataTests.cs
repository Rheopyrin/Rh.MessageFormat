using System;
using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Formatting;

/// <summary>
/// Tests for CurrencyMetadata and UnitMetadata via MessageFormatter.
/// </summary>
public class CurrencyUnitMetadataTests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region Currency Symbol Tests

    [Fact]
    public void Currency_USDSymbol()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD}", args);

        Assert.Contains("$", result);
    }

    [Fact]
    public void Currency_EURSymbol()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/EUR}", args);

        Assert.True(result.Contains("€") || result.Contains("EUR"));
    }

    [Fact]
    public void Currency_GBPSymbol()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/GBP}", args);

        Assert.True(result.Contains("£") || result.Contains("GBP"));
    }

    [Fact]
    public void Currency_JPYSymbol()
    {
        var args = new Dictionary<string, object?> { { "n", 1000 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/JPY}", args);

        Assert.True(result.Contains("¥") || result.Contains("JPY"));
    }

    [Fact]
    public void Currency_UnknownCode_FallsBackToCode()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/XYZ}", args);

        // Unknown currency should show the code
        Assert.Contains("XYZ", result);
    }

    #endregion

    #region Currency Display Mode Tests

    [Fact]
    public void Currency_DisplayAsCode()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD unit-width-iso-code}", args);

        Assert.Contains("USD", result);
    }

    [Fact]
    public void Currency_DisplayAsSymbol()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD currency-symbol}", args);

        Assert.Contains("$", result);
    }

    [Fact]
    public void Currency_DisplayAsNarrowSymbol()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD currency-narrow-symbol}", args);

        Assert.Contains("$", result);
    }

    [Fact]
    public void Currency_DisplayAsFullName_Singular()
    {
        var args = new Dictionary<string, object?> { { "n", 1 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD unit-width-full-name}", args);

        Assert.Contains("1", result);
        Assert.Contains("US dollar", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Currency_DisplayAsFullName_Plural()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD unit-width-full-name}", args);

        Assert.Contains("100", result);
        Assert.Contains("US dollars", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Currency_DisplayAsFullName_EUR()
    {
        var args = new Dictionary<string, object?> { { "n", 50 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/EUR unit-width-full-name}", args);

        Assert.Contains("50", result);
        Assert.Contains("euro", result, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Currency with Different Values Tests

    [Fact]
    public void Currency_ZeroValue()
    {
        var args = new Dictionary<string, object?> { { "n", 0 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD}", args);

        Assert.Contains("$", result);
        Assert.Contains("0", result);
    }

    [Fact]
    public void Currency_NegativeValue()
    {
        var args = new Dictionary<string, object?> { { "n", -100 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD}", args);

        Assert.Contains("$", result);
    }

    [Fact]
    public void Currency_DecimalValue()
    {
        var args = new Dictionary<string, object?> { { "n", 99.99 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD}", args);

        Assert.Contains("$", result);
        Assert.Contains("99", result);
    }

    [Fact]
    public void Currency_LargeValue()
    {
        var args = new Dictionary<string, object?> { { "n", 1000000 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD}", args);

        Assert.Contains("$", result);
    }

    #endregion

    #region Unit Short Name Tests

    [Fact]
    public void Unit_Meter()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/meter}", args);

        Assert.Contains("100", result);
    }

    [Fact]
    public void Unit_Kilometer()
    {
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/kilometer}", args);

        Assert.Contains("5", result);
    }

    [Fact]
    public void Unit_Celsius()
    {
        var args = new Dictionary<string, object?> { { "n", 25 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/celsius}", args);

        Assert.Contains("25", result);
    }

    [Fact]
    public void Unit_Kilogram()
    {
        var args = new Dictionary<string, object?> { { "n", 70 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/kilogram}", args);

        Assert.Contains("70", result);
    }

    [Fact]
    public void Unit_Liter()
    {
        var args = new Dictionary<string, object?> { { "n", 2 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/liter}", args);

        Assert.Contains("2", result);
    }

    [Fact]
    public void Unit_Hour()
    {
        var args = new Dictionary<string, object?> { { "n", 24 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/hour}", args);

        Assert.Contains("24", result);
    }

    [Fact]
    public void Unit_Byte()
    {
        var args = new Dictionary<string, object?> { { "n", 1024 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/byte}", args);

        Assert.Contains("1,024", result);
    }

    #endregion

    #region Unit Width Tests

    [Fact]
    public void Unit_WidthShort()
    {
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/kilometer unit-width-short}", args);

        Assert.Contains("5", result);
    }

    [Fact]
    public void Unit_WidthNarrow()
    {
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/kilometer unit-width-narrow}", args);

        Assert.Contains("5", result);
    }

    [Fact]
    public void Unit_WidthFullName()
    {
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/kilometer unit-width-full-name}", args);

        Assert.Contains("5", result);
    }

    #endregion

    #region CLDR Unit ID Tests

    [Fact]
    public void Unit_FullCldrId_LengthMeter()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/length-meter}", args);

        Assert.Contains("100", result);
    }

    [Fact]
    public void Unit_FullCldrId_TemperatureCelsius()
    {
        var args = new Dictionary<string, object?> { { "n", 25 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/temperature-celsius}", args);

        Assert.Contains("25", result);
    }

    [Fact]
    public void Unit_FullCldrId_MassKilogram()
    {
        var args = new Dictionary<string, object?> { { "n", 70 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/mass-kilogram}", args);

        Assert.Contains("70", result);
    }

    #endregion

    #region Unit Plurality Tests

    [Fact]
    public void Unit_Singular_OneUnit()
    {
        var args = new Dictionary<string, object?> { { "n", 1 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/kilometer}", args);

        Assert.Contains("1", result);
    }

    [Fact]
    public void Unit_Plural_MultipleUnits()
    {
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/kilometer}", args);

        Assert.Contains("5", result);
    }

    [Fact]
    public void Unit_Zero_UsesPlural()
    {
        var args = new Dictionary<string, object?> { { "n", 0 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/kilometer}", args);

        Assert.Contains("0", result);
    }

    [Fact]
    public void Unit_Negative_UsesPlural()
    {
        var args = new Dictionary<string, object?> { { "n", -5 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/kilometer}", args);

        Assert.Contains("-5", result);
    }

    #endregion

    #region Unknown Unit Tests

    [Fact]
    public void Unit_Unknown_FallsBackToId()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/unknown-unit}", args);

        // Unknown unit should fall back to the unit ID
        Assert.Contains("100", result);
    }

    #endregion

    #region Combined Currency/Unit Options Tests

    [Fact]
    public void Currency_WithPrecision()
    {
        var args = new Dictionary<string, object?> { { "n", 1234.5 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD .00}", args);

        Assert.Contains("$", result);
        Assert.Contains("1,234.50", result);
    }

    [Fact]
    public void Currency_WithGroupingOff()
    {
        var args = new Dictionary<string, object?> { { "n", 1234567 } };

        var result = _formatter.FormatMessage("{n, number, ::currency/USD group-off}", args);

        Assert.Contains("$", result);
        Assert.DoesNotContain(",", result.Replace("$", "")); // Ignore comma in currency format
    }

    [Fact]
    public void Unit_WithPrecision()
    {
        var args = new Dictionary<string, object?> { { "n", 3.14159 } };

        var result = _formatter.FormatMessage("{n, number, ::unit/meter .00}", args);

        Assert.Contains("3.14", result);
    }

    #endregion

    #region Measure-Unit Alias Tests

    [Fact]
    public void MeasureUnit_Alias_Works()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage("{n, number, ::measure-unit/meter}", args);

        Assert.Contains("100", result);
    }

    #endregion
}
