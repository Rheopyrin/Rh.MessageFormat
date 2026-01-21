using System;
using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Tests for list formatting.
/// </summary>
public class ListFormattingTests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region Conjunction Style Tests

    [Fact]
    public void List_Conjunction_TwoItems()
    {
        var args = new Dictionary<string, object?> { { "items", new[] { "apples", "oranges" } } };

        var result = _formatter.FormatMessage("{items, list}", args);

        Assert.Equal("apples and oranges", result);
    }

    [Fact]
    public void List_Conjunction_ThreeItems()
    {
        var args = new Dictionary<string, object?> { { "items", new[] { "apples", "oranges", "bananas" } } };

        var result = _formatter.FormatMessage("{items, list}", args);

        Assert.Equal("apples, oranges, and bananas", result);
    }

    [Fact]
    public void List_Conjunction_FourItems()
    {
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C", "D" } } };

        var result = _formatter.FormatMessage("{items, list, conjunction}", args);

        Assert.Equal("A, B, C, and D", result);
    }

    [Fact]
    public void List_Conjunction_Short()
    {
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C" } } };

        var result = _formatter.FormatMessage("{items, list, conjunction short}", args);

        Assert.Equal("A, B, & C", result);
    }

    [Fact]
    public void List_Conjunction_Narrow()
    {
        var args = new Dictionary<string, object?> { { "items", new[] { "A", "B", "C" } } };

        var result = _formatter.FormatMessage("{items, list, conjunction narrow}", args);

        Assert.Equal("A, B, C", result);
    }

    #endregion

    #region Disjunction Style Tests

    [Fact]
    public void List_Disjunction_TwoItems()
    {
        var args = new Dictionary<string, object?> { { "items", new[] { "red", "blue" } } };

        var result = _formatter.FormatMessage("{items, list, disjunction}", args);

        Assert.Equal("red or blue", result);
    }

    [Fact]
    public void List_Disjunction_ThreeItems()
    {
        var args = new Dictionary<string, object?> { { "items", new[] { "red", "green", "blue" } } };

        var result = _formatter.FormatMessage("{items, list, disjunction}", args);

        Assert.Equal("red, green, or blue", result);
    }

    #endregion

    #region Unit Style Tests

    [Fact]
    public void List_Unit_ThreeItems()
    {
        var args = new Dictionary<string, object?> { { "items", new[] { "5 feet", "6 inches", "tall" } } };

        var result = _formatter.FormatMessage("{items, list, unit}", args);

        Assert.Equal("5 feet, 6 inches, tall", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void List_SingleItem()
    {
        var args = new Dictionary<string, object?> { { "items", new[] { "apple" } } };

        var result = _formatter.FormatMessage("{items, list}", args);

        Assert.Equal("apple", result);
    }

    [Fact]
    public void List_EmptyList()
    {
        var args = new Dictionary<string, object?> { { "items", Array.Empty<string>() } };

        var result = _formatter.FormatMessage("Items: {items, list}", args);

        Assert.Equal("Items: ", result);
    }

    [Fact]
    public void List_NullValue()
    {
        var args = new Dictionary<string, object?> { { "items", null } };

        var result = _formatter.FormatMessage("Items: {items, list}", args);

        Assert.Equal("Items: ", result);
    }

    #endregion

    #region Different Collection Types

    [Fact]
    public void List_ListOfIntegers()
    {
        var args = new Dictionary<string, object?> { { "numbers", new[] { 1, 2, 3 } } };

        var result = _formatter.FormatMessage("{numbers, list}", args);

        Assert.Equal("1, 2, and 3", result);
    }

    [Fact]
    public void List_GenericList()
    {
        var items = new List<string> { "one", "two", "three" };
        var args = new Dictionary<string, object?> { { "items", items } };

        var result = _formatter.FormatMessage("{items, list}", args);

        Assert.Equal("one, two, and three", result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void List_InComplexMessage()
    {
        var args = new Dictionary<string, object?>
        {
            { "name", "Alice" },
            { "items", new[] { "apples", "oranges", "bananas" } }
        };

        var result = _formatter.FormatMessage("{name} bought {items, list}.", args);

        Assert.Equal("Alice bought apples, oranges, and bananas.", result);
    }

    [Fact]
    public void List_WithPlural()
    {
        var args = new Dictionary<string, object?>
        {
            { "count", 3 },
            { "items", new[] { "apples", "oranges", "bananas" } }
        };

        var result = _formatter.FormatMessage("You have {count, plural, one {# item} other {# items}}: {items, list}.", args);

        Assert.Equal("You have 3 items: apples, oranges, and bananas.", result);
    }

    [Fact]
    public void List_WithNumbers()
    {
        var args = new Dictionary<string, object?>
        {
            { "total", 150.99 },
            { "items", new[] { "shirt", "pants", "shoes" } }
        };

        var result = _formatter.FormatMessage("Total: {total, number, ::currency/USD} for {items, list}.", args);

        Assert.Contains("$", result);
        Assert.Contains("shirt, pants, and shoes", result);
    }

    #endregion
}
