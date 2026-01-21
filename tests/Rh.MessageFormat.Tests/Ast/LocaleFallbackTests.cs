using System.Collections.Generic;
using Rh.MessageFormat.Exceptions;
using Rh.MessageFormat.Options;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Tests for locale fallback behavior.
/// </summary>
public class LocaleFallbackTests
{
    #region Basic Fallback Tests

    [Fact]
    public void Locale_ExactMatch_UsesExactLocale()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithCommonLocales());
        var args = new Dictionary<string, object?> { { "n", 1 } };

        var result = formatter.FormatMessage("{n, plural, one {item} other {items}}", args);

        Assert.Equal("item", result);
    }

    [Fact]
    public void Locale_RegionalVariant_FallsBackToBase()
    {
        // Provider only has "en", not "en-US"
        var formatter = new MessageFormatter("en-US", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 1 } };

        var result = formatter.FormatMessage("{n, plural, one {item} other {items}}", args);

        Assert.Equal("item", result);
    }

    [Fact]
    public void Locale_UnderscoreSeparator_FallsBackToBase()
    {
        var formatter = new MessageFormatter("en_US", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 1 } };

        var result = formatter.FormatMessage("{n, plural, one {item} other {items}}", args);

        Assert.Equal("item", result);
    }

    [Fact]
    public void Locale_Unknown_FallsBackToDefault()
    {
        var formatter = new MessageFormatter("xx", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 1 } };

        // "xx" is not available, should fallback to "en"
        var result = formatter.FormatMessage("{n, plural, one {item} other {items}}", args);

        Assert.Equal("item", result);
    }

    [Fact]
    public void Locale_UnknownRegionalVariant_FallsBackToDefault()
    {
        var formatter = new MessageFormatter("xx-YY", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "n", 1 } };

        // "xx-YY" base "xx" is not available, should fallback to "en"
        var result = formatter.FormatMessage("{n, plural, one {item} other {items}}", args);

        Assert.Equal("item", result);
    }

    #endregion

    #region Custom Fallback Locale Tests

    [Fact]
    public void Locale_CustomFallback_UsesCustomFallback()
    {
        var options = TestOptions.WithCommonLocales("de-DE");
        var formatter = new MessageFormatter("xx", options);
        var args = new Dictionary<string, object?> { { "n", 1 } };

        // Unknown locale "xx" should fallback to "de-DE" for plural rules
        var result = formatter.FormatMessage("{n, plural, one {item} other {items}}", args);

        // German plural rules: 1 is "one"
        Assert.Equal("item", result);
    }

    [Fact]
    public void Locale_NumberFormattingUsesRequestedLocale()
    {
        var formatter = new MessageFormatter("de-DE", TestOptions.WithCommonLocales());
        var args = new Dictionary<string, object?> { { "n", 1000.5 } };

        // Number formatting uses the CultureInfo from the requested locale
        var result = formatter.FormatMessage("{n, number}", args);

        // German number formatting
        Assert.Contains("1.000,5", result);
    }

    #endregion

    #region Locale Chain Tests

    [Fact]
    public void Locale_FallbackChain_ExactFirst()
    {
        // Setup provider with both "en" and "en-GB"
        var provider = new MockCldrDataProvider()
            .WithLocale(MockCldrLocaleData.CreateEnglish())
            .WithLocale(new MockCldrLocaleData { Locale = "en-GB" }
                .WithPluralRule(ctx => ctx.I == 1 && ctx.V == 0 ? "one" : "other"));

        var options = new MessageFormatterOptions
        {
            CldrDataProvider = provider,
            DefaultFallbackLocale = "en"
        };

        var formatter = new MessageFormatter("en-GB", options);
        var args = new Dictionary<string, object?> { { "n", 1 } };

        // Should use "en-GB" directly, not fall back to "en"
        var result = formatter.FormatMessage("{n, plural, one {GB item} other {GB items}}", args);

        Assert.Equal("GB item", result);
    }

    [Fact]
    public void Locale_FallbackChain_BaseSecond()
    {
        var provider = new MockCldrDataProvider()
            .WithLocale(MockCldrLocaleData.CreateEnglish());

        var options = new MessageFormatterOptions
        {
            CldrDataProvider = provider,
            DefaultFallbackLocale = "en"
        };

        var formatter = new MessageFormatter("en-AU", options);
        var args = new Dictionary<string, object?> { { "n", 1 } };

        // "en-AU" not available, should fall back to "en"
        var result = formatter.FormatMessage("{n, plural, one {item} other {items}}", args);

        Assert.Equal("item", result);
    }

    #endregion

    #region No Locale Data Available Tests

    [Fact]
    public void Locale_NoDataAvailable_ThrowsInvalidLocaleException()
    {
        var provider = MockCldrDataProvider.CreateEmpty();
        var options = new MessageFormatterOptions
        {
            CldrDataProvider = provider,
            DefaultFallbackLocale = "en"
        };

        // No locale data at all, should throw exception
        var exception = Assert.Throws<InvalidLocaleException>(() => new MessageFormatter("en", options));

        Assert.Contains("en", exception.Message);
    }

    #endregion

    #region Locale Affects Culture Tests

    [Fact]
    public void Locale_AffectsNumberFormatting()
    {
        var options = TestOptions.WithCommonLocales();
        var formatterEn = new MessageFormatter("en", options);
        var formatterDe = new MessageFormatter("de-DE", options);
        var args = new Dictionary<string, object?> { { "n", 1234.56 } };

        var resultEn = formatterEn.FormatMessage("{n, number}", args);
        var resultDe = formatterDe.FormatMessage("{n, number}", args);

        // English uses comma for thousands, period for decimal
        Assert.Contains("1,234.56", resultEn);

        // German uses period for thousands, comma for decimal
        Assert.Contains("1.234,56", resultDe);
    }

    [Fact]
    public void Locale_AffectsDateFormatting()
    {
        var options = TestOptions.WithCommonLocales();
        var formatterEn = new MessageFormatter("en", options);
        var formatterDe = new MessageFormatter("de-DE", options);
        var date = new System.DateTime(2026, 6, 15);
        var args = new Dictionary<string, object?> { { "d", date } };

        var resultEn = formatterEn.FormatMessage("{d, date, short}", args);
        var resultDe = formatterDe.FormatMessage("{d, date, short}", args);

        // English: M/d/yyyy
        Assert.Contains("6", resultEn);
        Assert.Contains("15", resultEn);

        // German: dd.MM.yy
        Assert.Contains("15", resultDe);
        Assert.Contains("06", resultDe);
    }

    #endregion
}
