using Rh.MessageFormat.CldrGenerator.Generators;
using Rh.MessageFormat.CldrGenerator.Tests.Integration.Fixtures;
using Rh.MessageFormat.CldrGenerator.Tests.Integration.Helpers;

namespace Rh.MessageFormat.CldrGenerator.Tests.Integration.Tests;

/// <summary>
/// Tests for date pattern generation.
/// </summary>
[Collection("Integration")]
public class DatePatternGenerationTests : IDisposable
{
    private readonly CldrTestFixture _fixture;
    private readonly TestDirectoryManager _directoryManager;
    private readonly GeneratedCodeValidator? _validator;

    public DatePatternGenerationTests(CldrTestFixture fixture)
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
    [InlineData("en")]
    [InlineData("de")]
    [InlineData("ar")]
    public void DatePatterns_AreNotDefault(string locale)
    {
        Assert.NotNull(_validator);
        var patterns = _validator.TestGetDatePatterns(locale);

        Assert.NotNull(patterns);
        Assert.NotNull(patterns.Value.Date.Full);
        Assert.NotNull(patterns.Value.Time.Full);
        Assert.NotNull(patterns.Value.DateTime.Full);
    }

    [Fact]
    public void English_DatePatterns_HaveCorrectFormat()
    {
        Assert.NotNull(_validator);
        var patterns = _validator.TestGetDatePatterns("en");

        Assert.NotNull(patterns);

        // Date formats - converted to .NET format
        Assert.NotEmpty(patterns.Value.Date.Full);
        Assert.NotEmpty(patterns.Value.Date.Long);
        Assert.NotEmpty(patterns.Value.Date.Medium);
        Assert.NotEmpty(patterns.Value.Date.Short);

        // Time formats - converted to .NET format
        Assert.Contains(":", patterns.Value.Time.Short);

        // DateTime combination patterns
        Assert.Contains("{0}", patterns.Value.DateTime.Full);
        Assert.Contains("{1}", patterns.Value.DateTime.Full);
    }

    [Fact]
    public void German_DatePatterns_UseDotSeparator()
    {
        Assert.NotNull(_validator);
        var patterns = _validator.TestGetDatePatterns("de");

        Assert.NotNull(patterns);

        // German uses dots in date format (dd.MM.y)
        var shortDate = patterns.Value.Date.Short;
        Assert.NotEmpty(shortDate);
        // The converted format should have dots
        Assert.Contains(".", shortDate);
    }

    [Fact]
    public void Arabic_DatePatterns_Exist()
    {
        Assert.NotNull(_validator);
        var patterns = _validator.TestGetDatePatterns("ar");

        Assert.NotNull(patterns);

        // Arabic patterns should exist
        Assert.NotEmpty(patterns.Value.Date.Full);
        Assert.NotEmpty(patterns.Value.Time.Full);
    }

    [Fact]
    public void DateTimeFormats_ContainPlaceholders()
    {
        Assert.NotNull(_validator);

        foreach (var locale in CldrTestFixture.TestLocales)
        {
            var patterns = _validator.TestGetDatePatterns(locale);
            Assert.NotNull(patterns);

            // DateTime formats should contain {0} for time and {1} for date
            Assert.Contains("{0}", patterns.Value.DateTime.Full);
            Assert.Contains("{1}", patterns.Value.DateTime.Full);
        }
    }

    [Fact]
    public void GeneratedCode_ContainsDatePatternData()
    {
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);
        generator.GenerateAsync(_fixture.TestDataPath, outputDir).GetAwaiter().GetResult();

        var enFilePath = Path.Combine(outputDir, "CldrLocaleData_en.g.cs");
        var content = File.ReadAllText(enFilePath);

        Assert.Contains("DatePatternData", content);
        Assert.Contains("DateFormats", content);
        Assert.Contains("TimeFormats", content);
        Assert.Contains("DateTimeFormats", content);
    }

    public void Dispose()
    {
        _directoryManager.Dispose();
        _fixture.ResetLocaleFilter();
    }
}
