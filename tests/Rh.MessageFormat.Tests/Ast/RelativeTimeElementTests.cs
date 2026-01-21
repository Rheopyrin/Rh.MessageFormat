using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Unit tests for RelativeTimeElement covering type conversions and enum mappings.
/// </summary>
public class RelativeTimeElementTests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region Numeric Type Conversion Tests

    [Fact]
    public void RelativeTime_FloatValue()
    {
        var args = new Dictionary<string, object?> { { "value", 3.0f } };

        var result = _formatter.FormatMessage("{value, relativeTime, day}", args);

        Assert.Equal("in 3 days", result);
    }

    [Fact]
    public void RelativeTime_DecimalValue()
    {
        var args = new Dictionary<string, object?> { { "value", 2m } };

        var result = _formatter.FormatMessage("{value, relativeTime, day}", args);

        Assert.Equal("in 2 days", result);
    }

    [Fact]
    public void RelativeTime_ShortValue()
    {
        var args = new Dictionary<string, object?> { { "value", (short)-4 } };

        var result = _formatter.FormatMessage("{value, relativeTime, day}", args);

        Assert.Equal("4 days ago", result);
    }

    [Fact]
    public void RelativeTime_ByteValue()
    {
        var args = new Dictionary<string, object?> { { "value", (byte)5 } };

        var result = _formatter.FormatMessage("{value, relativeTime, day}", args);

        Assert.Equal("in 5 days", result);
    }

    [Fact]
    public void RelativeTime_SByteValue()
    {
        var args = new Dictionary<string, object?> { { "value", (sbyte)-3 } };

        var result = _formatter.FormatMessage("{value, relativeTime, day}", args);

        Assert.Equal("3 days ago", result);
    }

    [Fact]
    public void RelativeTime_UIntValue()
    {
        var args = new Dictionary<string, object?> { { "value", 7u } };

        var result = _formatter.FormatMessage("{value, relativeTime, day}", args);

        Assert.Equal("in 7 days", result);
    }

    [Fact]
    public void RelativeTime_ULongValue()
    {
        var args = new Dictionary<string, object?> { { "value", 10ul } };

        var result = _formatter.FormatMessage("{value, relativeTime, day}", args);

        Assert.Equal("in 10 days", result);
    }

    [Fact]
    public void RelativeTime_UShortValue()
    {
        var args = new Dictionary<string, object?> { { "value", (ushort)6 } };

        var result = _formatter.FormatMessage("{value, relativeTime, day}", args);

        Assert.Equal("in 6 days", result);
    }

    [Fact]
    public void RelativeTime_InvalidStringValue_DefaultsToZero()
    {
        var args = new Dictionary<string, object?> { { "value", "not-a-number" } };

        var result = _formatter.FormatMessage("{value, relativeTime, day long auto}", args);

        // Invalid string parses to 0, which with auto shows "today"
        Assert.Equal("today", result);
    }

    [Fact]
    public void RelativeTime_NegativeStringValue()
    {
        var args = new Dictionary<string, object?> { { "value", "-2" } };

        var result = _formatter.FormatMessage("{value, relativeTime, day}", args);

        Assert.Equal("2 days ago", result);
    }

    [Fact]
    public void RelativeTime_DecimalStringValue()
    {
        // Use actual double to avoid culture-dependent string parsing
        var args = new Dictionary<string, object?> { { "value", 3.5 } };

        var result = _formatter.FormatMessage("{value, relativeTime, day}", args);

        // 3.5 is not a whole number, so it formats with decimal
        Assert.StartsWith("in 3", result);
        Assert.EndsWith("5 days", result);
    }

    #endregion

    #region Style Enum Mapping Tests

    [Fact]
    public void RelativeTime_StyleLong_MapsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "value", 2 } };

        var result = _formatter.FormatMessage("{value, relativeTime, day long}", args);

        Assert.Equal("in 2 days", result);
    }

    [Fact]
    public void RelativeTime_StyleShort_MapsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "value", 2 } };

        var result = _formatter.FormatMessage("{value, relativeTime, day short}", args);

        Assert.Equal("in 2 days", result); // Short uses same pattern in test data
    }

    [Fact]
    public void RelativeTime_StyleNarrow_MapsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "value", 2 } };

        var result = _formatter.FormatMessage("{value, relativeTime, day narrow}", args);

        Assert.Equal("in 2d", result);
    }

    [Fact]
    public void RelativeTime_DefaultStyle_IsLong()
    {
        var args = new Dictionary<string, object?> { { "value", 2 } };

        var result = _formatter.FormatMessage("{value, relativeTime, day}", args);

        Assert.Equal("in 2 days", result);
    }

    #endregion

    #region Numeric Mode Enum Mapping Tests

    [Fact]
    public void RelativeTime_NumericAlways_UsesNumericFormat()
    {
        var args = new Dictionary<string, object?> { { "value", 1 } };

        // Without "auto", should always use numeric
        var result = _formatter.FormatMessage("{value, relativeTime, day long always}", args);

        Assert.Equal("in 1 day", result);
    }

    [Fact]
    public void RelativeTime_NumericAuto_UsesRelativeType()
    {
        var args = new Dictionary<string, object?> { { "value", 1 } };

        var result = _formatter.FormatMessage("{value, relativeTime, day long auto}", args);

        Assert.Equal("tomorrow", result);
    }

    [Fact]
    public void RelativeTime_DefaultNumeric_IsAlways()
    {
        var args = new Dictionary<string, object?> { { "value", -1 } };

        // Without specifying numeric mode, should default to "always"
        var result = _formatter.FormatMessage("{value, relativeTime, day long}", args);

        Assert.Equal("1 day ago", result);
    }

    #endregion

    #region Field Parsing Tests

    [Theory]
    [InlineData("year", 2, "in 2 years")]
    [InlineData("month", 3, "in 3 months")]
    [InlineData("week", 1, "in 1 week")]
    [InlineData("day", 5, "in 5 days")]
    [InlineData("hour", 4, "in 4 hours")]
    [InlineData("minute", 10, "in 10 minutes")]
    [InlineData("second", 30, "in 30 seconds")]
    public void RelativeTime_AllValidFields(string field, int value, string expected)
    {
        var args = new Dictionary<string, object?> { { "v", value } };

        var result = _formatter.FormatMessage($"{{v, relativeTime, {field}}}", args);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void RelativeTime_FieldIsCaseInsensitive()
    {
        var args = new Dictionary<string, object?> { { "value", 2 } };

        var resultLower = _formatter.FormatMessage("{value, relativeTime, day}", args);
        var resultUpper = _formatter.FormatMessage("{value, relativeTime, DAY}", args);
        var resultMixed = _formatter.FormatMessage("{value, relativeTime, Day}", args);

        Assert.Equal(resultLower, resultUpper);
        Assert.Equal(resultLower, resultMixed);
    }

    [Fact]
    public void RelativeTime_StyleIsCaseInsensitive()
    {
        var args = new Dictionary<string, object?> { { "value", 2 } };

        var resultLower = _formatter.FormatMessage("{value, relativeTime, day narrow}", args);
        var resultUpper = _formatter.FormatMessage("{value, relativeTime, day NARROW}", args);
        var resultMixed = _formatter.FormatMessage("{value, relativeTime, day Narrow}", args);

        Assert.Equal(resultLower, resultUpper);
        Assert.Equal(resultLower, resultMixed);
    }

    #endregion

    #region Argument Parsing Edge Cases

    [Fact]
    public void RelativeTime_ExtraWhitespace_HandledCorrectly()
    {
        var args = new Dictionary<string, object?> { { "value", 2 } };

        var result = _formatter.FormatMessage("{value, relativeTime,   day   long   auto}", args);

        Assert.Equal("in 2 days", result);
    }

    [Fact]
    public void RelativeTime_OnlyField_NoStyleOrNumeric()
    {
        var args = new Dictionary<string, object?> { { "value", 3 } };

        var result = _formatter.FormatMessage("{value, relativeTime, hour}", args);

        Assert.Equal("in 3 hours", result);
    }

    [Fact]
    public void RelativeTime_FieldAndStyle_NoNumeric()
    {
        var args = new Dictionary<string, object?> { { "value", -1 } };

        var result = _formatter.FormatMessage("{value, relativeTime, day short}", args);

        Assert.Equal("1 day ago", result);
    }

    #endregion
}
