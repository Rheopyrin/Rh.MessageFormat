using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

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

    /// <summary>
    /// Cache for property info arrays to avoid repeated reflection calls.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    /// <summary>
    /// Converts an object to a dictionary using reflection.
    /// Supports anonymous types, POCOs, and any object with public properties.
    /// Nested objects are recursively converted to nested dictionaries.
    /// </summary>
    /// <remarks>
    /// This method uses reflection to read public instance properties from the object.
    /// Nested complex objects (anonymous types, POCOs) are recursively converted to dictionaries.
    /// If the input is already a dictionary type, it is converted directly.
    /// <code>
    /// // Anonymous type
    /// var dict = ObjectToDictionary(new { name = "John", age = 30 });
    /// // Result: { "name": "John", "age": 30 }
    ///
    /// // Nested anonymous type
    /// var dict = ObjectToDictionary(new { user = new { name = "John" } });
    /// // Result: { "user": { "name": "John" } }
    /// </code>
    /// </remarks>
    /// <param name="obj">The object to convert. If null, returns an empty dictionary.</param>
    /// <returns>A dictionary containing the object's public properties as key-value pairs.</returns>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2072:UnrecognizedReflectionPattern",
        Justification = "Public properties are preserved for anonymous types and POCOs used with message formatting.")]
    public static Dictionary<string, object?> ObjectToDictionary(object? obj)
    {
        return ObjectToDictionaryInternal(obj, recursive: true);
    }

    /// <summary>
    /// Internal method to convert object to dictionary with optional recursion.
    /// </summary>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2072:UnrecognizedReflectionPattern",
        Justification = "Public properties are preserved for anonymous types and POCOs used with message formatting.")]
    private static Dictionary<string, object?> ObjectToDictionaryInternal(object? obj, bool recursive)
    {
        if (obj == null)
            return new Dictionary<string, object?>(StringComparer.Ordinal);

        // If it's already a dictionary, convert it (recursively if needed)
        if (obj is IReadOnlyDictionary<string, object?> readOnlyDict)
        {
            if (recursive)
            {
                var result = new Dictionary<string, object?>(StringComparer.Ordinal);
                foreach (var kvp in readOnlyDict)
                {
                    result[kvp.Key] = ConvertValueIfNeeded(kvp.Value);
                }
                return result;
            }
            return new Dictionary<string, object?>(readOnlyDict, StringComparer.Ordinal);
        }

        if (obj is IDictionary<string, object?> dict)
        {
            if (recursive)
            {
                var result = new Dictionary<string, object?>(StringComparer.Ordinal);
                foreach (var kvp in dict)
                {
                    result[kvp.Key] = ConvertValueIfNeeded(kvp.Value);
                }
                return result;
            }
            return new Dictionary<string, object?>(dict, StringComparer.Ordinal);
        }

        if (obj is IDictionary nonGenericDict)
        {
            var result = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (DictionaryEntry entry in nonGenericDict)
            {
                if (entry.Key is string stringKey)
                    result[stringKey] = recursive ? ConvertValueIfNeeded(entry.Value) : entry.Value;
            }
            return result;
        }

        // Use reflection to get properties
        var type = obj.GetType();
        var properties = GetCachedProperties(type);

        var dictionary = new Dictionary<string, object?>(properties.Length, StringComparer.Ordinal);
        foreach (var prop in properties)
        {
            if (prop.CanRead)
            {
                var value = prop.GetValue(obj);
                dictionary[prop.Name] = recursive ? ConvertValueIfNeeded(value) : value;
            }
        }

        return dictionary;
    }

    /// <summary>
    /// Converts a value to a dictionary if it's a complex object that should be nested.
    /// </summary>
    private static object? ConvertValueIfNeeded(object? value)
    {
        if (value == null)
            return null;

        var type = value.GetType();

        // Don't convert primitive types, strings, dates, etc.
        if (type.IsPrimitive || type.IsEnum ||
            type == typeof(string) || type == typeof(decimal) ||
            type == typeof(DateTime) || type == typeof(DateTimeOffset) ||
            type == typeof(TimeSpan) || type == typeof(Guid))
        {
            return value;
        }

        // Don't convert arrays or collections (they're values, not nested objects)
        if (type.IsArray || typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            // But if it's a dictionary, convert it
            if (value is IDictionary)
            {
                return ObjectToDictionaryInternal(value, recursive: true);
            }
            return value;
        }

        // Convert anonymous types and POCOs to dictionaries
        if (type.IsClass && !type.IsAbstract)
        {
            return ObjectToDictionaryInternal(value, recursive: true);
        }

        return value;
    }

    /// <summary>
    /// Gets cached properties for a type, with proper trimming annotations.
    /// Anonymous types are not cached to avoid unbounded cache growth.
    /// </summary>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070:UnrecognizedReflectionPattern",
        Justification = "The properties are only used for reading values, which is safe for trimming.")]
    private static PropertyInfo[] GetCachedProperties(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type)
    {
        // Don't cache anonymous types - they have unique names and would cause unbounded cache growth
        if (IsAnonymousType(type))
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        return PropertyCache.GetOrAdd(type, t =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
    }

    /// <summary>
    /// Determines if a type is an anonymous type.
    /// Anonymous types are compiler-generated and have specific characteristics.
    /// </summary>
    private static bool IsAnonymousType(Type type)
    {
        // Anonymous types have these characteristics:
        // 1. Compiler-generated attribute
        // 2. Name contains "AnonymousType"
        // 3. Are generic types with specific naming pattern like <>f__AnonymousType
        return type.IsClass &&
               type.IsSealed &&
               type.IsGenericType &&
               type.Name.Contains("AnonymousType") &&
               type.Name.StartsWith("<>");
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
