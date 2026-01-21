namespace Rh.MessageFormat;

/// <summary>
/// Constants used throughout the MessageFormat library.
/// Use with: using static Rh.MessageFormat.Constants;
/// </summary>
internal static class Constants
{
    /// <summary>
    /// Formatter type names used in message patterns.
    /// </summary>
    public static class Formatters
    {
        public const string Number = "number";
        public const string Date = "date";
        public const string Time = "time";
        public const string DateTime = "datetime";
        public const string Plural = "plural";
        public const string Select = "select";
        public const string SelectOrdinal = "selectordinal";
        public const string List = "list";
    }

    /// <summary>
    /// Style names for date, time, and number formatting.
    /// </summary>
    public static class Styles
    {
        public const string Short = "short";
        public const string Medium = "medium";
        public const string Long = "long";
        public const string Full = "full";
        public const string Narrow = "narrow";
        public const string Integer = "integer";
        public const string Currency = "currency";
        public const string Percent = "percent";
    }

    /// <summary>
    /// Plural category names from CLDR.
    /// </summary>
    public static class Plurals
    {
        public const string Zero = "zero";
        public const string One = "one";
        public const string Two = "two";
        public const string Few = "few";
        public const string Many = "many";
        public const string Other = "other";

        /// <summary>
        /// Common interned keys for faster string comparisons.
        /// </summary>
        public static readonly string[] CommonKeys = { Zero, One, Two, Few, Many, Other };
    }

    /// <summary>
    /// Number formatting constants.
    /// </summary>
    public static class Numbers
    {
        /// <summary>
        /// Multipliers for percent and permille.
        /// </summary>
        public static class Multipliers
        {
            public const int Percent = 100;
            public const int Permille = 1000;
            public const int CompactThreshold = 1000;
        }

        /// <summary>
        /// Standard .NET format strings.
        /// </summary>
        public static class Formats
        {
            public const string Integer = "N0";
            public const string Currency = "C";
            public const string Percent = "P";
            public const string Default = "#,##0.###";
            public const string DefaultWithDecimals = ".###";
            public const string Grouped = "#,##0";
            public const string TwoDecimals = "N2";
        }

        /// <summary>
        /// ICU number skeleton tokens.
        /// </summary>
        public static class Skeleton
        {
            // Base types
            public const string Percent = "percent";
            public const string Permille = "permille";

            // Notation
            public const string CompactShort = "compact-short";
            public const string CompactLong = "compact-long";
            public const string Scientific = "scientific";
            public const string Engineering = "engineering";

            // Sign display
            public const string SignAlways = "sign-always";
            public const string SignNever = "sign-never";
            public const string SignExceptZero = "sign-except-zero";
            public const string SignAccounting = "sign-accounting";
            public const string SignAccountingAlways = "sign-accounting-always";
            public const string SignAccountingExceptZero = "sign-accounting-except-zero";

            // Grouping
            public const string GroupOff = "group-off";
            public const string GroupMin2 = "group-min2";
            public const string GroupAuto = "group-auto";
            public const string GroupOnAligned = "group-on-aligned";
            public const string GroupAlways = "group-always";

            // Unit width
            public const string UnitWidthShort = "unit-width-short";
            public const string UnitWidthNarrow = "unit-width-narrow";
            public const string UnitWidthFullName = "unit-width-full-name";
            public const string UnitWidthIsoCode = "unit-width-iso-code";

            // Currency display
            public const string CurrencySymbol = "currency-symbol";
            public const string CurrencyNarrowSymbol = "currency-narrow-symbol";

            // Stems (used with / separator)
            public const string CurrencyStem = "currency";
            public const string ScaleStem = "scale";
            public const string UnitStem = "unit";
            public const string MeasureUnitStem = "measure-unit";
            public const string IntegerWidthStem = "integer-width";

            // Concise tokens
            public const char PercentChar = '%';
            public const char CompactShortChar = 'K';
            public const string CompactLongToken = "KK";
            public const string SignAlwaysToken = "+!";
            public const string SignNeverToken = "+_";
            public const string SignExceptZeroToken = "+?";
            public const string AccountingToken = "()";
            public const string GroupOffToken = ",_";
            public const string GroupMin2Token = ",?";
            public const string GroupAlwaysToken = ",!";
        }

        /// <summary>
        /// Compact notation suffixes.
        /// </summary>
        public static class CompactSuffixes
        {
            public static readonly string[] Short = { "", "K", "M", "B", "T" };
            public static readonly string[] Long = { "", " thousand", " million", " billion", " trillion" };
        }

        /// <summary>
        /// Symbols used in number formatting.
        /// </summary>
        public static class Symbols
        {
            public const char FractionStart = '.';
            public const char SignificantDigit = '@';
            public const char OptionalDigit = '#';
            public const char RequiredDigit = '0';
            public const char Wildcard = '*';
            public const char Plus = '+';
            public const char Minus = '-';
            public const string Permille = "\u2030";
            public const string PercentSymbol = "%";
        }
    }

    /// <summary>
    /// DateTime formatting constants.
    /// </summary>
    public static class DateTime
    {
        /// <summary>
        /// ICU skeleton field characters.
        /// </summary>
        public static class SkeletonChars
        {
            public const char Year = 'y';
            public const char Month = 'M';
            public const char StandaloneMonth = 'L';
            public const char Day = 'd';
            public const char DayOfWeek = 'E';
            public const char StandaloneDayOfWeek = 'c';
            public const char HourLocale = 'j';
            public const char HourLocaleNoAmPm = 'J';
            public const char Hour12 = 'h';
            public const char Hour24 = 'H';
            public const char Hour1To24 = 'k';
            public const char Hour0To11 = 'K';
            public const char Minute = 'm';
            public const char Second = 's';
            public const char FractionalSecond = 'S';
            public const char AmPm = 'a';
            public const char Era = 'G';
            public const char Quarter = 'Q';
            public const char StandaloneQuarter = 'q';
            public const char WeekOfYear = 'w';
            public const char DayOfYear = 'D';
            public const char TimezoneAbbr = 'z';
            public const char TimezoneOffset = 'Z';
            public const char TimezoneOffsetZ = 'X';
            public const char TimezoneOffsetNoZ = 'x';
            public const char TimezoneId = 'V';
        }

        /// <summary>
        /// .NET format strings for datetime.
        /// </summary>
        public static class Formats
        {
            public const string General = "G";
            public const string YearFull = "yyyy";
            public const string YearShort = "yy";
            public const string MonthNumeric = "M";
            public const string MonthPadded = "MM";
            public const string MonthAbbr = "MMM";
            public const string MonthFull = "MMMM";
            public const string MonthNarrow = "MMMMM";
            public const string DayNumeric = "d";
            public const string DayPadded = "dd";
            public const string DayOfWeekAbbr = "ddd";
            public const string DayOfWeekFull = "dddd";
            public const string Hour12Numeric = "h";
            public const string Hour12Padded = "hh";
            public const string Hour24Numeric = "H";
            public const string Hour24Padded = "HH";
            public const string MinuteNumeric = "m";
            public const string MinutePadded = "mm";
            public const string SecondNumeric = "s";
            public const string SecondPadded = "ss";
            public const string AmPm = "tt";
            public const string EraShort = "g";
            public const string EraLong = "gg";
            public const string Timezone = "zzz";
            public const string TimezoneLong = "zzzz";
            public const string TimezoneOffset = "K";
        }

        /// <summary>
        /// Fallback date patterns (English).
        /// </summary>
        public static class FallbackPatterns
        {
            public const string DateShort = "M/d/yy";
            public const string DateMedium = "MMM d, y";
            public const string DateLong = "MMMM d, y";
            public const string DateFull = "EEEE, MMMM d, y";

            public const string TimeShort = "h:mm a";
            public const string TimeMedium = "h:mm:ss a";
            public const string TimeLong = "h:mm:ss a z";
            public const string TimeFull = "h:mm:ss a zzzz";

            public const string DateTimeCombination = "{1}, {0}";
        }

        /// <summary>
        /// Separator characters.
        /// </summary>
        public static class Separators
        {
            public const char Colon = ':';
            public const char Space = ' ';
            public const char Comma = ',';
            public const char Dot = '.';
            public const char Dash = '-';
            public const char Slash = '/';
            public const char T = 'T';
        }
    }

    /// <summary>
    /// List formatting constants.
    /// </summary>
    public static class List
    {
        /// <summary>
        /// CLDR list style types.
        /// </summary>
        public static class StyleTypes
        {
            public const string Conjunction = "conjunction";
            public const string Disjunction = "disjunction";
            public const string Unit = "unit";
            public const string Standard = "standard";
            public const string Or = "or";
        }

        /// <summary>
        /// CLDR list width types.
        /// </summary>
        public static class WidthTypes
        {
            public const string Long = "long";
            public const string Short = "short";
            public const string Narrow = "narrow";
        }

        /// <summary>
        /// Fallback connectors (English).
        /// </summary>
        public static class FallbackConnectors
        {
            public const string Separator = ", ";
            public const string ConjunctionLast = ", and ";
            public const string ConjunctionPair = " and ";
            public const string DisjunctionLast = ", or ";
            public const string DisjunctionPair = " or ";
        }

        /// <summary>
        /// Pattern placeholder format.
        /// </summary>
        public static class Placeholders
        {
            public const string First = "{0}";
            public const string Second = "{1}";
            public const int PlaceholderLength = 3;
        }
    }

    /// <summary>
    /// Parser-related constants.
    /// </summary>
    public static class Parser
    {
        /// <summary>
        /// Special characters used in message patterns.
        /// </summary>
        public static class Chars
        {
            public const char OpenBrace = '{';
            public const char CloseBrace = '}';
            public const char Hash = '#';
            public const char Comma = ',';
            public const char Quote = '\'';
            public const char LessThan = '<';
            public const char GreaterThan = '>';
            public const char Slash = '/';
            public const char Underscore = '_';
            public const char Dash = '-';
            public const char Newline = '\n';
            public const char EqualsSign = '=';
            public const char Dot = '.';
        }

        /// <summary>
        /// Special sequences used in parsing.
        /// </summary>
        public static class Sequences
        {
            public const string SkeletonPrefix = "::";
            public const string OffsetKeyword = "offset:";
            public const string CloseTag = "</";
        }
    }

    /// <summary>
    /// Unit formatting constants.
    /// </summary>
    public static class Units
    {
        /// <summary>
        /// CLDR unit category prefixes.
        /// </summary>
        public static readonly string[] CategoryPrefixes =
        {
            "length-", "temperature-", "mass-", "volume-", "duration-",
            "digital-", "speed-", "area-", "pressure-", "energy-",
            "acceleration-", "angle-", "concentr-", "consumption-",
            "electric-", "frequency-", "force-", "graphics-", "light-",
            "power-", "torque-"
        };

        /// <summary>
        /// Unit display width types.
        /// </summary>
        public static class WidthTypes
        {
            public const string Long = "long";
            public const string Short = "short";
            public const string Narrow = "narrow";
        }
    }

    /// <summary>
    /// Common string constants.
    /// </summary>
    public static class Common
    {
        public const string Empty = "";
        public const string Space = " ";
        public const char DashChar = '-';
        public const char UnderscoreChar = '_';
    }
}
