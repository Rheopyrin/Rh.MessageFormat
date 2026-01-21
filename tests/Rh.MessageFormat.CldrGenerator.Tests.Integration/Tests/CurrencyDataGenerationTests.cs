using Rh.MessageFormat.CldrGenerator.Generators;
using Rh.MessageFormat.CldrGenerator.Tests.Integration.Fixtures;
using Rh.MessageFormat.CldrGenerator.Tests.Integration.Helpers;

namespace Rh.MessageFormat.CldrGenerator.Tests.Integration.Tests;

/// <summary>
/// Tests for currency data generation and access.
/// </summary>
[Collection("Integration")]
public class CurrencyDataGenerationTests : IDisposable
{
    private readonly CldrTestFixture _fixture;
    private readonly TestDirectoryManager _directoryManager;
    private readonly GeneratedCodeValidator? _validator;

    public CurrencyDataGenerationTests(CldrTestFixture fixture)
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
    [InlineData("en", "USD")]
    [InlineData("en", "EUR")]
    [InlineData("en", "GBP")]
    [InlineData("de", "USD")]
    [InlineData("de", "EUR")]
    [InlineData("de", "CHF")]
    [InlineData("ar", "USD")]
    [InlineData("ar", "EUR")]
    [InlineData("ar", "SAR")]
    public void TryGetCurrency_ReturnsTrue_ForExpectedCurrencies(string locale, string currencyCode)
    {
        Assert.NotNull(_validator);
        var currency = _validator.TestGetCurrency(locale, currencyCode);
        Assert.NotNull(currency);
    }

    [Theory]
    [InlineData("en", "XYZ")]
    [InlineData("de", "GBP")]
    [InlineData("ar", "CHF")]
    public void TryGetCurrency_ReturnsFalse_ForUnknownCurrencies(string locale, string currencyCode)
    {
        Assert.NotNull(_validator);
        var currency = _validator.TestGetCurrency(locale, currencyCode);
        Assert.Null(currency);
    }

    [Fact]
    public void English_USD_HasCorrectData()
    {
        Assert.NotNull(_validator);
        var currency = _validator.TestGetCurrency("en", "USD");

        Assert.NotNull(currency);
        Assert.Equal("USD", currency.Value.Code);
        Assert.Equal("$", currency.Value.Symbol);
        Assert.Equal("$", currency.Value.NarrowSymbol);
        Assert.Equal("US Dollar", currency.Value.DisplayName);
        Assert.Equal("US dollar", currency.Value.DisplayNameOne);
        Assert.Equal("US dollars", currency.Value.DisplayNameOther);
    }

    [Fact]
    public void German_EUR_HasCorrectData()
    {
        Assert.NotNull(_validator);
        var currency = _validator.TestGetCurrency("de", "EUR");

        Assert.NotNull(currency);
        Assert.Equal("EUR", currency.Value.Code);
        Assert.Equal("€", currency.Value.Symbol);
        Assert.Equal("Euro", currency.Value.DisplayName);
    }

    [Fact]
    public void Arabic_SAR_HasCorrectData()
    {
        Assert.NotNull(_validator);
        var currency = _validator.TestGetCurrency("ar", "SAR");

        Assert.NotNull(currency);
        Assert.Equal("SAR", currency.Value.Code);
        // Arabic currency should have Arabic display name
        Assert.Contains("ريال", currency.Value.DisplayName);
    }

    [Fact]
    public void TryGetCurrency_IsCaseInsensitive()
    {
        Assert.NotNull(_validator);

        var upper = _validator.TestGetCurrency("en", "USD");
        var lower = _validator.TestGetCurrency("en", "usd");
        var mixed = _validator.TestGetCurrency("en", "Usd");

        Assert.NotNull(upper);
        Assert.NotNull(lower);
        Assert.NotNull(mixed);
        Assert.Equal(upper.Value.Code, lower.Value.Code);
        Assert.Equal(upper.Value.Code, mixed.Value.Code);
    }

    [Fact]
    public void GeneratedCode_ContainsCurrencyArray()
    {
        var outputDir = _directoryManager.CreateTempDirectory();
        var generator = new LocaleCodeGenerator(_fixture.Config);
        generator.GenerateAsync(_fixture.TestDataPath, outputDir).GetAwaiter().GetResult();

        var enFilePath = Path.Combine(outputDir, "CldrLocaleData_en.g.cs");
        var content = File.ReadAllText(enFilePath);

        Assert.Contains("_currencies", content);
        Assert.Contains("CurrencyData[]", content);
        Assert.Contains("\"USD\"", content);
        Assert.Contains("\"EUR\"", content);
        Assert.Contains("\"GBP\"", content);
    }

    public void Dispose()
    {
        _directoryManager.Dispose();
        _fixture.ResetLocaleFilter();
    }
}
