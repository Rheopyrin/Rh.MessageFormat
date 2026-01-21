using Rh.MessageFormat.CldrGenerator.Plural.Parsing;
using Rh.MessageFormat.CldrGenerator.Plural.Parsing.AST;
using Xunit;

namespace Rh.MessageFormat.CldrGenerator.Tests.Plural.Parsing;

/// <summary>
/// Tests for RuleParser - CLDR plural rule parsing.
/// </summary>
public class RuleParserTests
{
    #region Basic Operand Tests

    [Fact]
    public void Parse_SimpleEquality_ReturnsCorrectCondition()
    {
        // CLDR rule: "n = 1" (singular in English)
        var parser = new RuleParser("n = 1");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        Assert.Single(conditions[0].AndConditions);

        var operation = conditions[0].AndConditions[0];
        Assert.IsType<VariableOperand>(operation.OperandLeft);
        Assert.Equal(OperandSymbol.AbsoluteValue, ((VariableOperand)operation.OperandLeft).Operand);
        Assert.Equal(Relation.Equals, operation.Relation);
        Assert.Single(operation.OperandRight);
        Assert.IsType<NumberOperand>(operation.OperandRight[0]);
        Assert.Equal(1, ((NumberOperand)operation.OperandRight[0]).Number);
    }

    [Theory]
    [InlineData("n", OperandSymbol.AbsoluteValue)]
    [InlineData("i", OperandSymbol.IntegerDigits)]
    [InlineData("v", OperandSymbol.VisibleFractionDigitNumber)]
    [InlineData("w", OperandSymbol.VisibleFractionDigitNumberWithoutTrailingZeroes)]
    [InlineData("f", OperandSymbol.VisibleFractionDigits)]
    [InlineData("t", OperandSymbol.VisibleFractionDigitsWithoutTrailingZeroes)]
    [InlineData("c", OperandSymbol.ExponentC)]
    [InlineData("e", OperandSymbol.ExponentE)]
    public void Parse_AllOperandSymbols_ParsedCorrectly(string symbol, OperandSymbol expected)
    {
        var parser = new RuleParser($"{symbol} = 0");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        var operation = conditions[0].AndConditions[0];
        Assert.IsType<VariableOperand>(operation.OperandLeft);
        Assert.Equal(expected, ((VariableOperand)operation.OperandLeft).Operand);
    }

    #endregion

    #region Relation Tests

    [Fact]
    public void Parse_NotEquals_ReturnsCorrectRelation()
    {
        var parser = new RuleParser("n != 0");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        var operation = conditions[0].AndConditions[0];
        Assert.Equal(Relation.NotEquals, operation.Relation);
    }

    [Fact]
    public void Parse_Equals_ReturnsCorrectRelation()
    {
        var parser = new RuleParser("n = 0");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        var operation = conditions[0].AndConditions[0];
        Assert.Equal(Relation.Equals, operation.Relation);
    }

    #endregion

    #region Range Tests

    [Fact]
    public void Parse_Range_ReturnsRangeOperand()
    {
        // CLDR rule: "n = 2..4" (e.g., used in Slavic languages for "few")
        var parser = new RuleParser("n = 2..4");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        var operation = conditions[0].AndConditions[0];
        Assert.Single(operation.OperandRight);
        Assert.IsType<RangeOperand>(operation.OperandRight[0]);

        var range = (RangeOperand)operation.OperandRight[0];
        Assert.Equal(2, range.Start);
        Assert.Equal(4, range.End);
    }

    [Fact]
    public void Parse_MultipleRanges_ReturnsAllRanges()
    {
        // CLDR rule: "n = 0..1, 11..19" (e.g., Lithuanian)
        var parser = new RuleParser("n = 0..1, 11..19");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        var operation = conditions[0].AndConditions[0];
        Assert.Equal(2, operation.OperandRight.Count);

        Assert.IsType<RangeOperand>(operation.OperandRight[0]);
        Assert.IsType<RangeOperand>(operation.OperandRight[1]);

        var range1 = (RangeOperand)operation.OperandRight[0];
        var range2 = (RangeOperand)operation.OperandRight[1];

        Assert.Equal(0, range1.Start);
        Assert.Equal(1, range1.End);
        Assert.Equal(11, range2.Start);
        Assert.Equal(19, range2.End);
    }

    #endregion

    #region Multiple Values Tests

    [Fact]
    public void Parse_MultipleNumbers_ReturnsAllNumbers()
    {
        // CLDR rule: "n = 0, 1, 2"
        var parser = new RuleParser("n = 0, 1, 2");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        var operation = conditions[0].AndConditions[0];
        Assert.Equal(3, operation.OperandRight.Count);

        Assert.All(operation.OperandRight, r => Assert.IsType<NumberOperand>(r));
        Assert.Equal(0, ((NumberOperand)operation.OperandRight[0]).Number);
        Assert.Equal(1, ((NumberOperand)operation.OperandRight[1]).Number);
        Assert.Equal(2, ((NumberOperand)operation.OperandRight[2]).Number);
    }

    [Fact]
    public void Parse_MixedNumbersAndRanges_ReturnsAll()
    {
        // CLDR rule: "n = 1, 3..5, 7"
        var parser = new RuleParser("n = 1, 3..5, 7");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        var operation = conditions[0].AndConditions[0];
        Assert.Equal(3, operation.OperandRight.Count);

        Assert.IsType<NumberOperand>(operation.OperandRight[0]);
        Assert.IsType<RangeOperand>(operation.OperandRight[1]);
        Assert.IsType<NumberOperand>(operation.OperandRight[2]);

        Assert.Equal(1, ((NumberOperand)operation.OperandRight[0]).Number);
        var range = (RangeOperand)operation.OperandRight[1];
        Assert.Equal(3, range.Start);
        Assert.Equal(5, range.End);
        Assert.Equal(7, ((NumberOperand)operation.OperandRight[2]).Number);
    }

    #endregion

    #region Modulo Tests

    [Fact]
    public void Parse_Modulo_ReturnsModuloOperand()
    {
        // CLDR rule: "n % 10 = 1" (e.g., Russian singular ends in 1 but not 11)
        var parser = new RuleParser("n % 10 = 1");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        var operation = conditions[0].AndConditions[0];
        Assert.IsType<ModuloOperand>(operation.OperandLeft);

        var modulo = (ModuloOperand)operation.OperandLeft;
        Assert.Equal(OperandSymbol.AbsoluteValue, modulo.Operand);
        Assert.Equal(10, modulo.ModValue);
    }

    [Fact]
    public void Parse_ModuloWithIntegerDigits_ReturnsCorrectOperand()
    {
        // CLDR rule: "i % 100 = 11..14"
        var parser = new RuleParser("i % 100 = 11..14");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        var operation = conditions[0].AndConditions[0];
        Assert.IsType<ModuloOperand>(operation.OperandLeft);

        var modulo = (ModuloOperand)operation.OperandLeft;
        Assert.Equal(OperandSymbol.IntegerDigits, modulo.Operand);
        Assert.Equal(100, modulo.ModValue);
    }

    #endregion

    #region And Conditions Tests

    [Fact]
    public void Parse_AndCondition_ReturnsBothOperations()
    {
        // CLDR rule: "n % 10 = 1 and n % 100 != 11"
        var parser = new RuleParser("n % 10 = 1 and n % 100 != 11");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        Assert.Equal(2, conditions[0].AndConditions.Count);

        var op1 = conditions[0].AndConditions[0];
        var op2 = conditions[0].AndConditions[1];

        Assert.IsType<ModuloOperand>(op1.OperandLeft);
        Assert.Equal(10, ((ModuloOperand)op1.OperandLeft).ModValue);
        Assert.Equal(Relation.Equals, op1.Relation);

        Assert.IsType<ModuloOperand>(op2.OperandLeft);
        Assert.Equal(100, ((ModuloOperand)op2.OperandLeft).ModValue);
        Assert.Equal(Relation.NotEquals, op2.Relation);
    }

    [Fact]
    public void Parse_MultipleAndConditions_ReturnsAll()
    {
        // CLDR rule: "i = 1 and v = 0 and f = 0"
        var parser = new RuleParser("i = 1 and v = 0 and f = 0");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        Assert.Equal(3, conditions[0].AndConditions.Count);
    }

    #endregion

    #region Or Conditions Tests

    [Fact]
    public void Parse_OrCondition_ReturnsTwoOrConditions()
    {
        // CLDR rule: "n = 1 or n = 2"
        var parser = new RuleParser("n = 1 or n = 2");
        var conditions = parser.ParseRuleContent();

        Assert.Equal(2, conditions.Count);

        Assert.Single(conditions[0].AndConditions);
        Assert.Single(conditions[1].AndConditions);

        var num1 = ((NumberOperand)conditions[0].AndConditions[0].OperandRight[0]).Number;
        var num2 = ((NumberOperand)conditions[1].AndConditions[0].OperandRight[0]).Number;

        Assert.Equal(1, num1);
        Assert.Equal(2, num2);
    }

    [Fact]
    public void Parse_ComplexOrAndCondition_ReturnsCorrectStructure()
    {
        // CLDR rule: "n = 1 and v = 0 or n = 2" (common in Slavic languages)
        var parser = new RuleParser("n = 1 and v = 0 or n = 2");
        var conditions = parser.ParseRuleContent();

        Assert.Equal(2, conditions.Count);

        // First or-condition has two and-conditions
        Assert.Equal(2, conditions[0].AndConditions.Count);

        // Second or-condition has one and-condition
        Assert.Single(conditions[1].AndConditions);
    }

    #endregion

    #region Samples Handling Tests

    [Fact]
    public void Parse_RuleWithSamples_IgnoresSamples()
    {
        // CLDR rules often include samples after @integer or @decimal
        var parser = new RuleParser("n = 1 @integer 1 @decimal 1.0, 1.00, 1.000");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        Assert.Single(conditions[0].AndConditions);
    }

    [Fact]
    public void Parse_RuleWithOnlyIntegerSamples_IgnoresSamples()
    {
        var parser = new RuleParser("i = 0 or n = 1 @integer 0, 1");
        var conditions = parser.ParseRuleContent();

        Assert.Equal(2, conditions.Count);
    }

    #endregion

    #region Whitespace Handling Tests

    [Fact]
    public void Parse_ExtraWhitespace_ParsesCorrectly()
    {
        var parser = new RuleParser("  n   =   1  ");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        Assert.Single(conditions[0].AndConditions);
    }

    [Fact]
    public void Parse_NoWhitespace_ParsesCorrectly()
    {
        var parser = new RuleParser("n=1");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
    }

    #endregion

    #region Real CLDR Rules Tests

    [Fact]
    public void Parse_EnglishOne_ParsesCorrectly()
    {
        // English "one" rule: i = 1 and v = 0
        var parser = new RuleParser("i = 1 and v = 0");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        Assert.Equal(2, conditions[0].AndConditions.Count);

        var op1 = conditions[0].AndConditions[0];
        var op2 = conditions[0].AndConditions[1];

        Assert.Equal(OperandSymbol.IntegerDigits, ((VariableOperand)op1.OperandLeft).Operand);
        Assert.Equal(OperandSymbol.VisibleFractionDigitNumber, ((VariableOperand)op2.OperandLeft).Operand);
    }

    [Fact]
    public void Parse_RussianOne_ParsesCorrectly()
    {
        // Russian "one" rule: v = 0 and i % 10 = 1 and i % 100 != 11
        var parser = new RuleParser("v = 0 and i % 10 = 1 and i % 100 != 11");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        Assert.Equal(3, conditions[0].AndConditions.Count);
    }

    [Fact]
    public void Parse_RussianFew_ParsesCorrectly()
    {
        // Russian "few" rule: v = 0 and i % 10 = 2..4 and i % 100 != 12..14
        var parser = new RuleParser("v = 0 and i % 10 = 2..4 and i % 100 != 12..14");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        Assert.Equal(3, conditions[0].AndConditions.Count);

        var rangeOp = conditions[0].AndConditions[1];
        Assert.IsType<RangeOperand>(rangeOp.OperandRight[0]);
    }

    [Fact]
    public void Parse_ArabicZero_ParsesCorrectly()
    {
        // Arabic "zero" rule: n = 0
        var parser = new RuleParser("n = 0");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        Assert.Equal(0, ((NumberOperand)conditions[0].AndConditions[0].OperandRight[0]).Number);
    }

    [Fact]
    public void Parse_ArabicTwo_ParsesCorrectly()
    {
        // Arabic "two" rule: n = 2
        var parser = new RuleParser("n = 2");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        Assert.Equal(2, ((NumberOperand)conditions[0].AndConditions[0].OperandRight[0]).Number);
    }

    [Fact]
    public void Parse_ArabicFew_ParsesCorrectly()
    {
        // Arabic "few" rule: n % 100 = 3..10
        var parser = new RuleParser("n % 100 = 3..10");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        var op = conditions[0].AndConditions[0];
        Assert.IsType<ModuloOperand>(op.OperandLeft);
        Assert.Equal(100, ((ModuloOperand)op.OperandLeft).ModValue);
    }

    [Fact]
    public void Parse_PolishFew_ParsesCorrectly()
    {
        // Polish "few" rule (simplified): v = 0 and i % 10 = 2..4 and i % 100 != 12..14
        var parser = new RuleParser("v = 0 and i % 10 = 2..4 and i % 100 != 12..14");
        var conditions = parser.ParseRuleContent();

        Assert.Single(conditions);
        Assert.Equal(3, conditions[0].AndConditions.Count);
    }

    #endregion

    #region Error Cases Tests

    [Fact]
    public void Parse_InvalidOperand_ThrowsException()
    {
        var parser = new RuleParser("x = 1");

        Assert.Throws<InvalidCharacterException>(() => parser.ParseRuleContent());
    }

    [Fact]
    public void Parse_InvalidRelation_ThrowsException()
    {
        var parser = new RuleParser("n > 1");

        Assert.Throws<InvalidCharacterException>(() => parser.ParseRuleContent());
    }

    [Fact]
    public void Parse_SingleDot_ThrowsException()
    {
        // Should be ".." for range, not single "."
        var parser = new RuleParser("n = 1.5");

        Assert.Throws<InvalidCharacterException>(() => parser.ParseRuleContent());
    }

    #endregion

    #region Empty Input Tests

    [Fact]
    public void Parse_EmptyString_ReturnsEmptyList()
    {
        var parser = new RuleParser("");
        var conditions = parser.ParseRuleContent();

        Assert.Empty(conditions);
    }

    [Fact]
    public void Parse_OnlyWhitespace_ThrowsException()
    {
        // The parser doesn't handle whitespace-only input gracefully
        // It attempts to parse an operand from whitespace
        var parser = new RuleParser("   ");

        Assert.Throws<InvalidCharacterException>(() => parser.ParseRuleContent());
    }

    [Fact]
    public void Parse_OnlySamples_ReturnsEmptyList()
    {
        var parser = new RuleParser("@integer 0~15, 100, 1000");
        var conditions = parser.ParseRuleContent();

        Assert.Empty(conditions);
    }

    #endregion
}
