using System;
using System.Collections.Generic;
using Rh.MessageFormat.Exceptions;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Tests for parser error handling and edge cases.
/// </summary>
public class ParserErrorTests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region Missing Closing Brace Tests

    [Fact]
    public void Parser_MissingClosingBrace_ThrowsException()
    {
        var args = new Dictionary<string, object?> { { "name", "Test" } };

        Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("Hello {name", args));
    }

    [Fact]
    public void Parser_MissingClosingBraceInPlural_ThrowsException()
    {
        var args = new Dictionary<string, object?> { { "count", 1 } };

        Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("{count, plural, one {item other {items}}", args));
    }

    [Fact]
    public void Parser_MissingClosingBraceAfterFormatter_ThrowsException()
    {
        var args = new Dictionary<string, object?> { { "n", 123 } };

        Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("{n, number", args));
    }

    #endregion

    #region Plural/Select Argument Errors

    [Fact]
    public void Parser_PluralWithoutCases_ThrowsException()
    {
        var args = new Dictionary<string, object?> { { "count", 1 } };

        Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("{count, plural, }", args));
    }

    [Fact]
    public void Parser_SelectWithoutCases_ThrowsException()
    {
        var args = new Dictionary<string, object?> { { "type", "a" } };

        Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("{type, select, }", args));
    }

    [Fact]
    public void Parser_SelectOrdinalWithoutCases_ThrowsException()
    {
        var args = new Dictionary<string, object?> { { "n", 1 } };

        Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("{n, selectordinal, }", args));
    }

    [Fact]
    public void Parser_PluralCaseMissingBrace_ThrowsException()
    {
        var args = new Dictionary<string, object?> { { "count", 1 } };

        Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("{count, plural, one item other {items}}", args));
    }

    [Fact]
    public void Parser_SelectCaseMissingBrace_ThrowsException()
    {
        var args = new Dictionary<string, object?> { { "type", "a" } };

        Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("{type, select, a value b {other}}", args));
    }

    #endregion

    #region Tag Error Tests

    [Fact]
    public void Parser_UnclosedTag_ThrowsException()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = c => $"**{c}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatMessage("<bold>text", args));
    }

    [Fact]
    public void Parser_MismatchedTag_ThrowsException()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = c => $"**{c}**";
        options.TagHandlers["italic"] = c => $"*{c}*";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatMessage("<bold>text</italic>", args));
    }

    [Fact]
    public void Parser_TagMissingClosingBracket_ThrowsException()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = c => $"**{c}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatMessage("<bold text</bold>", args));
    }

    [Fact]
    public void Parser_EmptyTagName_NotATag()
    {
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());
        var args = new Dictionary<string, object?>();

        // <> is not considered a tag start (no letter after <)
        // so it should parse as literal text
        var result = formatter.FormatMessage("<> test", args);
        Assert.Equal("<> test", result);
    }

    #endregion

    #region Valid Edge Cases

    [Fact]
    public void Parser_EmptyPattern_ReturnsEmpty()
    {
        var args = new Dictionary<string, object?>();

        var result = _formatter.FormatMessage("", args);

        Assert.Equal("", result);
    }

    [Fact]
    public void Parser_OnlyWhitespace_ReturnsWhitespace()
    {
        var args = new Dictionary<string, object?>();

        var result = _formatter.FormatMessage("   ", args);

        Assert.Equal("   ", result);
    }

    [Fact]
    public void Parser_NestedBraces_ParsesCorrectly()
    {
        var args = new Dictionary<string, object?> { { "count", 2 } };

        var result = _formatter.FormatMessage(
            "{count, plural, one {You have {count} item} other {You have {count} items}}",
            args);

        Assert.Equal("You have 2 items", result);
    }

    [Fact]
    public void Parser_EscapedQuotes_ParsesCorrectly()
    {
        var args = new Dictionary<string, object?>();

        var result = _formatter.FormatMessage("It''s a test", args);

        Assert.Equal("It's a test", result);
    }

    [Fact]
    public void Parser_EscapedBraces_ParsesCorrectly()
    {
        var args = new Dictionary<string, object?>();

        var result = _formatter.FormatMessage("Use '{ and '}' for placeholders", args);

        Assert.Contains("{", result);
    }

    [Fact]
    public void Parser_LessThanWithSpace_NotATag()
    {
        var args = new Dictionary<string, object?>();

        var result = _formatter.FormatMessage("5 < 10", args);

        Assert.Equal("5 < 10", result);
    }

    [Fact]
    public void Parser_LessThanWithDigit_NotATag()
    {
        var args = new Dictionary<string, object?>();

        var result = _formatter.FormatMessage("x<5", args);

        Assert.Equal("x<5", result);
    }

    [Fact]
    public void Parser_MultilinePattern_ParsesCorrectly()
    {
        var args = new Dictionary<string, object?> { { "name", "World" } };

        var result = _formatter.FormatMessage("Hello\n{name}!", args);

        Assert.Equal("Hello\nWorld!", result);
    }

    [Fact]
    public void Parser_VariableWithUnderscore_ParsesCorrectly()
    {
        var args = new Dictionary<string, object?> { { "user_name", "John" } };

        var result = _formatter.FormatMessage("Hello {user_name}!", args);

        Assert.Equal("Hello John!", result);
    }

    [Fact]
    public void Parser_VariableWithHyphen_ParsesCorrectly()
    {
        var args = new Dictionary<string, object?> { { "user-name", "Jane" } };

        var result = _formatter.FormatMessage("Hello {user-name}!", args);

        Assert.Equal("Hello Jane!", result);
    }

    [Fact]
    public void Parser_VariableWithNumber_ParsesCorrectly()
    {
        var args = new Dictionary<string, object?> { { "item1", "First" } };

        var result = _formatter.FormatMessage("Item: {item1}", args);

        Assert.Equal("Item: First", result);
    }

    #endregion

    #region Offset Parsing Tests

    [Fact]
    public void Parser_PluralOffsetInteger_ParsesCorrectly()
    {
        var args = new Dictionary<string, object?> { { "count", 5 } };

        var result = _formatter.FormatMessage(
            "{count, plural, offset:2 one {# person} other {# people}}",
            args);

        // 5 - 2 = 3, which is "other"
        Assert.Contains("3", result);
        Assert.Contains("people", result);
    }

    [Fact]
    public void Parser_PluralOffsetNegative_ParsesCorrectly()
    {
        var args = new Dictionary<string, object?> { { "count", 0 } };

        var result = _formatter.FormatMessage(
            "{count, plural, offset:-1 one {# item} other {# items}}",
            args);

        Assert.Contains("1", result);
    }

    #endregion

    #region Deeply Nested Patterns

    [Fact]
    public void Parser_DeeplyNestedPlural_ParsesCorrectly()
    {
        var args = new Dictionary<string, object?>
        {
            { "outer", 2 },
            { "inner", 1 }
        };

        var result = _formatter.FormatMessage(
            "{outer, plural, one {One outer with {inner, plural, one {one inner} other {# inners}}} other {# outers with {inner, plural, one {one inner} other {# inners}}}}",
            args);

        Assert.Contains("2 outers", result);
        Assert.Contains("one inner", result);
    }

    [Fact]
    public void Parser_SelectInsidePlural_ParsesCorrectly()
    {
        var args = new Dictionary<string, object?>
        {
            { "count", 1 },
            { "gender", "female" }
        };

        var result = _formatter.FormatMessage(
            "{count, plural, one {{gender, select, male {He has # item} female {She has # item} other {They have # item}}} other {{gender, select, male {He has # items} female {She has # items} other {They have # items}}}}",
            args);

        Assert.Equal("She has 1 item", result);
    }

    #endregion

    #region Whitespace Handling

    [Fact]
    public void Parser_WhitespaceAroundVariable_ParsesCorrectly()
    {
        var args = new Dictionary<string, object?> { { "name", "Test" } };

        var result = _formatter.FormatMessage("{  name  }", args);

        Assert.Equal("Test", result);
    }

    [Fact]
    public void Parser_WhitespaceAroundFormatter_ParsesCorrectly()
    {
        var args = new Dictionary<string, object?> { { "n", 123 } };

        var result = _formatter.FormatMessage("{  n  ,  number  }", args);

        Assert.Equal("123", result);
    }

    [Fact]
    public void Parser_WhitespaceInPluralCases_ParsesCorrectly()
    {
        var args = new Dictionary<string, object?> { { "count", 1 } };

        var result = _formatter.FormatMessage(
            "{count, plural,   one   {one item}   other   {# items}   }",
            args);

        Assert.Equal("one item", result);
    }

    #endregion
}
