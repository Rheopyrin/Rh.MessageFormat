using System;
using System.Collections.Generic;
using System.Text;
using Rh.MessageFormat.Pools;

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

        // First pass: check if any escaping is needed
        var needsEscaping = false;
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c == '&' || c == '"' || c == '\'' || c == '<' || c == '>')
            {
                needsEscaping = true;
                break;
            }
        }

        if (!needsEscaping)
            return value;

        // Second pass: build escaped string using pooled StringBuilder
        var sb = StringBuilderPool.Get();
        try
        {
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                switch (c)
                {
                    case '&':
                        sb.Append("&amp;");
                        break;
                    case '"':
                        sb.Append("&quot;");
                        break;
                    case '\'':
                        sb.Append("&#39;");
                        break;
                    case '<':
                        sb.Append("&lt;");
                        break;
                    case '>':
                        sb.Append("&gt;");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
        }
        finally
        {
            StringBuilderPool.Return(sb);
        }
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

        // Quick check: if no ampersand, nothing to unescape
        var ampIndex = value.IndexOf('&');
        if (ampIndex == -1)
            return value;

        var sb = StringBuilderPool.Get();
        try
        {
            var i = 0;
            while (i < value.Length)
            {
                if (value[i] == '&')
                {
                    // Try to match known entities
                    if (MatchEntity(value, i, "&gt;", out var len))
                    {
                        sb.Append('>');
                        i += len;
                    }
                    else if (MatchEntity(value, i, "&lt;", out len))
                    {
                        sb.Append('<');
                        i += len;
                    }
                    else if (MatchEntity(value, i, "&#39;", out len))
                    {
                        sb.Append('\'');
                        i += len;
                    }
                    else if (MatchEntity(value, i, "&#039;", out len))
                    {
                        sb.Append('\'');
                        i += len;
                    }
                    else if (MatchEntity(value, i, "&quot;", out len))
                    {
                        sb.Append('"');
                        i += len;
                    }
                    else if (MatchEntity(value, i, "&amp;", out len))
                    {
                        sb.Append('&');
                        i += len;
                    }
                    else
                    {
                        sb.Append(value[i]);
                        i++;
                    }
                }
                else
                {
                    sb.Append(value[i]);
                    i++;
                }
            }

            return sb.ToString();
        }
        finally
        {
            StringBuilderPool.Return(sb);
        }
    }

    private static bool MatchEntity(string value, int startIndex, string entity, out int length)
    {
        length = entity.Length;
        if (startIndex + length > value.Length)
            return false;

        for (var i = 0; i < length; i++)
        {
            if (value[startIndex + i] != entity[i])
                return false;
        }

        return true;
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
