using System;
using System.Runtime.CompilerServices;
using System.Text;
using Rh.MessageFormat.Formatting.Duration;

namespace Rh.MessageFormat.Ast.Elements;

/// <summary>
/// Duration display style.
/// </summary>
internal enum DurationStyle
{
    /// <summary>
    /// Long format: "1 hour 30 minutes"
    /// </summary>
    Long,

    /// <summary>
    /// Short format: "1 hr 30 min"
    /// </summary>
    Short,

    /// <summary>
    /// Narrow format: "1h 30m"
    /// </summary>
    Narrow,

    /// <summary>
    /// Timer format: "1:30:00"
    /// </summary>
    Timer
}

/// <summary>
/// Represents a duration format element: {d, duration} or {d, duration, style}
/// Formats TimeSpan, double (seconds), or ISO 8601 duration strings to localized human-readable strings.
/// </summary>
internal sealed class DurationElement : MessageElement
{
    private readonly string _variable;
    private readonly DurationStyle _style;
    private readonly string? _customFormat;

    public DurationElement(string variable, DurationStyle style, SourceSpan location)
        : base(location)
    {
        _variable = variable;
        _style = style;
        _customFormat = null;
    }

    public DurationElement(string variable, string customFormat, SourceSpan location)
        : base(location)
    {
        _variable = variable;
        _style = DurationStyle.Long;
        _customFormat = customFormat;
    }

    public string Variable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _variable;
    }

    public DurationStyle Style
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _style;
    }

    public string? CustomFormat
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _customFormat;
    }

    public override ElementType Type => ElementType.Duration;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Format(ref FormatterContext ctx, StringBuilder output)
    {
        var value = ctx.GetValue(_variable);
        var seconds = ConvertToSeconds(value);

        var styleStr = _style switch
        {
            DurationStyle.Short => DurationUnitFormat.Styles.SHORT,
            DurationStyle.Narrow => DurationUnitFormat.Styles.NARROW,
            DurationStyle.Timer => DurationUnitFormat.Styles.TIMER,
            _ => DurationUnitFormat.Styles.LONG
        };

        var options = new DurationUnitFormatOptions
        {
            Style = styleStr,
            Format = GetFormatTemplate(styleStr)
        };

        if (_customFormat != null)
        {
            options.Format = _customFormat;
        }

        var formatter = new DurationUnitFormat(ctx.Locale, options);
        var formatted = formatter.Format(seconds);
        output.Append(formatted);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetFormatTemplate(string style)
    {
        return style switch
        {
            DurationUnitFormat.Styles.TIMER => "{hours}:{minutes}:{seconds}",
            _ => "{years} {months} {days} {hours} {minutes} {seconds}"
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double ConvertToSeconds(object? value)
    {
        return value switch
        {
            null => 0,
            TimeSpan ts => ts.TotalSeconds,
            double d => d,
            float f => f,
            decimal m => (double)m,
            int i => i,
            long l => l,
            short s => s,
            byte b => b,
            sbyte sb => sb,
            uint ui => ui,
            ulong ul => ul,
            ushort us => us,
            string str => ParseDurationString(str),
            _ => double.TryParse(value.ToString(), out var result) ? result : 0
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double ParseDurationString(string str)
    {
        // Try parsing as ISO 8601 duration (e.g., "PT1H30M")
        if (ISODurationFormatter.TryParseToSeconds(str, out var seconds))
        {
            return seconds;
        }

        // Try parsing as plain number
        if (double.TryParse(str, out var result))
        {
            return result;
        }

        return 0;
    }
}
