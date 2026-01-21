using Rh.MessageFormat.CldrGenerator.Generators;
using Rh.MessageFormat.CldrGenerator.Tests.Integration.Fixtures;
using Rh.MessageFormat.CldrGenerator.Tests.Integration.Helpers;

namespace Rh.MessageFormat.CldrGenerator.Tests.Integration.Tests;

/// <summary>
/// Tests for list pattern generation.
/// </summary>
[Collection("Integration")]
public class ListPatternGenerationTests : IDisposable
{
    private readonly CldrTestFixture _fixture;
    private readonly TestDirectoryManager _directoryManager;
    private readonly GeneratedCodeValidator? _validator;

    public ListPatternGenerationTests(CldrTestFixture fixture)
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
    [InlineData("en", "standard")]
    [InlineData("en", "or")]
    [InlineData("de", "standard")]
    [InlineData("de", "or")]
    [InlineData("ar", "standard")]
    [InlineData("ar", "or")]
    public void TryGetListPattern_ReturnsTrue_ForExpectedTypes(string locale, string type)
    {
        Assert.NotNull(_validator);
        var pattern = _validator.TestGetListPattern(locale, type);
        Assert.NotNull(pattern);
    }

    [Theory]
    [InlineData("en", "unknown")]
    [InlineData("de", "unit-narrow")]
    public void TryGetListPattern_ReturnsFalse_ForUnknownTypes(string locale, string type)
    {
        Assert.NotNull(_validator);
        var pattern = _validator.TestGetListPattern(locale, type);
        Assert.Null(pattern);
    }

    [Fact]
    public void English_StandardListPattern_HasCorrectPatterns()
    {
        Assert.NotNull(_validator);
        var pattern = _validator.TestGetListPattern("en", "standard");

        Assert.NotNull(pattern);
        Assert.Equal("standard", pattern.Value.Type);
        Assert.Equal("{0}, {1}", pattern.Value.Start);
        Assert.Equal("{0}, {1}", pattern.Value.Middle);
        Assert.Equal("{0}, and {1}", pattern.Value.End);
        Assert.Equal("{0} and {1}", pattern.Value.Two);
    }

    [Fact]
    public void English_OrListPattern_HasCorrectPatterns()
    {
        Assert.NotNull(_validator);
        var pattern = _validator.TestGetListPattern("en", "or");

        Assert.NotNull(pattern);
        Assert.Equal("or", pattern.Value.Type);
        Assert.Equal("{0}, or {1}", pattern.Value.End);
        Assert.Equal("{0} or {1}", pattern.Value.Two);
    }

    [Fact]
    public void German_StandardListPattern_UsesUnd()
    {
        Assert.NotNull(_validator);
        var pattern = _validator.TestGetListPattern("de", "standard");

        Assert.NotNull(pattern);
        Assert.Contains("und", pattern.Value.End);
        Assert.Contains("und", pattern.Value.Two);
    }

    [Fact]
    public void German_OrListPattern_UsesOder()
    {
        Assert.NotNull(_validator);
        var pattern = _validator.TestGetListPattern("de", "or");

        Assert.NotNull(pattern);
        Assert.Contains("oder", pattern.Value.End);
        Assert.Contains("oder", pattern.Value.Two);
    }

    [Fact]
    public void Arabic_StandardListPattern_UsesArabicConjunction()
    {
        Assert.NotNull(_validator);
        var pattern = _validator.TestGetListPattern("ar", "standard");

        Assert.NotNull(pattern);
        // Arabic uses و (waw) for "and"
        Assert.Contains("و", pattern.Value.End);
    }

    [Fact]
    public void ListPatterns_ContainPlaceholders()
    {
        Assert.NotNull(_validator);

        foreach (var locale in CldrTestFixture.TestLocales)
        {
            var pattern = _validator.TestGetListPattern(locale, "standard");
            Assert.NotNull(pattern);

            Assert.Contains("{0}", pattern.Value.Start);
            Assert.Contains("{1}", pattern.Value.Start);
            Assert.Contains("{0}", pattern.Value.End);
            Assert.Contains("{1}", pattern.Value.End);
            Assert.Contains("{0}", pattern.Value.Two);
            Assert.Contains("{1}", pattern.Value.Two);
        }
    }

    [Fact]
    public void TryGetListPattern_IsCaseInsensitive()
    {
        Assert.NotNull(_validator);

        var lower = _validator.TestGetListPattern("en", "standard");
        var upper = _validator.TestGetListPattern("en", "STANDARD");
        var mixed = _validator.TestGetListPattern("en", "Standard");

        Assert.NotNull(lower);
        Assert.NotNull(upper);
        Assert.NotNull(mixed);
    }

    [Fact]
    public async Task GeneratedCode_ContainsListPatternDictionary()
    {
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);
        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);

        var enFilePath = Path.Combine(outputDir, "CldrLocaleData_en.g.cs");
        var content = File.ReadAllText(enFilePath);

        Assert.Contains("_listPatterns", content);
        Assert.Contains("ListPatternData", content);
        Assert.Contains("\"standard\"", content);
        Assert.Contains("\"or\"", content);
    }

    public void Dispose()
    {
        _directoryManager.Dispose();
        _fixture.ResetLocaleFilter();
    }
}
