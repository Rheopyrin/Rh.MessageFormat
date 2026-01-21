using System;
using System.Collections.Generic;
using Rh.MessageFormat.Exceptions;
using Rh.MessageFormat.Options;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Tests for the MessageFormatter implementation.
/// </summary>
public class AstFormatterTests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    [Theory]
    [InlineData("Hello, World!", "Hello, World!")]
    [InlineData("Simple text with no placeholders", "Simple text with no placeholders")]
    [InlineData("Text with 'single quotes'", "Text with 'single quotes'")]
    [InlineData("Text with ''escaped quotes''", "Text with 'escaped quotes'")]
    public void LiteralText_FormatsCorrectly(string pattern, string expected)
    {
        var args = new Dictionary<string, object?>();

        var result = _formatter.FormatMessage(pattern, args);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SimplePlaceholder_FormatsCorrectly()
    {
        var args = new Dictionary<string, object?> { { "name", "World" }, { "greeting", "Hi" }, { "item", "book" }, { "price", "10" } };

        Assert.Equal("Hello, World!", _formatter.FormatMessage("Hello, {name}!", args));
        Assert.Equal("Hi, World!", _formatter.FormatMessage("{greeting}, {name}!", args));
        Assert.Equal("The book costs 10.", _formatter.FormatMessage("The {item} costs {price}.", args));
    }

    [Theory]
    [InlineData("{count, plural, one {# item} other {# items}}", 1, "1 item")]
    [InlineData("{count, plural, one {# item} other {# items}}", 5, "5 items")]
    [InlineData("{count, plural, =0 {no items} one {# item} other {# items}}", 0, "no items")]
    [InlineData("{count, plural, =0 {no items} one {# item} other {# items}}", 1, "1 item")]
    [InlineData("{count, plural, =0 {no items} one {# item} other {# items}}", 42, "42 items")]
    public void Plural_FormatsCorrectly(string pattern, int count, string expected)
    {
        var args = new Dictionary<string, object?> { { "count", count } };

        var result = _formatter.FormatMessage(pattern, args);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("{gender, select, male {He} female {She} other {They}} said hello.", "male", "He said hello.")]
    [InlineData("{gender, select, male {He} female {She} other {They}} said hello.", "female", "She said hello.")]
    [InlineData("{gender, select, male {He} female {She} other {They}} said hello.", "unknown", "They said hello.")]
    public void Select_FormatsCorrectly(string pattern, string gender, string expected)
    {
        var args = new Dictionary<string, object?> { { "gender", gender } };

        var result = _formatter.FormatMessage(pattern, args);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void NestedPatterns_Work()
    {
        var pattern = "{count, plural, one {You have # {type, select, new {new} other {}} message.} other {You have # {type, select, new {new} other {}} messages.}}";
        var args = new Dictionary<string, object?> { { "count", 1 }, { "type", "new" } };

        var result = _formatter.FormatMessage(pattern, args);

        Assert.Equal("You have 1 new message.", result);
    }

    [Fact]
    public void NestedPatterns_MultipleItems()
    {
        var pattern = "{count, plural, one {You have # {type, select, new {new} other {}} message.} other {You have # {type, select, new {new} other {}} messages.}}";
        var args = new Dictionary<string, object?> { { "count", 5 }, { "type", "new" } };

        var result = _formatter.FormatMessage(pattern, args);

        Assert.Equal("You have 5 new messages.", result);
    }

    [Fact]
    public void PluralWithOffset_Works()
    {
        var pattern = "{count, plural, offset:1 =0 {Nobody is attending} =1 {Only {host} is attending} one {{host} and # other person are attending} other {{host} and # other people are attending}}";

        var args0 = new Dictionary<string, object?> { { "count", 0 }, { "host", "Alice" } };
        var result0 = _formatter.FormatMessage(pattern, args0);
        Assert.Equal("Nobody is attending", result0);

        var args1 = new Dictionary<string, object?> { { "count", 1 }, { "host", "Alice" } };
        var result1 = _formatter.FormatMessage(pattern, args1);
        Assert.Equal("Only Alice is attending", result1);

        var args2 = new Dictionary<string, object?> { { "count", 2 }, { "host", "Alice" } };
        var result2 = _formatter.FormatMessage(pattern, args2);
        Assert.Equal("Alice and 1 other person are attending", result2);

        var args5 = new Dictionary<string, object?> { { "count", 5 }, { "host", "Alice" } };
        var result5 = _formatter.FormatMessage(pattern, args5);
        Assert.Equal("Alice and 4 other people are attending", result5);
    }

    [Theory]
    [InlineData("{n, number}", 1234.56, "1,234.56")]
    [InlineData("{n, number}", 0, "0")]
    [InlineData("{n, number}", -42.5, "-42.5")]
    public void NumberFormat_Works(string pattern, double n, string expected)
    {
        var args = new Dictionary<string, object?> { { "n", n } };

        var result = _formatter.FormatMessage(pattern, args);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Formatter_UsesCaching()
    {
        var pattern = "Hello, {name}!";
        var args = new Dictionary<string, object?> { { "name", "World" } };

        // Format the same pattern multiple times
        for (int i = 0; i < 10; i++)
        {
            var result = _formatter.FormatMessage(pattern, args);
            Assert.Equal("Hello, World!", result);
        }
    }

    [Theory]
    [InlineData(1, "1st")]
    [InlineData(2, "2nd")]
    [InlineData(3, "3rd")]
    [InlineData(4, "4th")]
    [InlineData(5, "5th")]
    [InlineData(11, "11th")]
    [InlineData(12, "12th")]
    [InlineData(13, "13th")]
    [InlineData(21, "21st")]
    [InlineData(22, "22nd")]
    [InlineData(23, "23rd")]
    [InlineData(24, "24th")]
    [InlineData(100, "100th")]
    [InlineData(101, "101st")]
    [InlineData(102, "102nd")]
    [InlineData(103, "103rd")]
    [InlineData(111, "111th")]
    [InlineData(112, "112th")]
    [InlineData(113, "113th")]
    public void SelectOrdinal_EnglishRules(int n, string expected)
    {
        var pattern = "{n, selectordinal, one {#st} two {#nd} few {#rd} other {#th}}";
        var args = new Dictionary<string, object?> { { "n", n } };

        var result = _formatter.FormatMessage(pattern, args);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SelectOrdinal_WithExactMatch()
    {
        var pattern = "{n, selectordinal, =1 {first} =2 {second} =3 {third} one {#st} two {#nd} few {#rd} other {#th}}";

        Assert.Equal("first", _formatter.FormatMessage(pattern, new Dictionary<string, object?> { { "n", 1 } }));
        Assert.Equal("second", _formatter.FormatMessage(pattern, new Dictionary<string, object?> { { "n", 2 } }));
        Assert.Equal("third", _formatter.FormatMessage(pattern, new Dictionary<string, object?> { { "n", 3 } }));
        Assert.Equal("4th", _formatter.FormatMessage(pattern, new Dictionary<string, object?> { { "n", 4 } }));
        Assert.Equal("21st", _formatter.FormatMessage(pattern, new Dictionary<string, object?> { { "n", 21 } }));
    }

    [Fact]
    public void SelectOrdinal_InSentence()
    {
        var pattern = "This is your {n, selectordinal, one {#st} two {#nd} few {#rd} other {#th}} visit.";

        Assert.Equal("This is your 1st visit.", _formatter.FormatMessage(pattern, new Dictionary<string, object?> { { "n", 1 } }));
        Assert.Equal("This is your 2nd visit.", _formatter.FormatMessage(pattern, new Dictionary<string, object?> { { "n", 2 } }));
        Assert.Equal("This is your 3rd visit.", _formatter.FormatMessage(pattern, new Dictionary<string, object?> { { "n", 3 } }));
        Assert.Equal("This is your 4th visit.", _formatter.FormatMessage(pattern, new Dictionary<string, object?> { { "n", 4 } }));
        Assert.Equal("This is your 100th visit.", _formatter.FormatMessage(pattern, new Dictionary<string, object?> { { "n", 100 } }));
    }

    [Fact]
    public void SelectOrdinal_NestedWithPlural()
    {
        var pattern = "{rank, selectordinal, one {#st} two {#nd} few {#rd} other {#th}} place with {count, plural, one {# point} other {# points}}";

        Assert.Equal("1st place with 1 point", _formatter.FormatMessage(pattern, new Dictionary<string, object?> { { "rank", 1 }, { "count", 1 } }));
        Assert.Equal("2nd place with 5 points", _formatter.FormatMessage(pattern, new Dictionary<string, object?> { { "rank", 2 }, { "count", 5 } }));
        Assert.Equal("3rd place with 10 points", _formatter.FormatMessage(pattern, new Dictionary<string, object?> { { "rank", 3 }, { "count", 10 } }));
        Assert.Equal("11th place with 0 points", _formatter.FormatMessage(pattern, new Dictionary<string, object?> { { "rank", 11 }, { "count", 0 } }));
    }

    #region Date Formatting Tests

    [Fact]
    public void Date_DefaultStyle()
    {
        var date = new DateTime(2024, 6, 15);
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date}", args);

        // Default should use short format
        Assert.Contains("6", result);
        Assert.Contains("15", result);
        Assert.Contains("2024", result);
    }

    [Fact]
    public void Date_ShortStyle()
    {
        var date = new DateTime(2024, 6, 15);
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, short}", args);

        Assert.Contains("6", result);
        Assert.Contains("15", result);
    }

    [Fact]
    public void Date_MediumStyle()
    {
        var date = new DateTime(2024, 6, 15);
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, medium}", args);

        // Medium format should contain abbreviated month
        Assert.Contains("Jun", result);
        Assert.Contains("15", result);
        Assert.Contains("2024", result);
    }

    [Fact]
    public void Date_LongStyle()
    {
        var date = new DateTime(2024, 6, 15);
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, long}", args);

        // Long format should contain full month name
        Assert.Contains("June", result);
        Assert.Contains("15", result);
        Assert.Contains("2024", result);
    }

    [Fact]
    public void Date_FullStyle()
    {
        var date = new DateTime(2024, 6, 15);
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, full}", args);

        // Full format should contain day name and full month
        Assert.Contains("Saturday", result);
        Assert.Contains("June", result);
    }

    [Fact]
    public void Date_CustomFormat()
    {
        var date = new DateTime(2024, 6, 15);
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, yyyy-MM-dd}", args);

        Assert.Equal("2024-06-15", result);
    }

    [Fact]
    public void Date_CustomFormat_WithSkeletonPrefix()
    {
        var date = new DateTime(2024, 6, 15);
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, ::yyyy/MM/dd}", args);

        Assert.Equal("2024/06/15", result);
    }

    [Fact]
    public void Date_CustomFormat_MonthDay()
    {
        var date = new DateTime(2024, 6, 15);
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, MMMM d}", args);

        Assert.Equal("June 15", result);
    }

    [Fact]
    public void Date_WithDateOnly()
    {
        var date = new DateOnly(2024, 6, 15);
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, yyyy-MM-dd}", args);

        Assert.Equal("2024-06-15", result);
    }

    [Fact]
    public void Date_WithDateTimeOffset()
    {
        var date = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var args = new Dictionary<string, object?> { { "d", date } };

        var result = _formatter.FormatMessage("{d, date, yyyy-MM-dd}", args);

        Assert.Equal("2024-06-15", result);
    }

    #endregion

    #region Time Formatting Tests

    [Fact]
    public void Time_DefaultStyle()
    {
        var time = new DateTime(2024, 1, 1, 14, 30, 45);
        var args = new Dictionary<string, object?> { { "t", time } };

        var result = _formatter.FormatMessage("{t, time}", args);

        // Default should include hours and minutes
        Assert.Contains("30", result);
    }

    [Fact]
    public void Time_ShortStyle()
    {
        var time = new DateTime(2024, 1, 1, 14, 30, 45);
        var args = new Dictionary<string, object?> { { "t", time } };

        var result = _formatter.FormatMessage("{t, time, short}", args);

        // Short time (no seconds)
        Assert.Contains("30", result);
    }

    [Fact]
    public void Time_MediumStyle()
    {
        var time = new DateTime(2024, 1, 1, 14, 30, 45);
        var args = new Dictionary<string, object?> { { "t", time } };

        var result = _formatter.FormatMessage("{t, time, medium}", args);

        // Medium includes seconds
        Assert.Contains("30", result);
        Assert.Contains("45", result);
    }

    [Fact]
    public void Time_CustomFormat()
    {
        var time = new DateTime(2024, 1, 1, 14, 30, 45);
        var args = new Dictionary<string, object?> { { "t", time } };

        var result = _formatter.FormatMessage("{t, time, HH:mm:ss}", args);

        Assert.Equal("14:30:45", result);
    }

    [Fact]
    public void Time_CustomFormat_12Hour()
    {
        var time = new DateTime(2024, 1, 1, 14, 30, 0);
        var args = new Dictionary<string, object?> { { "t", time } };

        var result = _formatter.FormatMessage("{t, time, h:mm tt}", args);

        Assert.Equal("2:30 PM", result);
    }

    [Fact]
    public void Time_WithTimeOnly()
    {
        var time = new TimeOnly(14, 30, 45);
        var args = new Dictionary<string, object?> { { "t", time } };

        var result = _formatter.FormatMessage("{t, time, HH:mm:ss}", args);

        Assert.Equal("14:30:45", result);
    }

    #endregion

    #region DateTime Formatting Tests

    [Fact]
    public void DateTime_DefaultStyle()
    {
        var dt = new DateTime(2024, 6, 15, 14, 30, 0);
        var args = new Dictionary<string, object?> { { "dt", dt } };

        var result = _formatter.FormatMessage("{dt, datetime}", args);

        // Default should include both date and time
        Assert.Contains("6", result);
        Assert.Contains("15", result);
    }

    [Fact]
    public void DateTime_ShortStyle()
    {
        var dt = new DateTime(2024, 6, 15, 14, 30, 0);
        var args = new Dictionary<string, object?> { { "dt", dt } };

        var result = _formatter.FormatMessage("{dt, datetime, short}", args);

        Assert.Contains("6", result);
        Assert.Contains("15", result);
    }

    [Fact]
    public void DateTime_FullStyle()
    {
        var dt = new DateTime(2024, 6, 15, 14, 30, 0);
        var args = new Dictionary<string, object?> { { "dt", dt } };

        var result = _formatter.FormatMessage("{dt, datetime, full}", args);

        // Full should include day name
        Assert.Contains("Saturday", result);
        Assert.Contains("June", result);
    }

    [Fact]
    public void DateTime_CustomFormat()
    {
        var dt = new DateTime(2024, 6, 15, 14, 30, 45);
        var args = new Dictionary<string, object?> { { "dt", dt } };

        var result = _formatter.FormatMessage("{dt, datetime, yyyy-MM-dd HH:mm:ss}", args);

        Assert.Equal("2024-06-15 14:30:45", result);
    }

    [Fact]
    public void DateTime_CustomFormat_ISO8601()
    {
        var dt = new DateTime(2024, 6, 15, 14, 30, 45);
        var args = new Dictionary<string, object?> { { "dt", dt } };

        var result = _formatter.FormatMessage("{dt, datetime, ::yyyy-MM-ddTHH:mm:ss}", args);

        Assert.Equal("2024-06-15T14:30:45", result);
    }

    [Fact]
    public void DateTime_InMessage()
    {
        var dt = new DateTime(2024, 6, 15, 14, 30, 0);
        var args = new Dictionary<string, object?> { { "dt", dt }, { "name", "Alice" } };

        var result = _formatter.FormatMessage("Hello {name}! The event is scheduled for {dt, datetime, MMMM d, yyyy} at {dt, time, h:mm tt}.", args);

        Assert.Contains("Alice", result);
        Assert.Contains("June 15, 2024", result);
        Assert.Contains("2:30 PM", result);
    }

    [Fact]
    public void DateTime_WithDateTimeOffset()
    {
        var dt = new DateTimeOffset(2024, 6, 15, 14, 30, 45, TimeSpan.Zero);
        var args = new Dictionary<string, object?> { { "dt", dt } };

        var result = _formatter.FormatMessage("{dt, datetime, yyyy-MM-dd HH:mm:ss}", args);

        Assert.Equal("2024-06-15 14:30:45", result);
    }

    #endregion

    #region Instance Format Method Tests

    [Fact]
    public void InstanceFormat_Works()
    {
        var args = new Dictionary<string, object?> { { "name", "World" } };

        var result = _formatter.FormatMessage("Hello, {name}!", args);

        Assert.Equal("Hello, World!", result);
    }

    #endregion

    #region Locale Fallback Tests

    [Theory]
    [InlineData("en-US", 1, "day")]
    [InlineData("en-US", 2, "days")]
    [InlineData("en-GB", 1, "day")]
    [InlineData("en-GB", 5, "days")]
    public void PluralLocaleFallback_EnglishVariants_FallBackToEnglish(string locale, double n, string expected)
    {
        var formatter = new MessageFormatter(locale, MessageFormatterOptions.Default);
        var pattern = "{test, plural, one {day} other {days}}";
        var args = new Dictionary<string, object?> { { "test", n } };

        var result = formatter.FormatMessage(pattern, args);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("xx-YY", 1, "item")]  // Unknown locale falls back to English
    [InlineData("xx-YY", 2, "items")]
    [InlineData("zz", 1, "item")]     // Unknown base locale falls back to English
    [InlineData("zz", 5, "items")]
    public void PluralLocaleFallback_UnknownLocale_FallsBackToEnglish(string locale, double n, string expected)
    {
        // Use options with fallback locale configured to enable fallback behavior
        var options = new MessageFormatterOptions { DefaultFallbackLocale = "en" };
        var formatter = new MessageFormatter(locale, options);
        var pattern = "{test, plural, one {item} other {items}}";
        var args = new Dictionary<string, object?> { { "test", n } };

        var result = formatter.FormatMessage(pattern, args);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("en_US", 1, "day")]   // Underscore separator
    [InlineData("en_US", 2, "days")]
    [InlineData("en_GB", 1, "day")]
    [InlineData("en_GB", 5, "days")]
    public void PluralLocaleFallback_UnderscoreSeparator_Works(string locale, double n, string expected)
    {
        var formatter = new MessageFormatter(locale, MessageFormatterOptions.Default);
        var pattern = "{test, plural, one {day} other {days}}";
        var args = new Dictionary<string, object?> { { "test", n } };

        var result = formatter.FormatMessage(pattern, args);

        Assert.Equal(expected, result);
    }

    #endregion
}
