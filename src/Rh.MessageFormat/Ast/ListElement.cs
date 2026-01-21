using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Rh.MessageFormat.Formatting.Formatters;
using static Rh.MessageFormat.Constants;
using static Rh.MessageFormat.Constants.List;

namespace Rh.MessageFormat.Ast;

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
        var items = GetItems(value);

        if (items.Count == 0)
        {
            return;
        }

        var formatted = FormatList(items, ref ctx);
        output.Append(formatted);
    }

    private List<string> GetItems(object? value)
    {
        var items = new List<string>();

        if (value == null)
        {
            return items;
        }

        if (value is IEnumerable<string> stringEnumerable)
        {
            foreach (var item in stringEnumerable)
            {
                items.Add(item);
            }
        }
        else if (value is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                items.Add(item?.ToString() ?? Common.Empty);
            }
        }
        else
        {
            // Single item
            items.Add(value.ToString() ?? Common.Empty);
        }

        return items;
    }

    private string FormatList(List<string> items, ref FormatterContext ctx)
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
            return $"{items[0]}{pairSeparator}{items[1]}";
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetBaseLanguage(string locale)
    {
        var dashIndex = locale.IndexOf(Common.DashChar);
        if (dashIndex < 0) dashIndex = locale.IndexOf(Common.UnderscoreChar);

        return dashIndex > 0
            ? locale.Substring(0, dashIndex).ToLowerInvariant()
            : locale.ToLowerInvariant();
    }
}
