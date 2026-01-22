using System.Runtime.CompilerServices;
using System.Text;
using Rh.MessageFormat.Formatting.Formatters;
using static Rh.MessageFormat.Constants;
using static Rh.MessageFormat.Constants.RelativeTime;

namespace Rh.MessageFormat.Ast.Elements;

/// <summary>
/// Relative time display style.
/// </summary>
internal enum RelativeTimeStyle
{
    /// <summary>
    /// Long format: "in 3 days"
    /// </summary>
    Long,

    /// <summary>
    /// Short format: "in 3 days"
    /// </summary>
    Short,

    /// <summary>
    /// Narrow format: "in 3d"
    /// </summary>
    Narrow
}

/// <summary>
/// Numeric mode for relative time formatting.
/// </summary>
internal enum RelativeTimeNumeric
{
    /// <summary>
    /// Always use numeric format: "in 1 day"
    /// </summary>
    Always,

    /// <summary>
    /// Use relative strings when available: "tomorrow"
    /// </summary>
    Auto
}

/// <summary>
/// Represents a relative time format element: {days, relativeTime, day} or {days, relativeTime, day short auto}
/// Formats numeric values as localized relative time expressions.
/// </summary>
internal sealed class RelativeTimeElement : MessageElement
{
    private readonly string _variable;
    private readonly string _field;
    private readonly RelativeTimeStyle _style;
    private readonly RelativeTimeNumeric _numeric;

    public RelativeTimeElement(string variable, string field, RelativeTimeStyle style, RelativeTimeNumeric numeric, SourceSpan location)
        : base(location)
    {
        _variable = variable;
        _field = field;
        _style = style;
        _numeric = numeric;
    }

    public string Variable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _variable;
    }

    public string Field
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _field;
    }

    public RelativeTimeStyle Style
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _style;
    }

    public RelativeTimeNumeric Numeric
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _numeric;
    }

    public override ElementType Type => ElementType.RelativeTime;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Format(ref FormatterContext ctx, StringBuilder output)
    {
        var value = ctx.GetValue(_variable);
        var numericValue = ConvertToDouble(value);

        // Map enums to string values
        var styleStr = _style switch
        {
            RelativeTimeStyle.Short => Styles.Short,
            RelativeTimeStyle.Narrow => Styles.Narrow,
            _ => Styles.Long
        };

        var numericStr = _numeric switch
        {
            RelativeTimeNumeric.Auto => NumericMode.Auto,
            _ => NumericMode.Always
        };

        var formatted = RelativeTimeMetadata.Format(ref ctx, numericValue, _field, styleStr, numericStr);
        output.Append(formatted);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double ConvertToDouble(object? value)
    {
        return value switch
        {
            null => 0,
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
            _ => double.TryParse(value.ToString(), out var result) ? result : 0
        };
    }
}
