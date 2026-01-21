using Rh.MessageFormat.CldrGenerator.Generators;
using Rh.MessageFormat.CldrGenerator.Tests.Integration.Fixtures;
using Rh.MessageFormat.CldrGenerator.Tests.Integration.Helpers;

namespace Rh.MessageFormat.CldrGenerator.Tests.Integration.Tests;

/// <summary>
/// Tests for relative time data generation.
/// </summary>
[Collection("Integration")]
public class RelativeTimeDataGenerationTests : IDisposable
{
    private readonly CldrTestFixture _fixture;
    private readonly TestDirectoryManager _directoryManager;
    private readonly GeneratedCodeValidator? _validator;

    public RelativeTimeDataGenerationTests(CldrTestFixture fixture)
    {
        _fixture = fixture;
        _directoryManager = new TestDirectoryManager();
        _fixture.InitializeLocaleFilter();

        _validator = GenerateAndCompile().GetAwaiter().GetResult();
    }

    private async Task<GeneratedCodeValidator?> GenerateAndCompile()
    {
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);
        var compiler = new GeneratedCodeCompiler();

        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);
        var result = compiler.CompileDirectory(outputDir);

        if (!result.Success || result.Assembly == null)
            return null;

        return new GeneratedCodeValidator(result.Assembly);
    }

    [Fact]
    public void Validator_IsInitialized()
    {
        Assert.NotNull(_validator);
    }

    [Theory]
    [InlineData("en", "day", "long")]
    [InlineData("en", "year", "long")]
    [InlineData("en", "month", "long")]
    [InlineData("en", "week", "long")]
    [InlineData("en", "hour", "long")]
    [InlineData("en", "minute", "long")]
    [InlineData("en", "second", "long")]
    [InlineData("de", "day", "long")]
    [InlineData("ar", "day", "long")]
    public void TryGetRelativeTime_ReturnsTrue_ForExpectedFields(string locale, string field, string width)
    {
        Assert.NotNull(_validator);
        var data = _validator.TestGetRelativeTime(locale, field, width);
        Assert.NotNull(data);
    }

    [Theory]
    [InlineData("en", "invalid-field", "long")]
    [InlineData("en", "day", "invalid-width")]
    public void TryGetRelativeTime_ReturnsFalse_ForInvalidInputs(string locale, string field, string width)
    {
        Assert.NotNull(_validator);
        var data = _validator.TestGetRelativeTime(locale, field, width);
        Assert.Null(data);
    }

    [Fact]
    public void English_Day_HasCorrectRelativeTypes()
    {
        Assert.NotNull(_validator);
        var data = _validator.TestGetRelativeTime("en", "day", "long");

        Assert.NotNull(data);
        Assert.Equal("day", data.Value.Field);

        // Check relative types (-1, 0, 1)
        Assert.True(data.Value.TryGetRelativeType(-1, out var yesterday));
        Assert.Equal("yesterday", yesterday);

        Assert.True(data.Value.TryGetRelativeType(0, out var today));
        Assert.Equal("today", today);

        Assert.True(data.Value.TryGetRelativeType(1, out var tomorrow));
        Assert.Equal("tomorrow", tomorrow);
    }

    [Fact]
    public void English_Day_HasFuturePatterns()
    {
        Assert.NotNull(_validator);
        var data = _validator.TestGetRelativeTime("en", "day", "long");

        Assert.NotNull(data);
        Assert.NotNull(data.Value.FuturePatterns);

        Assert.True(data.Value.TryGetFuturePattern("one", out var one));
        Assert.Equal("in {0} day", one);

        Assert.True(data.Value.TryGetFuturePattern("other", out var other));
        Assert.Equal("in {0} days", other);
    }

    [Fact]
    public void English_Day_HasPastPatterns()
    {
        Assert.NotNull(_validator);
        var data = _validator.TestGetRelativeTime("en", "day", "long");

        Assert.NotNull(data);
        Assert.NotNull(data.Value.PastPatterns);

        Assert.True(data.Value.TryGetPastPattern("one", out var one));
        Assert.Equal("{0} day ago", one);

        Assert.True(data.Value.TryGetPastPattern("other", out var other));
        Assert.Equal("{0} days ago", other);
    }

    [Fact]
    public void English_Year_HasRelativeTypes()
    {
        Assert.NotNull(_validator);
        var data = _validator.TestGetRelativeTime("en", "year", "long");

        Assert.NotNull(data);

        Assert.True(data.Value.TryGetRelativeType(-1, out var lastYear));
        Assert.Equal("last year", lastYear);

        Assert.True(data.Value.TryGetRelativeType(0, out var thisYear));
        Assert.Equal("this year", thisYear);

        Assert.True(data.Value.TryGetRelativeType(1, out var nextYear));
        Assert.Equal("next year", nextYear);
    }

    [Theory]
    [InlineData("long")]
    [InlineData("short")]
    [InlineData("narrow")]
    public void English_Day_HasAllWidths(string width)
    {
        Assert.NotNull(_validator);
        var data = _validator.TestGetRelativeTime("en", "day", width);
        Assert.NotNull(data);
    }

    [Fact]
    public void German_Day_HasCorrectRelativeTypes()
    {
        Assert.NotNull(_validator);
        var data = _validator.TestGetRelativeTime("de", "day", "long");

        Assert.NotNull(data);

        // German: "gestern", "heute", "morgen"
        Assert.True(data.Value.TryGetRelativeType(-1, out var yesterday));
        Assert.Equal("gestern", yesterday);

        Assert.True(data.Value.TryGetRelativeType(0, out var today));
        Assert.Equal("heute", today);

        Assert.True(data.Value.TryGetRelativeType(1, out var tomorrow));
        Assert.Equal("morgen", tomorrow);
    }

    [Fact]
    public void Arabic_Day_HasAllPluralCategories()
    {
        Assert.NotNull(_validator);
        var data = _validator.TestGetRelativeTime("ar", "day", "long");

        Assert.NotNull(data);
        Assert.NotNull(data.Value.FuturePatterns);

        // Arabic has 6 plural categories
        Assert.True(data.Value.TryGetFuturePattern("zero", out _));
        Assert.True(data.Value.TryGetFuturePattern("one", out _));
        Assert.True(data.Value.TryGetFuturePattern("two", out _));
        Assert.True(data.Value.TryGetFuturePattern("few", out _));
        Assert.True(data.Value.TryGetFuturePattern("many", out _));
        Assert.True(data.Value.TryGetFuturePattern("other", out _));
    }

    [Fact]
    public void TryGetRelativeTime_IsCaseInsensitive()
    {
        Assert.NotNull(_validator);

        var lower = _validator.TestGetRelativeTime("en", "day", "long");
        var upper = _validator.TestGetRelativeTime("en", "DAY", "LONG");
        var mixed = _validator.TestGetRelativeTime("en", "Day", "Long");

        Assert.NotNull(lower);
        Assert.NotNull(upper);
        Assert.NotNull(mixed);
    }

    [Fact]
    public async Task GeneratedCode_ContainsRelativeTimeData()
    {
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);
        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);

        var enFilePath = Path.Combine(outputDir, "CldrLocaleData_en.g.cs");
        var content = File.ReadAllText(enFilePath);

        Assert.Contains("_relativeTimeDict", content);
        Assert.Contains("RelativeTimeData", content);
        Assert.Contains("TryGetRelativeTime", content);
        Assert.Contains("yesterday", content);
        Assert.Contains("tomorrow", content);
    }

    [Fact]
    public void RelativeTimeData_FallsBackToOther()
    {
        Assert.NotNull(_validator);
        var data = _validator.TestGetRelativeTime("en", "day", "long");

        Assert.NotNull(data);

        // English doesn't have "few" category, should fallback to "other"
        Assert.True(data.Value.TryGetFuturePattern("few", out var pattern));
        Assert.Equal("in {0} days", pattern); // Falls back to "other"
    }

    public void Dispose()
    {
        _directoryManager.Dispose();
        _fixture.ResetLocaleFilter();
    }
}
