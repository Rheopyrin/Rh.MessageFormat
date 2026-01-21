namespace Rh.MessageFormat.Abstractions.Models;

/// <summary>
/// Contains localized currency display data.
/// </summary>
public readonly struct CurrencyData
{
    /// <summary>
    /// The ISO 4217 currency code (e.g., "USD", "EUR").
    /// </summary>
    public readonly string Code;

    /// <summary>
    /// The currency symbol (e.g., "$", "€").
    /// </summary>
    public readonly string Symbol;

    /// <summary>
    /// The narrow currency symbol for compact display.
    /// </summary>
    public readonly string NarrowSymbol;

    /// <summary>
    /// The localized display name of the currency.
    /// </summary>
    public readonly string DisplayName;

    /// <summary>
    /// The singular form of the currency name (for CLDR plural category "one").
    /// </summary>
    public readonly string DisplayNameOne;

    /// <summary>
    /// The currency name for CLDR plural category "few" (used in some languages like Ukrainian, Polish).
    /// </summary>
    public readonly string? DisplayNameFew;

    /// <summary>
    /// The currency name for CLDR plural category "many" (used in some languages like Ukrainian, Russian).
    /// </summary>
    public readonly string? DisplayNameMany;

    /// <summary>
    /// The plural form of the currency name (for CLDR plural category "other").
    /// </summary>
    public readonly string DisplayNameOther;

    /// <summary>
    /// Creates a new CurrencyData instance.
    /// </summary>
    public CurrencyData(string code, string symbol, string narrowSymbol,
        string displayName, string displayNameOne, string? displayNameFew,
        string? displayNameMany, string displayNameOther)
    {
        Code = code;
        Symbol = symbol;
        NarrowSymbol = narrowSymbol;
        DisplayName = displayName;
        DisplayNameOne = displayNameOne;
        DisplayNameFew = displayNameFew;
        DisplayNameMany = displayNameMany;
        DisplayNameOther = displayNameOther;
    }
}
