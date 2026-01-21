using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Additional tests for plural and select formatting edge cases.
/// </summary>
public class PluralSelectEdgeCaseTests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region Plural Exact Match Tests

    [Theory]
    [InlineData(0, "zero items")]
    [InlineData(1, "one item")]
    [InlineData(2, "two items")]
    [InlineData(3, "3 items")]
    [InlineData(100, "100 items")]
    public void Plural_ExactMatch_TakesPrecedence(int count, string expected)
    {
        var args = new Dictionary<string, object?> { { "count", count } };

        var result = _formatter.FormatMessage(
            "{count, plural, =0 {zero items} =1 {one item} =2 {two items} one {# item} other {# items}}",
            args);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Plural_ExactMatchWithDecimal_05()
    {
        var args = new Dictionary<string, object?> { { "count", 0.5 } };

        var result = _formatter.FormatMessage(
            "{count, plural, =0 {zero items} =2 {two items} one {# item} other {# items}}",
            args);

        // Decimal separator can vary by culture
        Assert.Contains("5 items", result);
        Assert.StartsWith("0", result);
    }

    [Fact]
    public void Plural_ExactMatchWithDecimal_15()
    {
        var args = new Dictionary<string, object?> { { "count", 1.5 } };

        var result = _formatter.FormatMessage(
            "{count, plural, =0 {zero items} =2 {two items} one {# item} other {# items}}",
            args);

        // Decimal separator can vary by culture
        Assert.Contains("5 items", result);
        Assert.StartsWith("1", result);
    }

    [Fact]
    public void Plural_ExactMatchWithDecimal_20()
    {
        var args = new Dictionary<string, object?> { { "count", 2.0 } };

        var result = _formatter.FormatMessage(
            "{count, plural, =0 {zero items} =2 {two items} one {# item} other {# items}}",
            args);

        Assert.Equal("two items", result);
    }

    #endregion

    #region Plural Offset Edge Cases

    [Fact]
    public void Plural_OffsetZero_NoEffect()
    {
        var args = new Dictionary<string, object?> { { "count", 5 } };

        var result = _formatter.FormatMessage(
            "{count, plural, offset:0 one {# person} other {# people}}",
            args);

        Assert.Equal("5 people", result);
    }

    [Fact]
    public void Plural_OffsetLargerThanValue_NegativeResult()
    {
        var args = new Dictionary<string, object?> { { "count", 2 } };

        var result = _formatter.FormatMessage(
            "{count, plural, offset:5 one {# person} other {# people}}",
            args);

        // 2 - 5 = -3, which is "other"
        Assert.Contains("-3", result);
    }

    [Fact]
    public void Plural_ExactMatchIgnoresOffset()
    {
        var args = new Dictionary<string, object?> { { "count", 2 } };

        var result = _formatter.FormatMessage(
            "{count, plural, offset:1 =2 {exactly two} one {# person} other {# people}}",
            args);

        // =2 matches the original value (2), not the offset value (1)
        Assert.Equal("exactly two", result);
    }

    #endregion

    #region Plural Hash Replacement Tests

    [Fact]
    public void Plural_HashReplacementInNestedContent()
    {
        var args = new Dictionary<string, object?> { { "count", 5 } };

        var result = _formatter.FormatMessage(
            "{count, plural, one {You have # item in your cart} other {You have # items in your cart}}",
            args);

        Assert.Equal("You have 5 items in your cart", result);
    }

    [Fact]
    public void Plural_MultipleHashesReplaced()
    {
        var args = new Dictionary<string, object?> { { "count", 3 } };

        var result = _formatter.FormatMessage(
            "{count, plural, one {#, just #} other {#, total # items}}",
            args);

        Assert.Equal("3, total 3 items", result);
    }

    [Fact]
    public void Plural_HashWithOffset_ReplacesWithOffsetValue()
    {
        var args = new Dictionary<string, object?> { { "count", 5 } };

        var result = _formatter.FormatMessage(
            "{count, plural, offset:2 one {# other person} other {# other people}}",
            args);

        // 5 - 2 = 3
        Assert.Equal("3 other people", result);
    }

    #endregion

    #region Select Edge Cases

    [Fact]
    public void Select_OtherRequired_MatchesUnknown()
    {
        var args = new Dictionary<string, object?> { { "type", "unknown" } };

        var result = _formatter.FormatMessage(
            "{type, select, a {Type A} b {Type B} other {Unknown type}}",
            args);

        Assert.Equal("Unknown type", result);
    }

    [Fact]
    public void Select_CaseSensitive_ExactMatch()
    {
        var args1 = new Dictionary<string, object?> { { "type", "male" } };
        var args2 = new Dictionary<string, object?> { { "type", "Male" } };

        var pattern = "{type, select, male {He} Male {HE} other {They}}";

        Assert.Equal("He", _formatter.FormatMessage(pattern, args1));
        Assert.Equal("HE", _formatter.FormatMessage(pattern, args2));
    }

    [Fact]
    public void Select_EmptyKey_Handled()
    {
        var args = new Dictionary<string, object?> { { "type", "" } };

        var result = _formatter.FormatMessage(
            "{type, select, a {A} b {B} other {Empty or other}}",
            args);

        Assert.Equal("Empty or other", result);
    }

    [Fact]
    public void Select_WhitespaceKey_Handled()
    {
        var args = new Dictionary<string, object?> { { "type", "   " } };

        var result = _formatter.FormatMessage(
            "{type, select, a {A} b {B} other {Other}}",
            args);

        Assert.Equal("Other", result);
    }

    #endregion

    #region SelectOrdinal Edge Cases

    [Theory]
    [InlineData(1, "1st")]
    [InlineData(2, "2nd")]
    [InlineData(3, "3rd")]
    [InlineData(4, "4th")]
    [InlineData(11, "11th")]
    [InlineData(12, "12th")]
    [InlineData(13, "13th")]
    [InlineData(21, "21st")]
    [InlineData(22, "22nd")]
    [InlineData(23, "23rd")]
    [InlineData(31, "31st")]
    [InlineData(32, "32nd")]
    [InlineData(33, "33rd")]
    [InlineData(41, "41st")]
    [InlineData(42, "42nd")]
    [InlineData(43, "43rd")]
    [InlineData(100, "100th")]
    [InlineData(101, "101st")]
    [InlineData(111, "111th")]
    [InlineData(121, "121st")]
    public void SelectOrdinal_EnglishRules_ComprehensiveTest(int n, string expected)
    {
        var args = new Dictionary<string, object?> { { "n", n } };

        var result = _formatter.FormatMessage(
            "{n, selectordinal, one {#st} two {#nd} few {#rd} other {#th}}",
            args);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SelectOrdinal_WithOffset()
    {
        var args = new Dictionary<string, object?> { { "rank", 3 } };

        var result = _formatter.FormatMessage(
            "{rank, selectordinal, offset:1 one {#st} two {#nd} few {#rd} other {#th}}",
            args);

        // 3 - 1 = 2, ordinal for 2 is "two" -> "2nd"
        Assert.Equal("2nd", result);
    }

    [Fact]
    public void SelectOrdinal_ExactMatch()
    {
        var args = new Dictionary<string, object?> { { "n", 1 } };

        var result = _formatter.FormatMessage(
            "{n, selectordinal, =1 {first!} one {#st} two {#nd} few {#rd} other {#th}}",
            args);

        Assert.Equal("first!", result);
    }

    #endregion

    #region Nested Plural/Select Tests

    [Fact]
    public void Nested_PluralInSelect()
    {
        var args = new Dictionary<string, object?>
        {
            { "gender", "female" },
            { "count", 3 }
        };

        var result = _formatter.FormatMessage(
            "{gender, select, male {{count, plural, one {He has # item} other {He has # items}}} female {{count, plural, one {She has # item} other {She has # items}}} other {{count, plural, one {They have # item} other {They have # items}}}}",
            args);

        Assert.Equal("She has 3 items", result);
    }

    [Fact]
    public void Nested_SelectInPlural()
    {
        var args = new Dictionary<string, object?>
        {
            { "count", 1 },
            { "type", "book" }
        };

        var result = _formatter.FormatMessage(
            "{count, plural, one {# {type, select, book {book} magazine {magazine} other {item}}} other {# {type, select, book {books} magazine {magazines} other {items}}}}",
            args);

        Assert.Equal("1 book", result);
    }

    [Fact]
    public void Nested_ThreeLevelsDeep()
    {
        var args = new Dictionary<string, object?>
        {
            { "count", 2 },
            { "gender", "male" },
            { "type", "cat" }
        };

        var pattern = "{count, plural, " +
            "one {{gender, select, male {{type, select, cat {He has a cat} dog {He has a dog} other {He has a pet}}} other {{type, select, cat {They have a cat} other {They have a pet}}}}} " +
            "other {{gender, select, male {{type, select, cat {He has # cats} dog {He has # dogs} other {He has # pets}}} other {{type, select, cat {They have # cats} other {They have # pets}}}}}}";

        var result = _formatter.FormatMessage(pattern, args);

        Assert.Equal("He has 2 cats", result);
    }

    #endregion

    #region Empty Case Content Tests

    [Fact]
    public void Plural_EmptyCaseContent()
    {
        var args = new Dictionary<string, object?> { { "count", 0 } };

        var result = _formatter.FormatMessage(
            "{count, plural, =0 {} one {# item} other {# items}}",
            args);

        Assert.Equal("", result);
    }

    [Fact]
    public void Select_EmptyCaseContent()
    {
        var args = new Dictionary<string, object?> { { "type", "hidden" } };

        var result = _formatter.FormatMessage(
            "{type, select, hidden {} visible {Visible} other {Other}}",
            args);

        Assert.Equal("", result);
    }

    #endregion

    #region Large Numbers Tests

    [Fact]
    public void Plural_LargeNumber()
    {
        var args = new Dictionary<string, object?> { { "count", 1000000 } };

        var result = _formatter.FormatMessage(
            "{count, plural, one {# item} other {# items}}",
            args);

        Assert.Equal("1000000 items", result);
    }

    [Fact]
    public void Plural_NegativeNumber()
    {
        var args = new Dictionary<string, object?> { { "count", -5 } };

        var result = _formatter.FormatMessage(
            "{count, plural, one {# item} other {# items}}",
            args);

        Assert.Contains("-5", result);
    }

    #endregion
}
