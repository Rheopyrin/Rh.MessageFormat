namespace Rh.MessageFormat.CldrGenerator.Plural.Parsing.AST;

public class PluralRule
{
    public PluralRule(string[] locales, IReadOnlyList<Condition> conditions)
    {
        Locales = locales;
        Conditions = conditions;
    }

    public string[] Locales { get; }

    public IReadOnlyList<Condition> Conditions { get; }
}