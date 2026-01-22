using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Rh.MessageFormat.Ast;
using Rh.MessageFormat.Ast.Elements;
using Rh.MessageFormat.Exceptions;
using Rh.MessageFormat.Formatting.Formatters;
using Rh.MessageFormat.Pools;
using static Rh.MessageFormat.Constants;
using static Rh.MessageFormat.Constants.Numbers;

namespace Rh.MessageFormat.Formatting.Skeletons;

/// <summary>
/// Formats numbers according to NumberFormatOptions.
/// </summary>
internal static class NumberSkeletonFormatter
{
    /// <summary>
    /// Formats a number according to the given skeleton options.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Format(double value, NumberFormatOptions options, ref FormatterContext ctx)
    {
        // Apply scale
        var scaledValue = value * options.Scale;

        // Apply percent/permille
        if (options.IsPercent)
        {
            scaledValue *= Multipliers.Percent;
        }
        else if (options.IsPermille)
        {
            scaledValue *= Multipliers.Permille;
        }

        // Handle notation
        return options.Notation switch
        {
            NumberNotation.Scientific => FormatScientific(scaledValue, options, ctx.Culture),
            NumberNotation.Engineering => FormatEngineering(scaledValue, options, ctx.Culture),
            NumberNotation.CompactShort => FormatCompact(scaledValue, options, ref ctx, isLong: false),
            NumberNotation.CompactLong => FormatCompact(scaledValue, options, ref ctx, isLong: true),
            _ => FormatStandard(scaledValue, options, ref ctx)
        };
    }

    private static string FormatStandard(double value, NumberFormatOptions options, ref FormatterContext ctx)
    {
        var nfi = (NumberFormatInfo)ctx.Culture.NumberFormat.Clone();

        // Apply precision settings
        if (options.MinimumFractionDigits.HasValue)
        {
            nfi.NumberDecimalDigits = options.MinimumFractionDigits.Value;
        }

        // Apply grouping
        if (!options.UseGrouping)
        {
            nfi.NumberGroupSeparator = Common.Empty;
        }

        string formatted;

        // Currency formatting
        if (!string.IsNullOrEmpty(options.CurrencyCode))
        {
            formatted = FormatCurrency(value, options, ref ctx);
        }
        // Unit formatting
        else if (!string.IsNullOrEmpty(options.Unit))
        {
            formatted = FormatUnit(value, options, nfi, ref ctx);
        }
        // Percent formatting (already scaled)
        else if (options.IsPercent)
        {
            formatted = FormatPercent(value, options, nfi);
        }
        // Permille formatting (already scaled)
        else if (options.IsPermille)
        {
            formatted = FormatPermille(value, options, nfi);
        }
        // Regular number formatting
        else
        {
            formatted = FormatNumber(value, options, nfi);
        }

        // Apply sign display
        return ApplySignDisplay(formatted, value, options, nfi);
    }

    private static string FormatNumber(double value, NumberFormatOptions options, NumberFormatInfo nfi)
    {
        var format = BuildFormatString(options);
        return value.ToString(format, nfi);
    }

    private static string FormatPercent(double value, NumberFormatOptions options, NumberFormatInfo nfi)
    {
        // Value is already multiplied by 100
        var format = BuildFormatString(options);
        return value.ToString(format, nfi) + Symbols.PercentSymbol;
    }

    private static string FormatPermille(double value, NumberFormatOptions options, NumberFormatInfo nfi)
    {
        // Value is already multiplied by 1000
        var format = BuildFormatString(options);
        return value.ToString(format, nfi) + Symbols.Permille;
    }

    private static string FormatCurrency(double value, NumberFormatOptions options, ref FormatterContext ctx)
    {
        try
        {
            // Try to get culture for the currency
            var currencyCode = options.CurrencyCode!;
            var nfi = (NumberFormatInfo)ctx.Culture.NumberFormat.Clone();

            // Apply precision if specified
            if (options.MinimumFractionDigits.HasValue)
            {
                nfi.CurrencyDecimalDigits = options.MinimumFractionDigits.Value;
            }

            // Apply grouping
            if (!options.UseGrouping)
            {
                nfi.CurrencyGroupSeparator = Common.Empty;
            }

            // Get the currency symbol based on display option using generated CLDR metadata
            string symbol;
            switch (options.CurrencyDisplay)
            {
                case CurrencyDisplay.Code:
                    symbol = currencyCode.ToUpperInvariant() + Common.Space;
                    break;
                case CurrencyDisplay.Name:
                    // Use proper plural form based on the value and locale's plural rules
                    symbol = CurrencyMetadata.GetDisplayName(ref ctx, currencyCode, value) + Common.Space;
                    break;
                case CurrencyDisplay.NarrowSymbol:
                    symbol = CurrencyMetadata.GetNarrowSymbol(ref ctx, currencyCode);
                    break;
                default:
                    symbol = CurrencyMetadata.GetSymbol(ref ctx, currencyCode);
                    break;
            }

            nfi.CurrencySymbol = symbol;

            return value.ToString(Formats.Currency, nfi);
        }
        catch (Exception ex)
        {
            throw new MessageFormatterException($"Failed to format currency '{options.CurrencyCode}'", ex);
        }
    }

    private static string FormatUnit(double value, NumberFormatOptions options, NumberFormatInfo nfi, ref FormatterContext ctx)
    {
        var format = BuildFormatString(options);
        var formattedNumber = value.ToString(format, nfi);
        var unit = options.Unit!;

        // Get unit display string based on display option and locale's plural rules
        var unitString = GetUnitString(unit, options.UnitDisplay, value, ref ctx);

        // CLDR patterns contain {0} placeholder for the number
        if (!string.IsNullOrEmpty(unitString) && unitString.Contains(List.Placeholders.First))
        {
            return unitString.Replace(List.Placeholders.First, formattedNumber);
        }

        // Fallback for patterns without placeholder or when unit not found
        return $"{formattedNumber} {unitString ?? unit}";
    }

    private static string GetUnitString(string unit, UnitDisplay display, double value, ref FormatterContext ctx)
    {
        // Get unit display string from generated CLDR metadata
        // The metadata handles:
        // - Stripping ICU category prefixes (e.g., "length-meter" -> "meter")
        // - Resolving aliases (e.g., "metre" -> "meter")
        // - Locale fallback (exact -> base language -> English)
        var width = display switch
        {
            UnitDisplay.Long => Styles.Long,
            UnitDisplay.Narrow => Styles.Narrow,
            _ => Styles.Short
        };

        return UnitMetadata.GetUnitString(ref ctx, unit, width, value);
    }

    private static string FormatScientific(double value, NumberFormatOptions options, CultureInfo culture)
    {
        var minFraction = options.MinimumFractionDigits ?? 2;
        var format = "0." + new string(Symbols.RequiredDigit, minFraction) + "E+0";
        return value.ToString(format, culture);
    }

    private static string FormatEngineering(double value, NumberFormatOptions options, CultureInfo culture)
    {
        if (value == 0) return "0";

        var exponent = (int)Math.Floor(Math.Log10(Math.Abs(value)));
        var engExponent = (exponent / 3) * 3;
        var mantissa = value / Math.Pow(10, engExponent);

        var minFraction = options.MinimumFractionDigits ?? 2;
        var format = "0." + new string(Symbols.RequiredDigit, minFraction);

        return mantissa.ToString(format, culture) + "E" + (engExponent >= 0 ? "+" : Common.Empty) + engExponent;
    }

    private static string FormatCompact(double value, NumberFormatOptions options, ref FormatterContext ctx, bool isLong)
    {
        if (Math.Abs(value) < Multipliers.CompactThreshold)
        {
            return FormatStandard(value, options, ref ctx);
        }

        var absValue = Math.Abs(value);
        var exponent = (int)Math.Floor(Math.Log10(absValue) / 3) * 3;
        var suffixIndex = exponent / 3;

        var suffixes = isLong ? CompactSuffixes.Long : CompactSuffixes.Short;
        if (suffixIndex >= suffixes.Length)
        {
            suffixIndex = suffixes.Length - 1;
            exponent = suffixIndex * 3;
        }

        var scaledValue = value / Math.Pow(10, exponent);

        // Format with appropriate precision
        var minFraction = options.MinimumFractionDigits ?? 0;
        var maxFraction = options.MaximumFractionDigits ?? 1;

        var nfi = (NumberFormatInfo)ctx.Culture.NumberFormat.Clone();
        nfi.NumberDecimalDigits = maxFraction;

        if (!options.UseGrouping)
        {
            nfi.NumberGroupSeparator = Common.Empty;
        }

        var formatted = scaledValue.ToString($"N{maxFraction}", nfi);

        // Remove trailing zeros if minFraction is 0
        if (minFraction == 0 && formatted.Contains(nfi.NumberDecimalSeparator))
        {
            formatted = formatted.TrimEnd(Symbols.RequiredDigit).TrimEnd(nfi.NumberDecimalSeparator.ToCharArray());
        }

        return formatted + suffixes[suffixIndex];
    }

    private static string BuildFormatString(NumberFormatOptions options)
    {
        var sb = StringBuilderPool.Get();
        try
        {
            // Integer part
            var minInt = options.MinimumIntegerDigits ?? 1;
            if (options.UseGrouping && minInt <= 1)
            {
                sb.Append(Formats.Grouped);
            }
            else if (options.UseGrouping)
            {
                // With grouping and minimum digits, use format like #,000 for minInt=3
                sb.Append("#,");
                sb.Append(new string(Symbols.RequiredDigit, minInt));
            }
            else
            {
                sb.Append(new string(Symbols.RequiredDigit, minInt));
            }

            // Fraction part
            if (options.MinimumFractionDigits.HasValue || options.MaximumFractionDigits.HasValue)
            {
                sb.Append(Symbols.FractionStart);
                var minFrac = options.MinimumFractionDigits ?? 0;
                var maxFrac = options.MaximumFractionDigits ?? minFrac;

                sb.Append(new string(Symbols.RequiredDigit, minFrac));
                sb.Append(new string(Symbols.OptionalDigit, maxFrac - minFrac));
            }
            else if (minInt <= 1)
            {
                sb.Append(Formats.DefaultWithDecimals); // Default: up to 3 decimal places
            }

            return sb.ToString();
        }
        finally
        {
            StringBuilderPool.Return(sb);
        }
    }

    private static string ApplySignDisplay(string formatted, double value, NumberFormatOptions options, NumberFormatInfo nfi)
    {
        switch (options.SignDisplay)
        {
            case SignDisplay.Always:
                if (value > 0 && !formatted.StartsWith("+"))
                {
                    return "+" + formatted;
                }
                break;

            case SignDisplay.ExceptZero:
                if (value > 0)
                {
                    return "+" + formatted;
                }
                if (value == 0)
                {
                    // Remove negative sign if present
                    formatted = formatted.TrimStart(Common.DashChar);
                }
                break;

            case SignDisplay.Never:
                if (formatted.StartsWith("-") || formatted.StartsWith(nfi.NegativeSign))
                {
                    return formatted.TrimStart(Common.DashChar).TrimStart(nfi.NegativeSign.ToCharArray());
                }
                break;

            case SignDisplay.Accounting:
            case SignDisplay.AccountingAlways:
            case SignDisplay.AccountingExceptZero:
                if (value < 0)
                {
                    formatted = formatted.TrimStart(Common.DashChar).TrimStart(nfi.NegativeSign.ToCharArray());
                    return "(" + formatted + ")";
                }
                if (options.SignDisplay == SignDisplay.AccountingAlways && value > 0)
                {
                    return "+" + formatted;
                }
                if (options.SignDisplay == SignDisplay.AccountingExceptZero && value > 0)
                {
                    return "+" + formatted;
                }
                break;
        }

        return formatted;
    }
}