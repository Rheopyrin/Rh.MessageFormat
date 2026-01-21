namespace Rh.MessageFormat;

/// <summary>
/// Delegate for handling rich text tags.
/// Takes the formatted inner content and returns the transformed result.
/// </summary>
/// <param name="content">The formatted content within the tag.</param>
/// <returns>The transformed content after applying the tag.</returns>
/// <example>
/// <code>
/// // Simple markdown-style bold handler
/// TagHandler boldHandler = content => $"**{content}**";
///
/// // HTML bold handler
/// TagHandler htmlBoldHandler = content => $"&lt;strong&gt;{content}&lt;/strong&gt;";
/// </code>
/// </example>
public delegate string TagHandler(string content);