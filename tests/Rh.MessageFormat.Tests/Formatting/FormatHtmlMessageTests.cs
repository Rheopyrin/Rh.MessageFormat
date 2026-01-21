using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Formatting;

/// <summary>
/// Tests for FormatHtmlMessage - formatting messages with HTML markup and XSS prevention.
/// </summary>
public class FormatHtmlMessageTests
{
    private readonly MessageFormatter _formatter;

    public FormatHtmlMessageTests()
    {
        var options = TestOptions.WithEnglish();
        _formatter = new MessageFormatter("en", options);
    }

    #region Basic HTML Preservation Tests

    [Fact]
    public void FormatHtmlMessage_SimpleHtmlTag_PreservesTag()
    {
        var values = new Dictionary<string, object?>
        {
            ["name"] = "John"
        };

        var result = _formatter.FormatHtmlMessage("<b>{name}</b>", values);

        Assert.Equal("<b>John</b>", result);
    }

    [Fact]
    public void FormatHtmlMessage_AnchorTag_PreservesTag()
    {
        var values = new Dictionary<string, object?>
        {
            ["text"] = "Click here"
        };

        var result = _formatter.FormatHtmlMessage("<a href=\"https://example.com\">{text}</a>", values);

        Assert.Equal("<a href=\"https://example.com\">Click here</a>", result);
    }

    [Fact]
    public void FormatHtmlMessage_MultipleHtmlTags_PreservesAllTags()
    {
        var values = new Dictionary<string, object?>
        {
            ["text1"] = "Hello",
            ["text2"] = "World"
        };

        var result = _formatter.FormatHtmlMessage("<b>{text1}</b> <i>{text2}</i>", values);

        Assert.Equal("<b>Hello</b> <i>World</i>", result);
    }

    [Fact]
    public void FormatHtmlMessage_SelfClosingTag_PreservesTag()
    {
        var values = new Dictionary<string, object?>
        {
            ["before"] = "Hello",
            ["after"] = "World"
        };

        var result = _formatter.FormatHtmlMessage("{before}<br/>{after}", values);

        Assert.Equal("Hello<br/>World", result);
    }

    [Fact]
    public void FormatHtmlMessage_NestedHtmlTags_PreservesAllTags()
    {
        var values = new Dictionary<string, object?>
        {
            ["text"] = "important"
        };

        var result = _formatter.FormatHtmlMessage("<div><b><i>{text}</i></b></div>", values);

        Assert.Equal("<div><b><i>important</i></b></div>", result);
    }

    #endregion

    #region XSS Prevention Tests

    [Fact]
    public void FormatHtmlMessage_ScriptTagInValue_IsEscaped()
    {
        var values = new Dictionary<string, object?>
        {
            ["text"] = "<script>alert('xss')</script>"
        };

        var result = _formatter.FormatHtmlMessage("<div>{text}</div>", values);

        Assert.Equal("<div>&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;</div>", result);
        Assert.DoesNotContain("<script>", result);
    }

    [Fact]
    public void FormatHtmlMessage_HtmlTagInValue_IsEscaped()
    {
        var values = new Dictionary<string, object?>
        {
            ["text"] = "<b>bold</b>"
        };

        var result = _formatter.FormatHtmlMessage("<div>{text}</div>", values);

        Assert.Equal("<div>&lt;b&gt;bold&lt;/b&gt;</div>", result);
    }

    [Fact]
    public void FormatHtmlMessage_AngleBracketsInValue_AreEscaped()
    {
        var values = new Dictionary<string, object?>
        {
            ["text"] = "a < b > c"
        };

        var result = _formatter.FormatHtmlMessage("<span>{text}</span>", values);

        Assert.Equal("<span>a &lt; b &gt; c</span>", result);
    }

    [Fact]
    public void FormatHtmlMessage_QuotesInValue_AreEscaped()
    {
        var values = new Dictionary<string, object?>
        {
            ["text"] = "He said \"Hello\""
        };

        var result = _formatter.FormatHtmlMessage("<p>{text}</p>", values);

        Assert.Equal("<p>He said &quot;Hello&quot;</p>", result);
    }

    [Fact]
    public void FormatHtmlMessage_SingleQuotesInValue_AreEscaped()
    {
        var values = new Dictionary<string, object?>
        {
            ["text"] = "It's fine"
        };

        var result = _formatter.FormatHtmlMessage("<p>{text}</p>", values);

        Assert.Equal("<p>It&#39;s fine</p>", result);
    }

    [Fact]
    public void FormatHtmlMessage_AmpersandInValue_IsEscaped()
    {
        var values = new Dictionary<string, object?>
        {
            ["text"] = "Tom & Jerry"
        };

        var result = _formatter.FormatHtmlMessage("<span>{text}</span>", values);

        Assert.Equal("<span>Tom &amp; Jerry</span>", result);
    }

    [Fact]
    public void FormatHtmlMessage_OnEventHandler_IsEscaped()
    {
        var values = new Dictionary<string, object?>
        {
            ["text"] = "\" onclick=\"alert('xss')\""
        };

        var result = _formatter.FormatHtmlMessage("<a href=\"#\">{text}</a>", values);

        // The quotes are escaped, preventing attribute injection
        Assert.Contains("&quot;", result);
        // The original href attribute is preserved
        Assert.Contains("<a href=\"#\">", result);
    }

    #endregion

    #region Double-Escaping Prevention Tests

    [Fact]
    public void FormatHtmlMessage_AlreadyEscapedValue_NotDoubleEscaped()
    {
        var values = new Dictionary<string, object?>
        {
            ["text"] = "&lt;script&gt;alert('xss')&lt;/script&gt;"
        };

        var result = _formatter.FormatHtmlMessage("<div>{text}</div>", values);

        // Should not become &amp;lt; etc.
        Assert.Equal("<div>&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;</div>", result);
        Assert.DoesNotContain("&amp;lt;", result);
    }

    [Fact]
    public void FormatHtmlMessage_PartiallyEscapedValue_HandledCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["text"] = "&amp; and <tag>"
        };

        var result = _formatter.FormatHtmlMessage("<span>{text}</span>", values);

        // & should not become &amp;amp;, but <tag> should be escaped
        Assert.Contains("&amp;", result);
        Assert.Contains("&lt;tag&gt;", result);
    }

    #endregion

    #region Numeric and Non-String Values Tests

    [Fact]
    public void FormatHtmlMessage_NumericValue_FormattedCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["count"] = 42
        };

        var result = _formatter.FormatHtmlMessage("<span>{count} items</span>", values);

        Assert.Equal("<span>42 items</span>", result);
    }

    [Fact]
    public void FormatHtmlMessage_NullValue_HandledGracefully()
    {
        var values = new Dictionary<string, object?>
        {
            ["text"] = null
        };

        var result = _formatter.FormatHtmlMessage("<span>Value: {text}</span>", values);

        // Should not throw, behavior depends on formatter
        Assert.Contains("<span>", result);
    }

    #endregion

    #region Complex HTML Patterns Tests

    [Fact]
    public void FormatHtmlMessage_HtmlWithAttributes_PreservesAttributes()
    {
        var values = new Dictionary<string, object?>
        {
            ["name"] = "John"
        };

        var result = _formatter.FormatHtmlMessage(
            "<a href=\"https://example.com\" class=\"link\" target=\"_blank\">{name}</a>",
            values);

        Assert.Contains("href=\"https://example.com\"", result);
        Assert.Contains("class=\"link\"", result);
        Assert.Contains("target=\"_blank\"", result);
        Assert.Contains(">John<", result);
    }

    [Fact]
    public void FormatHtmlMessage_HtmlWithDynamicAttribute_ValueIsEscaped()
    {
        var values = new Dictionary<string, object?>
        {
            ["url"] = "https://example.com?a=1&b=2",
            ["text"] = "Link"
        };

        var result = _formatter.FormatHtmlMessage("<a href=\"{url}\">{text}</a>", values);

        // The & in the URL should be escaped
        Assert.Contains("&amp;", result);
        Assert.Contains(">Link<", result);
    }

    [Fact]
    public void FormatHtmlMessage_TableStructure_PreservesStructure()
    {
        var values = new Dictionary<string, object?>
        {
            ["name"] = "John",
            ["age"] = 30
        };

        var result = _formatter.FormatHtmlMessage(
            "<table><tr><td>{name}</td><td>{age}</td></tr></table>",
            values);

        Assert.Contains("<table>", result);
        Assert.Contains("<tr>", result);
        Assert.Contains("<td>John</td>", result);
        Assert.Contains("<td>30</td>", result);
        Assert.Contains("</table>", result);
    }

    #endregion

    #region Integration with ICU Features Tests

    [Fact]
    public void FormatHtmlMessage_WithPlural_WorksCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["count"] = 5
        };

        var result = _formatter.FormatHtmlMessage(
            "<b>{count, plural, one {# item} other {# items}}</b>",
            values);

        Assert.Equal("<b>5 items</b>", result);
    }

    [Fact]
    public void FormatHtmlMessage_WithSelect_WorksCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["gender"] = "female",
            ["name"] = "Alice"
        };

        var result = _formatter.FormatHtmlMessage(
            "<span>{gender, select, male {He} female {She} other {They}} is <b>{name}</b></span>",
            values);

        Assert.Equal("<span>She is <b>Alice</b></span>", result);
    }

    [Fact]
    public void FormatHtmlMessage_WithNumberFormat_WorksCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["price"] = 1234.56
        };

        var result = _formatter.FormatHtmlMessage(
            "<span class=\"price\">{price, number}</span>",
            values);

        Assert.Contains("<span class=\"price\">", result);
        Assert.Contains("</span>", result);
    }

    #endregion

    #region Real-World Scenarios Tests

    [Fact]
    public void FormatHtmlMessage_EmailTemplate_FormatsCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["userName"] = "John Doe",
            ["productName"] = "Widget <Pro>",
            ["orderNumber"] = "ORD-12345"
        };

        var result = _formatter.FormatHtmlMessage(
            "<p>Dear <b>{userName}</b>,</p><p>Thank you for ordering {productName}.</p><p>Order #: {orderNumber}</p>",
            values);

        Assert.Contains("<b>John Doe</b>", result);
        Assert.Contains("Widget &lt;Pro&gt;", result); // XSS in product name is escaped
        Assert.Contains("ORD-12345", result);
    }

    [Fact]
    public void FormatHtmlMessage_NotificationBanner_FormatsCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["message"] = "Your session will expire in 5 minutes",
            ["link"] = "/extend-session"
        };

        var result = _formatter.FormatHtmlMessage(
            "<div class=\"alert\"><span>{message}</span> <a href=\"{link}\">Extend</a></div>",
            values);

        Assert.Contains("class=\"alert\"", result);
        Assert.Contains(">Your session will expire in 5 minutes<", result);
        Assert.Contains("href=\"/extend-session\"", result);
    }

    [Fact]
    public void FormatHtmlMessage_UserGeneratedContent_IsSafe()
    {
        var values = new Dictionary<string, object?>
        {
            ["username"] = "<script>steal(cookies)</script>",
            ["comment"] = "Nice post! <img src=x onerror=alert(1)>"
        };

        var result = _formatter.FormatHtmlMessage(
            "<div class=\"comment\"><b>{username}</b>: {comment}</div>",
            values);

        // Script tags are escaped - the raw <script> tag is not executable
        Assert.DoesNotContain("<script>", result);
        Assert.Contains("&lt;script&gt;", result);
        // Img tag is escaped - the raw <img> tag is not executable
        Assert.DoesNotContain("<img", result);
        Assert.Contains("&lt;img", result);
        // Template HTML is preserved
        Assert.Contains("<div class=\"comment\">", result);
        Assert.Contains("<b>", result);
    }

    #endregion
}
