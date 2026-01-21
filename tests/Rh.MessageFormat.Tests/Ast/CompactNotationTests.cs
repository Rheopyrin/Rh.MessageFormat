using System.Collections.Generic;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Tests for compact number notation (K, M, B, T suffixes).
/// </summary>
public class CompactNotationTests
{
    private readonly MessageFormatter _formatter = new("en", TestOptions.WithEnglish());

    #region Short Notation Tests

    [Fact]
    public void CompactShort_Thousands()
    {
        var args = new Dictionary<string, object?> { { "n", 1500 } };

        var result = _formatter.FormatMessage("{n, number, ::compact-short}", args);

        Assert.Contains("K", result);
    }

    [Fact]
    public void CompactShort_Millions()
    {
        var args = new Dictionary<string, object?> { { "n", 2500000 } };

        var result = _formatter.FormatMessage("{n, number, ::compact-short}", args);

        Assert.Contains("M", result);
    }

    [Fact]
    public void CompactShort_Billions()
    {
        var args = new Dictionary<string, object?> { { "n", 3500000000 } };

        var result = _formatter.FormatMessage("{n, number, ::compact-short}", args);

        Assert.Contains("B", result);
    }

    [Fact]
    public void CompactShort_SmallNumber_NoCompact()
    {
        var args = new Dictionary<string, object?> { { "n", 500 } };

        var result = _formatter.FormatMessage("{n, number, ::compact-short}", args);

        // Numbers under 1000 should not be compacted
        Assert.DoesNotContain("K", result);
        Assert.Contains("500", result);
    }

    #endregion

    #region Long Notation Tests

    [Fact]
    public void CompactLong_Million()
    {
        var args = new Dictionary<string, object?> { { "n", 1000000 } };

        var result = _formatter.FormatMessage("{n, number, ::compact-long}", args);

        Assert.Contains("million", result);
    }

    [Fact]
    public void CompactLong_Billion()
    {
        var args = new Dictionary<string, object?> { { "n", 1000000000 } };

        var result = _formatter.FormatMessage("{n, number, ::compact-long}", args);

        Assert.Contains("billion", result);
    }

    #endregion

    #region Concise Syntax Tests

    [Fact]
    public void ConciseK_Thousands()
    {
        var args = new Dictionary<string, object?> { { "n", 5000 } };

        var result = _formatter.FormatMessage("{n, number, ::K}", args);

        Assert.Contains("K", result);
    }

    [Fact]
    public void ConciseKK_Long()
    {
        var args = new Dictionary<string, object?> { { "n", 1000000 } };

        var result = _formatter.FormatMessage("{n, number, ::KK}", args);

        Assert.Contains("million", result);
    }

    #endregion
}
