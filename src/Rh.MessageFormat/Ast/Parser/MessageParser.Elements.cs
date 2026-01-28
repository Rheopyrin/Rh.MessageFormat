using System;
using System.Runtime.CompilerServices;
using Rh.MessageFormat.Ast.Elements;
using Rh.MessageFormat.Exceptions;
using Rh.MessageFormat.Formatting.Skeletons;
using static Rh.MessageFormat.Constants;
using static Rh.MessageFormat.Constants.Parser;

namespace Rh.MessageFormat.Ast.Parser;

/// <summary>
/// Element creation methods.
/// </summary>
internal sealed partial class MessageParser
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MessageElement CreateElement(string variable, string? formatter, string? arguments, SourceSpan span)
    {
        if (formatter == null)
        {
            return new ArgumentElement(variable, span);
        }

        return formatter switch
        {
            Formatters.Number => CreateNumberElement(variable, arguments, span),
            Formatters.Date => CreateDateElement(variable, arguments, span),
            Formatters.Time => CreateTimeElement(variable, arguments, span),
            Formatters.DateTime => CreateDateTimeElement(variable, arguments, span),
            Formatters.DateRange => CreateDateRangeElement(variable, arguments, span),
            Formatters.Plural => CreatePluralElement(variable, arguments, span),
            Formatters.Select => CreateSelectElement(variable, arguments, span),
            Formatters.SelectOrdinal => CreateSelectOrdinalElement(variable, arguments, span),
            Formatters.List => CreateListElement(variable, arguments, span),
            Formatters.RelativeTime => CreateRelativeTimeElement(variable, arguments, span),
            Formatters.Duration => CreateDurationElement(variable, arguments, span),
            Formatters.NumberRange => CreateNumberRangeElement(variable, arguments, span),
            _ => new CustomFormatterElement(variable, formatter, arguments?.Trim(), span) // Custom formatter
        };
    }

    private NumberElement CreateNumberElement(string variable, string? arguments, SourceSpan span)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return new NumberElement(variable, NumberStyle.Default, span);
        }

        var trimmed = arguments.Trim();

        // Check for skeleton syntax (prefixed with ::)
        if (trimmed.StartsWith(Sequences.SkeletonPrefix))
        {
            var skeleton = trimmed.Substring(2);
            var options = NumberSkeletonParser.Parse(skeleton);
            return new NumberElement(variable, options, span);
        }

        // Check for predefined styles using case-insensitive comparison
        if (string.Equals(trimmed, Styles.Integer, StringComparison.OrdinalIgnoreCase))
        {
            return new NumberElement(variable, NumberStyle.Integer, span);
        }
        if (string.Equals(trimmed, Styles.Currency, StringComparison.OrdinalIgnoreCase))
        {
            return new NumberElement(variable, NumberStyle.Currency, span);
        }
        if (string.Equals(trimmed, Styles.Percent, StringComparison.OrdinalIgnoreCase))
        {
            return new NumberElement(variable, NumberStyle.Percent, span);
        }

        // If not a predefined style, treat as custom format string
        if (trimmed.Length > 0)
        {
            return new NumberElement(variable, trimmed, span);
        }

        return new NumberElement(variable, NumberStyle.Default, span);
    }

    private DateElement CreateDateElement(string variable, string? arguments, SourceSpan span)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return new DateElement(variable, DateStyle.Default, span);
        }

        var trimmed = arguments.Trim();

        // Check for skeleton syntax (prefixed with ::)
        if (trimmed.StartsWith(Sequences.SkeletonPrefix))
        {
            var skeleton = trimmed.Substring(2);
            return new DateElement(variable, skeleton, isSkeleton: true, span);
        }

        // Check for predefined styles using case-insensitive comparison
        if (string.Equals(trimmed, Styles.Short, StringComparison.OrdinalIgnoreCase))
        {
            return new DateElement(variable, DateStyle.Short, span);
        }
        if (string.Equals(trimmed, Styles.Medium, StringComparison.OrdinalIgnoreCase))
        {
            return new DateElement(variable, DateStyle.Medium, span);
        }
        if (string.Equals(trimmed, Styles.Long, StringComparison.OrdinalIgnoreCase))
        {
            return new DateElement(variable, DateStyle.Long, span);
        }
        if (string.Equals(trimmed, Styles.Full, StringComparison.OrdinalIgnoreCase))
        {
            return new DateElement(variable, DateStyle.Full, span);
        }

        // Custom format - use the original format string
        return new DateElement(variable, trimmed, span);
    }

    private TimeElement CreateTimeElement(string variable, string? arguments, SourceSpan span)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return new TimeElement(variable, TimeStyle.Default, span);
        }

        var trimmed = arguments.Trim();

        // Check for skeleton syntax (prefixed with ::)
        if (trimmed.StartsWith(Sequences.SkeletonPrefix))
        {
            var skeleton = trimmed.Substring(2);
            return new TimeElement(variable, skeleton, isSkeleton: true, span);
        }

        // Check for predefined styles using case-insensitive comparison
        if (string.Equals(trimmed, Styles.Short, StringComparison.OrdinalIgnoreCase))
        {
            return new TimeElement(variable, TimeStyle.Short, span);
        }
        if (string.Equals(trimmed, Styles.Medium, StringComparison.OrdinalIgnoreCase))
        {
            return new TimeElement(variable, TimeStyle.Medium, span);
        }
        if (string.Equals(trimmed, Styles.Long, StringComparison.OrdinalIgnoreCase))
        {
            return new TimeElement(variable, TimeStyle.Long, span);
        }
        if (string.Equals(trimmed, Styles.Full, StringComparison.OrdinalIgnoreCase))
        {
            return new TimeElement(variable, TimeStyle.Full, span);
        }

        // Custom format - use the original format string
        return new TimeElement(variable, trimmed, span);
    }

    private DateTimeElement CreateDateTimeElement(string variable, string? arguments, SourceSpan span)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return new DateTimeElement(variable, DateTimeStyle.Default, span);
        }

        var trimmed = arguments.Trim();

        // Check for skeleton syntax (prefixed with ::)
        if (trimmed.StartsWith(Sequences.SkeletonPrefix))
        {
            var skeleton = trimmed.Substring(2);
            return new DateTimeElement(variable, skeleton, isSkeleton: true, span);
        }

        // Check for predefined styles using case-insensitive comparison
        if (string.Equals(trimmed, Styles.Short, StringComparison.OrdinalIgnoreCase))
        {
            return new DateTimeElement(variable, DateTimeStyle.Short, span);
        }
        if (string.Equals(trimmed, Styles.Medium, StringComparison.OrdinalIgnoreCase))
        {
            return new DateTimeElement(variable, DateTimeStyle.Medium, span);
        }
        if (string.Equals(trimmed, Styles.Long, StringComparison.OrdinalIgnoreCase))
        {
            return new DateTimeElement(variable, DateTimeStyle.Long, span);
        }
        if (string.Equals(trimmed, Styles.Full, StringComparison.OrdinalIgnoreCase))
        {
            return new DateTimeElement(variable, DateTimeStyle.Full, span);
        }

        // Custom format - use the original format string
        return new DateTimeElement(variable, trimmed, span);
    }

    private DateRangeElement CreateDateRangeElement(string variable, string? arguments, SourceSpan span)
    {
        // Syntax: {start, daterange, end} or {start, daterange, end, style} or {start, daterange, end, ::skeleton}
        if (string.IsNullOrWhiteSpace(arguments))
        {
            throw new MessageFormatterException($"DateRange element requires an end variable at line {span.Line}, column {span.Column}");
        }

        var trimmed = arguments.Trim();

        // Find the first comma that separates end variable from style/skeleton
        var commaIndex = trimmed.IndexOf(',');
        string endVariable;
        string? style = null;

        if (commaIndex > 0)
        {
            // Has both end variable and style/skeleton
            endVariable = trimmed.Substring(0, commaIndex).Trim();
            style = trimmed.Substring(commaIndex + 1).Trim();
        }
        else
        {
            // Just the end variable
            endVariable = trimmed;
        }

        if (string.IsNullOrEmpty(endVariable))
        {
            throw new MessageFormatterException($"DateRange element requires an end variable at line {span.Line}, column {span.Column}");
        }

        return new DateRangeElement(variable, endVariable, style, span);
    }

    private ListElement CreateListElement(string variable, string? arguments, SourceSpan span)
    {
        var style = ListStyle.Conjunction;
        var width = ListWidth.Long;

        if (!string.IsNullOrWhiteSpace(arguments))
        {
            var trimmed = arguments.Trim();

            // Parse style: conjunction, disjunction, unit
            // Parse width: long, short, narrow
            // Can be combined: "conjunction long", "disjunction short", etc.
            // Use IndexOf with OrdinalIgnoreCase to avoid ToLowerInvariant() allocation

            if (trimmed.IndexOf(List.StyleTypes.Disjunction, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                style = ListStyle.Disjunction;
            }
            else if (trimmed.IndexOf(List.StyleTypes.Unit, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                style = ListStyle.Unit;
            }

            if (trimmed.IndexOf(Styles.Narrow, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                width = ListWidth.Narrow;
            }
            else if (trimmed.IndexOf(Styles.Short, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                width = ListWidth.Short;
            }
        }

        return new ListElement(variable, style, width, span);
    }

    private RelativeTimeElement CreateRelativeTimeElement(string variable, string? arguments, SourceSpan span)
    {
        // Default values
        var field = RelativeTime.Fields.Day;
        var style = RelativeTimeStyle.Long;
        var numeric = RelativeTimeNumeric.Always;

        if (!string.IsNullOrWhiteSpace(arguments))
        {
            var parts = arguments.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // First part is the field (required for meaningful usage)
            if (parts.Length > 0)
            {
                // Field names are stored lowercase in CLDR data
                field = parts[0].ToLowerInvariant();
            }

            // Second part is the style (optional)
            if (parts.Length > 1)
            {
                var stylePart = parts[1];
                if (string.Equals(stylePart, Styles.Short, StringComparison.OrdinalIgnoreCase))
                {
                    style = RelativeTimeStyle.Short;
                }
                else if (string.Equals(stylePart, Styles.Narrow, StringComparison.OrdinalIgnoreCase))
                {
                    style = RelativeTimeStyle.Narrow;
                }
            }

            // Third part is the numeric mode (optional)
            if (parts.Length > 2)
            {
                if (string.Equals(parts[2], RelativeTime.NumericMode.Auto, StringComparison.OrdinalIgnoreCase))
                {
                    numeric = RelativeTimeNumeric.Auto;
                }
            }
        }

        return new RelativeTimeElement(variable, field, style, numeric, span);
    }

    private DurationElement CreateDurationElement(string variable, string? arguments, SourceSpan span)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return new DurationElement(variable, DurationStyle.Long, span);
        }

        var trimmed = arguments.Trim();

        // Check for predefined styles using case-insensitive comparison
        if (string.Equals(trimmed, Styles.Long, StringComparison.OrdinalIgnoreCase))
        {
            return new DurationElement(variable, DurationStyle.Long, span);
        }
        if (string.Equals(trimmed, Styles.Short, StringComparison.OrdinalIgnoreCase))
        {
            return new DurationElement(variable, DurationStyle.Short, span);
        }
        if (string.Equals(trimmed, Styles.Narrow, StringComparison.OrdinalIgnoreCase))
        {
            return new DurationElement(variable, DurationStyle.Narrow, span);
        }
        if (string.Equals(trimmed, "timer", StringComparison.OrdinalIgnoreCase))
        {
            return new DurationElement(variable, DurationStyle.Timer, span);
        }

        // Custom format template (e.g., "{hours}:{minutes}")
        return new DurationElement(variable, trimmed, span);
    }

    private NumberRangeElement CreateNumberRangeElement(string variable, string? arguments, SourceSpan span)
    {
        // Syntax: {start, numberRange, end} or {start, numberRange, end, ::skeleton}
        if (string.IsNullOrWhiteSpace(arguments))
        {
            throw new MessageFormatterException($"NumberRange element requires an end variable at line {span.Line}, column {span.Column}");
        }

        var trimmed = arguments.Trim();

        // Find the first comma that separates end variable from skeleton
        var commaIndex = trimmed.IndexOf(',');
        string endVariable;
        string? format = null;

        if (commaIndex > 0)
        {
            // Has both end variable and format/skeleton
            endVariable = trimmed.Substring(0, commaIndex).Trim();
            format = trimmed.Substring(commaIndex + 1).Trim();
        }
        else
        {
            // Just the end variable
            endVariable = trimmed;
        }

        if (string.IsNullOrEmpty(endVariable))
        {
            throw new MessageFormatterException($"NumberRange element requires an end variable at line {span.Line}, column {span.Column}");
        }

        // Check for skeleton syntax
        if (!string.IsNullOrEmpty(format) && format.StartsWith(Sequences.SkeletonPrefix))
        {
            var skeleton = format.Substring(2);
            var options = NumberSkeletonParser.Parse(skeleton);
            return new NumberRangeElement(variable, endVariable, options, span);
        }

        return new NumberRangeElement(variable, endVariable, NumberRangeStyle.Default, span);
    }
}