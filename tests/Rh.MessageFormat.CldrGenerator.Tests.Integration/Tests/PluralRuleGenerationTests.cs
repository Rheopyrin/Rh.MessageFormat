using System.Globalization;
using Rh.MessageFormat.CldrGenerator.Generators;
using Rh.MessageFormat.CldrGenerator.Tests.Integration.Fixtures;
using Rh.MessageFormat.CldrGenerator.Tests.Integration.Helpers;

namespace Rh.MessageFormat.CldrGenerator.Tests.Integration.Tests;

/// <summary>
/// Tests for plural rule generation and execution.
/// </summary>
[Collection("Integration")]
public class PluralRuleGenerationTests : IDisposable
{
    private readonly CldrTestFixture _fixture;
    private readonly TestDirectoryManager _directoryManager;
    private readonly GeneratedCodeValidator? _validator;

    public PluralRuleGenerationTests(CldrTestFixture fixture)
    {
        _fixture = fixture;
        _directoryManager = new TestDirectoryManager();
        _fixture.InitializeLocaleFilter();

        // Generate and compile once for all tests
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

    // English plural rules: "one" for i=1, v=0; "other" for everything else
    [Theory]
    [InlineData(0, "other")]
    [InlineData(1, "one")]
    [InlineData(2, "other")]
    [InlineData(5, "other")]
    [InlineData(10, "other")]
    [InlineData(100, "other")]
    public void English_GetPluralCategory_ReturnsCorrectCategory(int number, string expectedCategory)
    {
        Assert.NotNull(_validator);
        var result = _validator.TestPluralCategory("en", number);
        Assert.Equal(expectedCategory, result);
    }

    // German plural rules: same as English - "one" for i=1, v=0
    [Theory]
    [InlineData(0, "other")]
    [InlineData(1, "one")]
    [InlineData(2, "other")]
    [InlineData(5, "other")]
    public void German_GetPluralCategory_ReturnsCorrectCategory(int number, string expectedCategory)
    {
        Assert.NotNull(_validator);
        var result = _validator.TestPluralCategory("de", number);
        Assert.Equal(expectedCategory, result);
    }

    // Arabic has complex plural rules with 6 categories
    [Theory]
    [InlineData(0, "zero")]
    [InlineData(1, "one")]
    [InlineData(2, "two")]
    [InlineData(3, "few")]      // n % 100 = 3..10
    [InlineData(5, "few")]
    [InlineData(10, "few")]
    [InlineData(11, "many")]    // n % 100 = 11..99
    [InlineData(25, "many")]
    [InlineData(99, "many")]
    [InlineData(100, "other")]  // 100 % 100 = 0, not in any range
    [InlineData(101, "other")]
    [InlineData(102, "other")]
    [InlineData(103, "few")]    // 103 % 100 = 3
    [InlineData(111, "many")]   // 111 % 100 = 11
    public void Arabic_GetPluralCategory_ReturnsCorrectCategory(int number, string expectedCategory)
    {
        Assert.NotNull(_validator);
        var result = _validator.TestPluralCategory("ar", number);
        Assert.Equal(expectedCategory, result);
    }

    // Test with decimal values - "one" requires v=0 (no decimal places)
    [Theory]
    [InlineData("1.0", "other")]   // v > 0
    [InlineData("1.5", "other")]
    [InlineData("2.0", "other")]
    public void English_GetPluralCategory_WithDecimal_ReturnsOther(string numberStr, string expectedCategory)
    {
        Assert.NotNull(_validator);
        var number = decimal.Parse(numberStr, CultureInfo.InvariantCulture);
        var result = _validator.TestPluralCategory("en", number);
        Assert.Equal(expectedCategory, result);
    }

    [Fact]
    public void GeneratedCode_ContainsExpectedPluralConditions_ForEnglish()
    {
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);
        generator.GenerateAsync(_fixture.TestDataPath, outputDir).GetAwaiter().GetResult();

        var enFilePath = Path.Combine(outputDir, "CldrLocaleData_en.g.cs");
        var content = File.ReadAllText(enFilePath);

        // English "one" rule: i = 1 and v = 0
        Assert.Contains("context.I", content);
        Assert.Contains("context.V", content);
    }

    [Fact]
    public void GeneratedCode_ContainsExpectedPluralConditions_ForArabic()
    {
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);
        generator.GenerateAsync(_fixture.TestDataPath, outputDir).GetAwaiter().GetResult();

        var arFilePath = Path.Combine(outputDir, "CldrLocaleData_ar.g.cs");
        var content = File.ReadAllText(arFilePath);

        // Arabic has modulo expressions for few and many
        Assert.Contains("context.N", content);

        // Should contain return statements for all 6 categories
        Assert.Contains("return \"zero\"", content);
        Assert.Contains("return \"one\"", content);
        Assert.Contains("return \"two\"", content);
        Assert.Contains("return \"few\"", content);
        Assert.Contains("return \"many\"", content);
        Assert.Contains("return \"other\"", content);
    }

    public void Dispose()
    {
        _directoryManager.Dispose();
        _fixture.ResetLocaleFilter();
    }
}
