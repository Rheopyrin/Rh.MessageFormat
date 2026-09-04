using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Hashing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BitFaster.Caching.Lru;
using Rh.MessageFormat.Abstractions.Interfaces;
using Rh.MessageFormat.Ast;
using Rh.MessageFormat.Ast.Elements;
using Rh.MessageFormat.Ast.Parser;
using Rh.MessageFormat.Custom;
using Rh.MessageFormat.Exceptions;
using Rh.MessageFormat.Formatting;
using Rh.MessageFormat.Options;
using Rh.MessageFormat.Pools;

namespace Rh.MessageFormat;

/// <summary>
///     The magical Message Formatter.
/// </summary>
public class MessageFormatter : IMessageFormatter
{
    #region Fields

    /// <summary>
    ///     The AST-based message parser.
    /// </summary>
    private readonly MessageParser _parser;

    /// <summary>
    ///     The formatter options.
    /// </summary>
    private readonly IMessageFormatterOptions _options;

    /// <summary>
    ///     The target locale for this formatter instance.
    /// </summary>
    private readonly string _locale;

    /// <summary>
    ///     The culture info for the target locale.
    /// </summary>
    private readonly CultureInfo _culture;

    /// <summary>
    ///     The pluralizer delegate for the target locale.
    /// </summary>
    private readonly PluralRuleDelegate _pluralizer;

    /// <summary>
    ///     The ordinalizer delegate for the target locale.
    /// </summary>
    private readonly OrdinalRuleDelegate _ordinalizer;

    /// <summary>
    ///     The custom formatters dictionary (readonly).
    /// </summary>
    private readonly IReadOnlyDictionary<string, CustomFormatterDelegate>? _formatters;

    /// <summary>
    ///     The tag handlers dictionary (readonly).
    /// </summary>
    private readonly IReadOnlyDictionary<string, TagHandler>? _tagHandlers;

    /// <summary>
    ///     The cache for parsed messages (null if caching is disabled).
    ///     Uses XxHash128 of the pattern as key for memory efficiency.
    /// </summary>
    private readonly ConcurrentLru<UInt128, ParsedMessage>? _parseCache;

    /// <summary>
    ///     Shared empty arguments dictionary for messages without variables.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, object?> EmptyArgs =
        new Dictionary<string, object?>(0, StringComparer.Ordinal);

    #endregion

    #region Constructors

    /// <summary>
    ///     Initializes a new instance of the <see cref="MessageFormatter" /> class
    ///     with the default options and locale.
    /// </summary>
    /// <param name="locale">
    ///     The target locale for formatting (e.g., "en", "en-US", "de-DE").
    ///     If null, uses the DefaultFallbackLocale from options.
    /// </param>
    public MessageFormatter(string locale)
        : this(locale, MessageFormatterOptions.Default)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MessageFormatter" /> class
    ///     with the specified options and locale.
    /// </summary>
    /// <param name="options">
    ///     The formatter options.
    /// </param>
    /// <param name="locale">
    ///     The target locale for formatting (e.g., "en", "en-US", "de-DE").
    ///     If null, uses the DefaultFallbackLocale from options.
    /// </param>
    public MessageFormatter(string locale, IMessageFormatterOptions options)
    {
        _locale = locale;
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _parser = new MessageParser();

        _culture = _options.CultureInfoCache.GetCulture(locale);

        var localeData = ResolveLocaleData(locale, _options.DefaultFallbackLocale);
        _pluralizer = GetPluralizer(localeData);
        _ordinalizer = GetOrdinalizer(localeData);

        // Initialize formatters and handlers dictionaries once
        _formatters = _options.CustomFormatters.Count > 0 ? _options.CustomFormatters.AsReadOnly() : null;
        _tagHandlers = _options.TagHandlers.Count > 0 ? _options.TagHandlers.AsReadOnly() : null;

        // Initialize parser cache if enabled
        if (_options.ParserCache.Enabled)
        {
            _parseCache = new ConcurrentLru<UInt128, ParsedMessage>(_options.ParserCache.MaxCapacity);
        }
    }

    #endregion

    #region Public Methods and Operators

    /// <summary>
    ///     Formats the message with the specified arguments using the locale configured in the constructor.
    /// </summary>
    /// <param name="pattern">
    ///     The pattern.
    /// </param>
    /// <param name="args">
    ///     The arguments.
    /// </param>
    /// <returns>
    ///     The formatted message.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string FormatMessage(string pattern, IReadOnlyDictionary<string, object?> args)
    {
        var ignoreTag = _options.IgnoreTag;
        var message = _parseCache != null
            ? _parseCache.GetOrAdd(GetPatternHash(pattern, ignoreTag), _ => _parser.Parse(pattern, ignoreTag))
            : _parser.Parse(pattern, ignoreTag);

        var output = StringBuilderPool.Get();
        try
        {
            var ctx = new FormatterContext(
                args,
                _culture,
                _locale,
                _options.DefaultFallbackLocale,
                _options.CldrDataProvider,
                _pluralizer,
                _ordinalizer,
                _formatters,
                _tagHandlers,
                _options.RequireAllVariables);
            message.Format(ref ctx, output);
            return output.ToString();
        }
        finally
        {
            StringBuilderPool.Return(output);
        }
    }

    /// <summary>
    ///     Formats a message that has no arguments using the locale configured in the constructor.
    /// </summary>
    /// <param name="pattern">
    ///     The pattern.
    /// </param>
    /// <returns>
    ///     The formatted message.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string FormatMessage(string pattern)
    {
        return FormatMessage(pattern, EmptyArgs);
    }

    /// <summary>
    ///     Formats the message with the specified arguments using the locale configured in the constructor.
    ///     This overload accepts any object (including anonymous types) and converts it to a dictionary.
    ///     It is safe for trimming and Native AOT: the public properties and fields of
    ///     <typeparamref name="T"/> are preserved at each call site.
    /// </summary>
    /// <remarks>
    ///     Note for trimming/Native AOT: only the members of <typeparamref name="T"/> itself are
    ///     statically preserved. Members of nested complex objects may be trimmed; use nested
    ///     dictionaries for nesting in trimmed applications.
    /// </remarks>
    /// <typeparam name="T">
    ///     The type of the arguments object. Its public properties and fields are preserved for trimming.
    /// </typeparam>
    /// <param name="pattern">
    ///     The pattern.
    /// </param>
    /// <param name="args">
    ///     The arguments as an object (anonymous type, POCO, record, or value tuple).
    /// </param>
    /// <returns>
    ///     The formatted message.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string FormatMessage<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields)] T>(
        string pattern, T args)
    {
        if (args is IReadOnlyDictionary<string, object?> dictionaryArgs)
            return FormatMessage(pattern, dictionaryArgs);

        return FormatMessage(pattern, VariableFlattener.ObjectToDictionary(args));
    }

    /// <summary>
    ///     Formats the message with the specified arguments using the locale configured in the constructor.
    ///     This overload accepts any object (including anonymous types) and converts it to a dictionary.
    /// </summary>
    /// <remarks>
    ///     This overload is not safe for trimming or Native AOT because the runtime type of
    ///     <paramref name="args"/> is not statically visible to the trimmer. Prefer the generic
    ///     <see cref="FormatMessage{T}(string, T)"/> overload, or pass a dictionary.
    /// </remarks>
    /// <param name="pattern">
    ///     The pattern.
    /// </param>
    /// <param name="args">
    ///     The arguments as an object (anonymous type, POCO, or any object with public properties).
    /// </param>
    /// <returns>
    ///     The formatted message.
    /// </returns>
    [RequiresUnreferencedCode(
        "Uses reflection over the runtime type of 'args', which the trimmer cannot analyze. " +
        "Use the generic FormatMessage<T> overload or pass an IReadOnlyDictionary<string, object?> instead.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string FormatMessage(string pattern, object? args = null)
    {
        var dictionary = VariableFlattener.ObjectToDictionary(args);
        return FormatMessage(pattern, dictionary);
    }

    /// <summary>
    ///     Formats a complex message with support for nested object values.
    ///     Nested objects are flattened using "__" as a separator.
    /// </summary>
    /// <remarks>
    ///     This method allows passing nested objects in the values dictionary.
    ///     For example:
    ///     <code>
    ///     var values = new Dictionary&lt;string, object?&gt;
    ///     {
    ///         ["user"] = new Dictionary&lt;string, object?&gt;
    ///         {
    ///             ["firstName"] = "John",
    ///             ["lastName"] = "Doe"
    ///         }
    ///     };
    ///     formatter.FormatComplexMessage("Hello {user__firstName} {user__lastName}", values);
    ///     // Returns: "Hello John Doe"
    ///     </code>
    /// </remarks>
    /// <param name="pattern">
    ///     The message pattern. Use "__" (double underscore) to reference nested values.
    /// </param>
    /// <param name="values">
    ///     The values dictionary, which may contain nested objects.
    /// </param>
    /// <returns>
    ///     The formatted message.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string FormatComplexMessage(string pattern, IReadOnlyDictionary<string, object?> values)
    {
        var flattenedValues = VariableFlattener.FlattenVariables(values);
        return FormatMessage(pattern, flattenedValues);
    }

    /// <summary>
    ///     Formats a complex message that has no arguments.
    /// </summary>
    /// <param name="pattern">
    ///     The message pattern. Use "__" (double underscore) to reference nested values.
    /// </param>
    /// <returns>
    ///     The formatted message.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string FormatComplexMessage(string pattern)
    {
        return FormatComplexMessage(pattern, EmptyArgs);
    }

    /// <summary>
    ///     Formats a complex message with support for nested object values.
    ///     This overload accepts any object (including anonymous types) and converts it to a dictionary.
    ///     It is safe for trimming and Native AOT: the public properties and fields of
    ///     <typeparamref name="T"/> are preserved at each call site.
    /// </summary>
    /// <remarks>
    ///     Note for trimming/Native AOT: only the members of <typeparamref name="T"/> itself are
    ///     statically preserved. Members of nested complex objects may be trimmed; use nested
    ///     dictionaries for nesting in trimmed applications.
    /// </remarks>
    /// <typeparam name="T">
    ///     The type of the values object. Its public properties and fields are preserved for trimming.
    /// </typeparam>
    /// <param name="pattern">
    ///     The message pattern. Use "__" (double underscore) to reference nested values.
    /// </param>
    /// <param name="values">
    ///     The values as an object (anonymous type, POCO, record, or value tuple).
    /// </param>
    /// <returns>
    ///     The formatted message.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string FormatComplexMessage<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields)] T>(
        string pattern, T values)
    {
        if (values is IReadOnlyDictionary<string, object?> dictionaryValues)
            return FormatComplexMessage(pattern, dictionaryValues);

        return FormatComplexMessage(pattern, VariableFlattener.ObjectToDictionary(values));
    }

    /// <summary>
    ///     Formats a complex message with support for nested object values.
    ///     This overload accepts any object (including anonymous types) and converts it to a dictionary.
    /// </summary>
    /// <remarks>
    ///     This overload is not safe for trimming or Native AOT because the runtime type of
    ///     <paramref name="values"/> is not statically visible to the trimmer. Prefer the generic
    ///     <see cref="FormatComplexMessage{T}(string, T)"/> overload, or pass a dictionary.
    /// </remarks>
    /// <param name="pattern">
    ///     The message pattern. Use "__" (double underscore) to reference nested values.
    /// </param>
    /// <param name="values">
    ///     The values as an object (anonymous type, POCO, or any object with public properties).
    /// </param>
    /// <returns>
    ///     The formatted message.
    /// </returns>
    [RequiresUnreferencedCode(
        "Uses reflection over the runtime type of 'values', which the trimmer cannot analyze. " +
        "Use the generic FormatComplexMessage<T> overload or pass an IReadOnlyDictionary<string, object?> instead.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string FormatComplexMessage(string pattern, object? values = null)
    {
        var dictionary = VariableFlattener.ObjectToDictionary(values);
        return FormatComplexMessage(pattern, dictionary);
    }

    /// <summary>
    ///     Formats a message containing HTML markup with safe variable substitution.
    ///     HTML tags in the message template are preserved, while variable values are HTML-escaped.
    /// </summary>
    /// <remarks>
    ///     This method is designed for messages that contain HTML markup. It:
    ///     <list type="bullet">
    ///         <item>Preserves HTML tags in the message template (e.g., &lt;a&gt;, &lt;b&gt;, &lt;br/&gt;)</item>
    ///         <item>HTML-escapes all variable values to prevent XSS attacks</item>
    ///         <item>Safely handles pre-escaped values to prevent double-escaping</item>
    ///     </list>
    /// </remarks>
    /// <param name="pattern">
    ///     The message pattern containing HTML markup.
    /// </param>
    /// <param name="values">
    ///     The values dictionary. All string values will be HTML-escaped.
    /// </param>
    /// <returns>
    ///     The formatted HTML message with escaped variable values.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string FormatHtmlMessage(string pattern, IReadOnlyDictionary<string, object?> values)
    {
        // HTML-escape all values to prevent XSS attacks
        var escapedValues = HtmlEncoder.EscapeValues(values);

        // Parse with ignoreTag=true to preserve HTML tags as literal text
        var message = _parseCache != null
            ? _parseCache.GetOrAdd(GetPatternHash(pattern, ignoreTag: true), _ => _parser.Parse(pattern, ignoreTag: true))
            : _parser.Parse(pattern, ignoreTag: true);

        var output = StringBuilderPool.Get();
        try
        {
            var ctx = new FormatterContext(
                escapedValues,
                _culture,
                _locale,
                _options.DefaultFallbackLocale,
                _options.CldrDataProvider,
                _pluralizer,
                _ordinalizer,
                _formatters,
                _tagHandlers,
                _options.RequireAllVariables);
            message.Format(ref ctx, output);
            return output.ToString();
        }
        finally
        {
            StringBuilderPool.Return(output);
        }
    }

    /// <summary>
    ///     Formats a message containing HTML markup that has no arguments.
    ///     HTML tags in the message template are preserved.
    /// </summary>
    /// <param name="pattern">
    ///     The message pattern containing HTML markup.
    /// </param>
    /// <returns>
    ///     The formatted HTML message.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string FormatHtmlMessage(string pattern)
    {
        return FormatHtmlMessage(pattern, EmptyArgs);
    }

    /// <summary>
    ///     Formats a message containing HTML markup with safe variable substitution.
    ///     This overload accepts any object (including anonymous types) and converts it to a dictionary.
    ///     It is safe for trimming and Native AOT: the public properties and fields of
    ///     <typeparamref name="T"/> are preserved at each call site.
    /// </summary>
    /// <remarks>
    ///     Note for trimming/Native AOT: only the members of <typeparamref name="T"/> itself are
    ///     statically preserved. Members of nested complex objects may be trimmed; use nested
    ///     dictionaries for nesting in trimmed applications.
    /// </remarks>
    /// <typeparam name="T">
    ///     The type of the values object. Its public properties and fields are preserved for trimming.
    /// </typeparam>
    /// <param name="pattern">
    ///     The message pattern containing HTML markup.
    /// </param>
    /// <param name="values">
    ///     The values as an object (anonymous type, POCO, record, or value tuple).
    ///     All string values will be HTML-escaped.
    /// </param>
    /// <returns>
    ///     The formatted HTML message with escaped variable values.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string FormatHtmlMessage<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields)] T>(
        string pattern, T values)
    {
        if (values is IReadOnlyDictionary<string, object?> dictionaryValues)
            return FormatHtmlMessage(pattern, dictionaryValues);

        return FormatHtmlMessage(pattern, VariableFlattener.ObjectToDictionary(values));
    }

    /// <summary>
    ///     Formats a message containing HTML markup with safe variable substitution.
    ///     This overload accepts any object (including anonymous types) and converts it to a dictionary.
    /// </summary>
    /// <remarks>
    ///     This overload is not safe for trimming or Native AOT because the runtime type of
    ///     <paramref name="values"/> is not statically visible to the trimmer. Prefer the generic
    ///     <see cref="FormatHtmlMessage{T}(string, T)"/> overload, or pass a dictionary.
    /// </remarks>
    /// <param name="pattern">
    ///     The message pattern containing HTML markup.
    /// </param>
    /// <param name="values">
    ///     The values as an object (anonymous type, POCO, or any object with public properties).
    ///     All string values will be HTML-escaped.
    /// </param>
    /// <returns>
    ///     The formatted HTML message with escaped variable values.
    /// </returns>
    [RequiresUnreferencedCode(
        "Uses reflection over the runtime type of 'values', which the trimmer cannot analyze. " +
        "Use the generic FormatHtmlMessage<T> overload or pass an IReadOnlyDictionary<string, object?> instead.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string FormatHtmlMessage(string pattern, object? values = null)
    {
        var dictionary = VariableFlattener.ObjectToDictionary(values);
        return FormatHtmlMessage(pattern, dictionary);
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Resolves locale data using the fallback chain: exact → base → fallback locale.
    ///     Throws <see cref="InvalidLocaleException"/> if no locale data can be resolved.
    /// </summary>
    /// <exception cref="InvalidLocaleException">
    ///     Thrown when neither the requested locale nor the fallback locale is supported.
    /// </exception>
    private ICldrLocaleData ResolveLocaleData(string locale, string? fallbackLocale)
    {
        var provider = _options.CldrDataProvider;

        // 1. Try exact match (e.g., "en-GB")
        if (provider.TryGetLocaleData(locale, out var data) && data != null)
            return data;

        // 2. Try base locale (e.g., "en-GB" → "en")
        var dashIndex = locale.IndexOf('-');
        if (dashIndex < 0) dashIndex = locale.IndexOf('_');

        if (dashIndex > 0)
        {
            var baseLocale = locale.Substring(0, dashIndex);
            if (provider.TryGetLocaleData(baseLocale, out data) && data != null)
                return data;
        }

        // 3. Try fallback locale if configured
        if (fallbackLocale != null && !locale.Equals(fallbackLocale, StringComparison.OrdinalIgnoreCase))
        {
            if (provider.TryGetLocaleData(fallbackLocale, out data) && data != null)
                return data;
        }

        // 4. No locale resolved - throw exception
        throw new InvalidLocaleException(locale, provider.AvailableLocales);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private PluralRuleDelegate GetPluralizer(ICldrLocaleData localeData) => localeData.GetPluralCategory;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private OrdinalRuleDelegate GetOrdinalizer(ICldrLocaleData localeData) => localeData.GetOrdinalCategory;

    /// <summary>
    ///     Computes a hash key for the pattern cache using XxHash128.
    ///     Uses different seeds to differentiate between normal and HTML parsing modes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt128 GetPatternHash(string pattern, bool ignoreTag)
    {
        var bytes = MemoryMarshal.AsBytes(pattern.AsSpan());
        return ignoreTag
            ? XxHash128.HashToUInt128(bytes, seed: 1)
            : XxHash128.HashToUInt128(bytes);
    }

    #endregion
}