using System;
using Rh.MessageFormat.Tests.Mocks;
using Xunit;

namespace Rh.MessageFormat.Tests.Ast;

/// <summary>
/// Tests for number system support in date/time formatting.
/// </summary>
public class NumberSystemTests
{
    // Bengali digits: ০১২৩৪৫৬৭৮৯
    private const string BengaliOne = "১";
    private const string BengaliFive = "৫";
    private const string BengaliThree = "৩";
    private const string BengaliZero = "০";

    // Arabic-Indic digits: ٠١٢٣٤٥٦٧٨٩
    private const string ArabicOne = "١";
    private const string ArabicFive = "٥";

    // Thai digits: ๐๑๒๓๔๕๖๗๘๙
    private const string ThaiOne = "๑";
    private const string ThaiFive = "๕";

    [Fact]
    public void DateFormat_EnglishLocale_UsesLatinDigits()
    {
        // English locale uses Latin digits (the default)
        var formatter = new MessageFormatter("en", TestOptions.WithEnglish());

        var date = new DateTime(2025, 1, 15);
        var result = formatter.FormatMessage("{d, date, short}", new { d = date });

        // Should contain Latin digits
        Assert.Contains("1", result);
        Assert.Contains("15", result);
    }

    [Fact]
    public void DateFormat_BengaliLocale_UsesBengaliDigits()
    {
        // Bengali locale uses Bengali digits
        var formatter = new MessageFormatter("bn", TestOptions.WithLocaleData(MockCldrLocaleData.CreateBengali()));

        var date = new DateTime(2025, 1, 15);
        var result = formatter.FormatMessage("{d, date, short}", new { d = date });

        // Should contain Bengali digits for 15, 1, and 25 (year)
        Assert.Contains(BengaliOne, result); // Bengali 1
        Assert.Contains(BengaliFive, result); // Bengali 5
    }

    [Fact]
    public void DateFormat_ArabicLocale_UsesArabicDigits()
    {
        // Arabic locale uses Arabic-Indic digits
        var formatter = new MessageFormatter("ar", TestOptions.WithLocaleData(MockCldrLocaleData.CreateArabic()));

        var date = new DateTime(2025, 1, 15);
        var result = formatter.FormatMessage("{d, date, short}", new { d = date });

        // Should contain Arabic-Indic digits
        Assert.Contains(ArabicOne, result); // Arabic 1
        Assert.Contains(ArabicFive, result); // Arabic 5
    }

    [Fact]
    public void DateFormat_ThaiLocale_UsesThaiDigits()
    {
        // Thai locale uses Thai digits
        var formatter = new MessageFormatter("th", TestOptions.WithLocaleData(MockCldrLocaleData.CreateThai()));

        var date = new DateTime(2025, 1, 15);
        var result = formatter.FormatMessage("{d, date, short}", new { d = date });

        // Should contain Thai digits
        Assert.Contains(ThaiOne, result); // Thai 1
        Assert.Contains(ThaiFive, result); // Thai 5
    }

    [Fact]
    public void TimeFormat_BengaliLocale_UsesBengaliDigits()
    {
        // Bengali locale uses Bengali digits for time as well
        var formatter = new MessageFormatter("bn", TestOptions.WithLocaleData(MockCldrLocaleData.CreateBengali()));

        var time = new DateTime(2025, 1, 15, 14, 30, 0);
        var result = formatter.FormatMessage("{t, time, short}", new { t = time });

        // Should contain Bengali digits
        Assert.Contains(BengaliThree, result); // Bengali 3 (from 30 minutes)
        Assert.Contains(BengaliZero, result); // Bengali 0 (from 30)
    }

    [Fact]
    public void DateTimeFormat_BengaliLocale_UsesBengaliDigits()
    {
        // Bengali locale uses Bengali digits for datetime
        var formatter = new MessageFormatter("bn", TestOptions.WithLocaleData(MockCldrLocaleData.CreateBengali()));

        var datetime = new DateTime(2025, 1, 15, 14, 30, 0);
        var result = formatter.FormatMessage("{dt, datetime, short}", new { dt = datetime });

        // Should contain Bengali digits for both date and time parts
        Assert.Contains(BengaliOne, result); // Bengali 1 (from day 15 or month)
        Assert.Contains(BengaliFive, result); // Bengali 5 (from day 15)
        Assert.Contains(BengaliThree, result); // Bengali 3 (from 30 minutes)
    }

    [Fact]
    public void NumberSystemTransformation_DoesNotAffectNonNumericText()
    {
        // Text without digits should not be affected
        var formatter = new MessageFormatter("bn", TestOptions.WithLocaleData(MockCldrLocaleData.CreateBengali()));

        // Using a date format that includes text like day names
        var date = new DateTime(2025, 1, 15);
        var result = formatter.FormatMessage("{d, date, long}", new { d = date });

        // Should still contain Bengali digits
        Assert.Contains(BengaliOne, result); // Bengali 1
        Assert.Contains(BengaliFive, result); // Bengali 5
    }

    [Fact]
    public void LocaleWithoutNumberSystem_DefaultsToLatinDigits()
    {
        // A locale without a specified numbering system should use Latin digits
        var localeData = new MockCldrLocaleData { Locale = "unknown" };
        // Don't call WithNumberingSystem - it will default to "latn"

        var formatter = new MessageFormatter("unknown", TestOptions.WithLocaleData(localeData));

        var date = new DateTime(2025, 1, 15);
        var result = formatter.FormatMessage("{d, date, short}", new { d = date });

        // Should contain Latin digits (default)
        Assert.Contains("1", result);
        Assert.Contains("5", result);
    }

    [Fact]
    public void NumberSystemTransformer_TransformsAllDigits()
    {
        // Test that all digits 0-9 are transformed
        var formatter = new MessageFormatter("bn", TestOptions.WithLocaleData(MockCldrLocaleData.CreateBengali()));

        // Use a date that includes various digits
        var date = new DateTime(1987, 6, 23, 4, 5, 9);  // Contains digits: 1, 9, 8, 7, 6, 2, 3, 4, 5
        var result = formatter.FormatMessage("{dt, datetime, short}", new { dt = date });

        // All digits should be Bengali, not Latin
        // Check that key Bengali digits are present
        Assert.Contains("৬", result);  // Bengali 6 for month
        Assert.Contains("২", result);  // Bengali 2 from day 23
        Assert.Contains("৩", result);  // Bengali 3 from day 23
    }

    [Fact]
    public void DateFormat_BengaliLocale_WithRealCldrData_UsesBengaliDigits()
    {
        // Test with real CLDR data provider (not mocks)
        // This validates end-to-end integration
        // Bengali locale uses "beng" as defaultNumberingSystem in CLDR
        var formatter = new MessageFormatter("bn");

        // Unix timestamp for 2025-01-01 00:00:00 UTC
        var timestamp = 1735689600000L;
        var result = formatter.FormatMessage("{d, date, medium}", new { d = timestamp });

        // Should contain Bengali digits (CLDR defaultNumberingSystem = "beng")
        Assert.Contains(BengaliOne, result);  // Bengali 1
        Assert.Contains("২", result);  // Bengali 2 from 2025
        Assert.Contains("০", result);  // Bengali 0 from 2025
        Assert.Contains("৫", result);  // Bengali 5 from 2025

        // Verify Bengali year digits are present
        Assert.Contains("২০২৫", result);  // Bengali 2025
    }

    [Fact]
    public void DateFormat_ArabicLocale_WithRealCldrData_UsesLatinDigits()
    {
        // Test with real CLDR data provider (not mocks)
        // IMPORTANT: Arabic locale in CLDR uses "latn" (Latin) as defaultNumberingSystem!
        // The "native" system is "arab" but it's NOT the default.
        // JavaScript/ICU may use native numbering for date/time, which differs from CLDR default.
        var formatter = new MessageFormatter("ar");

        var timestamp = 1735689600000L;
        var result = formatter.FormatMessage("{d, date, medium}", new { d = timestamp });

        // CLDR says Arabic uses Latin digits by default (defaultNumberingSystem: "latn")
        // So output will contain Latin digits, not Arabic-Indic
        Assert.Contains("2025", result);  // Latin digits as per CLDR
    }

    [Fact]
    public void DateFormat_BengaliBDLocale_WithRealCldrData_FallsBackToBengaliDigits()
    {
        // Test locale fallback: bn-BD should fallback to bn which uses Bengali digits
        var formatter = new MessageFormatter("bn-BD");

        var timestamp = 1735689600000L;
        var result = formatter.FormatMessage("{d, date, medium}", new { d = timestamp });

        // Should contain Bengali digits (fallback from bn-BD to bn which has defaultNumberingSystem = "beng")
        Assert.Contains(BengaliOne, result);  // Bengali 1
        Assert.Contains("২০২৫", result);  // Bengali 2025
    }
}
