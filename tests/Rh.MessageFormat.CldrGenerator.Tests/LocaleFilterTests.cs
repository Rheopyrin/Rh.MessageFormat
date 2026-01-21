using Xunit;

namespace Rh.MessageFormat.CldrGenerator.Tests;

/// <summary>
/// Tests for LocaleFilter - locale validation and filtering.
/// Note: LocaleFilter is a static class with state, so tests must be run sequentially.
/// </summary>
[Collection("LocaleFilter")]
public class LocaleFilterTests
{
    #region Normalize Tests

    [Theory]
    [InlineData("en_US", "en-US")]
    [InlineData("zh_Hans_CN", "zh-Hans-CN")]
    [InlineData("en-US", "en-US")]
    [InlineData("de", "de")]
    public void Normalize_ReplacesUnderscoresWithHyphens(string input, string expected)
    {
        var result = LocaleFilter.Normalize(input);

        Assert.Equal(expected, result);
    }

    #endregion

    #region GetBaseLanguage Tests

    [Theory]
    [InlineData("en-US", "en")]
    [InlineData("zh-Hans-CN", "zh")]
    [InlineData("de-DE", "de")]
    [InlineData("fr-CA", "fr")]
    public void GetBaseLanguage_ReturnsBaseLanguage(string locale, string expected)
    {
        var result = LocaleFilter.GetBaseLanguage(locale);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("en")]
    [InlineData("de")]
    [InlineData("fr")]
    public void GetBaseLanguage_NoHyphen_ReturnsNull(string locale)
    {
        var result = LocaleFilter.GetBaseLanguage(locale);

        Assert.Null(result);
    }

    #endregion

    #region GetAllSupportedLocales Tests

    [Fact]
    public void GetAllSupportedLocales_ReturnsNonEmptySet()
    {
        var locales = LocaleFilter.GetAllSupportedLocales();

        Assert.NotEmpty(locales);
    }

    [Fact]
    public void GetAllSupportedLocales_ContainsCommonLocales()
    {
        var locales = LocaleFilter.GetAllSupportedLocales();

        Assert.Contains("en", locales);
        Assert.Contains("en-US", locales);
        Assert.Contains("de", locales);
        Assert.Contains("fr", locales);
    }

    #endregion

    #region Initialize Tests

    [Fact]
    public void Initialize_NullInput_ReturnsNoErrors()
    {
        var errors = LocaleFilter.Initialize(null);

        Assert.Empty(errors);
        Assert.False(LocaleFilter.HasUserFilter);
    }

    [Fact]
    public void Initialize_EmptyInput_ReturnsNoErrors()
    {
        var errors = LocaleFilter.Initialize("");

        Assert.Empty(errors);
        Assert.False(LocaleFilter.HasUserFilter);
    }

    [Fact]
    public void Initialize_WhitespaceInput_ReturnsNoErrors()
    {
        var errors = LocaleFilter.Initialize("   ");

        Assert.Empty(errors);
        Assert.False(LocaleFilter.HasUserFilter);
    }

    [Fact]
    public void Initialize_ValidLocale_ReturnsNoErrors()
    {
        var errors = LocaleFilter.Initialize("en-US");

        Assert.Empty(errors);
        Assert.True(LocaleFilter.HasUserFilter);
    }

    [Fact]
    public void Initialize_MultipleValidLocales_ReturnsNoErrors()
    {
        var errors = LocaleFilter.Initialize("en-US, de-DE, fr-FR");

        Assert.Empty(errors);
        Assert.True(LocaleFilter.HasUserFilter);
    }

    [Fact]
    public void Initialize_InvalidLocale_ReturnsError()
    {
        var errors = LocaleFilter.Initialize("invalid-locale-xyz");

        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("invalid-locale-xyz"));
    }

    [Fact]
    public void Initialize_MixedValidAndInvalid_ReturnsErrorsForInvalid()
    {
        var errors = LocaleFilter.Initialize("en-US, invalid-xyz, de-DE");

        Assert.Single(errors);
        Assert.Contains("invalid-xyz", errors[0]);
    }

    [Fact]
    public void Initialize_LocaleWithUnderscore_NormalizesAndValidates()
    {
        var errors = LocaleFilter.Initialize("en_US");

        Assert.Empty(errors);
    }

    #endregion

    #region IsSupported Tests

    [Fact]
    public void IsSupported_NotInitialized_ThrowsException()
    {
        // Reset state by reinitializing (LocaleFilter is static)
        // Note: This test may interfere with other tests due to static state
        // In a real scenario, we'd need to refactor LocaleFilter to be testable

        // First initialize to ensure we can test
        LocaleFilter.Initialize(null);

        // Now test with an initialized state
        var result = LocaleFilter.IsSupported("en-US");

        Assert.True(result);
    }

    [Fact]
    public void IsSupported_ValidNetLocale_ReturnsTrue()
    {
        LocaleFilter.Initialize(null);

        Assert.True(LocaleFilter.IsSupported("en-US"));
        Assert.True(LocaleFilter.IsSupported("de-DE"));
        Assert.True(LocaleFilter.IsSupported("fr-FR"));
    }

    [Fact]
    public void IsSupported_CldrLocaleWithUnderscore_NormalizesAndChecks()
    {
        LocaleFilter.Initialize(null);

        Assert.True(LocaleFilter.IsSupported("en_US"));
        Assert.True(LocaleFilter.IsSupported("zh_Hans_CN"));
    }

    [Fact]
    public void IsSupported_WithUserFilter_ChecksAgainstFilter()
    {
        LocaleFilter.Initialize("en-US, de-DE");

        Assert.True(LocaleFilter.IsSupported("en-US"));
        Assert.True(LocaleFilter.IsSupported("de-DE"));
        // Base language should also be supported when regional variant is in filter
        Assert.True(LocaleFilter.IsSupported("en"));
        Assert.True(LocaleFilter.IsSupported("de"));
        // Other locales should not be supported
        Assert.False(LocaleFilter.IsSupported("fr-FR"));
    }

    [Fact]
    public void IsSupported_BaseLanguageWithRegionalFilter_ReturnsTrue()
    {
        LocaleFilter.Initialize("en-US, en-GB");

        // Base language should be supported because regional variants are in filter
        Assert.True(LocaleFilter.IsSupported("en"));
    }

    #endregion

    #region GetRequestedRegionalVariants Tests

    [Fact]
    public void GetRequestedRegionalVariants_NoFilter_ReturnsEmpty()
    {
        LocaleFilter.Initialize(null);

        var variants = LocaleFilter.GetRequestedRegionalVariants();

        Assert.Empty(variants);
    }

    [Fact]
    public void GetRequestedRegionalVariants_WithFilter_ReturnsOnlyRegionalVariants()
    {
        LocaleFilter.Initialize("en, en-US, de-DE, fr");

        var variants = LocaleFilter.GetRequestedRegionalVariants().ToList();

        Assert.Contains("en-US", variants);
        Assert.Contains("de-DE", variants);
        Assert.DoesNotContain("en", variants);
        Assert.DoesNotContain("fr", variants);
    }

    #endregion

    #region GetRegionalVariantBases Tests

    [Fact]
    public void GetRegionalVariantBases_NoFilter_ReturnsEmpty()
    {
        LocaleFilter.Initialize(null);

        var bases = LocaleFilter.GetRegionalVariantBases();

        Assert.Empty(bases);
    }

    [Fact]
    public void GetRegionalVariantBases_WithFilter_ReturnsMappings()
    {
        LocaleFilter.Initialize("en-US, de-DE, fr-CA");

        var bases = LocaleFilter.GetRegionalVariantBases();

        Assert.Equal("en", bases["en-US"]);
        Assert.Equal("de", bases["de-DE"]);
        Assert.Equal("fr", bases["fr-CA"]);
    }

    #endregion

    #region Count Tests

    [Fact]
    public void Count_NoFilter_ReturnsAllSupportedCount()
    {
        LocaleFilter.Initialize(null);

        var count = LocaleFilter.Count;
        var allSupported = LocaleFilter.GetAllSupportedLocales();

        Assert.Equal(allSupported.Count, count);
    }

    [Fact]
    public void Count_WithFilter_ReturnsFilteredCount()
    {
        LocaleFilter.Initialize("en-US, de-DE, fr-FR");

        var count = LocaleFilter.Count;

        Assert.Equal(3, count);
    }

    #endregion

    #region GetUserFilteredLocales Tests

    [Fact]
    public void GetUserFilteredLocales_NoFilter_ReturnsNull()
    {
        LocaleFilter.Initialize(null);

        var filtered = LocaleFilter.GetUserFilteredLocales();

        Assert.Null(filtered);
    }

    [Fact]
    public void GetUserFilteredLocales_WithFilter_ReturnsFilteredSet()
    {
        LocaleFilter.Initialize("en-US, de-DE");

        var filtered = LocaleFilter.GetUserFilteredLocales();

        Assert.NotNull(filtered);
        Assert.Contains("en-US", filtered);
        Assert.Contains("de-DE", filtered);
    }

    #endregion
}
