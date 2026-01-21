using System;
using System.Collections;
using System.Collections.Generic;

namespace Rh.MessageFormat.Formatting;

/// <summary>
/// Utility for flattening nested object hierarchies into flat dictionaries.
/// Used by FormatComplexMessage to support nested variable substitution.
/// </summary>
public static class VariableFlattener
{
    /// <summary>
    /// The default separator used to join nested keys.
    /// </summary>
    public const string DefaultSeparator = "__";

    /// <summary>
    /// Flattens a dictionary containing nested objects into a flat dictionary.
    /// </summary>
    /// <remarks>
    /// Nested dictionaries are flattened using the separator to join keys.
    /// For example:
    /// <code>
    /// Input:  { "user": { "firstName": "John", "lastName": "Doe" }, "status": "active" }
    /// Output: { "user__firstName": "John", "user__lastName": "Doe", "status": "active" }
    /// </code>
    /// </remarks>
    /// <param name="variables">The dictionary to flatten.</param>
    /// <param name="separator">The separator to use for joining keys. Default is "__".</param>
    /// <param name="shouldSkipFlatten">
    /// Optional predicate to determine if a value should be skipped from flattening.
    /// If the predicate returns true, the value is treated as a leaf node.
    /// </param>
    /// <returns>A new flat dictionary with all nested values.</returns>
    public static Dictionary<string, object?> FlattenVariables(
        IReadOnlyDictionary<string, object?> variables,
        string separator = DefaultSeparator,
        Func<object?, bool>? shouldSkipFlatten = null)
    {
        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        FlattenRecursive(variables, null, separator, shouldSkipFlatten, result);
        return result;
    }

    private static void FlattenRecursive(
        IReadOnlyDictionary<string, object?> source,
        string? prefix,
        string separator,
        Func<object?, bool>? shouldSkipFlatten,
        Dictionary<string, object?> result)
    {
        foreach (var kvp in source)
        {
            var key = prefix == null ? kvp.Key : $"{prefix}{separator}{kvp.Key}";
            var value = kvp.Value;

            // Check if we should skip flattening this value
            if (shouldSkipFlatten != null && shouldSkipFlatten(value))
            {
                result[key] = value;
                continue;
            }

            // Try to flatten nested dictionaries
            if (TryGetAsReadOnlyDictionary(value, out var nestedDict))
            {
                FlattenRecursive(nestedDict!, prefix == null ? kvp.Key : key, separator, shouldSkipFlatten, result);
            }
            else
            {
                result[key] = value;
            }
        }
    }

    private static bool TryGetAsReadOnlyDictionary(object? value, out IReadOnlyDictionary<string, object?>? dict)
    {
        dict = null;

        if (value == null)
            return false;

        // Check for IReadOnlyDictionary<string, object?>
        if (value is IReadOnlyDictionary<string, object?> readOnlyDict)
        {
            dict = readOnlyDict;
            return true;
        }

        // Check for Dictionary<string, object?>
        if (value is Dictionary<string, object?> regularDict)
        {
            dict = regularDict;
            return true;
        }

        // Check for IDictionary<string, object?>
        if (value is IDictionary<string, object?> idict)
        {
            dict = new DictionaryWrapper(idict);
            return true;
        }

        // Check for non-generic IDictionary with string keys
        if (value is IDictionary nonGenericDict)
        {
            var wrapper = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (DictionaryEntry entry in nonGenericDict)
            {
                if (entry.Key is string stringKey)
                {
                    wrapper[stringKey] = entry.Value;
                }
            }
            if (wrapper.Count > 0)
            {
                dict = wrapper;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Wrapper to adapt IDictionary to IReadOnlyDictionary.
    /// </summary>
    private sealed class DictionaryWrapper : IReadOnlyDictionary<string, object?>
    {
        private readonly IDictionary<string, object?> _inner;

        public DictionaryWrapper(IDictionary<string, object?> inner) => _inner = inner;

        public object? this[string key] => _inner[key];
        public IEnumerable<string> Keys => _inner.Keys;
        public IEnumerable<object?> Values => _inner.Values;
        public int Count => _inner.Count;
        public bool ContainsKey(string key) => _inner.ContainsKey(key);
        public bool TryGetValue(string key, out object? value) => _inner.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _inner.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
