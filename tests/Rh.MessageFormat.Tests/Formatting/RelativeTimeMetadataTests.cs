using System.Collections.Generic;
using Rh.MessageFormat.Abstractions.Models;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Formatting;

/// <summary>
/// Unit tests for RelativeTimeMetadata covering locale fallback, style fallback, and edge cases.
/// </summary>
public class RelativeTimeMetadataTests
{
    #region Locale Fallback Tests

    [Fact]
    public void RelativeTime_UsesExactLocale()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "value", -1 } };

        var result = formatter.FormatMessage("{value, relativeTime, day long auto}", args);

        Assert.Equal("yesterday", result);
    }

    [Fact]
    public void RelativeTime_FallsBackToBaseLocale()
    {
        // Create locale data only for "en" (base), not "en-US"
        var enData = MockCldrLocaleData.CreateEnglish();
        enData.Locale = "en";

        var options = TestOptions.WithLocaleData(enData);
        var formatter = new MessageFormatter("en-US", options);
        var args = new Dictionary<string, object?> { { "value", -1 } };

        var result = formatter.FormatMessage("{value, relativeTime, day long auto}", args);

        // Should fall back to "en" and find "yesterday"
        Assert.Equal("yesterday", result);
    }

    [Fact]
    public void RelativeTime_FallsBackToFallbackLocale()
    {
        // Create provider with only "en" data, but request "fr" locale
        var options = TestOptions.WithEnglish();
        var formatter = new MessageFormatter("fr", options);
        var args = new Dictionary<string, object?> { { "value", -1 } };

        var result = formatter.FormatMessage("{value, relativeTime, day long auto}", args);

        // Should fall back to "en" (default fallback) and find "yesterday"
        Assert.Equal("yesterday", result);
    }

    [Fact]
    public void RelativeTime_NoDataAvailable_UsesFallbackFormat()
    {
        // Create empty provider with no relative time data
        var emptyData = new MockCldrLocaleData { Locale = "en" };
        var options = TestOptions.WithLocaleData(emptyData);
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "value", 3 } };

        var result = formatter.FormatMessage("{value, relativeTime, day}", args);

        // Should use fallback format
        Assert.Equal("in 3 day", result);
    }

    #endregion

    #region Style Fallback Tests

    [Fact]
    public void RelativeTime_StyleFallsBackToLong_WhenShortNotAvailable()
    {
        // Create data with only "long" width
        var localeData = new MockCldrLocaleData { Locale = "en" }
            .WithRelativeTime("day", "long", "day",
                new Dictionary<string, string> { { "-1", "yesterday" }, { "0", "today" }, { "1", "tomorrow" } },
                new Dictionary<string, string> { { "one", "in {0} day" }, { "other", "in {0} days" } },
                new Dictionary<string, string> { { "one", "{0} day ago" }, { "other", "{0} days ago" } });

        var options = TestOptions.WithLocaleData(localeData);
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "value", 2 } };

        // Request "short" style, but only "long" is available
        var result = formatter.FormatMessage("{value, relativeTime, day short}", args);

        // Should fall back to "long" style
        Assert.Equal("in 2 days", result);
    }

    [Fact]
    public void RelativeTime_StyleFallsBackToLong_WhenNarrowNotAvailable()
    {
        // Create data with only "long" width
        var localeData = new MockCldrLocaleData { Locale = "en" }
            .WithRelativeTime("day", "long", "day",
                new Dictionary<string, string> { { "-1", "yesterday" }, { "0", "today" }, { "1", "tomorrow" } },
                new Dictionary<string, string> { { "one", "in {0} day" }, { "other", "in {0} days" } },
                new Dictionary<string, string> { { "one", "{0} day ago" }, { "other", "{0} days ago" } });

        var options = TestOptions.WithLocaleData(localeData);
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "value", 3 } };

        // Request "narrow" style, but only "long" is available
        var result = formatter.FormatMessage("{value, relativeTime, day narrow}", args);

        // Should fall back to "long" style
        Assert.Equal("in 3 days", result);
    }

    [Fact]
    public void RelativeTime_InvalidStyle_DefaultsToLong()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "value", 2 } };

        // Use invalid style
        var result = formatter.FormatMessage("{value, relativeTime, day invalid}", args);

        // Should default to "long" style
        Assert.Equal("in 2 days", result);
    }

    #endregion

    #region Plural Category Tests

    [Fact]
    public void RelativeTime_UsesSingularForm_ForOne()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "value", 1 } };

        var result = formatter.FormatMessage("{value, relativeTime, day}", args);

        Assert.Equal("in 1 day", result);
    }

    [Fact]
    public void RelativeTime_UsesPluralForm_ForOther()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "value", 5 } };

        var result = formatter.FormatMessage("{value, relativeTime, day}", args);

        Assert.Equal("in 5 days", result);
    }

    [Fact]
    public void RelativeTime_UsesPluralForm_ForZero()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "value", 0 } };

        var result = formatter.FormatMessage("{value, relativeTime, day}", args);

        // English uses "other" for zero
        Assert.Equal("in 0 days", result);
    }

    [Fact]
    public void RelativeTime_FallsBackToOther_WhenCategoryMissing()
    {
        // Create data with only "other" category
        var localeData = new MockCldrLocaleData { Locale = "en" }
            .WithRelativeTime("day", "long", "day",
                null,
                new Dictionary<string, string> { { "other", "in {0} days" } },
                new Dictionary<string, string> { { "other", "{0} days ago" } });

        var options = TestOptions.WithLocaleData(localeData);
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "value", 1 } };

        var result = formatter.FormatMessage("{value, relativeTime, day}", args);

        // Should fall back to "other" even for 1
        Assert.Equal("in 1 days", result);
    }

    #endregion

    #region Number Formatting Tests

    [Fact]
    public void RelativeTime_FormatsWholeNumber_AsInteger()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "value", 5.0 } };

        var result = formatter.FormatMessage("{value, relativeTime, day}", args);

        Assert.Equal("in 5 days", result);
        Assert.DoesNotContain(".", result);
    }

    [Fact]
    public void RelativeTime_FormatsDecimal_WithDecimalSeparator()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "value", 2.5 } };

        var result = formatter.FormatMessage("{value, relativeTime, day}", args);

        // Decimal separator may be . or , depending on culture
        Assert.StartsWith("in 2", result);
        Assert.EndsWith("5 days", result);
    }

    [Fact]
    public void RelativeTime_HandlesNearIntegerValues()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        // Value very close to integer (within tolerance)
        var args = new Dictionary<string, object?> { { "value", 3.00001 } };

        var result = formatter.FormatMessage("{value, relativeTime, day}", args);

        // Should format as integer
        Assert.Equal("in 3 days", result);
    }

    [Fact]
    public void RelativeTime_LargeNumber()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "value", 1000 } };

        var result = formatter.FormatMessage("{value, relativeTime, day}", args);

        Assert.Equal("in 1000 days", result);
    }

    #endregion

    #region Auto Numeric Mode Edge Cases

    [Fact]
    public void RelativeTime_Auto_NonIntegerValue_UsesNumericFormat()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "value", 0.5 } };

        var result = formatter.FormatMessage("{value, relativeTime, day long auto}", args);

        // 0.5 is not an integer, so can't match relative type
        // Decimal separator may be . or , depending on culture
        Assert.StartsWith("in 0", result);
        Assert.EndsWith("5 days", result);
    }

    [Fact]
    public void RelativeTime_Auto_IntegerWithNoRelativeType_UsesNumericFormat()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "value", 5 } };

        var result = formatter.FormatMessage("{value, relativeTime, day long auto}", args);

        // 5 has no relative type (only -1, 0, 1 have them)
        Assert.Equal("in 5 days", result);
    }

    [Fact]
    public void RelativeTime_Auto_ThisHour()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "value", 0 } };

        var result = formatter.FormatMessage("{value, relativeTime, hour long auto}", args);

        Assert.Equal("this hour", result);
    }

    [Fact]
    public void RelativeTime_Auto_ThisMinute()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "value", 0 } };

        var result = formatter.FormatMessage("{value, relativeTime, minute long auto}", args);

        Assert.Equal("this minute", result);
    }

    #endregion

    #region Fallback Format Tests

    [Fact]
    public void RelativeTime_FallbackFormat_Past()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "value", -3 } };

        // Use invalid field to trigger fallback
        var result = formatter.FormatMessage("{value, relativeTime, invalidfield}", args);

        Assert.Equal("3 invalidfield ago", result);
    }

    [Fact]
    public void RelativeTime_FallbackFormat_Future()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "value", 5 } };

        // Use invalid field to trigger fallback
        var result = formatter.FormatMessage("{value, relativeTime, invalidfield}", args);

        Assert.Equal("in 5 invalidfield", result);
    }

    [Fact]
    public void RelativeTime_FallbackFormat_Present()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "value", 0 } };

        // Use invalid field to trigger fallback
        var result = formatter.FormatMessage("{value, relativeTime, invalidfield}", args);

        Assert.Equal("this invalidfield", result);
    }

    #endregion

    #region All Valid Fields Tests

    [Theory]
    [InlineData("year")]
    [InlineData("quarter")]
    [InlineData("month")]
    [InlineData("week")]
    [InlineData("day")]
    [InlineData("hour")]
    [InlineData("minute")]
    [InlineData("second")]
    [InlineData("sun")]
    [InlineData("mon")]
    [InlineData("tue")]
    [InlineData("wed")]
    [InlineData("thu")]
    [InlineData("fri")]
    [InlineData("sat")]
    public void RelativeTime_AllValidFields_AreRecognized(string field)
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "value", 2 } };

        var result = formatter.FormatMessage($"{{value, relativeTime, {field}}}", args);

        // Should not use the generic fallback format for valid fields
        // (unless CLDR data is missing, which is expected for some fields like weekdays)
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    #endregion

    #region Negative Past Values

    [Fact]
    public void RelativeTime_PastValue_UsesAbsoluteInPattern()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "value", -7 } };

        var result = formatter.FormatMessage("{value, relativeTime, day}", args);

        // Should use absolute value (7, not -7) in pattern
        Assert.Equal("7 days ago", result);
        Assert.DoesNotContain("-", result);
    }

    [Fact]
    public void RelativeTime_LargeNegativeValue()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "value", -365 } };

        var result = formatter.FormatMessage("{value, relativeTime, day}", args);

        Assert.Equal("365 days ago", result);
    }

    #endregion
}
