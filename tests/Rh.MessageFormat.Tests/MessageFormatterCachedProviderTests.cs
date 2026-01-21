using System;
using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests;

/// <summary>
/// Tests for MessageFormatterCachedProvider.
/// </summary>
public class MessageFormatterCachedProviderTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithOptions_CreatesProvider()
    {
        var options = TestOptions.WithEnglish();

        var provider = new MessageFormatterCachedProvider(options);

        Assert.NotNull(provider);
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MessageFormatterCachedProvider(null!));
    }

    [Fact]
    public void Constructor_WithLocalesAndOptions_CreatesProvider()
    {
        var locales = new[] { "en", "de-DE" };
        var options = TestOptions.WithCommonLocales();

        var provider = new MessageFormatterCachedProvider(locales, options);

        Assert.NotNull(provider);
    }

    [Fact]
    public void Constructor_WithNullLocales_ThrowsArgumentNullException()
    {
        var options = TestOptions.WithEnglish();

        Assert.Throws<ArgumentNullException>(() => new MessageFormatterCachedProvider(null!, options));
    }

    [Fact]
    public void Constructor_WithLocalesAndNullOptions_ThrowsArgumentNullException()
    {
        var locales = new[] { "en" };

        Assert.Throws<ArgumentNullException>(() => new MessageFormatterCachedProvider(locales, null!));
    }

    #endregion

    #region GetFormatter Tests

    [Fact]
    public void GetFormatter_ReturnsFormatterForLocale()
    {
        var options = TestOptions.WithEnglish();
        var provider = new MessageFormatterCachedProvider(options);

        var formatter = provider.GetFormatter("en");

        Assert.NotNull(formatter);
    }

    [Fact]
    public void GetFormatter_FormatterWorksCorrectly()
    {
        var options = TestOptions.WithEnglish();
        var provider = new MessageFormatterCachedProvider(options);
        var args = new Dictionary<string, object?> { { "name", "World" } };

        var formatter = provider.GetFormatter("en");
        var result = formatter.FormatMessage("Hello, {name}!", args);

        Assert.Equal("Hello, World!", result);
    }

    [Fact]
    public void GetFormatter_ReturnsSameInstanceOnSecondCall()
    {
        var options = TestOptions.WithEnglish();
        var provider = new MessageFormatterCachedProvider(options);

        var formatter1 = provider.GetFormatter("en");
        var formatter2 = provider.GetFormatter("en");

        Assert.Same(formatter1, formatter2);
    }

    [Fact]
    public void GetFormatter_CacheIsCaseInsensitive()
    {
        var options = TestOptions.WithEnglish();
        var provider = new MessageFormatterCachedProvider(options);

        var formatter1 = provider.GetFormatter("en-US");
        var formatter2 = provider.GetFormatter("EN-US");
        var formatter3 = provider.GetFormatter("En-Us");

        Assert.Same(formatter1, formatter2);
        Assert.Same(formatter2, formatter3);
    }

    [Fact]
    public void GetFormatter_ReturnsDifferentInstancesForDifferentLocales()
    {
        var options = TestOptions.WithCommonLocales();
        var provider = new MessageFormatterCachedProvider(options);

        var formatterEn = provider.GetFormatter("en");
        var formatterDe = provider.GetFormatter("de-DE");

        Assert.NotSame(formatterEn, formatterDe);
    }

    [Fact]
    public void GetFormatter_WithNullLocale_ThrowsArgumentNullException()
    {
        var options = TestOptions.WithEnglish();
        var provider = new MessageFormatterCachedProvider(options);

        Assert.Throws<ArgumentNullException>(() => provider.GetFormatter(null!));
    }

    [Fact]
    public void GetFormatter_WithEmptyLocale_ThrowsArgumentNullException()
    {
        var options = TestOptions.WithEnglish();
        var provider = new MessageFormatterCachedProvider(options);

        Assert.Throws<ArgumentNullException>(() => provider.GetFormatter(""));
    }

    [Fact]
    public void GetFormatter_UsesCorrectLocaleForFormatting()
    {
        var options = TestOptions.WithCommonLocales();
        var provider = new MessageFormatterCachedProvider(options);
        var args = new Dictionary<string, object?> { { "n", 1234.56 } };

        var formatterEn = provider.GetFormatter("en");
        var formatterDe = provider.GetFormatter("de-DE");

        var resultEn = formatterEn.FormatMessage("{n, number}", args);
        var resultDe = formatterDe.FormatMessage("{n, number}", args);

        Assert.Contains("1,234.56", resultEn);
        Assert.Contains("1.234,56", resultDe);
    }

    #endregion

    #region Initialize Tests

    [Fact]
    public void Initialize_WithLocales_PreFillsCache()
    {
        var locales = new[] { "en", "de-DE", "fr-FR" };
        var options = TestOptions.WithCommonLocales();
        var provider = new MessageFormatterCachedProvider(locales, options);

        provider.Initialize();

        // Getting formatters should return cached instances
        var formatterEn = provider.GetFormatter("en");
        var formatterDe = provider.GetFormatter("de-DE");
        var formatterFr = provider.GetFormatter("fr-FR");

        Assert.NotNull(formatterEn);
        Assert.NotNull(formatterDe);
        Assert.NotNull(formatterFr);
    }

    [Fact]
    public void Initialize_WithoutLocales_DoesNothing()
    {
        var options = TestOptions.WithEnglish();
        var provider = new MessageFormatterCachedProvider(options);

        // Should not throw
        provider.Initialize();
    }

    [Fact]
    public void Initialize_WithEmptyLocalesList_DoesNothing()
    {
        var locales = Array.Empty<string>();
        var options = TestOptions.WithEnglish();
        var provider = new MessageFormatterCachedProvider(locales, options);

        // Should not throw
        provider.Initialize();
    }

    [Fact]
    public void Initialize_SkipsNullOrEmptyLocales()
    {
        var locales = new[] { "en", null!, "", "de-DE" };
        var options = TestOptions.WithCommonLocales();
        var provider = new MessageFormatterCachedProvider(locales, options);

        // Should not throw
        provider.Initialize();

        // Valid locales should be cached
        var formatterEn = provider.GetFormatter("en");
        var formatterDe = provider.GetFormatter("de-DE");

        Assert.NotNull(formatterEn);
        Assert.NotNull(formatterDe);
    }

    [Fact]
    public void Initialize_CanBeCalledMultipleTimes()
    {
        var locales = new[] { "en" };
        var options = TestOptions.WithEnglish();
        var provider = new MessageFormatterCachedProvider(locales, options);

        provider.Initialize();
        var formatter1 = provider.GetFormatter("en");

        provider.Initialize();
        var formatter2 = provider.GetFormatter("en");

        // Should return same cached instance
        Assert.Same(formatter1, formatter2);
    }

    #endregion

    #region Plural Formatting Tests

    [Fact]
    public void GetFormatter_PluralFormattingWorksCorrectly()
    {
        var options = TestOptions.WithEnglish();
        var provider = new MessageFormatterCachedProvider(options);

        var formatter = provider.GetFormatter("en");
        var args = new Dictionary<string, object?> { { "count", 1 } };

        var result = formatter.FormatMessage("{count, plural, one {# item} other {# items}}", args);

        Assert.Equal("1 item", result);
    }

    [Fact]
    public void GetFormatter_PluralFormattingWithDifferentLocales()
    {
        var options = TestOptions.WithCommonLocales();
        var provider = new MessageFormatterCachedProvider(options);

        var formatterEn = provider.GetFormatter("en");
        var formatterDe = provider.GetFormatter("de-DE");
        var args = new Dictionary<string, object?> { { "n", 1 } };

        var resultEn = formatterEn.FormatMessage("{n, plural, one {item} other {items}}", args);
        var resultDe = formatterDe.FormatMessage("{n, plural, one {Artikel} other {Artikel}}", args);

        Assert.Equal("item", resultEn);
        Assert.Equal("Artikel", resultDe);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Provider_WorksWithCustomFormatters()
    {
        // Use unique locale to avoid cache conflicts with other tests
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["upper"] = (value, style, locale, culture) =>
            value?.ToString()?.ToUpper() ?? "";

        var provider = new MessageFormatterCachedProvider(options);
        var formatter = provider.GetFormatter("en-custom-fmt");
        var args = new Dictionary<string, object?> { { "text", "hello" } };

        var result = formatter.FormatMessage("{text, upper}", args);

        Assert.Equal("HELLO", result);
    }

    [Fact]
    public void Provider_WorksWithTagHandlers()
    {
        // Use unique locale to avoid cache conflicts with other tests
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";

        var provider = new MessageFormatterCachedProvider(options);
        var formatter = provider.GetFormatter("en-tag-hdl");
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("<bold>text</bold>", args);

        Assert.Equal("**text**", result);
    }

    #endregion
}
