using System.Collections.Generic;
using Rh.MessageFormat.Exceptions;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Tests for rich text tag support.
/// </summary>
public class RichTextTagTests
{
    #region Options Configuration Tests

    [Fact]
    public void TagHandler_ConfiguredViaOptions_IsUsed()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("<bold>text</bold>", args);

        Assert.Equal("**text**", result);
    }

    [Fact]
    public void TagHandler_CaseInsensitive()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["Bold"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        // All cases should work
        Assert.Equal("**text**", formatter.FormatMessage("<bold>text</bold>", args));
        Assert.Equal("**text**", formatter.FormatMessage("<BOLD>text</BOLD>", args));
        Assert.Equal("**text**", formatter.FormatMessage("<Bold>text</Bold>", args));
    }

    #endregion

    #region Basic Formatting Tests

    [Fact]
    public void Tag_SimpleUsage()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("Hello <bold>world</bold>!", args);

        Assert.Equal("Hello **world**!", result);
    }

    [Fact]
    public void Tag_WithVariable()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "name", "John" } };

        var result = formatter.FormatMessage("Hello <bold>{name}</bold>!", args);

        Assert.Equal("Hello **John**!", result);
    }

    [Fact]
    public void Tag_MultipleVariables()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["em"] = content => $"_{content}_";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "first", "John" }, { "last", "Doe" } };

        var result = formatter.FormatMessage("<em>{first} {last}</em>", args);

        Assert.Equal("_John Doe_", result);
    }

    [Fact]
    public void Tag_NoHandler_StripsTag()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "name", "John" } };

        var result = formatter.FormatMessage("Hello <unknown>{name}</unknown>!", args);

        Assert.Equal("Hello John!", result);
    }

    [Fact]
    public void Tag_EmptyContent()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["empty"] = content => $"[{content}]";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("Before<empty></empty>After", args);

        Assert.Equal("Before[]After", result);
    }

    [Fact]
    public void Tag_MultipleSameTags()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["b"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("<b>one</b> and <b>two</b>", args);

        Assert.Equal("**one** and **two**", result);
    }

    [Fact]
    public void Tag_MultipleDifferentTags()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        options.TagHandlers["italic"] = content => $"*{content}*";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("<bold>bold</bold> and <italic>italic</italic>", args);

        Assert.Equal("**bold** and *italic*", result);
    }

    #endregion

    #region Nested Tags Tests

    [Fact]
    public void Tag_NestedTags()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        options.TagHandlers["italic"] = content => $"*{content}*";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("<bold><italic>text</italic></bold>", args);

        Assert.Equal("***text***", result);
    }

    [Fact]
    public void Tag_DeeplyNestedTags()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["a"] = content => $"[{content}]";
        options.TagHandlers["b"] = content => $"({content})";
        options.TagHandlers["c"] = content => $"<{content}>";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("<a><b><c>x</c></b></a>", args);

        Assert.Equal("[(<x>)]", result);
    }

    [Fact]
    public void Tag_NestedWithVariables()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["link"] = content => $"[{content}](url)";
        options.TagHandlers["bold"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "name", "Click here" } };

        var result = formatter.FormatMessage("<link><bold>{name}</bold></link>", args);

        Assert.Equal("[**Click here**](url)", result);
    }

    #endregion

    #region Tags with Formatters Tests

    [Fact]
    public void Tag_WithPlural()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["em"] = content => $"_{content}_";
        var formatter = new MessageFormatter("en", options);

        var args1 = new Dictionary<string, object?> { { "count", 1 } };
        var args5 = new Dictionary<string, object?> { { "count", 5 } };

        var pattern = "<em>You have {count, plural, one {# item} other {# items}}</em>";

        Assert.Equal("_You have 1 item_", formatter.FormatMessage(pattern, args1));
        Assert.Equal("_You have 5 items_", formatter.FormatMessage(pattern, args5));
    }

    [Fact]
    public void Tag_WithNumber()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["price"] = content => $"${content}";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "amount", 1234.56 } };

        var result = formatter.FormatMessage("<price>{amount, number}</price>", args);

        Assert.Equal("$1,234.56", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Tag_LiteralLessThanNotTag()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["b"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        // Less-than followed by space or digit is not a tag
        var result = formatter.FormatMessage("5 < 10 and <b>bold</b>", args);

        Assert.Equal("5 < 10 and **bold**", result);
    }

    [Fact]
    public void Tag_LiteralLessThanBeforeNumber()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("x < 5", args);

        Assert.Equal("x < 5", result);
    }

    [Fact]
    public void Tag_MixedContentWithLiterals()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["code"] = content => $"`{content}`";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "var", "x" } };

        var result = formatter.FormatMessage("The variable <code>{var}</code> is less than 10", args);

        Assert.Equal("The variable `x` is less than 10", result);
    }

    #endregion

    #region Error Cases

    [Fact]
    public void Tag_UnclosedTag_ThrowsException()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatMessage("Hello <bold>world", args));
    }

    [Fact]
    public void Tag_MismatchedClosingTag_ThrowsException()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        options.TagHandlers["italic"] = content => $"*{content}*";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        // This should throw because </italic> doesn't match <bold>
        Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatMessage("<bold>text</italic>", args));
    }

    [Fact]
    public void Tag_MissingClosingBracket_ThrowsException()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatMessage("<bold text</bold>", args));
    }

    #endregion

    #region Real-World Use Cases

    [Fact]
    public void Tag_MarkdownFormatting()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["b"] = content => $"**{content}**";
        options.TagHandlers["i"] = content => $"*{content}*";
        options.TagHandlers["code"] = content => $"`{content}`";
        options.TagHandlers["strike"] = content => $"~~{content}~~";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "name", "MessageFormat" } };

        var result = formatter.FormatMessage(
            "Welcome to <b>{name}</b>! It supports <i>italic</i>, <code>code</code>, and <strike>strikethrough</strike>.",
            args);

        Assert.Equal("Welcome to **MessageFormat**! It supports *italic*, `code`, and ~~strikethrough~~.", result);
    }

    [Fact]
    public void Tag_HtmlFormatting()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"<strong>{content}</strong>";
        options.TagHandlers["italic"] = content => $"<em>{content}</em>";
        options.TagHandlers["link"] = content => $"<a href=\"#\">{content}</a>";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "name", "World" } };

        var result = formatter.FormatMessage(
            "Hello <bold>{name}</bold>! <link>Click here</link> for more.",
            args);

        Assert.Equal("Hello <strong>World</strong>! <a href=\"#\">Click here</a> for more.", result);
    }

    [Fact]
    public void Tag_ReactComponentStyle()
    {
        var options = TestOptions.WithEnglish();
        // Simulating React-style component rendering
        options.TagHandlers["UserName"] = content => $"<UserName>{content}</UserName>";
        options.TagHandlers["Link"] = content => $"<Link to=\"/profile\">{content}</Link>";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "user", "John" } };

        var result = formatter.FormatMessage(
            "Hello <UserName>{user}</UserName>, <Link>view your profile</Link>.",
            args);

        Assert.Equal("Hello <UserName>John</UserName>, <Link to=\"/profile\">view your profile</Link>.", result);
    }

    [Fact]
    public void Tag_TextToSpeechMarkers()
    {
        var options = TestOptions.WithEnglish();
        // SSML-style markers for text-to-speech
        options.TagHandlers["emphasis"] = content => $"<emphasis level=\"strong\">{content}</emphasis>";
        options.TagHandlers["pause"] = content => $"<break time=\"500ms\"/>{content}";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "name", "user" } };

        var result = formatter.FormatMessage(
            "Hello <emphasis>{name}</emphasis>.<pause>How can I help you?</pause>",
            args);

        Assert.Equal("Hello <emphasis level=\"strong\">user</emphasis>.<break time=\"500ms\"/>How can I help you?", result);
    }

    #endregion
}
