using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Rh.MessageFormat.Formatting.Skeletons;

namespace Rh.MessageFormat.Ast.Elements;

/// <summary>
/// Number range display style.
/// </summary>
internal enum NumberRangeStyle
{
    /// <summary>
    /// Default style with locale-appropriate separator.
    /// </summary>
    Default,

    /// <summary>
    /// Short format.
    /// </summary>
    Short,

    /// <summary>
    /// Uses skeleton notation for formatting.
    /// </summary>
    Skeleton
}

/// <summary>
/// Represents a number range format element: {range, numberRange} or {range, numberRange, style}
/// Formats a number range with locale-appropriate separators.
/// </summary>
internal sealed class NumberRangeElement : MessageElement
{
    private readonly string _startVariable;
    private readonly string _endVariable;
    private readonly NumberRangeStyle _style;
    private readonly NumberFormatOptions? _skeletonOptions;

    /// <summary>
    /// Creates a number range element with two separate variables.
    /// </summary>
    public NumberRangeElement(string startVariable, string endVariable, NumberRangeStyle style, SourceSpan location)
        : base(location)
    {
        _startVariable = startVariable;
        _endVariable = endVariable;
        _style = style;
        _skeletonOptions = null;
    }

    /// <summary>
    /// Creates a number range element with skeleton formatting.
    /// </summary>
    public NumberRangeElement(string startVariable, string endVariable, NumberFormatOptions skeletonOptions, SourceSpan location)
        : base(location)
    {
        _startVariable = startVariable;
        _endVariable = endVariable;
        _style = NumberRangeStyle.Skeleton;
        _skeletonOptions = skeletonOptions;
    }

    public string StartVariable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _startVariable;
    }

    public string EndVariable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _endVariable;
    }

    public NumberRangeStyle Style
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _style;
    }

    public override ElementType Type => ElementType.NumberRange;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Format(ref FormatterContext ctx, StringBuilder output)
    {
        var startValue = ctx.GetDoubleValue(_startVariable);
        var endValue = ctx.GetDoubleValue(_endVariable);

        string startFormatted;
        string endFormatted;

        if (_skeletonOptions != null)
        {
            startFormatted = NumberSkeletonFormatter.Format(startValue, _skeletonOptions, ref ctx);
            endFormatted = NumberSkeletonFormatter.Format(endValue, _skeletonOptions, ref ctx);
        }
        else
        {
            var nfi = ctx.Culture.NumberFormat;
            startFormatted = startValue.ToString("N0", nfi);
            endFormatted = endValue.ToString("N0", nfi);
        }

        // Get the range separator based on locale
        var separator = GetRangeSeparator(ctx.Locale);

        output.Append(startFormatted);
        output.Append(separator);
        output.Append(endFormatted);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetRangeSeparator(string locale)
    {
        // Get language code
        var langCode = locale.Contains('-') ? locale.Substring(0, locale.IndexOf('-')) : locale;
        langCode = langCode.Contains('_') ? langCode.Substring(0, langCode.IndexOf('_')) : langCode;

        // Most locales use en-dash with spaces, but some have different patterns
        // This follows CLDR conventions where available
        return langCode switch
        {
            // Languages that use en-dash without spaces
            "ja" or "zh" or "ko" => "\u2013",  // CJK languages
            // Languages with specific patterns
            "ar" => " \u2013 ",  // Arabic: space-endash-space
            // Default: en-dash with non-breaking spaces (most European languages)
            _ => "\u2009\u2013\u2009"  // thin space, en-dash, thin space
        };
    }
}
