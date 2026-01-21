using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Rh.MessageFormat.Formatting.Formatters;
using Rh.MessageFormat.Formatting.Skeletons;

namespace Rh.MessageFormat.Ast;

/// <summary>
/// Time style for predefined formats.
/// </summary>
internal enum TimeStyle
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
/// Represents a time format element: {t, time} or {t, time, style}
/// Supports predefined styles (short, medium, long, full), custom format patterns, and ICU skeletons.
/// </summary>
internal sealed class TimeElement : MessageElement
{
    private readonly string _variable;
    private readonly TimeStyle _style;
    private readonly string? _customFormat;
    private readonly string? _skeleton;

    public TimeElement(string variable, TimeStyle style, SourceSpan location) : base(location)
    {
        _variable = variable;
        _style = style;
        _customFormat = null;
        _skeleton = null;
    }

    public TimeElement(string variable, string customFormat, SourceSpan location) : base(location)
    {
        _variable = variable;
        _style = TimeStyle.Custom;
        _customFormat = customFormat;
        _skeleton = null;
    }

    public TimeElement(string variable, string skeleton, bool isSkeleton, SourceSpan location) : base(location)
    {
        _variable = variable;
        _style = TimeStyle.Skeleton;
        _customFormat = null;
        _skeleton = skeleton;
    }

    public string Variable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _variable;
    }

    public TimeStyle Style
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

    public override ElementType Type => ElementType.Time;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Format(ref FormatterContext ctx, StringBuilder output)
    {
        var value = ctx.GetValue(_variable);
        if (value == null) return;

        var time = ConvertToDateTime(value);
        var formatted = FormatTime(time, ref ctx);
        output.Append(formatted);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DateTime ConvertToDateTime(object value)
    {
        return value switch
        {
            DateTime dt => dt,
            DateTimeOffset dto => dto.DateTime,
            TimeOnly t => DateTime.Today.Add(t.ToTimeSpan()),
            string s => DateTime.Parse(s, CultureInfo.InvariantCulture),
            _ => Convert.ToDateTime(value)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatTime(DateTime time, ref FormatterContext ctx)
    {
        // Skeleton formatting
        if (_style == TimeStyle.Skeleton && _skeleton != null)
        {
            var format = DateTimeSkeletonParser.ToFormatString(_skeleton, ctx.Culture);
            var formatted = time.ToString(format, ctx.Culture);
            return SkeletonPostProcessor.Process(formatted, time, ref ctx);
        }

        // Custom format string
        if (_style == TimeStyle.Custom && _customFormat != null)
        {
            return time.ToString(_customFormat, ctx.Culture);
        }

        return _style switch
        {
            TimeStyle.Short => time.ToString(DatePatternMetadata.GetTimePattern(ref ctx, "short"), ctx.Culture),
            TimeStyle.Medium => time.ToString(DatePatternMetadata.GetTimePattern(ref ctx, "medium"), ctx.Culture),
            TimeStyle.Long => time.ToString(DatePatternMetadata.GetTimePattern(ref ctx, "long"), ctx.Culture),
            TimeStyle.Full => time.ToString(DatePatternMetadata.GetTimePattern(ref ctx, "full"), ctx.Culture),
            _ => time.ToString(DatePatternMetadata.GetTimePattern(ref ctx, "medium"), ctx.Culture)
        };
    }
}
