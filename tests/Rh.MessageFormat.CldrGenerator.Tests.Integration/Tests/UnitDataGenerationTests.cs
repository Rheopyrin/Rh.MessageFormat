using Rh.MessageFormat.CldrGenerator.Generators;
using Rh.MessageFormat.CldrGenerator.Tests.Integration.Fixtures;
using Rh.MessageFormat.CldrGenerator.Tests.Integration.Helpers;

namespace Rh.MessageFormat.CldrGenerator.Tests.Integration.Tests;

/// <summary>
/// Tests for unit data generation.
/// </summary>
[Collection("Integration")]
public class UnitDataGenerationTests : IDisposable
{
    private readonly CldrTestFixture _fixture;
    private readonly TestDirectoryManager _directoryManager;
    private readonly GeneratedCodeValidator? _validator;

    public UnitDataGenerationTests(CldrTestFixture fixture)
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
    [InlineData("en", "length-meter")]
    [InlineData("en", "duration-hour")]
    [InlineData("de", "length-meter")]
    [InlineData("de", "duration-hour")]
    [InlineData("ar", "length-meter")]
    [InlineData("ar", "duration-hour")]
    public void TryGetUnit_ReturnsTrue_ForExpectedUnits(string locale, string unitId)
    {
        Assert.NotNull(_validator);
        var unit = _validator.TestGetUnit(locale, unitId);
        Assert.NotNull(unit);
    }

    [Theory]
    [InlineData("en", "unknown-unit")]
    [InlineData("de", "length-lightyear")]
    public void TryGetUnit_ReturnsFalse_ForUnknownUnits(string locale, string unitId)
    {
        Assert.NotNull(_validator);
        var unit = _validator.TestGetUnit(locale, unitId);
        Assert.Null(unit);
    }

    [Fact]
    public void English_LengthMeter_HasCorrectPatterns()
    {
        Assert.NotNull(_validator);
        var unit = _validator.TestGetUnit("en", "length-meter");

        Assert.NotNull(unit);
        Assert.Equal("length-meter", unit.Value.Id);
        Assert.NotNull(unit.Value.DisplayNames);

        // Check long patterns
        Assert.True(unit.Value.TryGetDisplayName("long", "one", out var longOne));
        Assert.Equal("{0} meter", longOne);

        Assert.True(unit.Value.TryGetDisplayName("long", "other", out var longOther));
        Assert.Equal("{0} meters", longOther);

        // Check short patterns
        Assert.True(unit.Value.TryGetDisplayName("short", "one", out var shortOne));
        Assert.Equal("{0} m", shortOne);
    }

    [Fact]
    public void German_DurationHour_HasCorrectPatterns()
    {
        Assert.NotNull(_validator);
        var unit = _validator.TestGetUnit("de", "duration-hour");

        Assert.NotNull(unit);
        Assert.Equal("duration-hour", unit.Value.Id);
        Assert.NotNull(unit.Value.DisplayNames);

        // German "Stunde" (singular) vs "Stunden" (plural)
        Assert.True(unit.Value.TryGetDisplayName("long", "one", out var longOne));
        Assert.Contains("Stunde", longOne);

        Assert.True(unit.Value.TryGetDisplayName("long", "other", out var longOther));
        Assert.Contains("Stunden", longOther);
    }

    [Fact]
    public void Arabic_LengthMeter_HasAllPluralCategories()
    {
        Assert.NotNull(_validator);
        var unit = _validator.TestGetUnit("ar", "length-meter");

        Assert.NotNull(unit);
        Assert.NotNull(unit.Value.DisplayNames);

        // Arabic has 6 plural categories for units
        Assert.True(unit.Value.TryGetDisplayName("long", "zero", out _));
        Assert.True(unit.Value.TryGetDisplayName("long", "one", out _));
        Assert.True(unit.Value.TryGetDisplayName("long", "two", out _));
        Assert.True(unit.Value.TryGetDisplayName("long", "few", out _));
        Assert.True(unit.Value.TryGetDisplayName("long", "many", out _));
        Assert.True(unit.Value.TryGetDisplayName("long", "other", out _));
    }

    [Fact]
    public void Units_HaveAllWidths()
    {
        Assert.NotNull(_validator);

        foreach (var locale in CldrTestFixture.TestLocales)
        {
            var unit = _validator.TestGetUnit(locale, "length-meter");
            Assert.NotNull(unit);
            Assert.NotNull(unit.Value.DisplayNames);

            // Should have long, short, and narrow patterns
            Assert.True(unit.Value.TryGetDisplayName("long", "other", out _),
                $"Locale {locale} should have long:other pattern");
            Assert.True(unit.Value.TryGetDisplayName("short", "other", out _),
                $"Locale {locale} should have short:other pattern");
            Assert.True(unit.Value.TryGetDisplayName("narrow", "other", out _),
                $"Locale {locale} should have narrow:other pattern");
        }
    }

    [Fact]
    public void UnitPatterns_ContainPlaceholder()
    {
        Assert.NotNull(_validator);

        foreach (var locale in CldrTestFixture.TestLocales)
        {
            var unit = _validator.TestGetUnit(locale, "length-meter");
            Assert.NotNull(unit);

            if (unit.Value.TryGetDisplayName("long", "other", out var pattern))
            {
                Assert.Contains("{0}", pattern);
            }
        }
    }

    [Fact]
    public void TryGetUnit_IsCaseInsensitive()
    {
        Assert.NotNull(_validator);

        var lower = _validator.TestGetUnit("en", "length-meter");
        var upper = _validator.TestGetUnit("en", "LENGTH-METER");
        var mixed = _validator.TestGetUnit("en", "Length-Meter");

        Assert.NotNull(lower);
        Assert.NotNull(upper);
        Assert.NotNull(mixed);
    }

    [Fact]
    public void GeneratedCode_ContainsUnitDictionary()
    {
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);
        generator.GenerateAsync(_fixture.TestDataPath, outputDir).GetAwaiter().GetResult();

        var enFilePath = Path.Combine(outputDir, "CldrLocaleData_en.g.cs");
        var content = File.ReadAllText(enFilePath);

        Assert.Contains("_unitDict", content);
        Assert.Contains("UnitData", content);
        Assert.Contains("length-meter", content);
        Assert.Contains("duration-hour", content);
    }

    public void Dispose()
    {
        _directoryManager.Dispose();
        _fixture.ResetLocaleFilter();
    }
}
