using System.Text;
using Rh.MessageFormat.CldrGenerator.Plural.Parsing;
using Rh.MessageFormat.CldrGenerator.Plural.Parsing.AST;
using Rh.MessageFormat.CldrGenerator.Plural.SourceGeneration;
using Rh.MessageFormat.CldrGenerator.Templates;

namespace Rh.MessageFormat.CldrGenerator.Generators;

/// <summary>
/// Generates a C# class file for a single locale using T4 templates.
/// </summary>
public class LocaleClassGenerator
{
    private readonly LocaleData _data;

    public LocaleClassGenerator(LocaleData data)
    {
        _data = data;
    }

    /// <summary>
    /// Prepares the template data for generation.
    /// </summary>
    public LocaleTemplateData PrepareTemplateData()
    {
        return new LocaleTemplateData
        {
            ClassName = GetClassName(_data.Locale),
            PluralRuleCode = GeneratePluralRuleCode(_data.PluralRules),
            OrdinalRuleCode = GeneratePluralRuleCode(_data.OrdinalRules),
            HasCurrencies = _data.Currencies.Count > 0,
            CurrencyArrayCode = GenerateCurrencyArrayCode(),
            HasUnits = _data.Units.Count > 0,
            UnitDictCode = GenerateUnitDictCode(),
            HasDatePatterns = _data.DatePatterns != null,
            DatePatternsCode = GenerateDatePatternsCode(),
            HasListPatterns = _data.ListPatterns.Count > 0,
            ListPatternsCode = GenerateListPatternsCode()
        };
    }

    /// <summary>
    /// Parses a CLDR rule string and returns a PluralRule.
    /// </summary>
    private static PluralRule ParseRule(string count, string ruleText)
    {
        var parser = new RuleParser(ruleText);
        var orConditions = parser.ParseRuleContent();
        var condition = new Condition(count, ruleText, orConditions);
        return new PluralRule(Array.Empty<string>(), new[] { condition });
    }

    private string GeneratePluralRuleCode(Dictionary<string, string> rules)
    {
        if (rules.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var (count, rule) in rules.Where(r => r.Key != "other"))
        {
            try
            {
                var parsed = ParseRule(count, rule);
                var generator = new RuleGenerator(parsed);
                var ruleCode = new StringBuilder();
                generator.WriteTo(ruleCode, 0);

                // Remove the final "return \"other\";" since we add it in the template
                var code = ruleCode.ToString().TrimEnd();
                if (code.EndsWith("return \"other\";"))
                {
                    code = code.Substring(0, code.Length - "return \"other\";".Length).TrimEnd();
                }

                // Indent each line properly (8 spaces base + preserve relative indentation)
                var lines = code.Split('\n');
                foreach (var line in lines)
                {
                    var lineContent = line.TrimEnd('\r');
                    if (string.IsNullOrWhiteSpace(lineContent))
                        continue;

                    // Count leading spaces to preserve relative indentation
                    var leadingSpaces = lineContent.Length - lineContent.TrimStart().Length;
                    sb.Append("        "); // 8 spaces base indent
                    sb.Append(' ', leadingSpaces); // Preserve relative indent
                    sb.AppendLine(lineContent.TrimStart());
                }
            }
            catch
            {
                // If parsing fails, skip this rule
            }
        }

        return sb.ToString().TrimEnd();
    }

    private string GenerateCurrencyArrayCode()
    {
        if (_data.Currencies.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var currency in _data.Currencies.Values)
        {
            var code = EscapeString(currency.Code);
            var symbol = EscapeString(currency.Symbol ?? currency.Code);
            var narrow = EscapeString(currency.NarrowSymbol ?? currency.Symbol ?? currency.Code);
            var displayName = EscapeString(currency.DisplayName ?? currency.Code);
            var one = EscapeString(currency.DisplayNameOne ?? currency.DisplayName ?? currency.Code);
            var few = currency.DisplayNameFew != null ? $"\"{EscapeString(currency.DisplayNameFew)}\"" : "null";
            var many = currency.DisplayNameMany != null ? $"\"{EscapeString(currency.DisplayNameMany)}\"" : "null";
            var other = EscapeString(currency.DisplayNameOther ?? currency.DisplayName ?? currency.Code);

            sb.AppendLine($"        new CurrencyData(\"{code}\", \"{symbol}\", \"{narrow}\", \"{displayName}\", \"{one}\", {few}, {many}, \"{other}\"),");
        }

        return sb.ToString().TrimEnd();
    }

    private string GenerateUnitDictCode()
    {
        if (_data.Units.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var unit in _data.Units.Values)
        {
            var id = EscapeString(unit.Id);
            if (unit.Patterns.Count == 0)
            {
                // No patterns - use parameterless overload (displayNames defaults to null)
                sb.AppendLine($"            dict[\"{id}\"] = new UnitData(\"{id}\");");
            }
            else
            {
                sb.AppendLine($"            dict[\"{id}\"] = new UnitData(\"{id}\", new Dictionary<string, string>");
                sb.AppendLine("            {");
                foreach (var (key, pattern) in unit.Patterns)
                {
                    sb.AppendLine($"                {{ \"{EscapeString(key)}\", \"{EscapeString(pattern)}\" }},");
                }
                sb.AppendLine("            });");
            }
        }

        return sb.ToString().TrimEnd();
    }

    private string GenerateDatePatternsCode()
    {
        if (_data.DatePatterns == null)
            return "default";

        var dp = _data.DatePatterns;

        var dateFormats = dp.DateFormats != null
            ? $"new DateFormats(\"{EscapeString(dp.DateFormats.Full ?? "")}\", \"{EscapeString(dp.DateFormats.Long ?? "")}\", \"{EscapeString(dp.DateFormats.Medium ?? "")}\", \"{EscapeString(dp.DateFormats.Short ?? "")}\")"
            : "default";

        var timeFormats = dp.TimeFormats != null
            ? $"new TimeFormats(\"{EscapeString(dp.TimeFormats.Full ?? "")}\", \"{EscapeString(dp.TimeFormats.Long ?? "")}\", \"{EscapeString(dp.TimeFormats.Medium ?? "")}\", \"{EscapeString(dp.TimeFormats.Short ?? "")}\")"
            : "default";

        var dateTimeFormats = dp.DateTimeFormats != null
            ? $"new DateTimeFormats(\"{EscapeString(dp.DateTimeFormats.Full ?? "")}\", \"{EscapeString(dp.DateTimeFormats.Long ?? "")}\", \"{EscapeString(dp.DateTimeFormats.Medium ?? "")}\", \"{EscapeString(dp.DateTimeFormats.Short ?? "")}\")"
            : "default";

        return $"new DatePatternData({dateFormats}, {timeFormats}, {dateTimeFormats})";
    }

    private string GenerateListPatternsCode()
    {
        if (_data.ListPatterns.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var pattern in _data.ListPatterns.Values)
        {
            var type = EscapeString(pattern.Type);
            var start = EscapeString(pattern.Start ?? "{0}, {1}");
            var middle = EscapeString(pattern.Middle ?? "{0}, {1}");
            var end = EscapeString(pattern.End ?? "{0}, {1}");
            var two = EscapeString(pattern.Two ?? "{0}, {1}");

            sb.AppendLine($"        {{ \"{type}\", new ListPatternData(\"{type}\", \"{start}\", \"{middle}\", \"{end}\", \"{two}\") }},");
        }

        return sb.ToString().TrimEnd();
    }

    private static string EscapeString(string? value)
    {
        if (value == null) return "";
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    public static string GetClassName(string locale)
    {
        // Convert locale to valid C# identifier
        // e.g., "en-US" -> "CldrLocaleData_en_US"
        var safeName = locale.Replace("-", "_").Replace(".", "_");
        return $"CldrLocaleData_{safeName}";
    }
}
