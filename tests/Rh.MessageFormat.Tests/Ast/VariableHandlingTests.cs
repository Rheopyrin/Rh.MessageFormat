using System;
using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Tests for variable handling, type conversion, and edge cases.
/// </summary>
public class VariableHandlingTests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region Missing Variable Tests

    [Fact]
    public void Variable_Missing_ReturnsEmpty()
    {
        var args = new Dictionary<string, object?>();

        var result = _formatter.FormatMessage("Hello, {name}!", args);

        Assert.Equal("Hello, !", result);
    }

    [Fact]
    public void Variable_MissingInPlural_UsesOther()
    {
        var args = new Dictionary<string, object?>();

        var result = _formatter.FormatMessage("{count, plural, one {item} other {items}}", args);

        // Missing variable treated as 0, which uses "other" in English
        Assert.Equal("items", result);
    }

    [Fact]
    public void Variable_MissingInSelect_UsesOther()
    {
        var args = new Dictionary<string, object?>();

        var result = _formatter.FormatMessage("{type, select, a {A} b {B} other {Other}}", args);

        Assert.Equal("Other", result);
    }

    #endregion

    #region Null Value Tests

    [Fact]
    public void Variable_NullValue_ReturnsEmpty()
    {
        var args = new Dictionary<string, object?> { { "name", null } };

        var result = _formatter.FormatMessage("Hello, {name}!", args);

        Assert.Equal("Hello, !", result);
    }

    [Fact]
    public void Variable_NullInNumber_ReturnsZero()
    {
        var args = new Dictionary<string, object?> { { "n", null } };

        var result = _formatter.FormatMessage("Value: {n, number}", args);

        // Null is converted to 0 for number formatting
        Assert.Equal("Value: 0", result);
    }

    [Fact]
    public void Variable_NullInPlural_UsesOther()
    {
        var args = new Dictionary<string, object?> { { "count", null } };

        var result = _formatter.FormatMessage("{count, plural, one {# item} other {# items}}", args);

        // Null treated as 0
        Assert.Contains("items", result);
    }

    #endregion

    #region Type Conversion Tests

    [Theory]
    [InlineData(42)]
    [InlineData(42.0)]
    [InlineData(42L)]
    [InlineData(42f)]
    public void Number_VariousNumericTypes_FormatCorrectly(object value)
    {
        var args = new Dictionary<string, object?> { { "n", value } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.Contains("42", result);
    }

    [Fact]
    public void Number_DecimalType_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "n", 42.5m } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.Contains("42.5", result);
    }

    [Fact]
    public void Number_StringNumeric_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "n", "123.45" } };

        var result = _formatter.FormatMessage("{n, number}", args);

        Assert.Contains("123", result);
    }

    [Fact]
    public void Plural_DoubleValue_WorksCorrectly()
    {
        var args = new Dictionary<string, object?> { { "count", 1.0 } };

        var result = _formatter.FormatMessage("{count, plural, one {# item} other {# items}}", args);

        Assert.Equal("1 item", result);
    }

    [Fact]
    public void Plural_DecimalValue_WorksCorrectly()
    {
        var args = new Dictionary<string, object?> { { "count", 1m } };

        var result = _formatter.FormatMessage("{count, plural, one {# item} other {# items}}", args);

        Assert.Equal("1 item", result);
    }

    [Fact]
    public void Plural_StringNumeric_WorksCorrectly()
    {
        var args = new Dictionary<string, object?> { { "count", "5" } };

        var result = _formatter.FormatMessage("{count, plural, one {# item} other {# items}}", args);

        Assert.Equal("5 items", result);
    }

    #endregion

    #region String Value Tests

    [Fact]
    public void Variable_StringValue_OutputsDirectly()
    {
        var args = new Dictionary<string, object?> { { "name", "World" } };

        var result = _formatter.FormatMessage("Hello, {name}!", args);

        Assert.Equal("Hello, World!", result);
    }

    [Fact]
    public void Variable_EmptyString_OutputsEmpty()
    {
        var args = new Dictionary<string, object?> { { "name", "" } };

        var result = _formatter.FormatMessage("Hello, {name}!", args);

        Assert.Equal("Hello, !", result);
    }

    [Fact]
    public void Variable_WhitespaceString_OutputsWhitespace()
    {
        var args = new Dictionary<string, object?> { { "name", "   " } };

        var result = _formatter.FormatMessage("Hello, {name}!", args);

        Assert.Equal("Hello,    !", result);
    }

    [Fact]
    public void Variable_StringWithSpecialChars_OutputsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "text", "<>&\"'" } };

        var result = _formatter.FormatMessage("Text: {text}", args);

        Assert.Equal("Text: <>&\"'", result);
    }

    [Fact]
    public void Variable_StringWithNewlines_OutputsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "text", "Line1\nLine2" } };

        var result = _formatter.FormatMessage("Text: {text}", args);

        Assert.Equal("Text: Line1\nLine2", result);
    }

    #endregion

    #region Select Value Tests

    [Fact]
    public void Select_IntValue_ConvertsToString()
    {
        var args = new Dictionary<string, object?> { { "type", 1 } };

        var result = _formatter.FormatMessage("{type, select, 1 {One} 2 {Two} other {Other}}", args);

        Assert.Equal("One", result);
    }

    [Fact]
    public void Select_BoolValue_True_MatchesLowercaseKey()
    {
        var args = new Dictionary<string, object?> { { "flag", true } };

        var result = _formatter.FormatMessage("{flag, select, true {Yes} false {No} other {Unknown}}", args);

        Assert.Equal("Yes", result);
    }

    [Fact]
    public void Select_BoolValue_False_MatchesLowercaseKey()
    {
        var args = new Dictionary<string, object?> { { "flag", false } };

        var result = _formatter.FormatMessage("{flag, select, true {Yes} false {No} other {Unknown}}", args);

        Assert.Equal("No", result);
    }

    [Fact]
    public void Select_NullValue_MatchesNullKey()
    {
        var args = new Dictionary<string, object?> { { "value", null } };

        var result = _formatter.FormatMessage("{value, select, null {Nothing} other {Something}}", args);

        Assert.Equal("Nothing", result);
    }

    [Fact]
    public void Select_NullValue_FallsBackToOther_WhenNoNullCase()
    {
        var args = new Dictionary<string, object?> { { "value", null } };

        var result = _formatter.FormatMessage("{value, select, a {A} b {B} other {Other}}", args);

        Assert.Equal("Other", result);
    }

    [Fact]
    public void Select_EnumValue_ConvertsToString()
    {
        var args = new Dictionary<string, object?> { { "day", DayOfWeek.Monday } };

        var result = _formatter.FormatMessage("{day, select, Monday {Start of week} Friday {End of week} other {Mid week}}", args);

        Assert.Equal("Start of week", result);
    }

    #endregion

    #region List Value Tests

    [Fact]
    public void List_ArrayValue_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C" } } };

        var result = _formatter.FormatMessage("{items, list}", args);

        Assert.Equal("A, B, and C", result);
    }

    [Fact]
    public void List_ListValue_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "items", new List<string> { "X", "Y" } } };

        var result = _formatter.FormatMessage("{items, list}", args);

        Assert.Equal("X and Y", result);
    }

    [Fact]
    public void List_IntArray_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "nums", new[] { 1, 2, 3 } } };

        var result = _formatter.FormatMessage("{nums, list}", args);

        Assert.Equal("1, 2, and 3", result);
    }

    [Fact]
    public void List_MixedTypes_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "items", new object[] { "Text", 42, true } } };

        var result = _formatter.FormatMessage("{items, list}", args);

        Assert.Contains("Text", result);
        Assert.Contains("42", result);
        Assert.Contains("True", result);
    }

    #endregion

    #region Object Value Tests

    [Fact]
    public void Variable_ObjectWithToString_UsesToString()
    {
        var obj = new TestObject { Value = "CustomValue" };
        var args = new Dictionary<string, object?> { { "obj", obj } };

        var result = _formatter.FormatMessage("Object: {obj}", args);

        Assert.Equal("Object: TestObject:CustomValue", result);
    }

    [Fact]
    public void Variable_AnonymousObject_UsesToString()
    {
        var obj = new { Name = "Test", Value = 42 };
        var args = new Dictionary<string, object?> { { "obj", obj } };

        var result = _formatter.FormatMessage("Object: {obj}", args);

        Assert.Contains("Name", result);
        Assert.Contains("Test", result);
    }

    #endregion

    #region Multiple Variables Tests

    [Fact]
    public void Multiple_SameVariableTwice_BothFormatted()
    {
        var args = new Dictionary<string, object?> { { "name", "World" } };

        var result = _formatter.FormatMessage("{name} and {name}", args);

        Assert.Equal("World and World", result);
    }

    [Fact]
    public void Multiple_DifferentVariables_AllFormatted()
    {
        var args = new Dictionary<string, object?>
        {
            { "first", "John" },
            { "last", "Doe" },
            { "age", 30 }
        };

        var result = _formatter.FormatMessage("{first} {last} is {age} years old.", args);

        Assert.Equal("John Doe is 30 years old.", result);
    }

    [Fact]
    public void Multiple_SomeVariablesMissing_PartialOutput()
    {
        var args = new Dictionary<string, object?>
        {
            { "first", "John" },
            { "age", 30 }
        };

        var result = _formatter.FormatMessage("{first} {last} is {age} years old.", args);

        Assert.Equal("John  is 30 years old.", result);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void Variable_CaseSensitive_ExactMatchRequired()
    {
        var args = new Dictionary<string, object?>
        {
            { "Name", "John" },
            { "name", "Jane" }
        };

        var result = _formatter.FormatMessage("{name} and {Name}", args);

        Assert.Equal("Jane and John", result);
    }

    #endregion

    private class TestObject
    {
        public string Value { get; set; } = "";

        public override string ToString() => $"TestObject:{Value}";
    }
}
