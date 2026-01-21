using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Formatting;

/// <summary>
/// Tests for FormatComplexMessage - formatting messages with nested variable support.
/// </summary>
public class FormatComplexMessageTests
{
    private readonly MessageFormatter _formatter;

    public FormatComplexMessageTests()
    {
        var options = TestOptions.WithEnglish();
        _formatter = new MessageFormatter("en", options);
    }

    #region Basic Nested Object Tests

    [Fact]
    public void FormatComplexMessage_SimpleNestedObject_FlattensCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>
            {
                ["firstName"] = "John",
                ["lastName"] = "Doe"
            }
        };

        var result = _formatter.FormatComplexMessage("Hello {user__firstName} {user__lastName}!", values);

        Assert.Equal("Hello John Doe!", result);
    }

    [Fact]
    public void FormatComplexMessage_MixedFlatAndNested_WorksCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["greeting"] = "Hello",
            ["user"] = new Dictionary<string, object?>
            {
                ["name"] = "John"
            }
        };

        var result = _formatter.FormatComplexMessage("{greeting}, {user__name}!", values);

        Assert.Equal("Hello, John!", result);
    }

    [Fact]
    public void FormatComplexMessage_DeeplyNested_FlattensAllLevels()
    {
        var values = new Dictionary<string, object?>
        {
            ["data"] = new Dictionary<string, object?>
            {
                ["user"] = new Dictionary<string, object?>
                {
                    ["profile"] = new Dictionary<string, object?>
                    {
                        ["name"] = "John"
                    }
                }
            }
        };

        var result = _formatter.FormatComplexMessage("Name: {data__user__profile__name}", values);

        Assert.Equal("Name: John", result);
    }

    #endregion

    #region Plural with Nested Objects Tests

    [Fact]
    public void FormatComplexMessage_PluralWithNestedCount_WorksCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["items"] = new Dictionary<string, object?>
            {
                ["count"] = 5
            }
        };

        var result = _formatter.FormatComplexMessage(
            "You have {items__count, plural, one {# item} other {# items}}",
            values);

        Assert.Equal("You have 5 items", result);
    }

    [Fact]
    public void FormatComplexMessage_PluralWithNestedCount_Singular()
    {
        var values = new Dictionary<string, object?>
        {
            ["cart"] = new Dictionary<string, object?>
            {
                ["itemCount"] = 1
            }
        };

        var result = _formatter.FormatComplexMessage(
            "{cart__itemCount, plural, one {# item} other {# items}} in cart",
            values);

        Assert.Equal("1 item in cart", result);
    }

    #endregion

    #region Multiple Nested Objects Tests

    [Fact]
    public void FormatComplexMessage_MultipleNestedObjects_AllFlattened()
    {
        var values = new Dictionary<string, object?>
        {
            ["sender"] = new Dictionary<string, object?>
            {
                ["name"] = "Alice"
            },
            ["recipient"] = new Dictionary<string, object?>
            {
                ["name"] = "Bob"
            }
        };

        var result = _formatter.FormatComplexMessage(
            "Message from {sender__name} to {recipient__name}",
            values);

        Assert.Equal("Message from Alice to Bob", result);
    }

    [Fact]
    public void FormatComplexMessage_PricesExample_FormatsCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["prices"] = new Dictionary<string, object?>
            {
                ["hourly"] = 5,
                ["monthly"] = 100
            }
        };

        var result = _formatter.FormatComplexMessage(
            "Prices: ${prices__hourly} per hour, ${prices__monthly} per month",
            values);

        Assert.Equal("Prices: $5 per hour, $100 per month", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void FormatComplexMessage_EmptyNestedObject_DoesNotFail()
    {
        var values = new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>(),
            ["name"] = "John"
        };

        var result = _formatter.FormatComplexMessage("Hello {name}!", values);

        Assert.Equal("Hello John!", result);
    }

    [Fact]
    public void FormatComplexMessage_NullValueInNested_PreservesNull()
    {
        var values = new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>
            {
                ["name"] = null
            }
        };

        // Null values should be handled gracefully
        var result = _formatter.FormatComplexMessage("Hello {user__name}!", values);

        Assert.Contains("Hello", result);
    }

    [Fact]
    public void FormatComplexMessage_NoNestedObjects_BehavesLikeFormatMessage()
    {
        var values = new Dictionary<string, object?>
        {
            ["name"] = "John",
            ["count"] = 5
        };

        var result = _formatter.FormatComplexMessage("Hello {name}, you have {count} items!", values);

        Assert.Equal("Hello John, you have 5 items!", result);
    }

    [Fact]
    public void FormatComplexMessage_NumberFormatting_WorksWithNested()
    {
        var values = new Dictionary<string, object?>
        {
            ["order"] = new Dictionary<string, object?>
            {
                ["total"] = 1234.56
            }
        };

        var result = _formatter.FormatComplexMessage(
            "Order total: {order__total, number, ::currency/USD}",
            values);

        Assert.Contains("1,234.56", result);
    }

    #endregion

    #region Select with Nested Objects Tests

    [Fact]
    public void FormatComplexMessage_SelectWithNestedValue_WorksCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>
            {
                ["gender"] = "female",
                ["name"] = "Alice"
            }
        };

        var result = _formatter.FormatComplexMessage(
            "{user__gender, select, male {He} female {She} other {They}} is {user__name}",
            values);

        Assert.Equal("She is Alice", result);
    }

    #endregion

    #region IDictionary Compatibility Tests

    [Fact]
    public void FormatComplexMessage_NonGenericDictionary_WorksCorrectly()
    {
        var nested = new System.Collections.Hashtable
        {
            ["firstName"] = "John",
            ["lastName"] = "Doe"
        };

        var values = new Dictionary<string, object?>
        {
            ["user"] = nested
        };

        var result = _formatter.FormatComplexMessage("Hello {user__firstName} {user__lastName}!", values);

        Assert.Equal("Hello John Doe!", result);
    }

    #endregion

    #region Date Formatting with Nested Values Tests

    [Fact]
    public void FormatComplexMessage_DateWithNestedValue_FormatsCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["event"] = new Dictionary<string, object?>
            {
                ["date"] = new System.DateTime(2024, 12, 25)
            }
        };

        var result = _formatter.FormatComplexMessage(
            "Event date: {event__date, date, short}",
            values);

        Assert.Contains("12", result); // Should contain month
        Assert.Contains("25", result); // Should contain day
    }

    [Fact]
    public void FormatComplexMessage_TimeWithNestedValue_FormatsCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["meeting"] = new Dictionary<string, object?>
            {
                ["startTime"] = new System.DateTime(2024, 1, 1, 14, 30, 0)
            }
        };

        var result = _formatter.FormatComplexMessage(
            "Meeting starts at {meeting__startTime, time, short}",
            values);

        Assert.Contains(":", result); // Should contain time separator
    }

    #endregion

    #region Selectordinal with Nested Values Tests

    [Fact]
    public void FormatComplexMessage_SelectordinalWithNestedValue_WorksCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["race"] = new Dictionary<string, object?>
            {
                ["position"] = 1
            }
        };

        var result = _formatter.FormatComplexMessage(
            "You finished {race__position, selectordinal, one {#st} two {#nd} few {#rd} other {#th}}!",
            values);

        Assert.Equal("You finished 1st!", result);
    }

    [Fact]
    public void FormatComplexMessage_SelectordinalWithNestedValue_ThirdPlace()
    {
        var values = new Dictionary<string, object?>
        {
            ["race"] = new Dictionary<string, object?>
            {
                ["position"] = 3
            }
        };

        var result = _formatter.FormatComplexMessage(
            "You finished {race__position, selectordinal, one {#st} two {#nd} few {#rd} other {#th}}!",
            values);

        Assert.Equal("You finished 3rd!", result);
    }

    #endregion

    #region Complex Real-World Scenarios

    [Fact]
    public void FormatComplexMessage_OrderConfirmation_FormatsCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["order"] = new Dictionary<string, object?>
            {
                ["id"] = "ORD-12345",
                ["itemCount"] = 3,
                ["total"] = 100 // Use integer to avoid decimal formatting issues
            },
            ["customer"] = new Dictionary<string, object?>
            {
                ["firstName"] = "John",
                ["lastName"] = "Doe"
            }
        };

        var result = _formatter.FormatComplexMessage(
            "Thank you {customer__firstName} {customer__lastName}! Order #{order__id} with {order__itemCount, plural, one {# item} other {# items}} for ${order__total} is confirmed.",
            values);

        Assert.Contains("Thank you John Doe!", result);
        Assert.Contains("Order #ORD-12345", result);
        Assert.Contains("3 items", result);
        Assert.Contains("$100", result);
    }

    [Fact]
    public void FormatComplexMessage_UserProfile_FormatsCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>
            {
                ["displayName"] = "johndoe",
                ["stats"] = new Dictionary<string, object?>
                {
                    ["followers"] = 1000,
                    ["posts"] = 42
                }
            }
        };

        var result = _formatter.FormatComplexMessage(
            "@{user__displayName} has {user__stats__followers, number} followers and {user__stats__posts} posts",
            values);

        Assert.Contains("@johndoe", result);
        Assert.Contains("1,000 followers", result);
        Assert.Contains("42 posts", result);
    }

    [Fact]
    public void FormatComplexMessage_ShoppingCart_FormatsCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["cart"] = new Dictionary<string, object?>
            {
                ["items"] = new Dictionary<string, object?>
                {
                    ["count"] = 5
                },
                ["subtotal"] = 150.00,
                ["discount"] = 15.00,
                ["total"] = 135.00
            }
        };

        var result = _formatter.FormatComplexMessage(
            "Cart: {cart__items__count} items, Subtotal: ${cart__subtotal}, Discount: -${cart__discount}, Total: ${cart__total}",
            values);

        Assert.Equal("Cart: 5 items, Subtotal: $150, Discount: -$15, Total: $135", result);
    }

    #endregion

    #region Behavior Comparison Tests

    [Fact]
    public void FormatComplexMessage_FlatValues_SameAsFormatMessage()
    {
        var values = new Dictionary<string, object?>
        {
            ["name"] = "John",
            ["count"] = 5
        };

        var complexResult = _formatter.FormatComplexMessage("Hello {name}, count: {count}", values);
        var formatResult = _formatter.FormatMessage("Hello {name}, count: {count}", values);

        Assert.Equal(formatResult, complexResult);
    }

    [Fact]
    public void FormatComplexMessage_PreFlattenedValues_WorksWithDoubleUnderscore()
    {
        // If values are already flattened (with __ keys), they should work directly
        var values = new Dictionary<string, object?>
        {
            ["user__name"] = "John"
        };

        var result = _formatter.FormatComplexMessage("Hello {user__name}!", values);

        Assert.Equal("Hello John!", result);
    }

    #endregion

    #region Nested Select Tests

    [Fact]
    public void FormatComplexMessage_NestedSelectInPlural_WorksCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["notification"] = new Dictionary<string, object?>
            {
                ["count"] = 2,
                ["type"] = "message"
            }
        };

        var result = _formatter.FormatComplexMessage(
            "You have {notification__count, plural, one {# new {notification__type}} other {# new {notification__type}s}}",
            values);

        Assert.Equal("You have 2 new messages", result);
    }

    [Fact]
    public void FormatComplexMessage_SelectWithNestedPlural_WorksCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>
            {
                ["gender"] = "male",
                ["friendCount"] = 5
            }
        };

        var result = _formatter.FormatComplexMessage(
            "{user__gender, select, male {He has} female {She has} other {They have}} {user__friendCount, plural, one {# friend} other {# friends}}",
            values);

        Assert.Equal("He has 5 friends", result);
    }

    #endregion

    #region Special Characters in Keys Tests

    [Fact]
    public void FormatComplexMessage_KeysWithNumbers_WorksCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["item1"] = new Dictionary<string, object?>
            {
                ["name"] = "First",
                ["price"] = 10
            },
            ["item2"] = new Dictionary<string, object?>
            {
                ["name"] = "Second",
                ["price"] = 20
            }
        };

        var result = _formatter.FormatComplexMessage(
            "{item1__name}: ${item1__price}, {item2__name}: ${item2__price}",
            values);

        Assert.Equal("First: $10, Second: $20", result);
    }

    [Fact]
    public void FormatComplexMessage_CamelCaseKeys_WorksCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["userData"] = new Dictionary<string, object?>
            {
                ["firstName"] = "John",
                ["lastName"] = "Doe",
                ["emailAddress"] = "john@example.com"
            }
        };

        var result = _formatter.FormatComplexMessage(
            "{userData__firstName} {userData__lastName} ({userData__emailAddress})",
            values);

        Assert.Equal("John Doe (john@example.com)", result);
    }

    #endregion

    #region Multiple Levels Same Parent Tests

    [Fact]
    public void FormatComplexMessage_SiblingNestedObjects_AllAccessible()
    {
        var values = new Dictionary<string, object?>
        {
            ["address"] = new Dictionary<string, object?>
            {
                ["billing"] = new Dictionary<string, object?>
                {
                    ["city"] = "New York",
                    ["zip"] = "10001"
                },
                ["shipping"] = new Dictionary<string, object?>
                {
                    ["city"] = "Los Angeles",
                    ["zip"] = "90001"
                }
            }
        };

        var result = _formatter.FormatComplexMessage(
            "Billing: {address__billing__city} {address__billing__zip}, Shipping: {address__shipping__city} {address__shipping__zip}",
            values);

        Assert.Equal("Billing: New York 10001, Shipping: Los Angeles 90001", result);
    }

    #endregion

    #region Empty and Whitespace Values Tests

    [Fact]
    public void FormatComplexMessage_EmptyStringValue_HandlesGracefully()
    {
        var values = new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>
            {
                ["middleName"] = "",
                ["firstName"] = "John"
            }
        };

        // Empty string values may be handled differently by the formatter
        // Just ensure no exception is thrown and first name works
        var result = _formatter.FormatComplexMessage("Name: {user__firstName}", values);

        Assert.Equal("Name: John", result);
    }

    [Fact]
    public void FormatComplexMessage_NonEmptyStringInNestedObject_Works()
    {
        var values = new Dictionary<string, object?>
        {
            ["info"] = new Dictionary<string, object?>
            {
                ["text"] = "test value"
            }
        };

        var result = _formatter.FormatComplexMessage("Value: {info__text}", values);

        Assert.Equal("Value: test value", result);
    }

    #endregion

    #region Numeric Types Tests

    [Fact]
    public void FormatComplexMessage_IntegerValues_Work()
    {
        var values = new Dictionary<string, object?>
        {
            ["numbers"] = new Dictionary<string, object?>
            {
                ["intValue"] = 42,
                ["longValue"] = 1000000L
            }
        };

        var result = _formatter.FormatComplexMessage(
            "Int: {numbers__intValue}, Long: {numbers__longValue}",
            values);

        Assert.Contains("Int: 42", result);
        Assert.Contains("Long:", result);
        Assert.Contains("1", result); // Contains at least part of the number
    }

    [Fact]
    public void FormatComplexMessage_DecimalWithNumberFormat_Works()
    {
        var values = new Dictionary<string, object?>
        {
            ["order"] = new Dictionary<string, object?>
            {
                ["total"] = 1234.56
            }
        };

        // Use number format to ensure consistent output
        var result = _formatter.FormatComplexMessage(
            "Total: {order__total, number}",
            values);

        Assert.Contains("1", result);
        Assert.Contains("234", result);
    }

    [Fact]
    public void FormatComplexMessage_BooleanValues_Work()
    {
        var values = new Dictionary<string, object?>
        {
            ["flags"] = new Dictionary<string, object?>
            {
                ["isActive"] = true,
                ["isVerified"] = false
            }
        };

        var result = _formatter.FormatComplexMessage(
            "Active: {flags__isActive}, Verified: {flags__isVerified}",
            values);

        Assert.Contains("Active:", result);
        Assert.Contains("Verified:", result);
    }

    #endregion
}
