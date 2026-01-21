using System.Globalization;

namespace Rh.MessageFormat.Custom;

/// <summary>
/// Delegate for custom value formatting.
/// </summary>
/// <param name="value">The value to format.</param>
/// <param name="style">The optional style/arguments from the pattern (e.g., "USD" from {price, money, USD}).</param>
/// <param name="locale">The current locale.</param>
/// <param name="culture">The current culture info.</param>
/// <returns>The formatted string.</returns>
public delegate string CustomFormatterDelegate(object? value, string? style, string locale, CultureInfo culture);