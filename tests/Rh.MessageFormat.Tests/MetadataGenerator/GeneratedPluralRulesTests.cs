using System.Collections.Generic;
using Xunit;

namespace Rh.MessageFormat.Tests.MetadataGenerator;

/// <summary>
/// Tests for the generated plural rules.
/// </summary>
public class GeneratedPluralRulesTests
{
    [Theory]
    [InlineData(0, "days")]
    [InlineData(1, "day")]
    [InlineData(101, "days")]
    [InlineData(102, "days")]
    [InlineData(105, "days")]
    public void En_PluralizerTests(double n, string expected)
    {
        var formatter = new MessageFormatter("en");
        var pattern = "{test, plural, one {day} other {days}}";
        var args = new Dictionary<string, object?> { { "test", n } };

        var result = formatter.FormatMessage(pattern, args);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, "Tage")]
    [InlineData(1, "Tag")]
    [InlineData(101, "Tage")]
    public void De_PluralizerTests(double n, string expected)
    {
        var formatter = new MessageFormatter("de");
        var pattern = "{test, plural, one {Tag} other {Tage}}";
        var args = new Dictionary<string, object?> { { "test", n } };

        var result = formatter.FormatMessage(pattern, args);

        Assert.Equal(expected, result);
    }
}
