using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Formatting;

/// <summary>
/// Tests for object overloads of FormatMessage, FormatComplexMessage, and FormatHtmlMessage.
/// Also tests nullable parameter behavior.
/// </summary>
public class ObjectOverloadTests
{
    private readonly MessageFormatter _formatter;

    public ObjectOverloadTests()
    {
        var options = TestOptions.WithEnglish();
        _formatter = new MessageFormatter("en", options);
    }

    #region FormatMessage with Object Tests

    [Fact]
    public void FormatMessage_AnonymousType_FormatsCorrectly()
    {
        var result = _formatter.FormatMessage(
            "Hello {name}, you have {count} messages",
            new { name = "John", count = 5 });

        Assert.Equal("Hello John, you have 5 messages", result);
    }

    [Fact]
    public void FormatMessage_Poco_FormatsCorrectly()
    {
        var person = new TestPersonModel { Name = "Alice", Age = 25 };

        var result = _formatter.FormatMessage(
            "{Name} is {Age} years old",
            person);

        Assert.Equal("Alice is 25 years old", result);
    }

    [Fact]
    public void FormatMessage_NullObject_FormatsStaticText()
    {
        var result = _formatter.FormatMessage("Hello World", (object?)null);

        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void FormatMessage_NoArgument_FormatsStaticText()
    {
        // Explicitly cast to resolve overload ambiguity
        var result = _formatter.FormatMessage("Hello World", (object?)null);

        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void FormatMessage_PluralWithAnonymousType_FormatsCorrectly()
    {
        var result = _formatter.FormatMessage(
            "You have {count, plural, one {# item} other {# items}}",
            new { count = 1 });

        Assert.Equal("You have 1 item", result);
    }

    [Fact]
    public void FormatMessage_PluralWithAnonymousType_Plural()
    {
        var result = _formatter.FormatMessage(
            "You have {count, plural, one {# item} other {# items}}",
            new { count = 5 });

        Assert.Equal("You have 5 items", result);
    }

    [Fact]
    public void FormatMessage_SelectWithAnonymousType_FormatsCorrectly()
    {
        var result = _formatter.FormatMessage(
            "{gender, select, male {He} female {She} other {They}} is here",
            new { gender = "female" });

        Assert.Equal("She is here", result);
    }

    [Fact]
    public void FormatMessage_NumberFormattingWithAnonymousType_FormatsCorrectly()
    {
        var result = _formatter.FormatMessage(
            "Total: {amount, number}",
            new { amount = 1234.56 });

        Assert.Contains("1,234", result);
    }

    [Fact]
    public void FormatMessage_DateFormattingWithAnonymousType_FormatsCorrectly()
    {
        var result = _formatter.FormatMessage(
            "Date: {date, date, short}",
            new { date = new System.DateTime(2024, 12, 25) });

        Assert.Contains("12", result);
        Assert.Contains("25", result);
    }

    #endregion

    #region FormatComplexMessage with Object Tests

    [Fact]
    public void FormatComplexMessage_NestedAnonymousType_FlattensAndFormats()
    {
        var result = _formatter.FormatComplexMessage(
            "Hello {user__firstName} {user__lastName}!",
            new { user = new { firstName = "John", lastName = "Doe" } });

        Assert.Equal("Hello John Doe!", result);
    }

    [Fact]
    public void FormatComplexMessage_DeeplyNestedAnonymousType_FlattensAllLevels()
    {
        var result = _formatter.FormatComplexMessage(
            "City: {address__home__city}",
            new { address = new { home = new { city = "New York" } } });

        Assert.Equal("City: New York", result);
    }

    [Fact]
    public void FormatComplexMessage_MixedFlatAndNestedAnonymousType_FormatsCorrectly()
    {
        var result = _formatter.FormatComplexMessage(
            "{greeting}, {user__name}!",
            new { greeting = "Hello", user = new { name = "John" } });

        Assert.Equal("Hello, John!", result);
    }

    [Fact]
    public void FormatComplexMessage_NullObject_FormatsStaticText()
    {
        var result = _formatter.FormatComplexMessage("Hello World", (object?)null);

        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void FormatComplexMessage_NoArgument_FormatsStaticText()
    {
        // Explicitly cast to resolve overload ambiguity
        var result = _formatter.FormatComplexMessage("Hello World", (object?)null);

        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void FormatComplexMessage_PluralWithNestedAnonymousType_FormatsCorrectly()
    {
        var result = _formatter.FormatComplexMessage(
            "Cart has {cart__itemCount, plural, one {# item} other {# items}}",
            new { cart = new { itemCount = 3 } });

        Assert.Equal("Cart has 3 items", result);
    }

    #endregion

    #region FormatHtmlMessage with Object Tests

    [Fact]
    public void FormatHtmlMessage_AnonymousType_FormatsCorrectly()
    {
        var result = _formatter.FormatHtmlMessage(
            "<b>Hello {name}</b>",
            new { name = "John" });

        Assert.Equal("<b>Hello John</b>", result);
    }

    [Fact]
    public void FormatHtmlMessage_AnonymousType_EscapesHtmlInValues()
    {
        var result = _formatter.FormatHtmlMessage(
            "<div>{text}</div>",
            new { text = "<script>alert('xss')</script>" });

        Assert.Contains("&lt;script&gt;", result);
        Assert.Contains("&lt;/script&gt;", result);
        Assert.DoesNotContain("<script>", result);
    }

    [Fact]
    public void FormatHtmlMessage_NullObject_FormatsStaticHtml()
    {
        var result = _formatter.FormatHtmlMessage("<b>Hello World</b>", (object?)null);

        Assert.Equal("<b>Hello World</b>", result);
    }

    [Fact]
    public void FormatHtmlMessage_NoArgument_FormatsStaticHtml()
    {
        // Explicitly cast to resolve overload ambiguity
        var result = _formatter.FormatHtmlMessage("<b>Hello World</b>", (object?)null);

        Assert.Equal("<b>Hello World</b>", result);
    }

    [Fact]
    public void FormatHtmlMessage_ComplexHtmlWithAnonymousType_FormatsCorrectly()
    {
        var result = _formatter.FormatHtmlMessage(
            "<a href=\"{link}\">Click here, {name}</a>",
            new { name = "John", link = "https://example.com" });

        Assert.Contains("href=\"https://example.com\"", result);
        Assert.Contains("Click here, John", result);
    }

    #endregion

    #region Comparison Tests - Object vs Dictionary

    [Fact]
    public void FormatMessage_ObjectAndDictionary_ProduceSameResult()
    {
        var dict = new Dictionary<string, object?>
        {
            ["name"] = "John",
            ["count"] = 5
        };

        var objectResult = _formatter.FormatMessage("Hello {name}, count: {count}", new { name = "John", count = 5 });
        var dictResult = _formatter.FormatMessage("Hello {name}, count: {count}", dict);

        Assert.Equal(dictResult, objectResult);
    }

    [Fact]
    public void FormatComplexMessage_ObjectAndDictionary_ProduceSameResult()
    {
        var dict = new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>
            {
                ["name"] = "John"
            }
        };

        var objectResult = _formatter.FormatComplexMessage("Hello {user__name}!", new { user = new { name = "John" } });
        var dictResult = _formatter.FormatComplexMessage("Hello {user__name}!", dict);

        Assert.Equal(dictResult, objectResult);
    }

    [Fact]
    public void FormatHtmlMessage_ObjectAndDictionary_ProduceSameResult()
    {
        var dict = new Dictionary<string, object?>
        {
            ["name"] = "John"
        };

        var objectResult = _formatter.FormatHtmlMessage("<b>{name}</b>", new { name = "John" });
        var dictResult = _formatter.FormatHtmlMessage("<b>{name}</b>", dict);

        Assert.Equal(dictResult, objectResult);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void FormatMessage_EmptyAnonymousType_FormatsStaticText()
    {
        var result = _formatter.FormatMessage("Hello World", new { });

        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void FormatMessage_AnonymousTypeWithNullProperty_HandlesGracefully()
    {
        var result = _formatter.FormatMessage("Value: {value}", new { value = (string?)null });

        Assert.Contains("Value:", result);
    }

    [Fact]
    public void FormatMessage_MultipleCallsWithDifferentObjects_WorkIndependently()
    {
        var result1 = _formatter.FormatMessage("Hello {name}", new { name = "John" });
        var result2 = _formatter.FormatMessage("Hello {name}", new { name = "Jane" });
        var result3 = _formatter.FormatMessage("Count: {count}", new { count = 42 });

        Assert.Equal("Hello John", result1);
        Assert.Equal("Hello Jane", result2);
        Assert.Equal("Count: 42", result3);
    }

    #endregion
}

/// <summary>
/// Test POCO class for object overload tests.
/// </summary>
public class TestPersonModel
{
    public string? Name { get; set; }
    public int Age { get; set; }
}
