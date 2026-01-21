namespace Rh.MessageFormat.CldrGenerator.Tests.Integration.Fixtures;

/// <summary>
/// Manages temporary output directories for integration tests.
/// </summary>
public sealed class TestDirectoryManager : IDisposable
{
    private readonly List<string> _tempDirectories = new();
    private bool _disposed;

    /// <summary>
    /// Creates a new temporary directory for test output.
    /// </summary>
    /// <returns>The path to the created directory.</returns>
    public string CreateTempDirectory()
    {
        var tempPath = Path.Combine(
            Path.GetTempPath(),
            "CldrGeneratorTests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(tempPath);
        _tempDirectories.Add(tempPath);

        return tempPath;
    }

    /// <summary>
    /// Cleans up all created temporary directories.
    /// </summary>
    public void Cleanup()
    {
        foreach (var dir in _tempDirectories)
        {
            try
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, recursive: true);
                }
            }
            catch
            {
                // Ignore cleanup failures
            }
        }
        _tempDirectories.Clear();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Cleanup();
            _disposed = true;
        }
    }
}
