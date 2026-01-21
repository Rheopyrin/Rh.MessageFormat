using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Rh.MessageFormat.Formatting.Skeletons;
using static Rh.MessageFormat.Constants.Numbers;

namespace Rh.MessageFormat.Ast;

/// <summary>
/// Number style for predefined formats.
/// </summary>
internal enum NumberStyle
{
    Default,
    Integer,
    Currency,
    Percent,
    Skeleton
}

/// <summary>
/// Represents a number format element: {n, number} or {n, number, style}
/// </summary>
internal sealed class NumberElement : MessageElement
{
    private readonly string _variable;
    private readonly NumberStyle _style;
    private readonly string? _customFormat;
    private readonly NumberFormatOptions? _skeletonOptions;

    public NumberElement(string variable, NumberStyle style, SourceSpan location) : base(location)
    {
        _variable = variable;
        _style = style;
        _customFormat = null;
        _skeletonOptions = null;
    }

    public NumberElement(string variable, string customFormat, SourceSpan location) : base(location)
    {
        _variable = variable;
        _style = NumberStyle.Default;
        _customFormat = customFormat;
        _skeletonOptions = null;
    }

    public NumberElement(string variable, NumberFormatOptions skeletonOptions, SourceSpan location) : base(location)
    {
        _variable = variable;
        _style = NumberStyle.Skeleton;
        _customFormat = null;
        _skeletonOptions = skeletonOptions;
    }

    public string Variable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _variable;
    }

    public NumberStyle Style
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _style;
    }

    public string? CustomFormat
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _customFormat;
    }

    public NumberFormatOptions? SkeletonOptions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _skeletonOptions;
    }

    public override ElementType Type => ElementType.Number;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Format(ref FormatterContext ctx, StringBuilder output)
    {
        var n = ctx.GetDoubleValue(_variable);
        var formatted = FormatNumber(n, ref ctx);
        output.Append(formatted);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatNumber(double n, ref FormatterContext ctx)
    {
        // Skeleton formatting takes precedence
        if (_style == NumberStyle.Skeleton && _skeletonOptions != null)
        {
            return NumberSkeletonFormatter.Format(n, _skeletonOptions, ref ctx);
        }

        // Custom format string (e.g., "0.00")
        if (!string.IsNullOrEmpty(_customFormat))
        {
            try
            {
                return n.ToString(_customFormat, ctx.Culture);
            }
            catch
            {
                // Fall through to default
            }
        }

        return _style switch
        {
            NumberStyle.Integer => ((long)n).ToString(Formats.Integer, ctx.Culture),
            NumberStyle.Currency => n.ToString(Formats.Currency, ctx.Culture),
            NumberStyle.Percent => n.ToString(Formats.Percent, ctx.Culture),
            _ => n.ToString(Formats.Default, ctx.Culture)
        };
    }
}
