using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Rh.MessageFormat.Abstractions.Interfaces;
using Rh.MessageFormat.Abstractions.Models;
using Rh.MessageFormat.Custom;
using Rh.MessageFormat.Exceptions;
using Rh.MessageFormat.Formatting;

namespace Rh.MessageFormat.Ast.Elements;

/// <summary>
/// Delegate for pluralization rules. Matches the ContextPluralizer signature.
/// </summary>
/// <param name="context">The plural context.</param>
/// <returns>The plural category (zero, one, two, few, many, other).</returns>
internal delegate string PluralRuleDelegate(PluralContext context);

/// <summary>
/// Delegate for ordinal rules. Same signature as plural rules.
/// </summary>
/// <param name="context">The plural context.</param>
/// <returns>The ordinal category (one, two, few, many, other).</returns>
internal delegate string OrdinalRuleDelegate(PluralContext context);

/// <summary>
/// Context for formatting messages. Uses ref struct for zero heap allocation.
/// </summary>
internal ref struct FormatterContext
{
    private readonly IReadOnlyDictionary<string, object?> _args;
    private readonly CultureInfo _culture;
    private readonly PluralRuleDelegate _pluralizer;
    private readonly OrdinalRuleDelegate _ordinalizer;
    private readonly string _locale;
    private readonly string? _fallbackLocale;
    private readonly ICldrDataProvider _cldrDataProvider;
    private readonly IReadOnlyDictionary<string, CustomFormatterDelegate>? _customFormatters;
    private readonly IReadOnlyDictionary<string, TagHandler>? _tagHandlers;
    private readonly bool _requireAllVariables;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FormatterContext(
        IReadOnlyDictionary<string, object?> args,
        CultureInfo culture,
        string locale,
        string? fallbackLocale,
        ICldrDataProvider cldrDataProvider,
        PluralRuleDelegate pluralizer,
        OrdinalRuleDelegate ordinalizer,
        IReadOnlyDictionary<string, CustomFormatterDelegate>? customFormatters = null,
        IReadOnlyDictionary<string, TagHandler>? tagHandlers = null,
        bool requireAllVariables = false)
    {
        _args = args;
        _culture = culture;
        _locale = locale;
        _fallbackLocale = fallbackLocale;
        _cldrDataProvider = cldrDataProvider;
        _pluralizer = pluralizer;
        _ordinalizer = ordinalizer;
        _customFormatters = customFormatters;
        _tagHandlers = tagHandlers;
        _requireAllVariables = requireAllVariables;
    }

    public CultureInfo Culture
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _culture;
    }

    public string Locale
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _locale;
    }

    public string? FallbackLocale
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _fallbackLocale;
    }

    public ICldrDataProvider CldrDataProvider
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _cldrDataProvider;
    }

    public bool RequireAllVariables
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _requireAllVariables;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? GetValue(string variable)
    {
        if (_args.TryGetValue(variable, out var v))
        {
            return v;
        }

        if (_requireAllVariables)
        {
            throw new MessageFormatterException($"Missing required variable '{variable}'.");
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(string variable, out object? value)
    {
        return _args.TryGetValue(variable, out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetPluralForm(double n)
    {
        var ctx = new PluralContext(n);
        return _pluralizer(ctx);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetPluralForm(PluralContext ctx)
    {
        return _pluralizer(ctx);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetOrdinalForm(double n)
    {
        var ctx = new PluralContext(n);
        return _ordinalizer(ctx);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetOrdinalForm(PluralContext ctx)
    {
        return _ordinalizer(ctx);
    }

    /// <summary>
    /// Gets a double value from the arguments, optimized to avoid boxing where possible.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetDoubleValue(string variable)
    {
        if (!_args.TryGetValue(variable, out var value))
        {
            if (_requireAllVariables)
            {
                throw new MessageFormatterException($"Missing required variable '{variable}'.");
            }
            return 0;
        }

        if (value == null)
        {
            return 0;
        }

        return value switch
        {
            int i => i,
            long l => l,
            double d => d,
            float f => f,
            decimal m => (double)m,
            short s => s,
            byte b => b,
            string str => double.Parse(str, CultureInfo.InvariantCulture),
            _ => Convert.ToDouble(value)
        };
    }

    /// <summary>
    /// Tries to get a custom formatter by name.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetCustomFormatter(string name, out CustomFormatterDelegate formatter)
    {
        if (_customFormatters != null && _customFormatters.TryGetValue(name, out var f))
        {
            formatter = f;
            return true;
        }

        formatter = null!;
        return false;
    }

    /// <summary>
    /// Tries to get a tag handler by name.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetTagHandler(string tagName, out TagHandler handler)
    {
        if (_tagHandlers != null && _tagHandlers.TryGetValue(tagName, out var h))
        {
            handler = h;
            return true;
        }

        handler = null!;
        return false;
    }

    /// <summary>
    /// Transforms Latin digits in the input to the locale's default numbering system.
    /// </summary>
    /// <param name="input">The input string containing Latin digits.</param>
    /// <returns>The transformed string, or the original if no transformation is needed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string TransformDigits(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Get locale's default numbering system
        if (!_cldrDataProvider.TryGetLocaleData(_locale, out var localeData) || localeData == null)
            return input;

        var numberingSystem = localeData.DefaultNumberingSystem;
        if (string.IsNullOrEmpty(numberingSystem) || numberingSystem == "latn")
            return input;

        // Get digits for the numbering system
        if (!_cldrDataProvider.TryGetNumberSystemDigits(numberingSystem, out var digits))
            return input;

        return NumberSystemTransformer.Transform(input, digits);
    }
}