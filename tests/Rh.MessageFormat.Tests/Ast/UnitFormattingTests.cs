using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Tests for unit formatting with number skeletons.
/// </summary>
public class UnitFormattingTests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region Length Units

    [Fact]
    public void Unit_Kilometer_Short()
    {
        var args = new Dictionary<string, object?> { { "distance", 5 } };

        var result = _formatter.FormatMessage("{distance, number, ::unit/kilometer}", args);

        Assert.Equal("5 km", result);
    }

    [Fact]
    public void Unit_Kilometer_Long()
    {
        var args = new Dictionary<string, object?> { { "distance", 5 } };

        var result = _formatter.FormatMessage("{distance, number, ::unit/kilometer unit-width-full-name}", args);

        Assert.Equal("5 kilometers", result);
    }

    [Fact]
    public void Unit_Kilometer_Singular()
    {
        var args = new Dictionary<string, object?> { { "distance", 1 } };

        var result = _formatter.FormatMessage("{distance, number, ::unit/kilometer unit-width-full-name}", args);

        Assert.Equal("1 kilometer", result);
    }

    [Fact]
    public void Unit_Meter_Short()
    {
        var args = new Dictionary<string, object?> { { "length", 100 } };

        var result = _formatter.FormatMessage("{length, number, ::unit/meter}", args);

        Assert.Equal("100 m", result);
    }

    [Fact]
    public void Unit_Mile_Long()
    {
        var args = new Dictionary<string, object?> { { "distance", 3 } };

        var result = _formatter.FormatMessage("{distance, number, ::unit/mile unit-width-full-name}", args);

        Assert.Equal("3 miles", result);
    }

    #endregion

    #region Temperature Units

    [Fact]
    public void Unit_Celsius_Short()
    {
        var args = new Dictionary<string, object?> { { "temp", 25 } };

        var result = _formatter.FormatMessage("{temp, number, ::unit/celsius}", args);

        Assert.Equal("25\u00B0C", result);
    }

    [Fact]
    public void Unit_Celsius_Long()
    {
        var args = new Dictionary<string, object?> { { "temp", 25 } };

        var result = _formatter.FormatMessage("{temp, number, ::unit/celsius unit-width-full-name}", args);

        Assert.Equal("25 degrees Celsius", result);
    }

    [Fact]
    public void Unit_Fahrenheit_Short()
    {
        var args = new Dictionary<string, object?> { { "temp", 77 } };

        var result = _formatter.FormatMessage("{temp, number, ::unit/fahrenheit}", args);

        Assert.Equal("77\u00B0F", result);
    }

    #endregion

    #region Mass Units

    [Fact]
    public void Unit_Kilogram_Short()
    {
        var args = new Dictionary<string, object?> { { "weight", 75 } };

        var result = _formatter.FormatMessage("{weight, number, ::unit/kilogram}", args);

        Assert.Equal("75 kg", result);
    }

    [Fact]
    public void Unit_Pound_Long()
    {
        var args = new Dictionary<string, object?> { { "weight", 150 } };

        var result = _formatter.FormatMessage("{weight, number, ::unit/pound unit-width-full-name}", args);

        Assert.Equal("150 pounds", result);
    }

    #endregion

    #region Volume Units

    [Fact]
    public void Unit_Liter_Short()
    {
        var args = new Dictionary<string, object?> { { "volume", 2.5 } };

        var result = _formatter.FormatMessage("{volume, number, ::unit/liter}", args);

        Assert.Contains("2.5", result);
        Assert.Contains("L", result);
    }

    #endregion

    #region Time Units

    [Fact]
    public void Unit_Hour_Short()
    {
        var args = new Dictionary<string, object?> { { "time", 3 } };

        var result = _formatter.FormatMessage("{time, number, ::unit/hour}", args);

        Assert.Equal("3 hrs", result); // plural form for 3
    }

    [Fact]
    public void Unit_Hour_Long()
    {
        var args = new Dictionary<string, object?> { { "time", 3 } };

        var result = _formatter.FormatMessage("{time, number, ::unit/hour unit-width-full-name}", args);

        Assert.Equal("3 hours", result);
    }

    #endregion

    #region Digital Units

    [Fact]
    public void Unit_Byte_Short()
    {
        var args = new Dictionary<string, object?> { { "size", 1024 } };

        var result = _formatter.FormatMessage("{size, number, ::unit/byte}", args);

        Assert.Contains("1,024", result);
        Assert.Contains("byte", result);
    }

    [Fact]
    public void Unit_Megabyte_Short()
    {
        var args = new Dictionary<string, object?> { { "size", 512 } };

        var result = _formatter.FormatMessage("{size, number, ::unit/megabyte}", args);

        Assert.Equal("512 MB", result);
    }

    #endregion

    #region Speed Units

    [Fact]
    public void Unit_KilometerPerHour_Short()
    {
        var args = new Dictionary<string, object?> { { "speed", 60 } };

        var result = _formatter.FormatMessage("{speed, number, ::unit/kilometer-per-hour}", args);

        Assert.Equal("60 km/h", result);
    }

    [Fact]
    public void Unit_MilePerHour_Short()
    {
        var args = new Dictionary<string, object?> { { "speed", 65 } };

        var result = _formatter.FormatMessage("{speed, number, ::unit/mile-per-hour}", args);

        Assert.Equal("65 mph", result);
    }

    #endregion

    #region Area Units

    [Fact]
    public void Unit_SquareMeter_Short()
    {
        var args = new Dictionary<string, object?> { { "area", 100 } };

        var result = _formatter.FormatMessage("{area, number, ::unit/square-meter}", args);

        Assert.Equal("100 m\u00B2", result);
    }

    #endregion

    #region Combined Options

    [Fact]
    public void Unit_WithPrecision()
    {
        var args = new Dictionary<string, object?> { { "distance", 3.14159 } };

        var result = _formatter.FormatMessage("{distance, number, ::unit/kilometer .00}", args);

        Assert.Equal("3.14 km", result);
    }

    [Fact]
    public void Unit_InComplexMessage()
    {
        var args = new Dictionary<string, object?>
        {
            { "city", "New York" },
            { "distance", 15.5 }
        };

        var result = _formatter.FormatMessage("{city} is {distance, number, ::unit/kilometer unit-width-full-name} away.", args);

        Assert.Equal("New York is 15.5 kilometers away.", result);
    }

    #endregion
}
