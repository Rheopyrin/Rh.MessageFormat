using System.Reflection;
using Rh.MessageFormat.Abstractions;
using Rh.MessageFormat.Abstractions.Interfaces;
using Rh.MessageFormat.Abstractions.Models;

namespace Rh.MessageFormat.CldrGenerator.Tests.Integration.Helpers;

/// <summary>
/// Helper class for validating generated locale data classes.
/// </summary>
public sealed class GeneratedCodeValidator
{
    private readonly Assembly _assembly;

    public GeneratedCodeValidator(Assembly assembly)
    {
        _assembly = assembly;
    }

    /// <summary>
    /// Gets an instance of a generated locale data class.
    /// </summary>
    public ICldrLocaleData? GetLocaleDataInstance(string locale)
    {
        var className = $"CldrLocaleData_{locale.Replace("-", "_")}";
        var fullName = $"Rh.MessageFormat.CldrData.Generated.{className}";

        var type = _assembly.GetType(fullName);
        if (type == null)
            return null;

        // Get the Instance property (singleton)
        var instanceProperty = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        return instanceProperty?.GetValue(null) as ICldrLocaleData;
    }

    /// <summary>
    /// Validates that a locale data class exists and implements ICldrLocaleData.
    /// </summary>
    public bool ValidateLocaleDataExists(string locale)
    {
        return GetLocaleDataInstance(locale) != null;
    }

    /// <summary>
    /// Tests GetPluralCategory with a given input.
    /// </summary>
    public string? TestPluralCategory(string locale, int number)
    {
        var instance = GetLocaleDataInstance(locale);
        if (instance == null)
            return null;

        var context = new PluralContext(number);
        return instance.GetPluralCategory(context);
    }

    /// <summary>
    /// Tests GetPluralCategory with a decimal input.
    /// </summary>
    public string? TestPluralCategory(string locale, decimal number)
    {
        var instance = GetLocaleDataInstance(locale);
        if (instance == null)
            return null;

        var context = new PluralContext(number);
        return instance.GetPluralCategory(context);
    }

    /// <summary>
    /// Tests GetOrdinalCategory with a given input.
    /// </summary>
    public string? TestOrdinalCategory(string locale, int number)
    {
        var instance = GetLocaleDataInstance(locale);
        if (instance == null)
            return null;

        var context = new PluralContext(number);
        return instance.GetOrdinalCategory(context);
    }

    /// <summary>
    /// Tests TryGetCurrency for a specific currency code.
    /// </summary>
    public CurrencyData? TestGetCurrency(string locale, string currencyCode)
    {
        var instance = GetLocaleDataInstance(locale);
        if (instance == null)
            return null;

        if (instance.TryGetCurrency(currencyCode, out var data))
            return data;

        return null;
    }

    /// <summary>
    /// Tests TryGetUnit for a specific unit ID.
    /// </summary>
    public UnitData? TestGetUnit(string locale, string unitId)
    {
        var instance = GetLocaleDataInstance(locale);
        if (instance == null)
            return null;

        if (instance.TryGetUnit(unitId, out var data))
            return data;

        return null;
    }

    /// <summary>
    /// Gets the DatePatterns property.
    /// </summary>
    public DatePatternData? TestGetDatePatterns(string locale)
    {
        var instance = GetLocaleDataInstance(locale);
        return instance?.DatePatterns;
    }

    /// <summary>
    /// Tests TryGetListPattern for a specific type.
    /// </summary>
    public ListPatternData? TestGetListPattern(string locale, string type)
    {
        var instance = GetLocaleDataInstance(locale);
        if (instance == null)
            return null;

        if (instance.TryGetListPattern(type, out var data))
            return data;

        return null;
    }

    /// <summary>
    /// Gets all types in the assembly that implement ICldrLocaleData.
    /// </summary>
    public IEnumerable<Type> GetAllLocaleDataTypes()
    {
        return _assembly.GetTypes()
            .Where(t => typeof(ICldrLocaleData).IsAssignableFrom(t) && !t.IsInterface);
    }
}
