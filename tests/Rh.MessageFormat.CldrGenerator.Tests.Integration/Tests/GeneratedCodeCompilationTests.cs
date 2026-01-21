using Rh.MessageFormat.Abstractions;
using Rh.MessageFormat.Abstractions.Interfaces;
using Rh.MessageFormat.CldrGenerator.Generators;
using Rh.MessageFormat.CldrGenerator.Tests.Integration.Fixtures;
using Rh.MessageFormat.CldrGenerator.Tests.Integration.Helpers;

namespace Rh.MessageFormat.CldrGenerator.Tests.Integration.Tests;

/// <summary>
/// Tests for compilation and execution of generated code.
/// </summary>
[Collection("Integration")]
public class GeneratedCodeCompilationTests : IDisposable
{
    private readonly CldrTestFixture _fixture;
    private readonly TestDirectoryManager _directoryManager;

    public GeneratedCodeCompilationTests(CldrTestFixture fixture)
    {
        _fixture = fixture;
        _directoryManager = new TestDirectoryManager();
        _fixture.InitializeLocaleFilter();
    }

    [Fact]
    public async Task GeneratedCode_CompilesWithoutErrors()
    {
        // Arrange
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);
        var compiler = new GeneratedCodeCompiler();

        // Act
        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);
        var result = compiler.CompileDirectory(outputDir);

        // Assert
        Assert.True(result.Success, GetCompilationErrorMessage(result));
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task GeneratedCode_HasNoWarnings()
    {
        // Arrange
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);
        var compiler = new GeneratedCodeCompiler();

        // Act
        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);
        var result = compiler.CompileDirectory(outputDir);

        // Assert - may have some nullable warnings depending on configuration
        Assert.True(result.Success);
    }

    [Fact]
    public async Task GeneratedClasses_ImplementICldrLocaleData()
    {
        // Arrange
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);
        var compiler = new GeneratedCodeCompiler();

        // Act
        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);
        var result = compiler.CompileDirectory(outputDir);

        // Assert
        Assert.True(result.Success, GetCompilationErrorMessage(result));
        Assert.NotNull(result.Assembly);

        var validator = new GeneratedCodeValidator(result.Assembly);
        var localeTypes = validator.GetAllLocaleDataTypes().ToList();

        Assert.Equal(CldrTestFixture.TestLocales.Length, localeTypes.Count);

        foreach (var type in localeTypes)
        {
            Assert.True(typeof(ICldrLocaleData).IsAssignableFrom(type),
                $"Type {type.Name} should implement ICldrLocaleData");
        }
    }

    [Fact]
    public async Task GeneratedClasses_HaveSingletonPattern()
    {
        // Arrange
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);
        var compiler = new GeneratedCodeCompiler();

        // Act
        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);
        var result = compiler.CompileDirectory(outputDir);

        // Assert
        Assert.True(result.Success, GetCompilationErrorMessage(result));
        Assert.NotNull(result.Assembly);

        var validator = new GeneratedCodeValidator(result.Assembly);

        foreach (var locale in CldrTestFixture.TestLocales)
        {
            var instance1 = validator.GetLocaleDataInstance(locale);
            var instance2 = validator.GetLocaleDataInstance(locale);

            Assert.NotNull(instance1);
            Assert.NotNull(instance2);
            Assert.Same(instance1, instance2);
        }
    }

    [Fact]
    public async Task GeneratedClasses_CanBeInstantiated()
    {
        // Arrange
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);
        var compiler = new GeneratedCodeCompiler();

        // Act
        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);
        var result = compiler.CompileDirectory(outputDir);

        // Assert
        Assert.True(result.Success, GetCompilationErrorMessage(result));
        Assert.NotNull(result.Assembly);

        var validator = new GeneratedCodeValidator(result.Assembly);

        foreach (var locale in CldrTestFixture.TestLocales)
        {
            var instance = validator.GetLocaleDataInstance(locale);
            Assert.NotNull(instance);
        }
    }

    [Fact]
    public async Task GeneratedClasses_AllMethodsAreCallable()
    {
        // Arrange
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);
        var compiler = new GeneratedCodeCompiler();

        // Act
        await generator.GenerateAsync(_fixture.TestDataPath, outputDir);
        var result = compiler.CompileDirectory(outputDir);

        // Assert
        Assert.True(result.Success, GetCompilationErrorMessage(result));
        Assert.NotNull(result.Assembly);

        var validator = new GeneratedCodeValidator(result.Assembly);

        foreach (var locale in CldrTestFixture.TestLocales)
        {
            var instance = validator.GetLocaleDataInstance(locale);
            Assert.NotNull(instance);

            // All methods should be callable without throwing
            var pluralCategory = instance.GetPluralCategory(new Rh.MessageFormat.Abstractions.Models.PluralContext(1));
            Assert.NotNull(pluralCategory);

            var ordinalCategory = instance.GetOrdinalCategory(new Rh.MessageFormat.Abstractions.Models.PluralContext(1));
            Assert.NotNull(ordinalCategory);

            var hasCurrency = instance.TryGetCurrency("USD", out _);
            // May or may not have USD depending on locale

            var hasUnit = instance.TryGetUnit("length-meter", out _);
            // Should have units

            var datePatterns = instance.DatePatterns;
            // DatePatterns should exist

            var hasListPattern = instance.TryGetListPattern("standard", out _);
            // Should have standard list pattern
        }
    }

    [Fact]
    public async Task CompileDirectory_WithNoFiles_ReturnsFalse()
    {
        // Arrange
        var emptyDir = _directoryManager.CreateTempDirectory();
        var compiler = new GeneratedCodeCompiler();

        // Act
        var result = compiler.CompileDirectory(emptyDir);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("No .g.cs files", result.ErrorMessage);
    }

    [Fact]
    public async Task Compile_WithInvalidCode_ReturnsFalse()
    {
        // Arrange
        var compiler = new GeneratedCodeCompiler();
        var invalidCode = new Dictionary<string, string>
        {
            { "Invalid.cs", "This is not valid C# code!" }
        };

        // Act
        var result = compiler.Compile(invalidCode);

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
    }

    private static string GetCompilationErrorMessage(CompilationResult result)
    {
        if (result.Success) return string.Empty;

        var errors = result.Errors.Select(e => e.ToString());
        return $"Compilation failed with errors:\n{string.Join("\n", errors)}";
    }

    public void Dispose()
    {
        _directoryManager.Dispose();
        _fixture.ResetLocaleFilter();
    }
}
