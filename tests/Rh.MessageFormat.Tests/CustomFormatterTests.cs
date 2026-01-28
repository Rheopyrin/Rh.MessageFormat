using System;
using System.Collections.Generic;
using System.Globalization;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests;

/// <summary>
/// Tests for CustomFormatterDelegate via options.
/// </summary>
public class CustomFormatterTests
{
    #region Options Configuration Tests

    [Fact]
    public void CustomFormatter_ConfiguredViaOptions_IsUsed()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["money"] = (value, style, locale, culture) => $"${value}";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "price", 100 } };

        var result = formatter.FormatMessage("{price, money}", args);

        Assert.Equal("$100", result);
    }

    [Fact]
    public void CustomFormatter_CaseInsensitive_ViaOptions()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["Money"] = (value, style, locale, culture) => $"${value}";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "price", 100 } };

        // Formatter name should match case-insensitively
        var result = formatter.FormatMessage("{price, money}", args);

        Assert.Equal("$100", result);
    }

    [Fact]
    public void CustomFormatter_MultipleFormatters_ViaOptions()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["upper"] = (value, style, locale, culture) =>
            value?.ToString()?.ToUpper() ?? "";
        options.CustomFormatters["lower"] = (value, style, locale, culture) =>
            value?.ToString()?.ToLower() ?? "";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "a", "Hello" }, { "b", "WORLD" } };

        var result = formatter.FormatMessage("{a, upper} {b, lower}", args);

        Assert.Equal("HELLO world", result);
    }

    #endregion

    #region Basic Formatting Tests

    [Fact]
    public void CustomFormatter_SimpleValue()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["uppercase"] = (value, style, locale, culture) =>
            value?.ToString()?.ToUpper() ?? "";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "name", "John" } };

        var result = formatter.FormatMessage("{name, uppercase}", args);

        Assert.Equal("JOHN", result);
    }

    [Fact]
    public void CustomFormatter_WithStyle()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["money"] = (value, style, locale, culture) =>
        {
            var amount = Convert.ToDecimal(value);
            return style switch
            {
                "USD" => amount.ToString("C", CultureInfo.GetCultureInfo("en-US")),
                "EUR" => amount.ToString("C", CultureInfo.GetCultureInfo("de-DE")),
                _ => amount.ToString("C", culture)
            };
        };
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "price", 99.99m } };

        var result = formatter.FormatMessage("{price, money, USD}", args);

        Assert.Equal("$99.99", result);
    }

    [Fact]
    public void CustomFormatter_StyleWithSpaces()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["case"] = (value, style, locale, culture) =>
        {
            var text = value?.ToString() ?? "";
            return style?.Trim() switch
            {
                "upper" => text.ToUpper(),
                "lower" => text.ToLower(),
                "title" => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text),
                _ => text
            };
        };
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "text", "hello" } };

        var result = formatter.FormatMessage("{text, case, upper}", args);

        Assert.Equal("HELLO", result);
    }

    [Fact]
    public void CustomFormatter_NullValue()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["safe"] = (value, style, locale, culture) =>
            value?.ToString() ?? "(null)";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "value", null } };

        var result = formatter.FormatMessage("{value, safe}", args);

        Assert.Equal("(null)", result);
    }

    [Fact]
    public void CustomFormatter_NumericValue()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["double"] = (value, style, locale, culture) =>
            (Convert.ToInt32(value) * 2).ToString();
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "n", 42 } };

        var result = formatter.FormatMessage("{n, double}", args);

        Assert.Equal("84", result);
    }

    [Fact]
    public void CustomFormatter_DateValue()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["isodate"] = (value, style, locale, culture) =>
            ((DateTime)value!).ToString("yyyy-MM-dd");
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "d", new DateTime(2024, 3, 15) } };

        var result = formatter.FormatMessage("{d, isodate}", args);

        Assert.Equal("2024-03-15", result);
    }

    [Fact]
    public void CustomFormatter_ReceivesCorrectLocale()
    {
        var options = TestOptions.WithEnglish();
        string? capturedLocale = null;
        options.CustomFormatters["capture"] = (value, style, locale, culture) =>
        {
            capturedLocale = locale;
            return "done";
        };
        var formatter = new MessageFormatter("de-DE", options);
        var args = new Dictionary<string, object?> { { "n", 1 } };

        formatter.FormatMessage("{n, capture}", args);

        Assert.Equal("de-DE", capturedLocale);
    }

    [Fact]
    public void CustomFormatter_ReceivesCorrectCulture()
    {
        var options = TestOptions.WithEnglish();
        CultureInfo? capturedCulture = null;
        options.CustomFormatters["capture"] = (value, style, locale, culture) =>
        {
            capturedCulture = culture;
            return "done";
        };
        var formatter = new MessageFormatter("de-DE", options);
        var args = new Dictionary<string, object?> { { "n", 1 } };

        formatter.FormatMessage("{n, capture}", args);

        Assert.NotNull(capturedCulture);
        Assert.Equal("de-DE", capturedCulture!.Name);
    }

    [Fact]
    public void CustomFormatter_ReceivesStyle()
    {
        var options = TestOptions.WithEnglish();
        string? capturedStyle = null;
        options.CustomFormatters["capture"] = (value, style, locale, culture) =>
        {
            capturedStyle = style;
            return "done";
        };
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "n", 1 } };

        formatter.FormatMessage("{n, capture, myStyle}", args);

        Assert.Equal("myStyle", capturedStyle);
    }

    [Fact]
    public void CustomFormatter_NoStyle_StyleIsNull()
    {
        var options = TestOptions.WithEnglish();
        string? capturedStyle = "not-null";
        options.CustomFormatters["capture"] = (value, style, locale, culture) =>
        {
            capturedStyle = style;
            return "done";
        };
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "n", 1 } };

        formatter.FormatMessage("{n, capture}", args);

        Assert.Null(capturedStyle);
    }

    #endregion

    #region Fallback Behavior Tests

    [Fact]
    public void CustomFormatter_NotRegistered_FallbackToString()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "value", 42 } };

        // No formatter registered for "unknown"
        var result = formatter.FormatMessage("{value, unknown}", args);

        Assert.Equal("42", result);
    }

    [Fact]
    public void CustomFormatter_NotRegistered_NullValue_EmptyOutput()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "value", null } };

        // No formatter registered for "unknown"
        var result = formatter.FormatMessage("{value, unknown}", args);

        Assert.Equal("", result);
    }

    #endregion

    #region Multiple Custom Formatters Tests

    [Fact]
    public void MultipleCustomFormatters_SameMessage()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["upper"] = (value, style, locale, culture) =>
            value?.ToString()?.ToUpper() ?? "";
        options.CustomFormatters["money"] = (value, style, locale, culture) =>
            "$" + Convert.ToDecimal(value).ToString("F2", CultureInfo.InvariantCulture);
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>
        {
            { "name", "john" },
            { "price", 99.99m }
        };

        var result = formatter.FormatMessage("Hello {name, upper}, total: {price, money}", args);

        Assert.Equal("Hello JOHN, total: $99.99", result);
    }

    [Fact]
    public void CustomFormatter_UsedMultipleTimes()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["upper"] = (value, style, locale, culture) =>
            value?.ToString()?.ToUpper() ?? "";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>
        {
            { "a", "hello" },
            { "b", "world" }
        };

        var result = formatter.FormatMessage("{a, upper} {b, upper}", args);

        Assert.Equal("HELLO WORLD", result);
    }

    #endregion

    #region Complex Scenarios Tests

    [Fact]
    public void CustomFormatter_InsidePlural()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["upper"] = (value, style, locale, culture) =>
            value?.ToString()?.ToUpper() ?? "";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>
        {
            { "count", 5 },
            { "name", "john" }
        };

        var result = formatter.FormatMessage(
            "{count, plural, one {{name, upper} has # item} other {{name, upper} has # items}}",
            args);

        Assert.Equal("JOHN has 5 items", result);
    }

    [Fact]
    public void CustomFormatter_InsideSelect()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["capitalize"] = (value, style, locale, culture) =>
        {
            var s = value?.ToString() ?? "";
            return s.Length > 0 ? char.ToUpper(s[0]) + s.Substring(1) : s;
        };
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>
        {
            { "gender", "female" },
            { "name", "jane" }
        };

        var result = formatter.FormatMessage(
            "{gender, select, male {{name, capitalize} is a man} female {{name, capitalize} is a woman} other {{name, capitalize} is a person}}",
            args);

        Assert.Equal("Jane is a woman", result);
    }

    [Fact]
    public void CustomFormatter_WithMixedBuiltInFormatters()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["upper"] = (value, style, locale, culture) =>
            value?.ToString()?.ToUpper() ?? "";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>
        {
            { "name", "john" },
            { "amount", 1234.56 },
            { "date", new DateTime(2024, 3, 15) }
        };

        var result = formatter.FormatMessage(
            "User: {name, upper}, Amount: {amount, number}, Date: {date, date, short}",
            args);

        Assert.Contains("JOHN", result);
        Assert.Contains("1,234.56", result);
    }

    [Fact]
    public void CustomFormatter_CustomDurationFormatter()
    {
        var options = TestOptions.WithEnglish();
        // Use "customDuration" instead of "duration" since duration is now a built-in formatter
        options.CustomFormatters["customDuration"] = (value, style, locale, culture) =>
        {
            var totalSeconds = Convert.ToInt32(value);
            var hours = totalSeconds / 3600;
            var minutes = (totalSeconds % 3600) / 60;
            var secs = totalSeconds % 60;

            return style switch
            {
                "short" => $"{hours}:{minutes:D2}:{secs:D2}",
                "long" => $"{hours} hours, {minutes} minutes, {secs} seconds",
                _ => $"{hours}h {minutes}m {secs}s"
            };
        };
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "seconds", 3661 } }; // 1h 1m 1s

        var shortResult = formatter.FormatMessage("{seconds, customDuration, short}", args);
        var longResult = formatter.FormatMessage("{seconds, customDuration, long}", args);
        var defaultResult = formatter.FormatMessage("{seconds, customDuration}", args);

        Assert.Equal("1:01:01", shortResult);
        Assert.Equal("1 hours, 1 minutes, 1 seconds", longResult);
        Assert.Equal("1h 1m 1s", defaultResult);
    }

    [Fact]
    public void CustomFormatter_FileSizeFormatter()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["filesize"] = (value, style, locale, culture) =>
        {
            var bytes = Convert.ToInt64(value);
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return size.ToString("0.##", CultureInfo.InvariantCulture) + " " + sizes[order];
        };
        var formatter = new MessageFormatter("en", options);

        var args1 = new Dictionary<string, object?> { { "size", 1024 } };
        var args2 = new Dictionary<string, object?> { { "size", 1536 } };
        var args3 = new Dictionary<string, object?> { { "size", 1048576 } };

        Assert.Equal("1 KB", formatter.FormatMessage("{size, filesize}", args1));
        Assert.Equal("1.5 KB", formatter.FormatMessage("{size, filesize}", args2));
        Assert.Equal("1 MB", formatter.FormatMessage("{size, filesize}", args3));
    }

    [Fact]
    public void CustomFormatter_RelativeTimeFormatter()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["reltime"] = (value, style, locale, culture) =>
        {
            var minutes = Convert.ToInt32(value);
            if (minutes == 0) return "just now";
            if (minutes < 0) return $"{-minutes} minutes ago";
            return $"in {minutes} minutes";
        };
        var formatter = new MessageFormatter("en", options);

        var args1 = new Dictionary<string, object?> { { "m", 0 } };
        var args2 = new Dictionary<string, object?> { { "m", -5 } };
        var args3 = new Dictionary<string, object?> { { "m", 10 } };

        Assert.Equal("just now", formatter.FormatMessage("{m, reltime}", args1));
        Assert.Equal("5 minutes ago", formatter.FormatMessage("{m, reltime}", args2));
        Assert.Equal("in 10 minutes", formatter.FormatMessage("{m, reltime}", args3));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void CustomFormatter_ReturnsEmptyString()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["empty"] = (value, style, locale, culture) => "";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "n", 1 } };

        var result = formatter.FormatMessage("before{n, empty}after", args);

        Assert.Equal("beforeafter", result);
    }

    [Fact]
    public void CustomFormatter_ReturnsSpecialCharacters()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["special"] = (value, style, locale, culture) => "<>&\"";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "n", 1 } };

        var result = formatter.FormatMessage("{n, special}", args);

        Assert.Equal("<>&\"", result);
    }

    [Fact]
    public void CustomFormatter_ReturnsNewlines()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["multiline"] = (value, style, locale, culture) => "line1\nline2";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "n", 1 } };

        var result = formatter.FormatMessage("{n, multiline}", args);

        Assert.Equal("line1\nline2", result);
    }

    [Fact]
    public void CustomFormatter_ThrowsException_Propagates()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["throws"] = (value, style, locale, culture) =>
            throw new InvalidOperationException("Test exception");
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "n", 1 } };

        Assert.Throws<InvalidOperationException>(() =>
            formatter.FormatMessage("{n, throws}", args));
    }

    #endregion
}
