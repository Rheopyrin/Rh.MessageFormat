using System;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;

namespace Rh.MessageFormat.Tests;

/// <summary>
/// Tests for CultureInfoCache functionality.
/// </summary>
public class CultureInfoCacheTests
{
    #region Basic Functionality Tests

    [Fact]
    public void GetCulture_ValidLocale_ReturnsCultureInfo()
    {
        var cache = new CultureInfoCache();

        var culture = cache.GetCulture("en-US");

        Assert.NotNull(culture);
        Assert.Equal("en-US", culture.Name);
    }

    [Fact]
    public void GetCulture_InvalidLocale_ReturnsValidCulture()
    {
        var cache = new CultureInfoCache();

        var culture = cache.GetCulture("invalid-locale-xyz");

        // .NET creates cultures for any string
        Assert.NotNull(culture);
    }

    [Fact]
    public void GetCulture_EmptyString_ReturnsValidCulture()
    {
        var cache = new CultureInfoCache();

        var culture = cache.GetCulture("");

        // Empty string creates a valid CultureInfo (InvariantCulture-like behavior)
        Assert.NotNull(culture);
    }

    [Fact]
    public void GetCulture_SameLocaleTwice_ReturnsCachedInstance()
    {
        var cache = new CultureInfoCache();

        var culture1 = cache.GetCulture("en-US");
        var culture2 = cache.GetCulture("en-US");

        Assert.Same(culture1, culture2);
    }

    [Fact]
    public void GetCulture_DifferentCasing_ReturnsCachedInstance()
    {
        var cache = new CultureInfoCache();

        var culture1 = cache.GetCulture("en-us");
        var culture2 = cache.GetCulture("EN-US");
        var culture3 = cache.GetCulture("En-Us");

        // Should return the same cached instance (case-insensitive)
        Assert.Same(culture1, culture2);
        Assert.Same(culture2, culture3);
    }

    #endregion

    #region Various Locale Formats

    [Theory]
    [InlineData("en")]
    [InlineData("en-US")]
    [InlineData("en-GB")]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    [InlineData("ja-JP")]
    [InlineData("zh-CN")]
    public void GetCulture_VariousValidLocales_ReturnsCorrectCulture(string locale)
    {
        var cache = new CultureInfoCache();

        var culture = cache.GetCulture(locale);

        Assert.NotNull(culture);
        Assert.Equal(locale, culture.Name, ignoreCase: true);
    }

    [Theory]
    [InlineData("xx")]
    [InlineData("xx-YY")]
    [InlineData("not-a-locale")]
    public void GetCulture_InvalidLocales_ReturnsValidCulture(string locale)
    {
        var cache = new CultureInfoCache();

        var culture = cache.GetCulture(locale);

        // .NET creates a CultureInfo for any locale string, just returns a valid instance
        Assert.NotNull(culture);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task GetCulture_ConcurrentAccess_ThreadSafe()
    {
        var cache = new CultureInfoCache();
        var locales = new[] { "en-US", "de-DE", "fr-FR", "ja-JP", "zh-CN" };
        var tasks = new Task[100];

        for (int i = 0; i < tasks.Length; i++)
        {
            var locale = locales[i % locales.Length];
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    var culture = cache.GetCulture(locale);
                    Assert.NotNull(culture);
                }
            });
        }

        await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task GetCulture_ConcurrentAccessSameLocale_ReturnsSameInstance()
    {
        var cache = new CultureInfoCache();
        CultureInfo? firstCulture = null;
        var tasks = new Task[50];

        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                var culture = cache.GetCulture("en-US");

                if (firstCulture == null)
                {
                    firstCulture = culture;
                }
                else
                {
                    Assert.Same(firstCulture, culture);
                }
            });
        }

        await Task.WhenAll(tasks);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void CultureInfoCache_UsedByMessageFormatter_WorksCorrectly()
    {
        var cache = new CultureInfoCache();
        var options = Mocks.TestOptions.WithEnglish();
        options.CultureInfoCache = cache;

        var formatterEn = new MessageFormatter("en-US", options);
        var formatterDe = new MessageFormatter("de-DE", options);
        var args = new System.Collections.Generic.Dictionary<string, object?> { { "n", 1234.56 } };

        // Format with different locales
        var resultEn = formatterEn.FormatMessage("{n, number}", args);
        var resultDe = formatterDe.FormatMessage("{n, number}", args);

        Assert.Contains("1,234.56", resultEn);
        Assert.Contains("1.234,56", resultDe);
    }

    #endregion
}
