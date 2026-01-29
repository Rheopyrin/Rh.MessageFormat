using System.Collections.Generic;
using Rh.MessageFormat.CldrData.Spellout;
using Xunit;

namespace Rh.MessageFormat.Tests;

/// <summary>
/// Tests for the spellout formatter (number to words conversion).
/// </summary>
public class SpelloutElementTests
{
    static SpelloutElementTests()
    {
        // Ensure spellout data provider is initialized
        // This triggers the ModuleInitializer if not already called
        SpelloutDataProvider.Initialize();
    }

    [Theory]
    [InlineData("en", 1, "one")]
    [InlineData("en", 2, "two")]
    [InlineData("en", 10, "ten")]
    [InlineData("en", 21, "twenty-one")]
    [InlineData("en", 100, "one hundred")]
    [InlineData("en", 123, "one hundred twenty-three")]
    [InlineData("en", 1000, "one thousand")]
    [InlineData("en", 1234, "one thousand two hundred thirty-four")]
    public void SpelloutCardinal_EnglishNumbers_FormatsCorrectly(string locale, int number, string expected)
    {
        var formatter = new MessageFormatter(locale);
        var args = new Dictionary<string, object?> { { "n", number } };
        var result = formatter.FormatMessage("{n, spellout}", args);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("en", 1, "first")]
    [InlineData("en", 2, "second")]
    [InlineData("en", 3, "third")]
    [InlineData("en", 4, "fourth")]
    [InlineData("en", 21, "twenty-first")]
    [InlineData("en", 100, "one hundredth")]
    public void SpelloutOrdinal_EnglishNumbers_FormatsCorrectly(string locale, int number, string expected)
    {
        var formatter = new MessageFormatter(locale);
        var args = new Dictionary<string, object?> { { "n", number } };
        var result = formatter.FormatMessage("{n, spellout, ordinal}", args);
        Assert.Equal(expected, result);
    }



    [Fact]
    public void Spellout_DefaultStyle_UsesCardinal()
    {
        var formatter = new MessageFormatter("en");
        var args = new Dictionary<string, object?> { { "n", 5 } };
        var result = formatter.FormatMessage("{n, spellout}", args);
        Assert.Equal("five", result);
    }

    [Fact]
    public void Spellout_CardinalStyle_Explicit()
    {
        var formatter = new MessageFormatter("en");
        var args = new Dictionary<string, object?> { { "n", 5 } };
        var result = formatter.FormatMessage("{n, spellout, cardinal}", args);
        Assert.Equal("five", result);
    }

    [Fact]
    public void Spellout_InMessage_FormatsCorrectly()
    {
        var formatter = new MessageFormatter("en");
        var args = new Dictionary<string, object?> { { "count", 3 } };
        var result = formatter.FormatMessage("You have {count, spellout} items", args);
        Assert.Equal("You have three items", result);
    }

    [Theory]
    [InlineData("en", 0, "zero")]
    public void Spellout_Zero_FormatsCorrectly(string locale, int number, string expected)
    {
        var formatter = new MessageFormatter(locale);
        var args = new Dictionary<string, object?> { { "n", number } };
        var result = formatter.FormatMessage("{n, spellout}", args);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("en-US", 42, "forty-two")]
    [InlineData("en-GB", 42, "forty-two")]
    public void Spellout_LocaleVariants_UsesBaseLanguage(string locale, int number, string expected)
    {
        var formatter = new MessageFormatter(locale);
        var args = new Dictionary<string, object?> { { "n", number } };
        var result = formatter.FormatMessage("{n, spellout}", args);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Spellout_LargeNumber_FormatsCorrectly()
    {
        var formatter = new MessageFormatter("en");
        var args = new Dictionary<string, object?> { { "n", 1000000 } };
        var result = formatter.FormatMessage("{n, spellout}", args);
        Assert.Equal("one million", result);
    }

    [Theory]
    [InlineData("en", 2000, "two thousand")]
    public void SpelloutYear_EnglishYears_FormatsCorrectly(string locale, int year, string expected)
    {
        var formatter = new MessageFormatter(locale);
        var args = new Dictionary<string, object?> { { "n", year } };
        var result = formatter.FormatMessage("{n, spellout, year}", args);
        Assert.Equal(expected, result);
    }
}
