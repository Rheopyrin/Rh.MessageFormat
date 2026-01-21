using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Rh.MessageFormat.Formatting.Formatters;
using Rh.MessageFormat.Formatting.Skeletons;

namespace Rh.MessageFormat.Ast;

/// <summary>
/// Date style for predefined formats.
/// </summary>
internal enum DateStyle
{
    Default,
    Short,
    Medium,
    Long,
    Full,
    Custom,
    Skeleton
}

/// <summary>
/// Represents a date format element: {d, date} or {d, date, style}
/// Supports predefined styles (short, medium, long, full), custom format patterns, and ICU skeletons.
/// </summary>
internal sealed class DateElement : MessageElement
{
    private readonly string _variable;
    private readonly DateStyle _style;
    private readonly string? _customFormat;
    private readonly string? _skeleton;

    public DateElement(string variable, DateStyle style, SourceSpan location) : base(location)
    {
        _variable = variable;
        _style = style;
        _customFormat = null;
        _skeleton = null;
    }

    public DateElement(string variable, string customFormat, SourceSpan location) : base(location)
    {
        _variable = variable;
        _style = DateStyle.Custom;
        _customFormat = customFormat;
        _skeleton = null;
    }

    public DateElement(string variable, string skeleton, bool isSkeleton, SourceSpan location) : base(location)
    {
        _variable = variable;
        _style = DateStyle.Skeleton;
        _customFormat = null;
        _skeleton = skeleton;
    }

    public string Variable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _variable;
    }

    public DateStyle Style
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _style;
    }

    public string? CustomFormat
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _customFormat;
    }

    public string? Skeleton
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _skeleton;
    }

    public override ElementType Type => ElementType.Date;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Format(ref FormatterContext ctx, StringBuilder output)
    {
        var value = ctx.GetValue(_variable);
        if (value == null) return;

        var date = ConvertToDateTime(value);
        var formatted = FormatDate(date, ref ctx);
        output.Append(formatted);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DateTime ConvertToDateTime(object value)
    {
        return value switch
        {
            DateTime dt => dt,
            DateTimeOffset dto => dto.DateTime,
            DateOnly d => d.ToDateTime(TimeOnly.MinValue),
            string s => DateTime.Parse(s, CultureInfo.InvariantCulture),
            _ => Convert.ToDateTime(value)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatDate(DateTime date, ref FormatterContext ctx)
    {
        // Skeleton formatting
        if (_style == DateStyle.Skeleton && _skeleton != null)
        {
            var format = DateTimeSkeletonParser.ToFormatString(_skeleton, ctx.Culture);
            return date.ToString(format, ctx.Culture);
        }

        // Custom format string
        if (_style == DateStyle.Custom && _customFormat != null)
        {
            return date.ToString(_customFormat, ctx.Culture);
        }

        return _style switch
        {
            DateStyle.Short => date.ToString(DatePatternMetadata.GetDatePattern(ref ctx, Constants.Styles.Short), ctx.Culture),
            DateStyle.Medium => date.ToString(DatePatternMetadata.GetDatePattern(ref ctx, Constants.Styles.Medium), ctx.Culture),
            DateStyle.Long => date.ToString(DatePatternMetadata.GetDatePattern(ref ctx, Constants.Styles.Long), ctx.Culture),
            DateStyle.Full => date.ToString(DatePatternMetadata.GetDatePattern(ref ctx, Constants.Styles.Full), ctx.Culture),
            _ => date.ToString(DatePatternMetadata.GetDatePattern(ref ctx, Constants.Styles.Short), ctx.Culture)
        };
    }
}
