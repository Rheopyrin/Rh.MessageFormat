using System.Collections.Generic;
using Rh.MessageFormat.Formatting;
using Xunit;

namespace Rh.MessageFormat.Tests.Formatting;

/// <summary>
/// Tests for HtmlEncoder utility.
/// </summary>
public class HtmlEncoderTests
{
    #region Escape Tests

    [Fact]
    public void Escape_Ampersand_EncodesCorrectly()
    {
        var result = HtmlEncoder.Escape("Tom & Jerry");

        Assert.Equal("Tom &amp; Jerry", result);
    }

    [Fact]
    public void Escape_LessThan_EncodesCorrectly()
    {
        var result = HtmlEncoder.Escape("a < b");

        Assert.Equal("a &lt; b", result);
    }

    [Fact]
    public void Escape_GreaterThan_EncodesCorrectly()
    {
        var result = HtmlEncoder.Escape("a > b");

        Assert.Equal("a &gt; b", result);
    }

    [Fact]
    public void Escape_DoubleQuote_EncodesCorrectly()
    {
        var result = HtmlEncoder.Escape("He said \"Hello\"");

        Assert.Equal("He said &quot;Hello&quot;", result);
    }

    [Fact]
    public void Escape_SingleQuote_EncodesCorrectly()
    {
        var result = HtmlEncoder.Escape("It's fine");

        Assert.Equal("It&#39;s fine", result);
    }

    [Fact]
    public void Escape_AllSpecialCharacters_EncodesCorrectly()
    {
        var result = HtmlEncoder.Escape("<a href=\"test\">Tom & Jerry's</a>");

        Assert.Equal("&lt;a href=&quot;test&quot;&gt;Tom &amp; Jerry&#39;s&lt;/a&gt;", result);
    }

    [Fact]
    public void Escape_NullString_ReturnsNull()
    {
        var result = HtmlEncoder.Escape(null!);

        Assert.Null(result);
    }

    [Fact]
    public void Escape_EmptyString_ReturnsEmpty()
    {
        var result = HtmlEncoder.Escape("");

        Assert.Equal("", result);
    }

    [Fact]
    public void Escape_NoSpecialCharacters_ReturnsUnchanged()
    {
        var result = HtmlEncoder.Escape("Hello World");

        Assert.Equal("Hello World", result);
    }

    #endregion

    #region Unescape Tests

    [Fact]
    public void Unescape_Ampersand_DecodesCorrectly()
    {
        var result = HtmlEncoder.Unescape("Tom &amp; Jerry");

        Assert.Equal("Tom & Jerry", result);
    }

    [Fact]
    public void Unescape_LessThan_DecodesCorrectly()
    {
        var result = HtmlEncoder.Unescape("a &lt; b");

        Assert.Equal("a < b", result);
    }

    [Fact]
    public void Unescape_GreaterThan_DecodesCorrectly()
    {
        var result = HtmlEncoder.Unescape("a &gt; b");

        Assert.Equal("a > b", result);
    }

    [Fact]
    public void Unescape_DoubleQuote_DecodesCorrectly()
    {
        var result = HtmlEncoder.Unescape("He said &quot;Hello&quot;");

        Assert.Equal("He said \"Hello\"", result);
    }

    [Fact]
    public void Unescape_SingleQuote_DecodesCorrectly()
    {
        var result = HtmlEncoder.Unescape("It&#39;s fine");

        Assert.Equal("It's fine", result);
    }

    [Fact]
    public void Unescape_SingleQuoteAlternate_DecodesCorrectly()
    {
        var result = HtmlEncoder.Unescape("It&#039;s fine");

        Assert.Equal("It's fine", result);
    }

    [Fact]
    public void Unescape_AllEntities_DecodesCorrectly()
    {
        var result = HtmlEncoder.Unescape("&lt;a href=&quot;test&quot;&gt;Tom &amp; Jerry&#39;s&lt;/a&gt;");

        Assert.Equal("<a href=\"test\">Tom & Jerry's</a>", result);
    }

    [Fact]
    public void Unescape_NullString_ReturnsNull()
    {
        var result = HtmlEncoder.Unescape(null!);

        Assert.Null(result);
    }

    [Fact]
    public void Unescape_EmptyString_ReturnsEmpty()
    {
        var result = HtmlEncoder.Unescape("");

        Assert.Equal("", result);
    }

    [Fact]
    public void Unescape_NoEntities_ReturnsUnchanged()
    {
        var result = HtmlEncoder.Unescape("Hello World");

        Assert.Equal("Hello World", result);
    }

    #endregion

    #region SafeEscape Tests

    [Fact]
    public void SafeEscape_PlainText_Escapes()
    {
        var result = HtmlEncoder.SafeEscape("<script>alert('xss')</script>");

        Assert.Equal("&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;", result);
    }

    [Fact]
    public void SafeEscape_AlreadyEscaped_DoesNotDoubleEscape()
    {
        var result = HtmlEncoder.SafeEscape("&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;");

        Assert.Equal("&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;", result);
        Assert.DoesNotContain("&amp;lt;", result);
    }

    [Fact]
    public void SafeEscape_PartiallyEscaped_HandlesCorrectly()
    {
        var result = HtmlEncoder.SafeEscape("&amp; and <tag>");

        // Should decode &amp; to &, then escape everything
        Assert.Equal("&amp; and &lt;tag&gt;", result);
    }

    [Fact]
    public void SafeEscape_MixedContent_HandlesCorrectly()
    {
        var result = HtmlEncoder.SafeEscape("Hello &lt;World&gt; & <Universe>");

        // Unescapes &lt;World&gt; to <World>, then escapes everything
        Assert.Equal("Hello &lt;World&gt; &amp; &lt;Universe&gt;", result);
    }

    [Fact]
    public void SafeEscape_NullString_ReturnsNull()
    {
        var result = HtmlEncoder.SafeEscape(null!);

        Assert.Null(result);
    }

    [Fact]
    public void SafeEscape_EmptyString_ReturnsEmpty()
    {
        var result = HtmlEncoder.SafeEscape("");

        Assert.Equal("", result);
    }

    #endregion

    #region EscapeValues Tests

    [Fact]
    public void EscapeValues_StringValue_IsEscaped()
    {
        var values = new Dictionary<string, object?>
        {
            ["text"] = "<script>alert('xss')</script>"
        };

        var result = HtmlEncoder.EscapeValues(values);

        Assert.Equal("&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;", result["text"]);
    }

    [Fact]
    public void EscapeValues_NullValue_PreservesNull()
    {
        var values = new Dictionary<string, object?>
        {
            ["text"] = null
        };

        var result = HtmlEncoder.EscapeValues(values);

        Assert.Null(result["text"]);
    }

    [Fact]
    public void EscapeValues_NumericValue_IsConvertedAndEscaped()
    {
        var values = new Dictionary<string, object?>
        {
            ["count"] = 42
        };

        var result = HtmlEncoder.EscapeValues(values);

        Assert.Equal("42", result["count"]);
    }

    [Fact]
    public void EscapeValues_MultipleValues_AllEscaped()
    {
        var values = new Dictionary<string, object?>
        {
            ["name"] = "<John>",
            ["age"] = 30,
            ["quote"] = "He said \"Hello\""
        };

        var result = HtmlEncoder.EscapeValues(values);

        Assert.Equal("&lt;John&gt;", result["name"]);
        Assert.Equal("30", result["age"]);
        Assert.Equal("He said &quot;Hello&quot;", result["quote"]);
    }

    [Fact]
    public void EscapeValues_AlreadyEscapedString_DoesNotDoubleEscape()
    {
        var values = new Dictionary<string, object?>
        {
            ["text"] = "&lt;script&gt;"
        };

        var result = HtmlEncoder.EscapeValues(values);

        Assert.Equal("&lt;script&gt;", result["text"]);
        Assert.DoesNotContain("&amp;lt;", result["text"]?.ToString());
    }

    [Fact]
    public void EscapeValues_EmptyDictionary_ReturnsEmpty()
    {
        var values = new Dictionary<string, object?>();

        var result = HtmlEncoder.EscapeValues(values);

        Assert.Empty(result);
    }

    [Fact]
    public void EscapeValues_BooleanValue_IsConvertedToString()
    {
        var values = new Dictionary<string, object?>
        {
            ["flag"] = true
        };

        var result = HtmlEncoder.EscapeValues(values);

        Assert.Equal("True", result["flag"]);
    }

    [Fact]
    public void EscapeValues_DateValue_IsConvertedToString()
    {
        var date = new System.DateTime(2024, 12, 25);
        var values = new Dictionary<string, object?>
        {
            ["date"] = date
        };

        var result = HtmlEncoder.EscapeValues(values);

        Assert.NotNull(result["date"]);
        Assert.IsType<string>(result["date"]);
    }

    #endregion

    #region Roundtrip Tests

    [Fact]
    public void EscapeUnescape_Roundtrip_RestoresOriginal()
    {
        var original = "<a href=\"test\">Tom & Jerry's</a>";

        var escaped = HtmlEncoder.Escape(original);
        var unescaped = HtmlEncoder.Unescape(escaped);

        Assert.Equal(original, unescaped);
    }

    [Fact]
    public void UnescapeEscape_Roundtrip_RestoresEscaped()
    {
        var escaped = "&lt;a href=&quot;test&quot;&gt;Tom &amp; Jerry&#39;s&lt;/a&gt;";

        var unescaped = HtmlEncoder.Unescape(escaped);
        var reescaped = HtmlEncoder.Escape(unescaped);

        Assert.Equal(escaped, reescaped);
    }

    #endregion
}
