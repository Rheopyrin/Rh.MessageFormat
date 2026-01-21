using System.Collections.Generic;
using Rh.MessageFormat.Abstractions.Models;
using Xunit;

namespace Rh.MessageFormat.Tests.Models;

/// <summary>
/// Tests for Abstractions model classes: PluralContext and UnitData.
/// </summary>
public class AbstractionsModelTests
{
    #region PluralContext Tests

    [Fact]
    public void PluralContext_Integer_CalculatesOperands()
    {
        var ctx = new PluralContext(42);

        Assert.Equal(42, ctx.Number);
        Assert.Equal(42, ctx.N);
        Assert.Equal(42, ctx.I);
        Assert.Equal(0, ctx.V);
        Assert.Equal(0, ctx.W);
        Assert.Equal(0, ctx.F);
        Assert.Equal(0, ctx.T);
        Assert.Equal(0, ctx.C);
        Assert.Equal(0, ctx.E);
    }

    [Fact]
    public void PluralContext_NegativeInteger_AbsoluteValueForN()
    {
        var ctx = new PluralContext(-5);

        Assert.Equal(-5, ctx.Number);
        Assert.Equal(5, ctx.N); // N is absolute value
        Assert.Equal(-5, ctx.I);
        Assert.Equal(0, ctx.V);
        Assert.Equal(0, ctx.W);
        Assert.Equal(0, ctx.F);
        Assert.Equal(0, ctx.T);
    }

    [Fact]
    public void PluralContext_Zero_CalculatesOperands()
    {
        var ctx = new PluralContext(0);

        Assert.Equal(0, ctx.Number);
        Assert.Equal(0, ctx.N);
        Assert.Equal(0, ctx.I);
        Assert.Equal(0, ctx.V);
        Assert.Equal(0, ctx.W);
        Assert.Equal(0, ctx.F);
        Assert.Equal(0, ctx.T);
    }

    [Fact]
    public void PluralContext_Decimal_CalculatesOperands()
    {
        var ctx = new PluralContext(1.5m);

        Assert.Equal(1.5, ctx.Number);
        Assert.Equal(1.5, ctx.N);
        Assert.Equal(1, ctx.I);
        Assert.Equal(1, ctx.V); // V = 1 visible fraction digit
        Assert.Equal(1, ctx.W); // W = 1 without trailing zeros
        Assert.Equal(5, ctx.F); // F = 5 (fraction digits as integer)
        Assert.Equal(5, ctx.T); // T = 5 (without trailing zeros)
    }

    [Fact]
    public void PluralContext_DecimalWithTrailingZeros_CalculatesOperands()
    {
        var ctx = new PluralContext(1.50m);

        Assert.Equal(1.5, ctx.Number);
        Assert.Equal(1.5, ctx.N);
        Assert.Equal(1, ctx.I);
        // 1.5m.ToString() = "1.5" - no trailing zeros preserved
        Assert.True(ctx.V >= 1);
    }

    [Fact]
    public void PluralContext_Double_CalculatesOperands()
    {
        var ctx = new PluralContext(2.5);

        Assert.Equal(2.5, ctx.Number);
        Assert.Equal(2.5, ctx.N);
        Assert.Equal(2, ctx.I);
        Assert.Equal(1, ctx.V);
        Assert.Equal(1, ctx.W);
        Assert.Equal(5, ctx.F);
        Assert.Equal(5, ctx.T);
    }

    [Fact]
    public void PluralContext_DoubleWholeNumber_CalculatesOperands()
    {
        var ctx = new PluralContext(3.0);

        Assert.Equal(3, ctx.Number);
        Assert.Equal(3, ctx.N);
        Assert.Equal(3, ctx.I);
        // 3.0.ToString() = "3" - no decimal point
        Assert.Equal(0, ctx.V);
        Assert.Equal(0, ctx.W);
    }

    [Fact]
    public void PluralContext_String_CalculatesOperands()
    {
        var ctx = new PluralContext("1.23");

        Assert.Equal(1.23, ctx.Number);
        Assert.Equal(1.23, ctx.N);
        Assert.Equal(1, ctx.I);
        Assert.Equal(2, ctx.V); // V = 2 visible fraction digits
        Assert.Equal(2, ctx.W); // W = 2 without trailing zeros
        Assert.Equal(23, ctx.F); // F = 23
        Assert.Equal(23, ctx.T); // T = 23
    }

    [Fact]
    public void PluralContext_StringWithTrailingZeros_CalculatesOperands()
    {
        var ctx = new PluralContext("1.50");

        Assert.Equal(1.5, ctx.Number);
        Assert.Equal(1.5, ctx.N);
        Assert.Equal(1, ctx.I);
        Assert.Equal(2, ctx.V); // V = 2 visible fraction digits
        Assert.Equal(1, ctx.W); // W = 1 without trailing zeros
        Assert.Equal(50, ctx.F); // F = 50
        Assert.Equal(5, ctx.T); // T = 5 (without trailing zeros)
    }

    [Fact]
    public void PluralContext_StringWithMultipleTrailingZeros_CalculatesOperands()
    {
        var ctx = new PluralContext("1.200");

        Assert.Equal(1.2, ctx.Number);
        Assert.Equal(1.2, ctx.N);
        Assert.Equal(1, ctx.I);
        Assert.Equal(3, ctx.V); // V = 3 visible fraction digits
        Assert.Equal(1, ctx.W); // W = 1 without trailing zeros
        Assert.Equal(200, ctx.F); // F = 200
        Assert.Equal(2, ctx.T); // T = 2 (without trailing zeros)
    }

    [Fact]
    public void PluralContext_StringInteger_CalculatesOperands()
    {
        var ctx = new PluralContext("42");

        Assert.Equal(42, ctx.Number);
        Assert.Equal(42, ctx.N);
        Assert.Equal(42, ctx.I);
        Assert.Equal(0, ctx.V);
        Assert.Equal(0, ctx.W);
        Assert.Equal(0, ctx.F);
        Assert.Equal(0, ctx.T);
    }

    [Fact]
    public void PluralContext_NegativeDouble_CalculatesOperands()
    {
        var ctx = new PluralContext(-3.14);

        Assert.Equal(-3.14, ctx.Number);
        Assert.Equal(3.14, ctx.N); // N is absolute value
        Assert.Equal(-3, ctx.I);
        Assert.Equal(2, ctx.V);
    }

    [Fact]
    public void PluralContext_VerySmallDecimal_CalculatesOperands()
    {
        var ctx = new PluralContext("0.001");

        Assert.Equal(0.001, ctx.Number);
        Assert.Equal(0.001, ctx.N);
        Assert.Equal(0, ctx.I);
        Assert.Equal(3, ctx.V); // V = 3 visible fraction digits
        Assert.Equal(3, ctx.W); // W = 3 (001 trimmed of trailing zeros is "001" which has length 3 - no trailing zeros)
        Assert.Equal(1, ctx.F); // F = 001 as integer = 1
        Assert.Equal(1, ctx.T); // T = 1
    }

    [Fact]
    public void PluralContext_LargeInteger_CalculatesOperands()
    {
        var ctx = new PluralContext(1000000);

        Assert.Equal(1000000, ctx.Number);
        Assert.Equal(1000000, ctx.N);
        Assert.Equal(1000000, ctx.I);
        Assert.Equal(0, ctx.V);
        Assert.Equal(0, ctx.W);
    }

    [Fact]
    public void PluralContext_CompactExponent_DefaultsToZero()
    {
        var ctx = new PluralContext(1000);

        // C and E are currently always 0 (compact exponent not implemented)
        Assert.Equal(0, ctx.C);
        Assert.Equal(0, ctx.E);
    }

    [Fact]
    public void PluralContext_AllZerosFraction_CalculatesOperands()
    {
        var ctx = new PluralContext("5.00");

        Assert.Equal(5, ctx.Number);
        Assert.Equal(5, ctx.N);
        Assert.Equal(5, ctx.I);
        Assert.Equal(2, ctx.V); // V = 2 visible fraction digits
        Assert.Equal(0, ctx.W); // W = 0 without trailing zeros (all zeros trimmed)
    }

    #endregion

    #region UnitData Tests

    [Fact]
    public void UnitData_Constructor_SetsProperties()
    {
        var displayNames = new Dictionary<string, string>
        {
            { "long:one", "meter" },
            { "long:other", "meters" },
            { "short:one", "m" },
            { "short:other", "m" }
        };

        var unitData = new UnitData("length-meter", displayNames);

        Assert.Equal("length-meter", unitData.Id);
        Assert.NotNull(unitData.DisplayNames);
    }

    [Fact]
    public void UnitData_Constructor_NullDisplayNames()
    {
        var unitData = new UnitData("length-meter", null);

        Assert.Equal("length-meter", unitData.Id);
        Assert.Null(unitData.DisplayNames);
    }

    [Fact]
    public void UnitData_TryGetDisplayName_Found()
    {
        var displayNames = new Dictionary<string, string>
        {
            { "long:one", "meter" },
            { "long:other", "meters" }
        };

        var unitData = new UnitData("length-meter", displayNames);

        var result = unitData.TryGetDisplayName("long", "one", out var displayName);

        Assert.True(result);
        Assert.Equal("meter", displayName);
    }

    [Fact]
    public void UnitData_TryGetDisplayName_FallbackToOther()
    {
        var displayNames = new Dictionary<string, string>
        {
            { "long:other", "meters" }
        };

        var unitData = new UnitData("length-meter", displayNames);

        var result = unitData.TryGetDisplayName("long", "one", out var displayName);

        Assert.True(result);
        Assert.Equal("meters", displayName);
    }

    [Fact]
    public void UnitData_TryGetDisplayName_NotFound()
    {
        var displayNames = new Dictionary<string, string>
        {
            { "long:one", "meter" }
        };

        var unitData = new UnitData("length-meter", displayNames);

        var result = unitData.TryGetDisplayName("short", "one", out var displayName);

        Assert.False(result);
        Assert.Equal(string.Empty, displayName);
    }

    [Fact]
    public void UnitData_TryGetDisplayName_NullDisplayNames()
    {
        var unitData = new UnitData("length-meter", null);

        var result = unitData.TryGetDisplayName("long", "one", out var displayName);

        Assert.False(result);
        Assert.Equal(string.Empty, displayName);
    }

    [Fact]
    public void UnitData_TryGetDisplayName_OtherCategory_NoFallback()
    {
        var displayNames = new Dictionary<string, string>
        {
            { "long:one", "meter" }
        };

        var unitData = new UnitData("length-meter", displayNames);

        // When count is "other" and not found, no fallback happens
        var result = unitData.TryGetDisplayName("long", "other", out var displayName);

        Assert.False(result);
        Assert.Equal(string.Empty, displayName);
    }

    [Fact]
    public void UnitData_TryGetDisplayName_ShortWidth()
    {
        var displayNames = new Dictionary<string, string>
        {
            { "short:one", "m" },
            { "short:other", "m" }
        };

        var unitData = new UnitData("length-meter", displayNames);

        var result = unitData.TryGetDisplayName("short", "other", out var displayName);

        Assert.True(result);
        Assert.Equal("m", displayName);
    }

    [Fact]
    public void UnitData_TryGetDisplayName_NarrowWidth()
    {
        var displayNames = new Dictionary<string, string>
        {
            { "narrow:one", "m" },
            { "narrow:other", "m" }
        };

        var unitData = new UnitData("length-meter", displayNames);

        var result = unitData.TryGetDisplayName("narrow", "one", out var displayName);

        Assert.True(result);
        Assert.Equal("m", displayName);
    }

    [Fact]
    public void UnitData_TryGetDisplayName_FewCategory()
    {
        var displayNames = new Dictionary<string, string>
        {
            { "long:few", "metry" },
            { "long:other", "metrów" }
        };

        var unitData = new UnitData("length-meter", displayNames);

        var result = unitData.TryGetDisplayName("long", "few", out var displayName);

        Assert.True(result);
        Assert.Equal("metry", displayName);
    }

    [Fact]
    public void UnitData_TryGetDisplayName_ManyCategory()
    {
        var displayNames = new Dictionary<string, string>
        {
            { "long:many", "metrów" },
            { "long:other", "metry" }
        };

        var unitData = new UnitData("length-meter", displayNames);

        var result = unitData.TryGetDisplayName("long", "many", out var displayName);

        Assert.True(result);
        Assert.Equal("metrów", displayName);
    }

    #endregion

    #region DateFormats Tests

    [Fact]
    public void DateFormats_DefaultConstructor()
    {
        var formats = new DateFormats();

        Assert.Null(formats.Short);
        Assert.Null(formats.Medium);
        Assert.Null(formats.Long);
        Assert.Null(formats.Full);
    }

    [Fact]
    public void DateFormats_Constructor()
    {
        var formats = new DateFormats(
            full: "EEEE, MMMM d, y",
            @long: "MMMM d, y",
            medium: "MMM d, y",
            @short: "M/d/yy"
        );

        Assert.Equal("M/d/yy", formats.Short);
        Assert.Equal("MMM d, y", formats.Medium);
        Assert.Equal("MMMM d, y", formats.Long);
        Assert.Equal("EEEE, MMMM d, y", formats.Full);
    }

    #endregion

    #region TimeFormats Tests

    [Fact]
    public void TimeFormats_DefaultConstructor()
    {
        var formats = new TimeFormats();

        Assert.Null(formats.Short);
        Assert.Null(formats.Medium);
        Assert.Null(formats.Long);
        Assert.Null(formats.Full);
    }

    [Fact]
    public void TimeFormats_Constructor()
    {
        var formats = new TimeFormats(
            full: "h:mm:ss a zzzz",
            @long: "h:mm:ss a z",
            medium: "h:mm:ss a",
            @short: "h:mm a"
        );

        Assert.Equal("h:mm a", formats.Short);
        Assert.Equal("h:mm:ss a", formats.Medium);
        Assert.Equal("h:mm:ss a z", formats.Long);
        Assert.Equal("h:mm:ss a zzzz", formats.Full);
    }

    #endregion

    #region DateTimeFormats Tests

    [Fact]
    public void DateTimeFormats_DefaultConstructor()
    {
        var formats = new DateTimeFormats();

        Assert.Null(formats.Short);
        Assert.Null(formats.Medium);
        Assert.Null(formats.Long);
        Assert.Null(formats.Full);
    }

    [Fact]
    public void DateTimeFormats_Constructor()
    {
        var formats = new DateTimeFormats(
            full: "{1} 'at' {0}",
            @long: "{1} 'at' {0}",
            medium: "{1}, {0}",
            @short: "{1}, {0}"
        );

        Assert.Equal("{1}, {0}", formats.Short);
        Assert.Equal("{1}, {0}", formats.Medium);
        Assert.Equal("{1} 'at' {0}", formats.Long);
        Assert.Equal("{1} 'at' {0}", formats.Full);
    }

    #endregion

    #region CurrencyData Tests

    [Fact]
    public void CurrencyData_DefaultConstructor()
    {
        var data = new CurrencyData();

        Assert.Null(data.Symbol);
        Assert.Null(data.NarrowSymbol);
        Assert.Null(data.DisplayName);
        Assert.Null(data.DisplayNameOne);
        Assert.Null(data.DisplayNameFew);
        Assert.Null(data.DisplayNameMany);
        Assert.Null(data.DisplayNameOther);
    }

    [Fact]
    public void CurrencyData_Constructor()
    {
        var data = new CurrencyData(
            code: "USD",
            symbol: "$",
            narrowSymbol: "$",
            displayName: "US Dollar",
            displayNameOne: "US dollar",
            displayNameFew: null,
            displayNameMany: null,
            displayNameOther: "US dollars"
        );

        Assert.Equal("USD", data.Code);
        Assert.Equal("$", data.Symbol);
        Assert.Equal("$", data.NarrowSymbol);
        Assert.Equal("US Dollar", data.DisplayName);
        Assert.Equal("US dollar", data.DisplayNameOne);
        Assert.Null(data.DisplayNameFew);
        Assert.Null(data.DisplayNameMany);
        Assert.Equal("US dollars", data.DisplayNameOther);
    }

    [Fact]
    public void CurrencyData_Constructor_WithFewAndMany()
    {
        var data = new CurrencyData(
            code: "UAH",
            symbol: "₴",
            narrowSymbol: "₴",
            displayName: "Ukrainian hryvnia",
            displayNameOne: "гривня",
            displayNameFew: "гривні",
            displayNameMany: "гривень",
            displayNameOther: "гривні"
        );

        Assert.Equal("UAH", data.Code);
        Assert.Equal("₴", data.Symbol);
        Assert.Equal("₴", data.NarrowSymbol);
        Assert.Equal("Ukrainian hryvnia", data.DisplayName);
        Assert.Equal("гривня", data.DisplayNameOne);
        Assert.Equal("гривні", data.DisplayNameFew);
        Assert.Equal("гривень", data.DisplayNameMany);
        Assert.Equal("гривні", data.DisplayNameOther);
    }

    #endregion

    #region ListPatternData Tests

    [Fact]
    public void ListPatternData_DefaultConstructor()
    {
        var data = new ListPatternData();

        Assert.Null(data.Start);
        Assert.Null(data.Middle);
        Assert.Null(data.End);
        Assert.Null(data.Two);
    }

    [Fact]
    public void ListPatternData_Constructor()
    {
        var data = new ListPatternData(
            type: "standard",
            start: "{0}, {1}",
            middle: "{0}, {1}",
            end: "{0}, and {1}",
            two: "{0} and {1}"
        );

        Assert.Equal("standard", data.Type);
        Assert.Equal("{0}, {1}", data.Start);
        Assert.Equal("{0}, {1}", data.Middle);
        Assert.Equal("{0}, and {1}", data.End);
        Assert.Equal("{0} and {1}", data.Two);
    }

    #endregion

    #region DatePatternData Tests

    [Fact]
    public void DatePatternData_DefaultConstructor()
    {
        var data = new DatePatternData();

        // Default struct has default values for nested structs
        Assert.Null(data.Date.Short);
        Assert.Null(data.Time.Short);
        Assert.Null(data.DateTime.Short);
    }

    [Fact]
    public void DatePatternData_Constructor()
    {
        var dateFormats = new DateFormats("full", "long", "medium", "short");
        var timeFormats = new TimeFormats("full", "long", "medium", "short");
        var dtFormats = new DateTimeFormats("full", "long", "medium", "short");

        var data = new DatePatternData(dateFormats, timeFormats, dtFormats);

        Assert.Equal("short", data.Date.Short);
        Assert.Equal("short", data.Time.Short);
        Assert.Equal("short", data.DateTime.Short);
    }

    #endregion
}
