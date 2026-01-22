using System;
using System.Runtime.CompilerServices;
using System.Text;
using Rh.MessageFormat.Formatting.Formatters;

namespace Rh.MessageFormat.Ast.Elements;

/// <summary>
/// Represents a date range format element: {start, daterange, end} or {start, daterange, end, style}
/// Supports predefined styles (short, medium, long, full) and ICU skeletons.
/// </summary>
internal sealed class DateRangeElement : MessageElement
{
    private readonly string _startVariable;
    private readonly string _endVariable;
    private readonly string? _style;
    private readonly string? _skeleton;

    /// <summary>
    /// Creates a DateRangeElement with a style.
    /// </summary>
    /// <param name="startVariable">The start date variable name.</param>
    /// <param name="endVariable">The end date variable name.</param>
    /// <param name="style">The style (short, medium, long, full) or null for default.</param>
    /// <param name="location">The source location.</param>
    public DateRangeElement(string startVariable, string endVariable, string? style, SourceSpan location)
        : base(location)
    {
        _startVariable = startVariable;
        _endVariable = endVariable;

        // Check if it's a skeleton (starts with ::)
        if (!string.IsNullOrEmpty(style) && style.StartsWith("::", StringComparison.Ordinal))
        {
            _skeleton = style.Substring(2);
            _style = null;
        }
        else
        {
            _style = style;
            _skeleton = null;
        }
    }

    /// <summary>
    /// Gets the start date variable name.
    /// </summary>
    public string StartVariable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _startVariable;
    }

    /// <summary>
    /// Gets the end date variable name.
    /// </summary>
    public string EndVariable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _endVariable;
    }

    /// <summary>
    /// Gets the style or null if using a skeleton.
    /// </summary>
    public string? Style
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _style;
    }

    /// <summary>
    /// Gets the skeleton or null if using a style.
    /// </summary>
    public string? Skeleton
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _skeleton;
    }

    /// <inheritdoc />
    public override ElementType Type => ElementType.DateRange;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Format(ref FormatterContext ctx, StringBuilder output)
    {
        var startValue = ctx.GetValue(_startVariable);
        var endValue = ctx.GetValue(_endVariable);

        if (startValue == null || endValue == null)
        {
            // If either value is null, output empty or fallback
            return;
        }

        var startDate = ConvertToDateTime(startValue);
        var endDate = ConvertToDateTime(endValue);

        // Ensure start is before end
        if (startDate > endDate)
        {
            // Swap them
            (startDate, endDate) = (endDate, startDate);
        }

        var formatted = DateRangeMetadata.Format(ref ctx, startDate, endDate, _style, _skeleton);
        output.Append(formatted);
    }

    /// <summary>
    /// Converts various date types to DateTime.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DateTime ConvertToDateTime(object value)
    {
        return value switch
        {
            DateTime dt => dt,
            DateTimeOffset dto => dto.LocalDateTime,
            DateOnly d => d.ToDateTime(TimeOnly.MinValue),
            long ticks => new DateTime(ticks),
            string s when DateTime.TryParse(s, out var parsed) => parsed,
            _ => throw new FormatException($"Cannot convert {value.GetType().Name} to DateTime for date range formatting.")
        };
    }
}
