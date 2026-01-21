using System;
using System.Collections.Generic;
using System.Globalization;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Tests for custom formatter support.
/// </summary>
public class CustomFormatterTests
{
    #region Options Configuration Tests

    [Fact]
    public void CustomFormatter_ConfiguredViaOptions_IsUsed()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["money"] = (value, style, locale, culture) => "$" + value;
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "price", 100 } };

        var result = formatter.FormatMessage("{price, money}", args);

        Assert.Equal("$100", result);
    }

    [Fact]
    public void CustomFormatter_CaseInsensitive()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["Money"] = (value, style, locale, culture) => "$" + value;
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "price", 100 } };

        // All cases should work
        Assert.Equal("$100", formatter.FormatMessage("{price, money}", args));
        Assert.Equal("$100", formatter.FormatMessage("{price, MONEY}", args));
        Assert.Equal("$100", formatter.FormatMessage("{price, Money}", args));
    }

    #endregion

    #region Formatting Tests

    [Fact]
    public void CustomFormatter_SimpleUsage()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["uppercase"] = (value, style, locale, culture) =>
            value?.ToString()?.ToUpper() ?? "";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "name", "world" } };

        var result = formatter.FormatMessage("Hello, {name, uppercase}!", args);

        Assert.Equal("Hello, WORLD!", result);
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
                "GBP" => amount.ToString("C", CultureInfo.GetCultureInfo("en-GB")),
                _ => amount.ToString("C", culture)
            };
        };
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "price", 99.99m } };

        Assert.Equal("The price is $99.99", formatter.FormatMessage("The price is {price, money, USD}", args));
    }

    [Fact]
    public void CustomFormatter_ReceivesLocale()
    {
        var options = TestOptions.WithFrench();
        string? receivedLocale = null;
        options.CustomFormatters["test"] = (value, style, locale, culture) =>
        {
            receivedLocale = locale;
            return value?.ToString() ?? "";
        };
        var formatter = new MessageFormatter("fr-FR", options);
        var args = new Dictionary<string, object?> { { "val", "test" } };

        formatter.FormatMessage("{val, test}", args);

        Assert.Equal("fr-FR", receivedLocale);
    }

    [Fact]
    public void CustomFormatter_ReceivesCulture()
    {
        var options = TestOptions.WithGerman();
        CultureInfo? receivedCulture = null;
        options.CustomFormatters["test"] = (value, style, locale, culture) =>
        {
            receivedCulture = culture;
            return value?.ToString() ?? "";
        };
        var formatter = new MessageFormatter("de-DE", options);
        var args = new Dictionary<string, object?> { { "val", "test" } };

        formatter.FormatMessage("{val, test}", args);

        Assert.NotNull(receivedCulture);
        Assert.Equal("de-DE", receivedCulture.Name);
    }

    [Fact]
    public void CustomFormatter_NullValue_HandledGracefully()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["safe"] = (value, style, locale, culture) =>
            value?.ToString() ?? "(null)";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "val", null } };

        var result = formatter.FormatMessage("Value: {val, safe}", args);

        Assert.Equal("Value: (null)", result);
    }

    [Fact]
    public void CustomFormatter_UnregisteredFormatter_FallsBackToValue()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "val", "test" } };

        var result = formatter.FormatMessage("{val, unknown}", args);

        Assert.Equal("test", result);
    }

    [Fact]
    public void CustomFormatter_UnregisteredFormatter_NullValue_OutputsNothing()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "val", null } };

        var result = formatter.FormatMessage("Value: {val, unknown}", args);

        Assert.Equal("Value: ", result);
    }

    [Fact]
    public void CustomFormatter_MultipleFormatters()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["upper"] = (value, style, locale, culture) =>
            value?.ToString()?.ToUpper() ?? "";
        options.CustomFormatters["lower"] = (value, style, locale, culture) =>
            value?.ToString()?.ToLower() ?? "";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "a", "Hello" }, { "b", "World" } };

        var result = formatter.FormatMessage("{a, upper} {b, lower}!", args);

        Assert.Equal("HELLO world!", result);
    }

    [Fact]
    public void CustomFormatter_WithSpacesInStyle()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["format"] = (value, style, locale, culture) =>
            $"[{style}]";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "val", "x" } };

        var result = formatter.FormatMessage("{val, format, some style}", args);

        Assert.Equal("[some style]", result);
    }

    #endregion

    #region Real-World Use Cases

    [Fact]
    public void CustomFormatter_DurationFormatter()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["duration"] = (value, style, locale, culture) =>
        {
            var seconds = Convert.ToInt32(value);
            var ts = TimeSpan.FromSeconds(seconds);

            return style switch
            {
                "short" => $"{ts.Hours}:{ts.Minutes:D2}:{ts.Seconds:D2}",
                "long" => $"{ts.Hours} hours, {ts.Minutes} minutes, {ts.Seconds} seconds",
                _ => ts.ToString()
            };
        };
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "time", 3661 } };

        Assert.Equal("Duration: 1:01:01", formatter.FormatMessage("Duration: {time, duration, short}", args));
        Assert.Equal("Duration: 1 hours, 1 minutes, 1 seconds", formatter.FormatMessage("Duration: {time, duration, long}", args));
    }

    [Fact]
    public void CustomFormatter_RelativeTimeFormatter()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["reltime"] = (value, style, locale, culture) =>
        {
            var minutes = Convert.ToInt32(value);

            return minutes switch
            {
                0 => "just now",
                1 => "1 minute ago",
                < 60 => $"{minutes} minutes ago",
                < 120 => "1 hour ago",
                _ => $"{minutes / 60} hours ago"
            };
        };
        var formatter = new MessageFormatter("en", options);

        var args5 = new Dictionary<string, object?> { { "mins", 5 } };
        var args90 = new Dictionary<string, object?> { { "mins", 90 } };

        Assert.Equal("Posted 5 minutes ago", formatter.FormatMessage("Posted {mins, reltime}", args5));
        Assert.Equal("Posted 1 hour ago", formatter.FormatMessage("Posted {mins, reltime}", args90));
    }

    [Fact]
    public void CustomFormatter_FileSize()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["filesize"] = (value, style, locale, culture) =>
        {
            var bytes = Convert.ToInt64(value);

            return bytes switch
            {
                < 1024 => $"{bytes} B",
                < 1024 * 1024 => (bytes / 1024.0).ToString("F1", CultureInfo.InvariantCulture) + " KB",
                < 1024 * 1024 * 1024 => (bytes / (1024.0 * 1024)).ToString("F1", CultureInfo.InvariantCulture) + " MB",
                _ => (bytes / (1024.0 * 1024 * 1024)).ToString("F1", CultureInfo.InvariantCulture) + " GB"
            };
        };
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "size", 1536000 } };

        var result = formatter.FormatMessage("File size: {size, filesize}", args);

        Assert.Equal("File size: 1.5 MB", result);
    }

    #endregion
}
