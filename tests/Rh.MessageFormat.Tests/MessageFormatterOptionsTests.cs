using System;
using System.Collections.Generic;
using System.Globalization;
using Rh.MessageFormat.Abstractions;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests;

/// <summary>
/// Tests for MessageFormatterOptions and MessageFormatter configuration.
/// </summary>
public class MessageFormatterOptionsTests
{
    #region Default Options Tests

    [Fact]
    public void Default_ReturnsNewInstance()
    {
        var options1 = MessageFormatterOptions.Default;
        var options2 = MessageFormatterOptions.Default;

        // Each call creates a new instance
        Assert.NotSame(options1, options2);
    }

    [Fact]
    public void Default_HasCldrDataProvider()
    {
        var options = MessageFormatterOptions.Default;

        Assert.NotNull(options.CldrDataProvider);
    }

    [Fact]
    public void Default_HasCultureInfoCache()
    {
        var options = MessageFormatterOptions.Default;

        Assert.NotNull(options.CultureInfoCache);
    }

    [Fact]
    public void Default_HasDefaultFallbackLocale()
    {
        var options = MessageFormatterOptions.Default;

        Assert.Equal("en", options.DefaultFallbackLocale);
    }

    [Fact]
    public void Default_HasEmptyCustomFormatters()
    {
        var options = MessageFormatterOptions.Default;

        Assert.NotNull(options.CustomFormatters);
        Assert.Empty(options.CustomFormatters);
    }

    [Fact]
    public void Default_HasEmptyTagHandlers()
    {
        var options = MessageFormatterOptions.Default;

        Assert.NotNull(options.TagHandlers);
        Assert.Empty(options.TagHandlers);
    }

    #endregion

    #region Custom Options Tests

    [Fact]
    public void Options_CanSetCldrDataProvider()
    {
        var mockProvider = MockCldrDataProvider.CreateWithEnglish();
        var options = new MessageFormatterOptions
        {
            CldrDataProvider = mockProvider
        };

        Assert.Same(mockProvider, options.CldrDataProvider);
    }

    [Fact]
    public void Options_CanSetCultureInfoCache()
    {
        var cache = new CultureInfoCache();
        var options = new MessageFormatterOptions
        {
            CultureInfoCache = cache
        };

        Assert.Same(cache, options.CultureInfoCache);
    }

    [Fact]
    public void Options_CanSetDefaultFallbackLocale()
    {
        var options = new MessageFormatterOptions
        {
            DefaultFallbackLocale = "de"
        };

        Assert.Equal("de", options.DefaultFallbackLocale);
    }

    [Fact]
    public void Options_CanSetCustomFormatters()
    {
        var formatters = new Dictionary<string, CustomFormatterDelegate>
        {
            ["custom"] = (v, s, l, c) => v?.ToString() ?? ""
        };

        var options = new MessageFormatterOptions
        {
            CustomFormatters = formatters
        };

        Assert.Same(formatters, options.CustomFormatters);
    }

    [Fact]
    public void Options_CanSetTagHandlers()
    {
        var handlers = new Dictionary<string, TagHandler>
        {
            ["bold"] = content => $"**{content}**"
        };

        var options = new MessageFormatterOptions
        {
            TagHandlers = handlers
        };

        Assert.Same(handlers, options.TagHandlers);
    }

    #endregion

    #region MessageFormatter with Options Tests

    [Fact]
    public void Formatter_DefaultConstructor_UsesDefaultOptions()
    {
        var formatter = new MessageFormatter("en");
        var args = new Dictionary<string, object?> { { "name", "World" } };

        var result = formatter.FormatMessage("Hello, {name}!", args);

        Assert.Equal("Hello, World!", result);
    }

    [Fact]
    public void Formatter_WithNullOptions_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => new MessageFormatter(null!));
    }

    [Fact]
    public void Formatter_WithCustomFallbackLocale_UsesIt()
    {
        var options = TestOptions.WithEnglish();
        options.DefaultFallbackLocale = "en";

        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "n", 1 } };

        var result = formatter.FormatMessage("{n, plural, one {item} other {items}}", args);

        Assert.Equal("item", result);
    }

    [Fact]
    public void Formatter_WithPreConfiguredFormatters_UsesThemInFormatting()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["upper"] = (v, s, l, c) => v?.ToString()?.ToUpper() ?? "";

        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "text", "hello" } };

        var result = formatter.FormatMessage("{text, upper}", args);

        Assert.Equal("HELLO", result);
    }

    [Fact]
    public void Formatter_WithPreConfiguredTagHandlers_UsesThemInFormatting()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";

        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("<bold>text</bold>", args);

        Assert.Equal("**text**", result);
    }

    #endregion

    #region Options Dictionary Configuration Tests

    [Fact]
    public void Options_CustomFormatters_CaseInsensitive()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["Upper"] = (v, s, l, c) => v?.ToString()?.ToUpper() ?? "";

        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "text", "hello" } };

        // All cases should work
        Assert.Equal("HELLO", formatter.FormatMessage("{text, upper}", args));
        Assert.Equal("HELLO", formatter.FormatMessage("{text, UPPER}", args));
        Assert.Equal("HELLO", formatter.FormatMessage("{text, Upper}", args));
    }

    [Fact]
    public void Options_TagHandlers_CaseInsensitive()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["Bold"] = content => $"**{content}**";

        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        // All cases should work
        Assert.Equal("**text**", formatter.FormatMessage("<bold>text</bold>", args));
        Assert.Equal("**text**", formatter.FormatMessage("<BOLD>text</BOLD>", args));
        Assert.Equal("**text**", formatter.FormatMessage("<Bold>text</Bold>", args));
    }

    [Fact]
    public void Options_CustomFormatters_CanHaveMultiple()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["upper"] = (v, s, l, c) => v?.ToString()?.ToUpper() ?? "";
        options.CustomFormatters["lower"] = (v, s, l, c) => v?.ToString()?.ToLower() ?? "";

        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "a", "Hello" }, { "b", "WORLD" } };

        var result = formatter.FormatMessage("{a, upper} {b, lower}", args);

        Assert.Equal("HELLO world", result);
    }

    [Fact]
    public void Options_TagHandlers_CanHaveMultiple()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        options.TagHandlers["italic"] = content => $"*{content}*";

        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("<bold>hello</bold> <italic>world</italic>", args);

        Assert.Equal("**hello** *world*", result);
    }

    #endregion

    #region Locale Override Tests

    [Fact]
    public void FormatMessage_WithLocale_UsesSpecifiedLocale()
    {
        var options = TestOptions.WithCommonLocales();
        var formatterEn = new MessageFormatter("en", options);
        var formatterDe = new MessageFormatter("de-DE", options);
        var args = new Dictionary<string, object?> { { "n", 1000.5 } };

        var resultEn = formatterEn.FormatMessage("{n, number}", args);
        var resultDe = formatterDe.FormatMessage("{n, number}", args);

        // Different thousand separators and decimal points
        Assert.Contains("1,000.5", resultEn);
        Assert.Contains("1.000,5", resultDe);
    }

    [Fact]
    public void FormatMessage_WithFallbackLocale_UsesFallback()
    {
        var options = TestOptions.WithEnglish();
        var formatter = new MessageFormatter("xx-YY", options);
        var args = new Dictionary<string, object?> { { "n", 1 } };

        // Unknown locale should fall back to "en"
        var result = formatter.FormatMessage("{n, plural, one {item} other {items}}", args);

        Assert.Equal("item", result);
    }

    #endregion
}
