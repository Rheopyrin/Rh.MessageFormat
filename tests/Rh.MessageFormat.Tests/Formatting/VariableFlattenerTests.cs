using System.Collections.Generic;
using Rh.MessageFormat.Formatting;
using Xunit;

namespace Rh.MessageFormat.Tests.Formatting;

/// <summary>
/// Tests for VariableFlattener utility.
/// </summary>
public class VariableFlattenerTests
{
    #region Basic Flattening Tests

    [Fact]
    public void FlattenVariables_FlatDictionary_ReturnsSame()
    {
        var input = new Dictionary<string, object?>
        {
            ["name"] = "John",
            ["age"] = 30
        };

        var result = VariableFlattener.FlattenVariables(input);

        Assert.Equal(2, result.Count);
        Assert.Equal("John", result["name"]);
        Assert.Equal(30, result["age"]);
    }

    [Fact]
    public void FlattenVariables_OneLevel_FlattensCorrectly()
    {
        var input = new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>
            {
                ["firstName"] = "John",
                ["lastName"] = "Doe"
            }
        };

        var result = VariableFlattener.FlattenVariables(input);

        Assert.Equal(2, result.Count);
        Assert.Equal("John", result["user__firstName"]);
        Assert.Equal("Doe", result["user__lastName"]);
    }

    [Fact]
    public void FlattenVariables_TwoLevels_FlattensCorrectly()
    {
        var input = new Dictionary<string, object?>
        {
            ["data"] = new Dictionary<string, object?>
            {
                ["user"] = new Dictionary<string, object?>
                {
                    ["name"] = "John"
                }
            }
        };

        var result = VariableFlattener.FlattenVariables(input);

        Assert.Single(result);
        Assert.Equal("John", result["data__user__name"]);
    }

    [Fact]
    public void FlattenVariables_MixedFlatAndNested_CombinesCorrectly()
    {
        var input = new Dictionary<string, object?>
        {
            ["status"] = "active",
            ["user"] = new Dictionary<string, object?>
            {
                ["name"] = "John"
            },
            ["count"] = 5
        };

        var result = VariableFlattener.FlattenVariables(input);

        Assert.Equal(3, result.Count);
        Assert.Equal("active", result["status"]);
        Assert.Equal("John", result["user__name"]);
        Assert.Equal(5, result["count"]);
    }

    #endregion

    #region Custom Separator Tests

    [Fact]
    public void FlattenVariables_CustomSeparator_UsesSeparator()
    {
        var input = new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>
            {
                ["name"] = "John"
            }
        };

        var result = VariableFlattener.FlattenVariables(input, ".");

        Assert.Single(result);
        Assert.Equal("John", result["user.name"]);
    }

    [Fact]
    public void FlattenVariables_EmptySeparator_ConcatenatesKeys()
    {
        var input = new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>
            {
                ["Name"] = "John"
            }
        };

        var result = VariableFlattener.FlattenVariables(input, "");

        Assert.Single(result);
        Assert.Equal("John", result["userName"]);
    }

    #endregion

    #region Skip Predicate Tests

    [Fact]
    public void FlattenVariables_SkipPredicate_SkipsMatchingValues()
    {
        var customObject = new { Value = "special" };

        var input = new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>
            {
                ["name"] = "John"
            },
            ["special"] = customObject
        };

        // Skip anonymous objects
        var result = VariableFlattener.FlattenVariables(input, "__", v => v?.GetType().IsAnonymousType() == true);

        Assert.Equal(2, result.Count);
        Assert.Equal("John", result["user__name"]);
        Assert.Same(customObject, result["special"]);
    }

    [Fact]
    public void FlattenVariables_SkipNestedDictionary_TreatsAsLeaf()
    {
        var nestedDict = new Dictionary<string, object?>
        {
            ["nested"] = "value"
        };

        var input = new Dictionary<string, object?>
        {
            ["data"] = nestedDict
        };

        // Skip all dictionaries (treat them as leaf values)
        var result = VariableFlattener.FlattenVariables(input, "__", v => v is Dictionary<string, object?>);

        Assert.Single(result);
        Assert.Same(nestedDict, result["data"]);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void FlattenVariables_EmptyDictionary_ReturnsEmpty()
    {
        var input = new Dictionary<string, object?>();

        var result = VariableFlattener.FlattenVariables(input);

        Assert.Empty(result);
    }

    [Fact]
    public void FlattenVariables_EmptyNestedDictionary_NoKeysAdded()
    {
        var input = new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>(),
            ["name"] = "John"
        };

        var result = VariableFlattener.FlattenVariables(input);

        Assert.Single(result);
        Assert.Equal("John", result["name"]);
    }

    [Fact]
    public void FlattenVariables_NullValue_PreservesNull()
    {
        var input = new Dictionary<string, object?>
        {
            ["value"] = null
        };

        var result = VariableFlattener.FlattenVariables(input);

        Assert.Single(result);
        Assert.Null(result["value"]);
    }

    [Fact]
    public void FlattenVariables_NullValueInNested_PreservesNull()
    {
        var input = new Dictionary<string, object?>
        {
            ["user"] = new Dictionary<string, object?>
            {
                ["name"] = null
            }
        };

        var result = VariableFlattener.FlattenVariables(input);

        Assert.Single(result);
        Assert.Null(result["user__name"]);
    }

    [Fact]
    public void FlattenVariables_NonStringKeyDictionary_IgnoresNonStringKeys()
    {
        var hashtable = new System.Collections.Hashtable
        {
            ["stringKey"] = "value1",
            [123] = "value2" // Non-string key
        };

        var input = new Dictionary<string, object?>
        {
            ["data"] = hashtable
        };

        var result = VariableFlattener.FlattenVariables(input);

        // Only string key should be flattened
        Assert.Single(result);
        Assert.Equal("value1", result["data__stringKey"]);
    }

    #endregion

    #region Default Separator Constant Test

    [Fact]
    public void DefaultSeparator_IsDoubleUnderscore()
    {
        Assert.Equal("__", VariableFlattener.DefaultSeparator);
    }

    #endregion
}

/// <summary>
/// Extension methods for type checking.
/// </summary>
internal static class TypeExtensions
{
    public static bool IsAnonymousType(this System.Type type)
    {
        return type.Name.Contains("AnonymousType");
    }
}
