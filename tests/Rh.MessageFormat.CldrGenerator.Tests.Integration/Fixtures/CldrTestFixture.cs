using Microsoft.Extensions.Configuration;

namespace Rh.MessageFormat.CldrGenerator.Tests.Integration.Fixtures;

/// <summary>
/// Shared test fixture providing configuration and test data paths.
/// </summary>
public sealed class CldrTestFixture : IDisposable
{
    public CldrConfig Config { get; }
    public string TestDataPath { get; }
    public TestDirectoryManager DirectoryManager { get; }

    /// <summary>
    /// Test locales used in integration tests.
    /// </summary>
    public static readonly string[] TestLocales = { "en", "de", "ar" };

    public CldrTestFixture()
    {
        // Load test configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.test.json", optional: false)
            .Build();

        Config = new CldrConfig();
        configuration.GetSection("Cldr").Bind(Config);

        // Get test data path
        TestDataPath = Path.Combine(AppContext.BaseDirectory, "TestData");

        DirectoryManager = new TestDirectoryManager();
    }

    /// <summary>
    /// Initializes the LocaleFilter for tests with the test locales.
    /// </summary>
    public void InitializeLocaleFilter()
    {
        var localesInput = string.Join(",", TestLocales);
        LocaleFilter.Initialize(localesInput);
    }

    /// <summary>
    /// Resets the LocaleFilter for subsequent tests.
    /// This uses reflection to reset the static state.
    /// </summary>
    public void ResetLocaleFilter()
    {
        // Reset by initializing with null (no filter)
        LocaleFilter.Initialize(null);
    }

    public void Dispose()
    {
        DirectoryManager.Dispose();
        ResetLocaleFilter();
    }
}

/// <summary>
/// Collection definition for integration tests requiring shared fixture.
/// </summary>
[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<CldrTestFixture>
{
}
