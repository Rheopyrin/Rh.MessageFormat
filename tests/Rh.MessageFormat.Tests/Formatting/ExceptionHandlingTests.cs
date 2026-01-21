using System;
using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Formatting;

/// <summary>
/// Tests for exception handling in MessageFormatter.
/// Tests that MessageFormatterException is thrown for invalid messages and missing required options.
/// </summary>
public class ExceptionHandlingTests
{
    private readonly MessageFormatter _formatter;

    public ExceptionHandlingTests()
    {
        var options = TestOptions.WithEnglish();
        _formatter = new MessageFormatter("en", options);
    }

    #region Parse-time Exception Tests - Invalid Message Syntax

    [Fact]
    public void FormatMessage_UnclosedPlaceholder_ThrowsMessageFormatterException()
    {
        var args = new Dictionary<string, object?> { ["name"] = "John" };

        var ex = Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("Hello {name", args));

        Assert.Contains("closing brace", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatMessage_UnclosedPlaceholderWithFormatter_ThrowsMessageFormatterException()
    {
        var args = new Dictionary<string, object?> { ["count"] = 5 };

        var ex = Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("You have {count, number", args));

        Assert.Contains("closing brace", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatMessage_PluralWithoutCases_ThrowsMessageFormatterException()
    {
        var args = new Dictionary<string, object?> { ["count"] = 5 };

        var ex = Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("{count, plural, }", args));

        Assert.Contains("requires cases", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatMessage_PluralWithEmptyArguments_ThrowsMessageFormatterException()
    {
        var args = new Dictionary<string, object?> { ["count"] = 5 };

        var ex = Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("{count, plural,}", args));

        Assert.Contains("requires cases", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatMessage_SelectWithoutCases_ThrowsMessageFormatterException()
    {
        var args = new Dictionary<string, object?> { ["gender"] = "male" };

        var ex = Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("{gender, select, }", args));

        Assert.Contains("requires cases", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatMessage_SelectOrdinalWithoutCases_ThrowsMessageFormatterException()
    {
        var args = new Dictionary<string, object?> { ["position"] = 1 };

        var ex = Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("{position, selectordinal, }", args));

        Assert.Contains("requires cases", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatMessage_PluralCaseMissingOpenBrace_ThrowsMessageFormatterException()
    {
        var args = new Dictionary<string, object?> { ["count"] = 5 };

        var ex = Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("{count, plural, one item other {items}}", args));

        Assert.Contains("Expected '{'", ex.Message);
    }

    [Fact]
    public void FormatMessage_SelectCaseMissingOpenBrace_ThrowsMessageFormatterException()
    {
        var args = new Dictionary<string, object?> { ["type"] = "a" };

        var ex = Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("{type, select, a value other {default}}", args));

        Assert.Contains("Expected '{'", ex.Message);
    }

    [Fact]
    public void FormatMessage_NestedUnclosedPlaceholder_ThrowsMessageFormatterException()
    {
        var args = new Dictionary<string, object?> { ["count"] = 5 };

        Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("{count, plural, one {# item other {# items}}", args));
    }

    [Fact]
    public void FormatMessage_UnclosedNestedBrace_ThrowsMessageFormatterException()
    {
        var args = new Dictionary<string, object?> { ["count"] = 5, ["name"] = "Test" };

        Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("{count, plural, one {{name} other {items}}", args));
    }

    #endregion

    #region Parse-time Exception Tests - Rich Text Tags

    [Fact]
    public void FormatMessage_UnclosedTag_ThrowsMessageFormatterException()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = c => $"**{c}**";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var ex = Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatMessage("<bold>text without closing tag", args));

        Assert.Contains("Unclosed tag", ex.Message);
    }

    [Fact]
    public void FormatMessage_MismatchedClosingTag_ThrowsMessageFormatterException()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["bold"] = c => $"**{c}**";
        options.TagHandlers["italic"] = c => $"*{c}*";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        // Mismatched tag causes "Unclosed tag" error for the opening tag
        // because it looks for </bold> but finds </italic> which it skips looking for <italic> start
        var ex = Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatMessage("<bold>text</italic>", args));

        // The parser sees </italic> and interprets it as looking for tag name after </
        // Since 'italic' != 'bold', it continues and eventually reaches end without finding </bold>
        Assert.True(
            ex.Message.Contains("Unclosed tag") || ex.Message.Contains("Expected tag name"),
            $"Expected error about unclosed tag or tag name, got: {ex.Message}");
    }

    [Fact]
    public void FormatMessage_TagMissingClosingBracket_ThrowsMessageFormatterException()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["link"] = c => $"[{c}]";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var ex = Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatMessage("<link content</link>", args));

        Assert.Contains("Expected '>'", ex.Message);
    }

    [Fact]
    public void FormatMessage_NestedUnclosedTag_ThrowsMessageFormatterException()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["outer"] = c => $"[{c}]";
        options.TagHandlers["inner"] = c => $"({c})";
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatMessage("<outer><inner>text</outer>", args));
    }

    #endregion

    #region Runtime Exception Tests - Missing 'other' Case

    [Fact]
    public void FormatMessage_PluralWithoutOther_ThrowsMessageFormatterException()
    {
        var args = new Dictionary<string, object?> { ["count"] = 5 };

        // Pattern has 'one' but no 'other' - should throw when count doesn't match 'one'
        var ex = Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("{count, plural, one {# item}}", args));

        Assert.Contains("'other' option not found", ex.Message);
    }

    [Fact]
    public void FormatMessage_PluralWithoutOther_CountIsOne_Succeeds()
    {
        var args = new Dictionary<string, object?> { ["count"] = 1 };

        // When count is 1, 'one' case matches, so no exception even without 'other'
        var result = _formatter.FormatMessage("{count, plural, one {# item}}", args);

        Assert.Equal("1 item", result);
    }

    [Fact]
    public void FormatMessage_SelectWithoutOther_ThrowsMessageFormatterException()
    {
        var args = new Dictionary<string, object?> { ["type"] = "unknown" };

        // Pattern has 'a' and 'b' but no 'other' - should throw when type doesn't match
        var ex = Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("{type, select, a {Option A} b {Option B}}", args));

        Assert.Contains("'other' option not found", ex.Message);
    }

    [Fact]
    public void FormatMessage_SelectWithoutOther_MatchingValue_Succeeds()
    {
        var args = new Dictionary<string, object?> { ["type"] = "a" };

        // When type matches 'a', no exception even without 'other'
        var result = _formatter.FormatMessage("{type, select, a {Option A} b {Option B}}", args);

        Assert.Equal("Option A", result);
    }

    [Fact]
    public void FormatMessage_SelectOrdinalWithoutOther_ThrowsMessageFormatterException()
    {
        var args = new Dictionary<string, object?> { ["position"] = 5 };

        // Pattern has 'one' but no 'other' - should throw when ordinal form doesn't match
        var ex = Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("{position, selectordinal, one {#st}}", args));

        Assert.Contains("'other' option not found", ex.Message);
    }

    [Fact]
    public void FormatMessage_SelectOrdinalWithoutOther_MatchingValue_Succeeds()
    {
        var args = new Dictionary<string, object?> { ["position"] = 1 };

        // When position is 1, 'one' ordinal case matches
        var result = _formatter.FormatMessage("{position, selectordinal, one {#st}}", args);

        Assert.Equal("1st", result);
    }

    #endregion

    #region Missing Variable Tests - Graceful Handling (No Exception)

    [Fact]
    public void FormatMessage_MissingVariable_ReturnsEmptyString()
    {
        var args = new Dictionary<string, object?>();

        // Missing variable doesn't throw - it returns empty
        var result = _formatter.FormatMessage("Hello {name}!", args);

        Assert.Equal("Hello !", result);
    }

    [Fact]
    public void FormatMessage_MissingVariableWithOtherVariables_ReturnsPartialResult()
    {
        var args = new Dictionary<string, object?> { ["greeting"] = "Hello" };

        var result = _formatter.FormatMessage("{greeting} {name}!", args);

        Assert.Equal("Hello !", result);
    }

    [Fact]
    public void FormatMessage_MissingNumericVariable_TreatedAsZero()
    {
        var args = new Dictionary<string, object?>();

        // For plural, missing variable is treated as 0
        var result = _formatter.FormatMessage("{count, plural, =0 {No items} one {# item} other {# items}}", args);

        Assert.Equal("No items", result);
    }

    [Fact]
    public void FormatMessage_MissingNumericVariableWithoutZeroCase_UsesOther()
    {
        var args = new Dictionary<string, object?>();

        // For plural, missing variable is treated as 0, which should use 'other'
        var result = _formatter.FormatMessage("{count, plural, one {# item} other {# items}}", args);

        Assert.Equal("0 items", result);
    }

    [Fact]
    public void FormatMessage_MissingSelectVariable_TreatedAsEmptyString()
    {
        var args = new Dictionary<string, object?>();

        // For select, missing variable is treated as empty string
        var result = _formatter.FormatMessage("{type, select, a {Option A} other {Default}}", args);

        Assert.Equal("Default", result);
    }

    [Fact]
    public void FormatMessage_NullVariable_TreatedAsEmpty()
    {
        var args = new Dictionary<string, object?> { ["name"] = null };

        var result = _formatter.FormatMessage("Hello {name}!", args);

        Assert.Equal("Hello !", result);
    }

    [Fact]
    public void FormatMessage_NullVariableInPlural_TreatedAsZero()
    {
        var args = new Dictionary<string, object?> { ["count"] = null };

        var result = _formatter.FormatMessage("{count, plural, =0 {Zero} one {One} other {Many}}", args);

        Assert.Equal("Zero", result);
    }

    #endregion

    #region Exception Message Quality Tests

    [Fact]
    public void FormatMessage_ParseError_IncludesLineAndColumn()
    {
        var args = new Dictionary<string, object?>();

        var ex = Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("Line one\n{unclosed", args));

        Assert.Contains("line", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("column", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatMessage_TagError_IncludesTagName()
    {
        var options = TestOptions.WithEnglish();
        options.TagHandlers["myTag"] = c => c;
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var ex = Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatMessage("<myTag>unclosed content", args));

        Assert.Contains("myTag", ex.Message);
    }

    [Fact]
    public void FormatMessage_PluralError_IndicatesPluralContext()
    {
        var args = new Dictionary<string, object?> { ["n"] = 5 };

        var ex = Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("{n, plural, one {item}}", args));

        Assert.Contains("plural", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatMessage_SelectError_IndicatesSelectContext()
    {
        var args = new Dictionary<string, object?> { ["t"] = "x" };

        var ex = Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage("{t, select, a {A}}", args));

        Assert.Contains("select", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Constructor Exception Tests

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new MessageFormatter("en", null!));
    }

    #endregion

    #region FormatComplexMessage Exception Tests

    [Fact]
    public void FormatComplexMessage_InvalidPattern_ThrowsMessageFormatterException()
    {
        var values = new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>
            {
                ["name"] = "John"
            }
        };

        var ex = Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatComplexMessage("Hello {user__name", values));

        Assert.Contains("closing brace", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatComplexMessage_MissingNestedValue_ReturnsEmpty()
    {
        var values = new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>
            {
                ["name"] = "John"
            }
        };

        // user__email doesn't exist - returns empty
        var result = _formatter.FormatComplexMessage("Email: {user__email}", values);

        Assert.Equal("Email: ", result);
    }

    #endregion

    #region FormatHtmlMessage Exception Tests

    [Fact]
    public void FormatHtmlMessage_InvalidPattern_ThrowsMessageFormatterException()
    {
        var values = new Dictionary<string, object?> { ["name"] = "John" };

        var ex = Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatHtmlMessage("<div>{name</div>", values));

        Assert.Contains("closing brace", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatHtmlMessage_MissingVariable_ReturnsEmptyString()
    {
        var values = new Dictionary<string, object?>();

        // Missing variable doesn't throw - HTML tags preserved, value empty
        var result = _formatter.FormatHtmlMessage("<b>{name}</b>", values);

        Assert.Equal("<b></b>", result);
    }

    [Fact]
    public void FormatHtmlMessage_PluralWithoutOther_ThrowsMessageFormatterException()
    {
        var values = new Dictionary<string, object?> { ["count"] = 5 };

        var ex = Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatHtmlMessage("<span>{count, plural, one {# item}}</span>", values));

        Assert.Contains("'other' option not found", ex.Message);
    }

    #endregion

    #region Complex Nested Pattern Exception Tests

    [Fact]
    public void FormatMessage_DeeplyNestedUnclosedBrace_ThrowsMessageFormatterException()
    {
        var args = new Dictionary<string, object?>
        {
            ["count"] = 2,
            ["gender"] = "male"
        };

        Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage(
                "{count, plural, one {{gender, select, male {He has # item} other {other}}} other {{gender, select, male {He has # items other {other}}}}",
                args));
    }

    [Fact]
    public void FormatMessage_PluralInsideSelectWithoutOther_ThrowsMessageFormatterException()
    {
        var args = new Dictionary<string, object?>
        {
            ["gender"] = "unknown",
            ["count"] = 5
        };

        var ex = Assert.Throws<MessageFormatterException>(() =>
            _formatter.FormatMessage(
                "{gender, select, male {{count, plural, one {He has # item} other {He has # items}}} female {{count, plural, one {She has # item} other {She has # items}}}}",
                args));

        Assert.Contains("'other' option not found", ex.Message);
    }

    #endregion

    #region RequireAllVariables Option Tests

    [Fact]
    public void FormatMessage_RequireAllVariables_MissingVariable_ThrowsMessageFormatterException()
    {
        var options = TestOptions.WithEnglish();
        options.RequireAllVariables = true;
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var ex = Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatMessage("Hello {name}!", args));

        Assert.Contains("Missing required variable", ex.Message);
        Assert.Contains("name", ex.Message);
    }

    [Fact]
    public void FormatMessage_RequireAllVariables_AllVariablesPresent_Succeeds()
    {
        var options = TestOptions.WithEnglish();
        options.RequireAllVariables = true;
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { ["name"] = "John" };

        var result = formatter.FormatMessage("Hello {name}!", args);

        Assert.Equal("Hello John!", result);
    }

    [Fact]
    public void FormatMessage_RequireAllVariables_NullValue_DoesNotThrow()
    {
        var options = TestOptions.WithEnglish();
        options.RequireAllVariables = true;
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { ["name"] = null };

        // Variable is present but null - should not throw
        var result = formatter.FormatMessage("Hello {name}!", args);

        Assert.Equal("Hello !", result);
    }

    [Fact]
    public void FormatMessage_RequireAllVariables_MissingPluralVariable_ThrowsMessageFormatterException()
    {
        var options = TestOptions.WithEnglish();
        options.RequireAllVariables = true;
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var ex = Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatMessage("{count, plural, one {# item} other {# items}}", args));

        Assert.Contains("Missing required variable", ex.Message);
        Assert.Contains("count", ex.Message);
    }

    [Fact]
    public void FormatMessage_RequireAllVariables_MissingSelectVariable_ThrowsMessageFormatterException()
    {
        var options = TestOptions.WithEnglish();
        options.RequireAllVariables = true;
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var ex = Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatMessage("{gender, select, male {He} female {She} other {They}}", args));

        Assert.Contains("Missing required variable", ex.Message);
        Assert.Contains("gender", ex.Message);
    }

    [Fact]
    public void FormatMessage_RequireAllVariables_MultipleMissingVariables_ThrowsForFirst()
    {
        var options = TestOptions.WithEnglish();
        options.RequireAllVariables = true;
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var ex = Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatMessage("Hello {firstName} {lastName}!", args));

        // Should throw for the first missing variable encountered
        Assert.Contains("Missing required variable", ex.Message);
        Assert.Contains("firstName", ex.Message);
    }

    [Fact]
    public void FormatMessage_RequireAllVariables_PartiallyMissingVariables_ThrowsForMissing()
    {
        var options = TestOptions.WithEnglish();
        options.RequireAllVariables = true;
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { ["firstName"] = "John" };

        var ex = Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatMessage("Hello {firstName} {lastName}!", args));

        Assert.Contains("Missing required variable", ex.Message);
        Assert.Contains("lastName", ex.Message);
    }

    [Fact]
    public void FormatMessage_RequireAllVariables_False_MissingVariable_ReturnsEmpty()
    {
        var options = TestOptions.WithEnglish();
        options.RequireAllVariables = false; // Default
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        // Should NOT throw - returns empty for missing variable
        var result = formatter.FormatMessage("Hello {name}!", args);

        Assert.Equal("Hello !", result);
    }

    [Fact]
    public void FormatMessage_RequireAllVariables_NoVariablesInPattern_Succeeds()
    {
        var options = TestOptions.WithEnglish();
        options.RequireAllVariables = true;
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        // No variables in pattern - should succeed
        var result = formatter.FormatMessage("Hello World!", args);

        Assert.Equal("Hello World!", result);
    }

    [Fact]
    public void FormatMessage_RequireAllVariables_NumberFormat_MissingVariable_ThrowsMessageFormatterException()
    {
        var options = TestOptions.WithEnglish();
        options.RequireAllVariables = true;
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var ex = Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatMessage("Total: {amount, number}", args));

        Assert.Contains("Missing required variable", ex.Message);
        Assert.Contains("amount", ex.Message);
    }

    [Fact]
    public void FormatMessage_RequireAllVariables_DateFormat_MissingVariable_ThrowsMessageFormatterException()
    {
        var options = TestOptions.WithEnglish();
        options.RequireAllVariables = true;
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var ex = Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatMessage("Date: {date, date}", args));

        Assert.Contains("Missing required variable", ex.Message);
        Assert.Contains("date", ex.Message);
    }

    [Fact]
    public void FormatComplexMessage_RequireAllVariables_MissingNestedVariable_ThrowsMessageFormatterException()
    {
        var options = TestOptions.WithEnglish();
        options.RequireAllVariables = true;
        var formatter = new MessageFormatter("en", options);
        var values = new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>
            {
                ["firstName"] = "John"
            }
        };

        var ex = Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatComplexMessage("Hello {user__firstName} {user__lastName}!", values));

        Assert.Contains("Missing required variable", ex.Message);
        Assert.Contains("user__lastName", ex.Message);
    }

    [Fact]
    public void FormatHtmlMessage_RequireAllVariables_MissingVariable_ThrowsMessageFormatterException()
    {
        var options = TestOptions.WithEnglish();
        options.RequireAllVariables = true;
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?>();

        var ex = Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatHtmlMessage("<b>{name}</b>", args));

        Assert.Contains("Missing required variable", ex.Message);
        Assert.Contains("name", ex.Message);
    }

    [Fact]
    public void FormatHtmlMessage_RequireAllVariables_AllVariablesPresent_Succeeds()
    {
        var options = TestOptions.WithEnglish();
        options.RequireAllVariables = true;
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { ["name"] = "John" };

        var result = formatter.FormatHtmlMessage("<b>{name}</b>", args);

        Assert.Equal("<b>John</b>", result);
    }

    [Fact]
    public void FormatMessage_RequireAllVariables_NestedPluralMissingInnerVariable_ThrowsMessageFormatterException()
    {
        var options = TestOptions.WithEnglish();
        options.RequireAllVariables = true;
        var formatter = new MessageFormatter("en", options);
        var args = new Dictionary<string, object?> { ["count"] = 2 };

        var ex = Assert.Throws<MessageFormatterException>(() =>
            formatter.FormatMessage("{count, plural, one {# item for {name}} other {# items for {name}}}", args));

        Assert.Contains("Missing required variable", ex.Message);
        Assert.Contains("name", ex.Message);
    }

    [Fact]
    public void FormatMessage_RequireAllVariables_DefaultIsFalse()
    {
        var options = new MessageFormatterOptions();

        Assert.False(options.RequireAllVariables);
    }

    #endregion

    #region Edge Cases That Should Not Throw

    [Fact]
    public void FormatMessage_EmptyPattern_ReturnsEmptyString()
    {
        var args = new Dictionary<string, object?>();

        var result = _formatter.FormatMessage("", args);

        Assert.Equal("", result);
    }

    [Fact]
    public void FormatMessage_EmptyArgs_WithLiteralsOnly_Succeeds()
    {
        var args = new Dictionary<string, object?>();

        var result = _formatter.FormatMessage("Hello World!", args);

        Assert.Equal("Hello World!", result);
    }

    [Fact]
    public void FormatMessage_WhitespaceOnlyPattern_ReturnsWhitespace()
    {
        var args = new Dictionary<string, object?>();

        var result = _formatter.FormatMessage("   \n\t   ", args);

        Assert.Equal("   \n\t   ", result);
    }

    [Fact]
    public void FormatMessage_LessThanSymbol_NotTag_NoException()
    {
        var args = new Dictionary<string, object?>();

        // < followed by space or digit is not a tag
        var result = _formatter.FormatMessage("5 < 10 and 3 <4", args);

        Assert.Equal("5 < 10 and 3 <4", result);
    }

    [Fact]
    public void FormatMessage_UnregisteredTag_TreatedAsLiteral()
    {
        var args = new Dictionary<string, object?>();

        // Tags without handlers are treated as literal text when ignoreTag is true (FormatHtmlMessage)
        // or parsed as tags (which may throw if unclosed)
        var result = _formatter.FormatHtmlMessage("<unknown>content</unknown>", args);

        Assert.Equal("<unknown>content</unknown>", result);
    }

    [Fact]
    public void FormatMessage_ValidPluralWithAllCases_NoException()
    {
        var args = new Dictionary<string, object?> { ["count"] = 5 };

        var result = _formatter.FormatMessage(
            "{count, plural, =0 {none} one {# item} other {# items}}",
            args);

        Assert.Equal("5 items", result);
    }

    [Fact]
    public void FormatMessage_ValidSelectWithOther_NoException()
    {
        var args = new Dictionary<string, object?> { ["type"] = "unknown" };

        var result = _formatter.FormatMessage(
            "{type, select, a {Option A} b {Option B} other {Unknown}}",
            args);

        Assert.Equal("Unknown", result);
    }

    #endregion
}
