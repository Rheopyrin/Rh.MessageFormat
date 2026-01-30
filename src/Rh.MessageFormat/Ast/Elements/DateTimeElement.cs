using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Rh.MessageFormat.Formatting.Formatters;
using Rh.MessageFormat.Formatting.Skeletons;

namespace Rh.MessageFormat.Ast.Elements;

/// <summary>
/// DateTime style for predefined formats.
/// </summary>
internal enum DateTimeStyle
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
/// Represents a datetime format element: {dt, datetime} or {dt, datetime, style}
/// Combines both date and time formatting in a single element.
/// Supports predefined styles (short, medium, long, full), custom format patterns, and ICU skeletons.
/// </summary>
internal sealed class DateTimeElement : MessageElement
{
    private readonly string _variable;
    private readonly DateTimeStyle _style;
    private readonly string? _customFormat;
    private readonly string? _skeleton;

    public DateTimeElement(string variable, DateTimeStyle style, SourceSpan location) : base(location)
    {
        _variable = variable;
        _style = style;
        _customFormat = null;
        _skeleton = null;
    }

    public DateTimeElement(string variable, string customFormat, SourceSpan location) : base(location)
    {
        _variable = variable;
        _style = DateTimeStyle.Custom;
        _customFormat = customFormat;
        _skeleton = null;
    }

    public DateTimeElement(string variable, string skeleton, bool isSkeleton, SourceSpan location) : base(location)
    {
        _variable = variable;
        _style = DateTimeStyle.Skeleton;
        _customFormat = null;
        _skeleton = skeleton;
    }

    public string Variable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _variable;
    }

    public DateTimeStyle Style
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

    public override ElementType Type => ElementType.DateTime;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Format(ref FormatterContext ctx, StringBuilder output)
    {
        var value = ctx.GetValue(_variable);
        if (value == null) return;

        var dateTime = ConvertToDateTime(value);
        var formatted = FormatDateTime(dateTime, ref ctx);
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
            TimeOnly t => DateTime.Today.Add(t.ToTimeSpan()),
            string s => ParseString(s),
            // ICU standard: numeric values are interpreted as milliseconds since Unix epoch
            long ms => DateTimeOffset.FromUnixTimeMilliseconds(ms).DateTime,
            int ms => DateTimeOffset.FromUnixTimeMilliseconds(ms).DateTime,
            double ms => DateTimeOffset.FromUnixTimeMilliseconds((long)ms).DateTime,
            _ => Convert.ToDateTime(value)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DateTime ParseString(string s)
    {
        // Check if string is a numeric value (milliseconds since Unix epoch)
        if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ms))
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(ms).DateTime;
        }

        // Otherwise parse as date/time string
        return DateTime.Parse(s, CultureInfo.InvariantCulture);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatDateTime(DateTime dateTime, ref FormatterContext ctx)
    {
        // Skeleton formatting
        if (_style == DateTimeStyle.Skeleton && _skeleton != null)
        {
            var format = DateTimeSkeletonParser.ToFormatString(_skeleton, ctx.Culture);
            var formatted = dateTime.ToString(format, ctx.Culture);
            return SkeletonPostProcessor.Process(formatted, dateTime, ref ctx);
        }

        // Custom format string
        if (_style == DateTimeStyle.Custom && _customFormat != null)
        {
            return dateTime.ToString(_customFormat, ctx.Culture);
        }

        var styleStr = _style switch
        {
            DateTimeStyle.Short => "short",
            DateTimeStyle.Medium => "medium",
            DateTimeStyle.Long => "long",
            DateTimeStyle.Full => "full",
            _ => "short"  // Default uses short format like .NET's "g" format specifier
        };

        var datePattern = DatePatternMetadata.GetDatePattern(ref ctx, styleStr);
        var timePattern = DatePatternMetadata.GetTimePattern(ref ctx, styleStr);
        var dateTimePattern = DatePatternMetadata.GetDateTimePattern(ref ctx, styleStr);

        var formattedDate = dateTime.ToString(datePattern, ctx.Culture);
        var formattedTime = dateTime.ToString(timePattern, ctx.Culture);

        return string.Format(dateTimePattern, formattedTime, formattedDate);
    }
}
