using System.Collections.Generic;
using Rh.MessageFormat.Abstractions.Interfaces;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Formatting;

/// <summary>
/// Tests for the trim-safe generic overloads of FormatMessage, FormatComplexMessage,
/// and FormatHtmlMessage introduced for Native AOT support (GitHub issue #1).
/// The generic overloads carry [DynamicallyAccessedMembers] annotations so that the
/// trimmer preserves the public properties and fields of the argument type.
/// </summary>
public class GenericOverloadTests
{
    private readonly MessageFormatter _formatter;

    public GenericOverloadTests()
    {
        var options = TestOptions.WithEnglish();
        _formatter = new MessageFormatter("en", options);
    }

    private class Person
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    private record PersonRecord(string FirstName, string LastName);

    private class PersonWithFields
    {
        public string? FirstName;
        public string? LastName;
    }

    [Fact]
    public void FormatMessage_Generic_AnonymousType_FormatsCorrectly()
    {
        var result = _formatter.FormatMessage(
            "{FirstName} {LastName}",
            new { FirstName = "Mike", LastName = "Smith" });

        Assert.Equal("Mike Smith", result);
    }

    [Fact]
    public void FormatMessage_Generic_Poco_FormatsCorrectly()
    {
        var result = _formatter.FormatMessage(
            "{FirstName} {LastName}",
            new Person { FirstName = "Mike", LastName = "Smith" });

        Assert.Equal("Mike Smith", result);
    }

    [Fact]
    public void FormatMessage_Generic_Record_FormatsCorrectly()
    {
        var result = _formatter.FormatMessage(
            "{FirstName} {LastName}",
            new PersonRecord("Mike", "Smith"));

        Assert.Equal("Mike Smith", result);
    }

    [Fact]
    public void FormatMessage_Generic_PublicFields_FormatsCorrectly()
    {
        var result = _formatter.FormatMessage(
            "{FirstName} {LastName}",
            new PersonWithFields { FirstName = "Mike", LastName = "Smith" });

        Assert.Equal("Mike Smith", result);
    }

    [Fact]
    public void FormatMessage_Generic_ValueTuple_FormatsFieldsByItemName()
    {
        // Tuple element names are erased at runtime, so fields are Item1/Item2.
        var result = _formatter.FormatMessage(
            "{Item1} {Item2}",
            (FirstName: "Mike", LastName: "Smith"));

        Assert.Equal("Mike Smith", result);
    }

    [Fact]
    public void FormatMessage_Generic_Dictionary_ForwardsToDictionaryOverload()
    {
        // A dictionary passed to the generic overload must behave like the
        // dedicated dictionary overload (no reflection over the dictionary type).
        var args = new Dictionary<string, object?>
        {
            ["FirstName"] = "Mike",
            ["LastName"] = "Smith"
        };

        var result = _formatter.FormatMessage("{FirstName} {LastName}", args);

        Assert.Equal("Mike Smith", result);
    }

    [Fact]
    public void FormatMessage_NoArgsOverload_FormatsStaticText()
    {
        var result = _formatter.FormatMessage("Hello World");

        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void FormatComplexMessage_Generic_NestedPoco_FormatsCorrectly()
    {
        var result = _formatter.FormatComplexMessage(
            "Hello {user__FirstName} {user__LastName}",
            new { user = new Person { FirstName = "Mike", LastName = "Smith" } });

        Assert.Equal("Hello Mike Smith", result);
    }

    [Fact]
    public void FormatComplexMessage_NoArgsOverload_FormatsStaticText()
    {
        var result = _formatter.FormatComplexMessage("Hello World");

        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void FormatHtmlMessage_Generic_Poco_EscapesValues()
    {
        var result = _formatter.FormatHtmlMessage(
            "<b>{FirstName}</b>",
            new Person { FirstName = "<script>", LastName = "Smith" });

        Assert.Equal("<b>&lt;script&gt;</b>", result);
    }

    [Fact]
    public void FormatHtmlMessage_NoArgsOverload_PreservesMarkup()
    {
        var result = _formatter.FormatHtmlMessage("<b>Hello</b>");

        Assert.Equal("<b>Hello</b>", result);
    }

    [Fact]
    public void FormatMessage_Generic_ThroughInterface_FormatsCorrectly()
    {
        IMessageFormatter formatter = _formatter;

        var result = formatter.FormatMessage(
            "{FirstName} {LastName}",
            new Person { FirstName = "Mike", LastName = "Smith" });

        Assert.Equal("Mike Smith", result);
    }
}
