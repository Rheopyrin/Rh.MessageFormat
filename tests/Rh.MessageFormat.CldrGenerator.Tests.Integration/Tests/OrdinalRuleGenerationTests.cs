using Rh.MessageFormat.CldrGenerator.Generators;
using Rh.MessageFormat.CldrGenerator.Tests.Integration.Fixtures;
using Rh.MessageFormat.CldrGenerator.Tests.Integration.Helpers;

namespace Rh.MessageFormat.CldrGenerator.Tests.Integration.Tests;

/// <summary>
/// Tests for ordinal rule generation and execution.
/// </summary>
[Collection("Integration")]
public class OrdinalRuleGenerationTests : IDisposable
{
    private readonly CldrTestFixture _fixture;
    private readonly TestDirectoryManager _directoryManager;
    private readonly GeneratedCodeValidator? _validator;

    public OrdinalRuleGenerationTests(CldrTestFixture fixture)
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

    // English ordinals: 1st, 2nd, 3rd, 4th
    // "one" for n % 10 = 1 and n % 100 != 11
    // "two" for n % 10 = 2 and n % 100 != 12
    // "few" for n % 10 = 3 and n % 100 != 13
    // "other" for everything else
    [Theory]
    [InlineData(1, "one")]    // 1st
    [InlineData(2, "two")]    // 2nd
    [InlineData(3, "few")]    // 3rd
    [InlineData(4, "other")]  // 4th
    [InlineData(5, "other")]
    [InlineData(10, "other")]
    [InlineData(11, "other")] // 11th (exception)
    [InlineData(12, "other")] // 12th (exception)
    [InlineData(13, "other")] // 13th (exception)
    [InlineData(21, "one")]   // 21st
    [InlineData(22, "two")]   // 22nd
    [InlineData(23, "few")]   // 23rd
    [InlineData(100, "other")]
    [InlineData(101, "one")]  // 101st
    [InlineData(111, "other")] // 111th (exception)
    public void English_GetOrdinalCategory_ReturnsCorrectCategory(int number, string expectedCategory)
    {
        Assert.NotNull(_validator);
        var result = _validator.TestOrdinalCategory("en", number);
        Assert.Equal(expectedCategory, result);
    }

    // German ordinals: only "other" category
    [Theory]
    [InlineData(1, "other")]
    [InlineData(2, "other")]
    [InlineData(3, "other")]
    [InlineData(10, "other")]
    [InlineData(21, "other")]
    public void German_GetOrdinalCategory_AlwaysReturnsOther(int number, string expectedCategory)
    {
        Assert.NotNull(_validator);
        var result = _validator.TestOrdinalCategory("de", number);
        Assert.Equal(expectedCategory, result);
    }

    // Arabic ordinals: only "other" category
    [Theory]
    [InlineData(1, "other")]
    [InlineData(2, "other")]
    [InlineData(3, "other")]
    [InlineData(10, "other")]
    public void Arabic_GetOrdinalCategory_AlwaysReturnsOther(int number, string expectedCategory)
    {
        Assert.NotNull(_validator);
        var result = _validator.TestOrdinalCategory("ar", number);
        Assert.Equal(expectedCategory, result);
    }

    [Fact]
    public void GeneratedCode_ContainsExpectedOrdinalConditions_ForEnglish()
    {
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);
        generator.GenerateAsync(_fixture.TestDataPath, outputDir).GetAwaiter().GetResult();

        var enFilePath = Path.Combine(outputDir, "CldrLocaleData_en.g.cs");
        var content = File.ReadAllText(enFilePath);

        // English ordinal rules use modulo 10 and modulo 100
        // The generated code should contain checks for "one", "two", "few"
        Assert.Contains("GetOrdinalCategory", content);
    }

    [Fact]
    public void GeneratedCode_OrdinalMethod_ReturnsOther_ForGerman()
    {
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);
        generator.GenerateAsync(_fixture.TestDataPath, outputDir).GetAwaiter().GetResult();

        var deFilePath = Path.Combine(outputDir, "CldrLocaleData_de.g.cs");
        var content = File.ReadAllText(deFilePath);

        // German only has "other" ordinal, so GetOrdinalCategory should just return "other"
        Assert.Contains("GetOrdinalCategory", content);
        Assert.Contains("return \"other\"", content);
    }

    public void Dispose()
    {
        _directoryManager.Dispose();
        _fixture.ResetLocaleFilter();
    }
}
