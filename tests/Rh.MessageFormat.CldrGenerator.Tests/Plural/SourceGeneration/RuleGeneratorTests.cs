using System.Text;
using Rh.MessageFormat.CldrGenerator.Plural.Parsing;
using Rh.MessageFormat.CldrGenerator.Plural.Parsing.AST;
using Rh.MessageFormat.CldrGenerator.Plural.SourceGeneration;
using Xunit;

namespace Rh.MessageFormat.CldrGenerator.Tests.Plural.SourceGeneration;

/// <summary>
/// Tests for RuleGenerator - C# code generation from parsed CLDR rules.
/// </summary>
public class RuleGeneratorTests
{
    #region Helper Methods

    private static PluralRule CreateRule(string ruleText, string count = "one")
    {
        var parser = new RuleParser(ruleText);
        var orConditions = parser.ParseRuleContent();
        var condition = new Condition(count, ruleText, orConditions);
        return new PluralRule(["en"], [condition]);
    }

    private static string GenerateCode(PluralRule rule, int indent = 0)
    {
        var generator = new RuleGenerator(rule);
        var builder = new StringBuilder();
        generator.WriteTo(builder, indent);
        return builder.ToString();
    }

    #endregion

    #region Basic Code Generation Tests

    [Fact]
    public void Generate_SimpleEquality_GeneratesCorrectCode()
    {
        var rule = CreateRule("n = 1");
        var code = GenerateCode(rule);

        Assert.Contains("if (", code);
        Assert.Contains("context.N == 1", code);
        Assert.Contains("return \"one\"", code);
        Assert.Contains("return \"other\"", code);
    }

    [Fact]
    public void Generate_NotEquals_GeneratesCorrectCode()
    {
        var rule = CreateRule("n != 0");
        var code = GenerateCode(rule);

        Assert.Contains("context.N != 0", code);
    }

    [Fact]
    public void Generate_ReturnsOther_AtEnd()
    {
        var rule = CreateRule("n = 1");
        var code = GenerateCode(rule);

        Assert.EndsWith("return \"other\";\n", code);
    }

    #endregion

    #region Operand Mapping Tests

    [Theory]
    [InlineData("n = 1", "context.N")]
    [InlineData("i = 1", "context.I")]
    [InlineData("v = 0", "context.V")]
    [InlineData("w = 0", "context.W")]
    [InlineData("f = 0", "context.F")]
    [InlineData("t = 0", "context.T")]
    [InlineData("c = 0", "context.C")]
    [InlineData("e = 0", "context.E")]
    public void Generate_OperandSymbols_MappedCorrectly(string ruleText, string expectedVariable)
    {
        var rule = CreateRule(ruleText);
        var code = GenerateCode(rule);

        Assert.Contains(expectedVariable, code);
    }

    #endregion

    #region Modulo Tests

    [Fact]
    public void Generate_Modulo_GeneratesModuloExpression()
    {
        var rule = CreateRule("n % 10 = 1");
        var code = GenerateCode(rule);

        Assert.Contains("context.N % 10 == 1", code);
    }

    [Fact]
    public void Generate_ModuloWithRange_GeneratesCorrectCode()
    {
        var rule = CreateRule("i % 100 = 11..14");
        var code = GenerateCode(rule);

        Assert.Contains("context.I % 100 >= 11", code);
        Assert.Contains("context.I % 100 <= 14", code);
    }

    #endregion

    #region Range Tests

    [Fact]
    public void Generate_Range_GeneratesRangeCheck()
    {
        var rule = CreateRule("n = 2..4");
        var code = GenerateCode(rule);

        Assert.Contains("context.N >= 2", code);
        Assert.Contains("context.N <= 4", code);
    }

    [Fact]
    public void Generate_RangeWithNotEquals_GeneratesInvertedRangeCheck()
    {
        var rule = CreateRule("n != 12..14");
        var code = GenerateCode(rule);

        Assert.Contains("context.N < 12", code);
        Assert.Contains("context.N > 14", code);
    }

    #endregion

    #region Multiple Values Tests

    [Fact]
    public void Generate_MultipleNumbers_GeneratesOrExpressions()
    {
        var rule = CreateRule("n = 0, 1");
        var code = GenerateCode(rule);

        Assert.Contains("context.N == 0", code);
        Assert.Contains("context.N == 1", code);
        Assert.Contains(" || ", code);
    }

    [Fact]
    public void Generate_MultipleNumbersNotEquals_GeneratesAndExpressions()
    {
        var rule = CreateRule("n != 11, 12");
        var code = GenerateCode(rule);

        Assert.Contains("context.N != 11", code);
        Assert.Contains("context.N != 12", code);
        Assert.Contains(" && ", code);
    }

    #endregion

    #region And Conditions Tests

    [Fact]
    public void Generate_AndCondition_GeneratesAndOperator()
    {
        var rule = CreateRule("i = 1 and v = 0");
        var code = GenerateCode(rule);

        Assert.Contains("context.I == 1", code);
        Assert.Contains("context.V == 0", code);
        Assert.Contains(") && (", code);
    }

    [Fact]
    public void Generate_MultipleAndConditions_GeneratesMultipleAnds()
    {
        var rule = CreateRule("v = 0 and i % 10 = 1 and i % 100 != 11");
        var code = GenerateCode(rule);

        Assert.Contains("context.V == 0", code);
        Assert.Contains("context.I % 10 == 1", code);
        Assert.Contains("context.I % 100 != 11", code);
    }

    #endregion

    #region Or Conditions Tests

    [Fact]
    public void Generate_OrCondition_GeneratesOrOperator()
    {
        var rule = CreateRule("n = 1 or n = 2");
        var code = GenerateCode(rule);

        Assert.Contains("context.N == 1", code);
        Assert.Contains("context.N == 2", code);
        Assert.Contains(" || ", code);
    }

    #endregion

    #region Real CLDR Rules Tests

    [Fact]
    public void Generate_EnglishOne_GeneratesCorrectCode()
    {
        // English "one" rule: i = 1 and v = 0
        var rule = CreateRule("i = 1 and v = 0");
        var code = GenerateCode(rule);

        Assert.Contains("if ((context.I == 1) && (context.V == 0))", code);
        Assert.Contains("return \"one\"", code);
    }

    [Fact]
    public void Generate_RussianOne_GeneratesCorrectCode()
    {
        // Russian "one" rule: v = 0 and i % 10 = 1 and i % 100 != 11
        var rule = CreateRule("v = 0 and i % 10 = 1 and i % 100 != 11");
        var code = GenerateCode(rule);

        Assert.Contains("context.V == 0", code);
        Assert.Contains("context.I % 10 == 1", code);
        Assert.Contains("context.I % 100 != 11", code);
    }

    [Fact]
    public void Generate_RussianFew_GeneratesCorrectCode()
    {
        // Russian "few" rule: v = 0 and i % 10 = 2..4 and i % 100 != 12..14
        var parser = new RuleParser("v = 0 and i % 10 = 2..4 and i % 100 != 12..14");
        var orConditions = parser.ParseRuleContent();
        var condition = new Condition("few", "v = 0 and i % 10 = 2..4 and i % 100 != 12..14", orConditions);
        var rule = new PluralRule(["ru"], [condition]);

        var code = GenerateCode(rule);

        Assert.Contains("context.V == 0", code);
        Assert.Contains("context.I % 10 >= 2", code);
        Assert.Contains("context.I % 10 <= 4", code);
        Assert.Contains("context.I % 100 < 12", code);
        Assert.Contains("context.I % 100 > 14", code);
        Assert.Contains("return \"few\"", code);
    }

    [Fact]
    public void Generate_ArabicFew_GeneratesCorrectCode()
    {
        // Arabic "few" rule: n % 100 = 3..10
        var parser = new RuleParser("n % 100 = 3..10");
        var orConditions = parser.ParseRuleContent();
        var condition = new Condition("few", "n % 100 = 3..10", orConditions);
        var rule = new PluralRule(["ar"], [condition]);

        var code = GenerateCode(rule);

        Assert.Contains("context.N % 100 >= 3", code);
        Assert.Contains("context.N % 100 <= 10", code);
        Assert.Contains("return \"few\"", code);
    }

    #endregion

    #region Multiple Conditions Tests

    [Fact]
    public void Generate_MultipleConditions_GeneratesAllReturns()
    {
        // Create rule with multiple conditions (one, few)
        var parser1 = new RuleParser("n = 1");
        var orConditions1 = parser1.ParseRuleContent();
        var condition1 = new Condition("one", "n = 1", orConditions1);

        var parser2 = new RuleParser("n = 2..4");
        var orConditions2 = parser2.ParseRuleContent();
        var condition2 = new Condition("few", "n = 2..4", orConditions2);

        var rule = new PluralRule(["test"], [condition1, condition2]);

        var code = GenerateCode(rule);

        Assert.Contains("return \"one\"", code);
        Assert.Contains("return \"few\"", code);
        Assert.Contains("return \"other\"", code);
    }

    #endregion

    #region Indentation Tests

    [Fact]
    public void Generate_WithIndent_AppliesIndentation()
    {
        var rule = CreateRule("n = 1");
        var code = GenerateCode(rule, 8);

        // Code should start with indentation
        Assert.StartsWith("        if", code);
    }

    [Fact]
    public void Generate_ReturnStatements_AreIndented()
    {
        var rule = CreateRule("n = 1");
        var code = GenerateCode(rule, 4);

        var lines = code.Split('\n');
        var returnLine = lines.First(l => l.Contains("return \"one\""));

        // Return inside if should have extra indentation
        Assert.StartsWith("        ", returnLine);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Generate_ZeroValue_GeneratesCorrectComparison()
    {
        var rule = CreateRule("n = 0");
        var code = GenerateCode(rule);

        Assert.Contains("context.N == 0", code);
    }

    [Fact]
    public void Generate_LargeNumber_GeneratesCorrectComparison()
    {
        var rule = CreateRule("n = 1000000");
        var code = GenerateCode(rule);

        Assert.Contains("context.N == 1000000", code);
    }

    [Fact]
    public void Generate_LargeModulo_GeneratesCorrectExpression()
    {
        var rule = CreateRule("n % 1000 = 100");
        var code = GenerateCode(rule);

        Assert.Contains("context.N % 1000 == 100", code);
    }

    #endregion
}
