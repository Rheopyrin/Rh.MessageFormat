namespace Rh.MessageFormat.Options;

/// <summary>
/// Options for configuring the parsed message cache.
/// </summary>
public sealed class ParserCacheOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the parser cache is enabled.
    /// Default is <c>true</c>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of parsed messages to cache.
    /// Default is 1024.
    /// </summary>
    public int MaxCapacity { get; set; } = 1024;
}
