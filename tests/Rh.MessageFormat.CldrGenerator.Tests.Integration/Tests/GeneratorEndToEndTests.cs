using Rh.MessageFormat.CldrGenerator.Generators;
using Rh.MessageFormat.CldrGenerator.Tests.Integration.Fixtures;
using Rh.MessageFormat.CldrGenerator.Tests.Integration.Helpers;

namespace Rh.MessageFormat.CldrGenerator.Tests.Integration.Tests;

/// <summary>
/// End-to-end tests for the complete generator pipeline.
/// </summary>
[Collection("Integration")]
public class GeneratorEndToEndTests : IDisposable
{
    private readonly CldrTestFixture _fixture;
    private readonly TestDirectoryManager _directoryManager;

    public GeneratorEndToEndTests(CldrTestFixture fixture)
    {
        _fixture = fixture;
        _directoryManager = new TestDirectoryManager();
        _fixture.InitializeLocaleFilter();
    }

    [Fact]
    public async Task GenerateAsync_CreatesExpectedFiles()
    {
        // Arrange
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);

        // Act
        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);

        // Assert - Check locale files exist
        foreach (var locale in CldrTestFixture.TestLocales)
        {
            var className = $"CldrLocaleData_{locale.Replace("-", "_")}";
            var fileName = $"{className}.g.cs";
            var filePath = Path.Combine(outputDir, fileName);

            Assert.True(File.Exists(filePath), $"Expected file {fileName} to exist");
        }

        // Assert - Check provider registry exists
        var registryPath = Path.Combine(outputDir, "CldrDataProvider.g.cs");
        Assert.True(File.Exists(registryPath), "Expected CldrDataProvider.g.cs to exist");
    }

    [Fact]
    public async Task GenerateAsync_GeneratedFilesHaveCorrectStructure()
    {
        // Arrange
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);

        // Act
        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);

        // Assert - Check file contents have expected structure
        var enFilePath = Path.Combine(outputDir, "CldrLocaleData_en.g.cs");
        var enContent = await File.ReadAllTextAsync(enFilePath);

        Assert.Contains("class CldrLocaleData_en", enContent);
        Assert.Contains("ICldrLocaleData", enContent);
        Assert.Contains("GetPluralCategory", enContent);
        Assert.Contains("GetOrdinalCategory", enContent);
        Assert.Contains("TryGetCurrency", enContent);
        Assert.Contains("TryGetUnit", enContent);
        Assert.Contains("DatePatterns", enContent);
        Assert.Contains("TryGetListPattern", enContent);
    }

    [Fact]
    public async Task GenerateAsync_ProviderRegistryContainsAllLocales()
    {
        // Arrange
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);

        // Act
        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);

        // Assert
        var registryPath = Path.Combine(outputDir, "CldrDataProvider.g.cs");
        var content = await File.ReadAllTextAsync(registryPath);

        foreach (var locale in CldrTestFixture.TestLocales)
        {
            Assert.Contains($"\"{locale}\"", content);
        }
    }

    [Fact]
    public async Task GenerateAsync_GeneratedCodeCompiles()
    {
        // Arrange
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);
        var compiler = new GeneratedCodeCompiler();

        // Act
        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);
        var result = compiler.CompileDirectory(outputDir);

        // Assert
        Assert.True(result.Success, $"Compilation failed: {string.Join(Environment.NewLine, result.Errors.Select(e => e.ToString()))}");
        Assert.NotNull(result.Assembly);
    }

    [Fact]
    public async Task GenerateAsync_GeneratedClassesImplementInterface()
    {
        // Arrange
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);
        var compiler = new GeneratedCodeCompiler();

        // Act
        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);
        var result = compiler.CompileDirectory(outputDir);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Assembly);

        var validator = new GeneratedCodeValidator(result.Assembly);

        foreach (var locale in CldrTestFixture.TestLocales)
        {
            Assert.True(validator.ValidateLocaleDataExists(locale), $"Locale {locale} should have a generated class");
        }
    }

    public void Dispose()
    {
        _directoryManager.Dispose();
        _fixture.ResetLocaleFilter();
    }
}
