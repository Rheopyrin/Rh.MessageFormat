using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Rh.MessageFormat.Formatting;

/// <summary>
/// Utility for HTML encoding and decoding values for safe HTML message formatting.
/// Used by FormatHtmlMessage to prevent XSS attacks.
/// </summary>
public static class HtmlEncoder
{
    /// <summary>
    /// HTML-encodes a string, converting special characters to their HTML entity equivalents.
    /// </summary>
    /// <param name="value">The string to encode.</param>
    /// <returns>The HTML-encoded string.</returns>
    public static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value
            .Replace("&", "&amp;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    /// <summary>
    /// HTML-decodes a string, converting HTML entities back to their character equivalents.
    /// </summary>
    /// <param name="value">The HTML-encoded string to decode.</param>
    /// <returns>The decoded string.</returns>
    public static string Unescape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value
            .Replace("&gt;", ">")
            .Replace("&lt;", "<")
            .Replace("&#39;", "'")
            .Replace("&#039;", "'")
            .Replace("&quot;", "\"")
            .Replace("&amp;", "&");
    }

    /// <summary>
    /// Safely encodes a string for use in HTML, handling already-encoded values
    /// to prevent double-encoding.
    /// </summary>
    /// <param name="value">The string to encode.</param>
    /// <returns>The safely HTML-encoded string.</returns>
    public static string SafeEscape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // First unescape to handle already-encoded values, then escape
        // This prevents double-encoding (e.g., &amp;lt; becoming &amp;amp;lt;)
        return Escape(Unescape(value));
    }

    /// <summary>
    /// HTML-escapes all string values in a dictionary for safe HTML message formatting.
    /// </summary>
    /// <param name="values">The values dictionary to process.</param>
    /// <returns>A new dictionary with all string values HTML-escaped.</returns>
    public static Dictionary<string, object?> EscapeValues(IReadOnlyDictionary<string, object?> values)
    {
        var escapedValues = new Dictionary<string, object?>(values.Count);

        foreach (var kvp in values)
        {
            var value = kvp.Value;

            if (value is string stringValue)
            {
                // Use SafeEscape to prevent double-encoding
                escapedValues[kvp.Key] = SafeEscape(stringValue);
            }
            else if (value == null)
            {
                escapedValues[kvp.Key] = null;
            }
            else
            {
                // For non-string values, convert to string and escape
                // Numbers, dates, etc. are converted to their string representation
                var stringified = value.ToString();
                escapedValues[kvp.Key] = stringified != null ? Escape(stringified) : value;
            }
        }

        return escapedValues;
    }
}
