using Rh.MessageFormat.CldrGenerator.Generators;
using Rh.MessageFormat.CldrGenerator.Tests.Integration.Fixtures;

namespace Rh.MessageFormat.CldrGenerator.Tests.Integration.Tests;

/// <summary>
/// Tests for provider registry generation.
/// </summary>
[Collection("Integration")]
public class ProviderRegistryGenerationTests : IDisposable
{
    private readonly CldrTestFixture _fixture;
    private readonly TestDirectoryManager _directoryManager;

    public ProviderRegistryGenerationTests(CldrTestFixture fixture)
    {
        _fixture = fixture;
        _directoryManager = new TestDirectoryManager();
        _fixture.InitializeLocaleFilter();
    }

    [Fact]
    public async Task ProviderRegistry_IsGenerated()
    {
        // Arrange
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);

        // Act
        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);

        // Assert
        var registryPath = Path.Combine(outputDir, "CldrDataProvider.g.cs");
        Assert.True(File.Exists(registryPath));
    }

    [Fact]
    public async Task ProviderRegistry_ContainsAllLocales()
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
    public async Task ProviderRegistry_HasCorrectNamespace()
    {
        // Arrange
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);

        // Act
        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);

        // Assert
        var registryPath = Path.Combine(outputDir, "CldrDataProvider.g.cs");
        var content = await File.ReadAllTextAsync(registryPath);

        Assert.Contains("namespace Rh.MessageFormat.CldrData.Services", content);
    }

    [Fact]
    public async Task ProviderRegistry_UsesPartialClass()
    {
        // Arrange
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);

        // Act
        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);

        // Assert
        var registryPath = Path.Combine(outputDir, "CldrDataProvider.g.cs");
        var content = await File.ReadAllTextAsync(registryPath);

        Assert.Contains("partial class CldrDataProvider", content);
    }

    [Fact]
    public async Task ProviderRegistry_ContainsLocaleDictionary()
    {
        // Arrange
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);

        // Act
        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);

        // Assert
        var registryPath = Path.Combine(outputDir, "CldrDataProvider.g.cs");
        var content = await File.ReadAllTextAsync(registryPath);

        Assert.Contains("_locales", content);
        Assert.Contains("Dictionary<string, Func<ICldrLocaleData>>", content);
    }

    [Fact]
    public async Task ProviderRegistry_ReferencesGeneratedClasses()
    {
        // Arrange
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);

        // Act
        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);

        // Assert
        var registryPath = Path.Combine(outputDir, "CldrDataProvider.g.cs");
        var content = await File.ReadAllTextAsync(registryPath);

        // Should reference the generated locale classes
        foreach (var locale in CldrTestFixture.TestLocales)
        {
            var className = $"CldrLocaleData_{locale.Replace("-", "_")}";
            Assert.Contains(className, content);
        }
    }

    [Fact]
    public async Task ProviderRegistry_UsesLazyInstantiation()
    {
        // Arrange
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);

        // Act
        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);

        // Assert
        var registryPath = Path.Combine(outputDir, "CldrDataProvider.g.cs");
        var content = await File.ReadAllTextAsync(registryPath);

        // Dictionary uses Func<ICldrLocaleData> for lazy instantiation
        Assert.Contains(".Instance", content);
    }

    [Fact]
    public async Task ProviderRegistry_HasGeneratedComment()
    {
        // Arrange
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);

        // Act
        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);

        // Assert
        var registryPath = Path.Combine(outputDir, "CldrDataProvider.g.cs");
        var content = await File.ReadAllTextAsync(registryPath);

        Assert.Contains("<auto-generated />", content);
        Assert.Contains("Generated from CLDR data", content);
    }

    public void Dispose()
    {
        _directoryManager.Dispose();
        _fixture.ResetLocaleFilter();
    }
}
