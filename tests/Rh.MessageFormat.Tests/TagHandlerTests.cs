using System;
using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests;

/// <summary>
/// Tests for TagHandler delegate via options.
/// </summary>
public class TagHandlerTests
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
    public void TagHandler_CaseInsensitive_ViaOptions()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["Bold"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        // Tag name in message should match case-insensitively
        var result = formatter.FormatMessage("<bold>text</bold>", args);

        Assert.Equal("**text**", result);
    }

    [Fact]
    public void TagHandler_MultipleHandlers_ViaOptions()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        options.TagHandlers["italic"] = content => $"*{content}*";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("<bold>Hello</bold> <italic>World</italic>!", args);

        Assert.Equal("**Hello** *World*!", result);
    }

    #endregion

    #region Basic Tag Formatting Tests

    [Fact]
    public void Tag_BoldMarkdown()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("Hello <bold>World</bold>!", args);

        Assert.Equal("Hello **World**!", result);
    }

    [Fact]
    public void Tag_ItalicMarkdown()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["italic"] = content => $"*{content}*";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("Hello <italic>World</italic>!", args);

        Assert.Equal("Hello *World*!", result);
    }

    [Fact]
    public void Tag_HtmlBold()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["b"] = content => $"<strong>{content}</strong>";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("Hello <b>World</b>!", args);

        Assert.Equal("Hello <strong>World</strong>!", result);
    }

    [Fact]
    public void Tag_Link()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["link"] = content => $"[{content}](https://example.com)";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("Visit <link>our site</link>!", args);

        Assert.Equal("Visit [our site](https://example.com)!", result);
    }

    [Fact]
    public void Tag_EmptyContent()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("Before<bold></bold>After", args);

        Assert.Equal("Before****After", result);
    }

    [Fact]
    public void Tag_WithVariableInside()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "name", "John" } };

        var result = formatter.FormatMessage("Hello <bold>{name}</bold>!", args);

        Assert.Equal("Hello **John**!", result);
    }

    [Fact]
    public void Tag_WithMultipleVariables()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>
        {
            { "first", "John" },
            { "last", "Doe" }
        };

        var result = formatter.FormatMessage("Hello <bold>{first} {last}</bold>!", args);

        Assert.Equal("Hello **John Doe**!", result);
    }

    #endregion

    #region No Handler Tests

    [Fact]
    public void Tag_NoHandler_ContentPassedThrough()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?>();

        // No handler registered for "unknown"
        var result = formatter.FormatMessage("Hello <unknown>World</unknown>!", args);

        // Content should pass through without the tag
        Assert.Equal("Hello World!", result);
    }

    [Fact]
    public void Tag_NoHandler_VariableStillFormatted()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?> { { "name", "John" } };

        // No handler registered
        var result = formatter.FormatMessage("<unknown>{name}</unknown>", args);

        Assert.Equal("John", result);
    }

    #endregion

    #region Multiple Tags Tests

    [Fact]
    public void MultipleTags_SameType()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("<bold>Hello</bold> <bold>World</bold>!", args);

        Assert.Equal("**Hello** **World**!", result);
    }

    [Fact]
    public void MultipleTags_DifferentTypes()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        options.TagHandlers["italic"] = content => $"*{content}*";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("<bold>Hello</bold> <italic>World</italic>!", args);

        Assert.Equal("**Hello** *World*!", result);
    }

    [Fact]
    public void NestedTags_OuterHasHandler_InnerDoesNot()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("<bold><unknown>text</unknown></bold>", args);

        Assert.Equal("**text**", result);
    }

    [Fact]
    public void NestedTags_BothHaveHandlers()
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
    public void NestedTags_DeepNesting()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["a"] = content => $"[{content}]";
        options.TagHandlers["b"] = content => $"({content})";
        options.TagHandlers["c"] = content => $"<{content}>";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("<a><b><c>text</c></b></a>", args);

        Assert.Equal("[(<text>)]", result);
    }

    #endregion

    #region Tags with ICU Constructs Tests

    [Fact]
    public void Tag_WithPlural()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "count", 5 } };

        var result = formatter.FormatMessage(
            "<bold>{count, plural, one {# item} other {# items}}</bold>",
            args);

        Assert.Equal("**5 items**", result);
    }

    [Fact]
    public void Tag_WithSelect()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "gender", "female" } };

        var result = formatter.FormatMessage(
            "<bold>{gender, select, male {He} female {She} other {They}}</bold> said hello",
            args);

        Assert.Equal("**She** said hello", result);
    }

    [Fact]
    public void Tag_InsidePluralCase()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "count", 1 } };

        var result = formatter.FormatMessage(
            "{count, plural, one {<bold># item</bold>} other {# items}}",
            args);

        Assert.Equal("**1 item**", result);
    }

    [Fact]
    public void Tag_InsideSelectCase()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>
        {
            { "gender", "male" },
            { "name", "John" }
        };

        var result = formatter.FormatMessage(
            "{gender, select, male {<bold>{name}</bold> is a man} other {{name} is a person}}",
            args);

        Assert.Equal("**John** is a man", result);
    }

    [Fact]
    public void Tag_WithNumber()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["strong"] = content => $"<strong>{content}</strong>";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "price", 99.99 } };

        var result = formatter.FormatMessage("Price: <strong>{price, number}</strong>", args);

        Assert.Contains("<strong>", result);
        Assert.Contains("99.99", result);
        Assert.Contains("</strong>", result);
    }

    [Fact]
    public void Tag_WithDate()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["em"] = content => $"<em>{content}</em>";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "date", new DateTime(2024, 3, 15) } };

        var result = formatter.FormatMessage("Date: <em>{date, date, short}</em>", args);

        Assert.StartsWith("Date: <em>", result);
        Assert.EndsWith("</em>", result);
    }

    #endregion

    #region Complex Scenarios Tests

    [Fact]
    public void Tag_AnsiColorHandler()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["red"] = content => $"\u001b[31m{content}\u001b[0m";
        options.TagHandlers["green"] = content => $"\u001b[32m{content}\u001b[0m";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "status", "Error" } };

        var result = formatter.FormatMessage("<red>{status}</red>: Something went wrong", args);

        Assert.Equal("\u001b[31mError\u001b[0m: Something went wrong", result);
    }

    [Fact]
    public void Tag_UppercaseTransform()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["upper"] = content => content.ToUpper();
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "name", "john" } };

        var result = formatter.FormatMessage("Hello <upper>{name}</upper>!", args);

        Assert.Equal("Hello JOHN!", result);
    }

    [Fact]
    public void Tag_ReverseTransform()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["reverse"] = content =>
        {
            var arr = content.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        };
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("<reverse>Hello</reverse>", args);

        Assert.Equal("olleH", result);
    }

    [Fact]
    public void Tag_WrapWithCount()
    {
        var count = 0;
        var options = TestOptions.WithEnglish();
        options.TagHandlers["counted"] = content =>
        {
            count++;
            return $"[{count}:{content}]";
        };
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage(
            "<counted>A</counted> <counted>B</counted> <counted>C</counted>",
            args);

        Assert.Equal("[1:A] [2:B] [3:C]", result);
    }

    [Fact]
    public void Tag_HtmlEscaping()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["safe"] = content =>
            content
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "input", "<script>alert('xss')</script>" } };

        var result = formatter.FormatMessage("<safe>{input}</safe>", args);

        Assert.Equal("&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;", result);
    }

    [Fact]
    public void Tag_Conditionally_ReturnsEmpty()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["hide"] = content => "";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("Before<hide>Hidden Content</hide>After", args);

        Assert.Equal("BeforeAfter", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Tag_HandlerReturnsNull()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["null"] = content => null!;
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        // Should not throw, but behavior depends on implementation
        var result = formatter.FormatMessage("<null>text</null>", args);

        Assert.NotNull(result);
    }

    [Fact]
    public void Tag_ContentWithSpecialCharacters()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["wrap"] = content => $"[{content}]";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("<wrap>Special: <>&\"</wrap>", args);

        Assert.Equal("[Special: <>&\"]", result);
    }

    [Fact]
    public void Tag_ContentWithNewlines()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["wrap"] = content => $"[{content}]";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var result = formatter.FormatMessage("<wrap>Line1\nLine2</wrap>", args);

        Assert.Equal("[Line1\nLine2]", result);
    }

    [Fact]
    public void Tag_HandlerThrowsException_Propagates()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["throws"] = content =>
            throw new InvalidOperationException("Test exception");
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        Assert.Throws<InvalidOperationException>(() =>
            formatter.FormatMessage("<throws>text</throws>", args));
    }

    [Fact]
    public void Tag_CaseInsensitiveMatching()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["Bold"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        // Tag name in message should match case-insensitively
        var result = formatter.FormatMessage("<bold>text</bold>", args);

        Assert.Equal("**text**", result);
    }

    #endregion

    #region Combined Custom Formatters and Tags Tests

    [Fact]
    public void CustomFormatterAndTag_Together()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["upper"] = (value, style, locale, culture) =>
            value?.ToString()?.ToUpper() ?? "";
        options.TagHandlers["bold"] = content => $"**{content}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>
        {
            { "name", "john" },
            { "price", 99.99m }
        };

        var result = formatter.FormatMessage(
            "Hello <bold>{name, upper}</bold>, total: {price, number}",
            args);

        Assert.Equal("Hello **JOHN**, total: 99.99", result);
    }

    [Fact]
    public void CustomFormatterInsideTag()
    {
        var options = TestOptions.WithEnglish();
        options.CustomFormatters["money"] = (value, style, locale, culture) =>
            "$" + Convert.ToDecimal(value).ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        options.TagHandlers["highlight"] = content => $">>>{content}<<<";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { { "price", 99.99m } };

        var result = formatter.FormatMessage("<highlight>{price, money}</highlight>", args);

        Assert.Equal(">>>$99.99<<<", result);
    }

    #endregion
}
