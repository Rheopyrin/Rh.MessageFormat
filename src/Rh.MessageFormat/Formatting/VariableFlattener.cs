using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using BitFaster.Caching.Lru;

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
    /// Cache for member info arrays to avoid repeated reflection calls.
    /// Uses LRU eviction with fixed capacity to prevent unbounded growth.
    /// </summary>
    private static readonly ConcurrentLru<Type, TypeAccessors> AccessorCache = new(1024);

    /// <summary>
    /// Converts an object to a dictionary using reflection.
    /// Supports anonymous types, POCOs, value tuples, and any object with public properties or fields.
    /// Nested objects are recursively converted to nested dictionaries.
    /// This overload is safe for trimming and Native AOT: the generic type parameter is annotated
    /// so the compiler preserves the public properties and fields of <typeparamref name="T"/> at each call site.
    /// </summary>
    /// <remarks>
    /// This method uses reflection to read public instance properties and fields from the object.
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
    /// Note for trimming/Native AOT: only the members of <typeparamref name="T"/> itself are
    /// statically preserved. Members of nested complex objects may be trimmed unless those types
    /// are otherwise rooted; use nested dictionaries for nesting in trimmed applications.
    /// </remarks>
    /// <typeparam name="T">The type of the object to convert. Its public properties and fields are preserved for trimming.</typeparam>
    /// <param name="obj">The object to convert. If null, returns an empty dictionary.</param>
    /// <returns>A dictionary containing the object's public properties and fields as key-value pairs.</returns>
    public static Dictionary<string, object?> ObjectToDictionary<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields)] T>(
        T? obj)
    {
        return ObjectToDictionaryInternal(obj, recursive: true);
    }

    /// <summary>
    /// Converts an object to a dictionary using reflection.
    /// Supports anonymous types, POCOs, value tuples, and any object with public properties or fields.
    /// Nested objects are recursively converted to nested dictionaries.
    /// </summary>
    /// <remarks>
    /// This overload is not safe for trimming or Native AOT because the runtime type of
    /// <paramref name="obj"/> is not statically visible to the trimmer. Prefer the generic
    /// <see cref="ObjectToDictionary{T}(T)"/> overload, or pass a dictionary.
    /// </remarks>
    /// <param name="obj">The object to convert. If null, returns an empty dictionary.</param>
    /// <returns>A dictionary containing the object's public properties and fields as key-value pairs.</returns>
    [RequiresUnreferencedCode(
        "Uses reflection over the runtime type of 'obj', which the trimmer cannot analyze. " +
        "Use the generic ObjectToDictionary<T> overload or pass an IReadOnlyDictionary<string, object?> instead.")]
    public static Dictionary<string, object?> ObjectToDictionary(object? obj)
    {
        return ObjectToDictionaryInternal(obj, recursive: true);
    }

    /// <summary>
    /// Internal method to convert object to dictionary with optional recursion.
    /// </summary>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2072:UnrecognizedReflectionPattern",
        Justification = "Top-level argument types are preserved via the DynamicallyAccessedMembers annotation " +
                        "on the generic public entry points; the non-generic entry points are marked " +
                        "RequiresUnreferencedCode. Nested complex objects are a documented limitation " +
                        "under trimming (use nested dictionaries instead).")]
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

        // Use reflection to get properties and fields
        var type = obj.GetType();
        var accessors = GetCachedAccessors(type);
        var properties = accessors.Properties;
        var fields = accessors.Fields;

        var dictionary = new Dictionary<string, object?>(properties.Length + fields.Length, StringComparer.Ordinal);
        foreach (var prop in properties)
        {
            if (prop.CanRead)
            {
                var value = prop.GetValue(obj);
                dictionary[prop.Name] = recursive ? ConvertValueIfNeeded(value) : value;
            }
        }

        // Public instance fields (e.g. ValueTuple Item1..ItemN, POCOs with public fields).
        // Properties take precedence when a field shares the same name.
        foreach (var field in fields)
        {
            if (!dictionary.ContainsKey(field.Name))
            {
                var value = field.GetValue(obj);
                dictionary[field.Name] = recursive ? ConvertValueIfNeeded(value) : value;
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
    /// Gets cached properties and fields for a type, with proper trimming annotations.
    /// Uses LRU cache with fixed capacity, so all types including anonymous types are cached.
    /// </summary>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070:UnrecognizedReflectionPattern",
        Justification = "The annotation on the parameter does not flow into the cache factory lambda, " +
                        "but the members are guaranteed to be preserved by the callers' annotations.")]
    private static TypeAccessors GetCachedAccessors(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields)] Type type)
    {
        return AccessorCache.GetOrAdd(type, t => new TypeAccessors(
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance),
            t.GetFields(BindingFlags.Public | BindingFlags.Instance)));
    }

    /// <summary>
    /// Cached reflection members for a type.
    /// </summary>
    private sealed class TypeAccessors
    {
        public TypeAccessors(PropertyInfo[] properties, FieldInfo[] fields)
        {
            Properties = properties;
            Fields = fields;
        }

        public PropertyInfo[] Properties { get; }
        public FieldInfo[] Fields { get; }
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
