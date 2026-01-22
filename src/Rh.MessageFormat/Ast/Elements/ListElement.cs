using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Rh.MessageFormat.Formatting.Formatters;
using Rh.MessageFormat.Pools;
using static Rh.MessageFormat.Constants;
using static Rh.MessageFormat.Constants.List;

namespace Rh.MessageFormat.Ast.Elements;

/// <summary>
/// List formatting style.
/// </summary>
internal enum ListStyle
{
    /// <summary>
    /// Conjunction: "A, B, and C"
    /// </summary>
    Conjunction,

    /// <summary>
    /// Disjunction: "A, B, or C"
    /// </summary>
    Disjunction,

    /// <summary>
    /// Unit: "A, B, C" (no conjunction word)
    /// </summary>
    Unit
}

/// <summary>
/// List display width.
/// </summary>
internal enum ListWidth
{
    /// <summary>
    /// Long format: "A, B, and C"
    /// </summary>
    Long,

    /// <summary>
    /// Short format: "A, B, &amp; C"
    /// </summary>
    Short,

    /// <summary>
    /// Narrow format: "A, B, C"
    /// </summary>
    Narrow
}

/// <summary>
/// Represents a list format element: {items, list} or {items, list, style}
/// Formats arrays/collections as localized lists.
/// </summary>
internal sealed class ListElement : MessageElement
{
    private readonly string _variable;
    private readonly ListStyle _style;
    private readonly ListWidth _width;

    public ListElement(string variable, ListStyle style, ListWidth width, SourceSpan location)
        : base(location)
    {
        _variable = variable;
        _style = style;
        _width = width;
    }

    public string Variable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _variable;
    }

    public ListStyle Style
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _style;
    }

    public ListWidth Width
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _width;
    }

    public override ElementType Type => ElementType.List;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Format(ref FormatterContext ctx, StringBuilder output)
    {
        var value = ctx.GetValue(_variable);
        var (items, needsDisposal) = GetItems(value);

        if (items.Count == 0)
        {
            return;
        }

        try
        {
            var formatted = FormatList(items, ref ctx);
            output.Append(formatted);
        }
        finally
        {
            // Only return to pool if we allocated a new list
            if (needsDisposal && items is List<string> list)
            {
                list.Clear();
            }
        }
    }

    private static (IReadOnlyList<string> items, bool needsDisposal) GetItems(object? value)
    {
        if (value == null)
        {
            return (Array.Empty<string>(), false);
        }

        // Fast path: already a usable type - no allocation needed
        if (value is IReadOnlyList<string> readOnlyList)
        {
            return (readOnlyList, false);
        }

        if (value is string[] array)
        {
            return (array, false);
        }

        // Need to materialize the enumerable
        if (value is IEnumerable<string> stringEnumerable)
        {
            var list = new List<string>(stringEnumerable);
            return (list, true);
        }

        if (value is IEnumerable enumerable)
        {
            var list = new List<string>();
            foreach (var item in enumerable)
            {
                list.Add(item?.ToString() ?? Common.Empty);
            }
            return (list, true);
        }

        // Single item - use single-element array to avoid list allocation
        return (new[] { value.ToString() ?? Common.Empty }, true);
    }

    private string FormatList(IReadOnlyList<string> items, ref FormatterContext ctx)
    {
        if (items.Count == 0)
        {
            return Common.Empty;
        }

        if (items.Count == 1)
        {
            return items[0];
        }

        // Get locale-specific connectors
        var (separator, lastSeparator, pairSeparator) = GetConnectors(ref ctx);

        if (items.Count == 2)
        {
            return string.Concat(items[0], pairSeparator, items[1]);
        }

        // 3+ items
        var sb = StringBuilderPool.Get();
        try
        {
            for (int i = 0; i < items.Count; i++)
            {
                sb.Append(items[i]);

                if (i < items.Count - 2)
                {
                    sb.Append(separator);
                }
                else if (i == items.Count - 2)
                {
                    sb.Append(lastSeparator);
                }
            }

            return sb.ToString();
        }
        finally
        {
            StringBuilderPool.Return(sb);
        }
    }

    private (string separator, string lastSeparator, string pairSeparator) GetConnectors(ref FormatterContext ctx)
    {
        // Map style enum to CLDR style name
        var style = _style switch
        {
            ListStyle.Conjunction => StyleTypes.Conjunction,
            ListStyle.Disjunction => StyleTypes.Disjunction,
            ListStyle.Unit => StyleTypes.Unit,
            _ => StyleTypes.Conjunction
        };

        // Map width enum to CLDR width name
        var width = _width switch
        {
            ListWidth.Long => WidthTypes.Long,
            ListWidth.Short => WidthTypes.Short,
            ListWidth.Narrow => WidthTypes.Narrow,
            _ => WidthTypes.Long
        };

        // Try to get connectors from generated CLDR metadata
        return ListPatternMetadata.GetConnectors(ref ctx, style, width);
    }
}
