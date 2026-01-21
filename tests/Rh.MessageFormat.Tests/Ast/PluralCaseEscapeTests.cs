using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Tests for PluralCase escape sequence handling and # replacement logic.
/// </summary>
public class PluralCaseEscapeTests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region Hash Replacement Tests

    [Fact]
    public void Plural_HashReplacement_Basic()
    {
        var args = new Dictionary<string, object?> { { "count", 5 } };

        var result = _formatter.FormatMessage(
            "{count, plural, one {# item} other {# items}}",
            args);

        Assert.Equal("5 items", result);
    }

    [Fact]
    public void Plural_MultipleHashes_AllReplaced()
    {
        var args = new Dictionary<string, object?> { { "count", 3 } };

        var result = _formatter.FormatMessage(
            "{count, plural, other {# + # = #}}",
            args);

        Assert.Equal("3 + 3 = 3", result);
    }

    [Fact]
    public void Plural_HashWithOffset_UsesOffsetValue()
    {
        var args = new Dictionary<string, object?> { { "count", 5 } };

        var result = _formatter.FormatMessage(
            "{count, plural, offset:2 other {# more}}",
            args);

        // 5 - 2 = 3
        Assert.Equal("3 more", result);
    }

    [Fact]
    public void Plural_HashAtStart()
    {
        var args = new Dictionary<string, object?> { { "n", 7 } };

        var result = _formatter.FormatMessage(
            "{n, plural, other {# is the number}}",
            args);

        Assert.Equal("7 is the number", result);
    }

    [Fact]
    public void Plural_HashAtEnd()
    {
        var args = new Dictionary<string, object?> { { "n", 7 } };

        var result = _formatter.FormatMessage(
            "{n, plural, other {The number is #}}",
            args);

        Assert.Equal("The number is 7", result);
    }

    [Fact]
    public void Plural_HashAlone()
    {
        var args = new Dictionary<string, object?> { { "n", 42 } };

        var result = _formatter.FormatMessage(
            "{n, plural, other {#}}",
            args);

        Assert.Equal("42", result);
    }

    #endregion

    #region Escape Sequence Tests

    [Fact]
    public void Plural_EscapedQuote_ProducesQuote()
    {
        var args = new Dictionary<string, object?> { { "count", 5 } };

        var result = _formatter.FormatMessage(
            "{count, plural, other {It''s # items}}",
            args);

        Assert.Contains("It's", result);
        Assert.Contains("5", result);
    }

    [Fact]
    public void Plural_HashInsideNestedBraces_OuterReplaced()
    {
        var args = new Dictionary<string, object?>
        {
            { "count", 5 },
            { "name", "test" }
        };

        var result = _formatter.FormatMessage(
            "{count, plural, other {{name} has # items}}",
            args);

        // The # outside nested braces should be replaced
        Assert.Equal("test has 5 items", result);
    }

    [Fact]
    public void Plural_DoubleQuote_EscapedCorrectly()
    {
        var args = new Dictionary<string, object?> { { "count", 5 } };

        var result = _formatter.FormatMessage(
            "{count, plural, other {# items (that''s it)}}",
            args);

        Assert.Contains("5 items", result);
        Assert.Contains("that's it", result);
    }

    #endregion

    #region Exact Match Tests

    [Fact]
    public void Plural_ExactMatch_Integer()
    {
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = _formatter.FormatMessage(
            "{n, plural, =5 {exactly five} other {other}}",
            args);

        Assert.Equal("exactly five", result);
    }

    [Fact]
    public void Plural_ExactMatch_Zero()
    {
        var args = new Dictionary<string, object?> { { "n", 0 } };

        var result = _formatter.FormatMessage(
            "{n, plural, =0 {none} other {some}}",
            args);

        Assert.Equal("none", result);
    }

    [Fact]
    public void Plural_ExactMatch_Negative()
    {
        var args = new Dictionary<string, object?> { { "n", -1 } };

        var result = _formatter.FormatMessage(
            "{n, plural, =-1 {minus one} other {other}}",
            args);

        Assert.Equal("minus one", result);
    }

    [Fact]
    public void Plural_ExactMatch_Decimal()
    {
        var args = new Dictionary<string, object?> { { "n", 2.5 } };

        var result = _formatter.FormatMessage(
            "{n, plural, =2.5 {two and a half} other {other}}",
            args);

        Assert.Equal("two and a half", result);
    }

    [Fact]
    public void Plural_ExactMatch_TakesPrecedence()
    {
        var args = new Dictionary<string, object?> { { "n", 1 } };

        var result = _formatter.FormatMessage(
            "{n, plural, =1 {exactly one} one {one item} other {other}}",
            args);

        // Exact match =1 should take precedence over category "one"
        Assert.Equal("exactly one", result);
    }

    [Fact]
    public void Plural_ExactMatch_WithHash()
    {
        var args = new Dictionary<string, object?> { { "n", 100 } };

        var result = _formatter.FormatMessage(
            "{n, plural, =100 {exactly # (one hundred)} other {other}}",
            args);

        Assert.Equal("exactly 100 (one hundred)", result);
    }

    #endregion

    #region Complex Nested Content Tests

    [Fact]
    public void Plural_NestedPlural_HashInOuter()
    {
        var args = new Dictionary<string, object?>
        {
            { "files", 3 },
            { "folders", 2 }
        };

        var result = _formatter.FormatMessage(
            "{files, plural, other {# files in {folders, plural, one {# folder} other {# folders}}}}",
            args);

        Assert.Equal("3 files in 2 folders", result);
    }

    [Fact]
    public void Plural_NestedSelect_HashWorks()
    {
        var args = new Dictionary<string, object?>
        {
            { "count", 5 },
            { "type", "book" }
        };

        var result = _formatter.FormatMessage(
            "{count, plural, other {# {type, select, book {books} other {items}}}}",
            args);

        Assert.Equal("5 books", result);
    }

    [Fact]
    public void Plural_DeepNesting_HashCorrectlyReplaced()
    {
        var args = new Dictionary<string, object?>
        {
            { "outer", 10 },
            { "inner", 5 }
        };

        var result = _formatter.FormatMessage(
            "{outer, plural, other {Outer: # {inner, plural, other {Inner: #}}}}",
            args);

        Assert.Equal("Outer: 10 Inner: 5", result);
    }

    #endregion

    #region SelectOrdinal Hash Tests

    [Fact]
    public void SelectOrdinal_HashReplacement()
    {
        var args = new Dictionary<string, object?> { { "n", 1 } };

        var result = _formatter.FormatMessage(
            "{n, selectordinal, one {#st} two {#nd} few {#rd} other {#th}}",
            args);

        Assert.Equal("1st", result);
    }

    [Fact]
    public void SelectOrdinal_HashWithOffset()
    {
        var args = new Dictionary<string, object?> { { "n", 3 } };

        var result = _formatter.FormatMessage(
            "{n, selectordinal, offset:1 one {#st} two {#nd} few {#rd} other {#th}}",
            args);

        // 3 - 1 = 2, ordinal for 2 is "two" -> "2nd"
        Assert.Equal("2nd", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Plural_EmptyContent()
    {
        var args = new Dictionary<string, object?> { { "n", 0 } };

        var result = _formatter.FormatMessage(
            "{n, plural, =0 {} other {has items}}",
            args);

        Assert.Equal("", result);
    }

    [Fact]
    public void Plural_OnlyHash()
    {
        var args = new Dictionary<string, object?> { { "n", 123 } };

        var result = _formatter.FormatMessage(
            "{n, plural, other {#}}",
            args);

        Assert.Equal("123", result);
    }

    [Fact]
    public void Plural_ConsecutiveHashes()
    {
        var args = new Dictionary<string, object?> { { "n", 7 } };

        var result = _formatter.FormatMessage(
            "{n, plural, other {##}}",
            args);

        Assert.Equal("77", result);
    }

    [Fact]
    public void Plural_HashWithSpecialCharacters()
    {
        var args = new Dictionary<string, object?> { { "n", 5 } };

        // Note: Single quote is an escape character in ICU MessageFormat
        // So we test without it, or use '' for a literal quote
        var result = _formatter.FormatMessage(
            "{n, plural, other {#: <>&\"}}",
            args);

        Assert.Equal("5: <>&\"", result);
    }

    [Fact]
    public void Plural_HashWithNewlines()
    {
        var args = new Dictionary<string, object?> { { "n", 5 } };

        var result = _formatter.FormatMessage(
            "{n, plural, other {Line1: #\nLine2}}",
            args);

        Assert.Contains("Line1: 5", result);
        Assert.Contains("Line2", result);
    }

    #endregion
}
