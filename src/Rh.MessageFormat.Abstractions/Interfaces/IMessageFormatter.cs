using System.Collections.Generic;

namespace Rh.MessageFormat.Abstractions.Interfaces;

/// <summary>
///     Interface for message formatting.
/// </summary>
public interface IMessageFormatter
{
    /// <summary>
    ///     Formats the message with the specified arguments using the locale configured in the constructor.
    /// </summary>
    /// <param name="pattern">
    ///     The pattern.
    /// </param>
    /// <param name="args">
    ///     The arguments.
    /// </param>
    /// <returns>
    ///     The formatted message.
    /// </returns>
    string FormatMessage(string pattern, IReadOnlyDictionary<string, object?> args);

    /// <summary>
    ///     Formats the message with the specified arguments using the locale configured in the constructor.
    ///     This overload accepts any object (including anonymous types) and converts it to a dictionary.
    /// </summary>
    /// <param name="pattern">
    ///     The pattern.
    /// </param>
    /// <param name="args">
    ///     The arguments as an object (anonymous type, POCO, or any object with public properties).
    /// </param>
    /// <returns>
    ///     The formatted message.
    /// </returns>
    /// <example>
    ///     <code>
    ///     formatter.FormatMessage("Hello {name}, you have {count} messages", new { name = "John", count = 5 });
    ///     </code>
    /// </example>
    string FormatMessage(string pattern, object? args = null);

    /// <summary>
    ///     Formats a complex message with support for nested object values.
    ///     Nested objects are flattened using "__" as a separator.
    /// </summary>
    /// <remarks>
    ///     This method allows passing nested objects in the values dictionary.
    ///     For example:
    ///     <code>
    ///     var values = new Dictionary&lt;string, object?&gt;
    ///     {
    ///         ["user"] = new Dictionary&lt;string, object?&gt;
    ///         {
    ///             ["firstName"] = "John",
    ///             ["lastName"] = "Doe"
    ///         }
    ///     };
    ///     formatter.FormatComplexMessage("Hello {user__firstName} {user__lastName}", values);
    ///     // Returns: "Hello John Doe"
    ///     </code>
    /// </remarks>
    /// <param name="pattern">
    ///     The message pattern. Use "__" (double underscore) to reference nested values.
    /// </param>
    /// <param name="values">
    ///     The values dictionary, which may contain nested objects.
    /// </param>
    /// <returns>
    ///     The formatted message.
    /// </returns>
    string FormatComplexMessage(string pattern, IReadOnlyDictionary<string, object?> values);

    /// <summary>
    ///     Formats a complex message with support for nested object values.
    ///     This overload accepts any object (including anonymous types) and converts it to a dictionary.
    /// </summary>
    /// <param name="pattern">
    ///     The message pattern. Use "__" (double underscore) to reference nested values.
    /// </param>
    /// <param name="values">
    ///     The values as an object (anonymous type, POCO, or any object with public properties).
    /// </param>
    /// <returns>
    ///     The formatted message.
    /// </returns>
    /// <example>
    ///     <code>
    ///     formatter.FormatComplexMessage("Hello {user__firstName}", new { user = new { firstName = "John" } });
    ///     </code>
    /// </example>
    string FormatComplexMessage(string pattern, object? values = null);

    /// <summary>
    ///     Formats a message containing HTML markup with safe variable substitution.
    ///     HTML tags in the message template are preserved, while variable values are HTML-escaped.
    /// </summary>
    /// <remarks>
    ///     This method is designed for messages that contain HTML markup. It:
    ///     <list type="bullet">
    ///         <item>Preserves HTML tags in the message template (e.g., &lt;a&gt;, &lt;b&gt;, &lt;br/&gt;)</item>
    ///         <item>HTML-escapes all variable values to prevent XSS attacks</item>
    ///         <item>Safely handles pre-escaped values to prevent double-escaping</item>
    ///     </list>
    ///     <para>
    ///     Example:
    ///     <code>
    ///     var values = new Dictionary&lt;string, object?&gt;
    ///     {
    ///         ["name"] = "John",
    ///         ["link"] = "https://example.com"
    ///     };
    ///     formatter.FormatHtmlMessage("&lt;b&gt;Hello {name}&lt;/b&gt;, visit &lt;a href=\"{link}\"&gt;here&lt;/a&gt;", values);
    ///     // Returns: "&lt;b&gt;Hello John&lt;/b&gt;, visit &lt;a href=\"https://example.com\"&gt;here&lt;/a&gt;"
    ///     </code>
    ///     </para>
    ///     <para>
    ///     XSS Prevention:
    ///     <code>
    ///     var values = new Dictionary&lt;string, object?&gt;
    ///     {
    ///         ["text"] = "&lt;script&gt;alert('xss')&lt;/script&gt;"
    ///     };
    ///     formatter.FormatHtmlMessage("&lt;div&gt;{text}&lt;/div&gt;", values);
    ///     // Returns: "&lt;div&gt;&amp;lt;script&amp;gt;alert('xss')&amp;lt;/script&amp;gt;&lt;/div&gt;"
    ///     </code>
    ///     </para>
    /// </remarks>
    /// <param name="pattern">
    ///     The message pattern containing HTML markup.
    /// </param>
    /// <param name="values">
    ///     The values dictionary. All string values will be HTML-escaped.
    /// </param>
    /// <returns>
    ///     The formatted HTML message with escaped variable values.
    /// </returns>
    string FormatHtmlMessage(string pattern, IReadOnlyDictionary<string, object?> values);

    /// <summary>
    ///     Formats a message containing HTML markup with safe variable substitution.
    ///     This overload accepts any object (including anonymous types) and converts it to a dictionary.
    /// </summary>
    /// <param name="pattern">
    ///     The message pattern containing HTML markup.
    /// </param>
    /// <param name="values">
    ///     The values as an object (anonymous type, POCO, or any object with public properties).
    ///     All string values will be HTML-escaped.
    /// </param>
    /// <returns>
    ///     The formatted HTML message with escaped variable values.
    /// </returns>
    /// <example>
    ///     <code>
    ///     formatter.FormatHtmlMessage("&lt;b&gt;Hello {name}&lt;/b&gt;", new { name = "John" });
    ///     </code>
    /// </example>
    string FormatHtmlMessage(string pattern, object? values = null);
}
