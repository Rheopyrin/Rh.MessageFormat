using Rh.MessageFormat.CldrGenerator.Plural.Parsing.AST;
using Xunit;

namespace Rh.MessageFormat.CldrGenerator.Tests.Plural.Parsing.AST;

/// <summary>
/// Tests for AST (Abstract Syntax Tree) classes used in CLDR plural rule parsing.
/// </summary>
public class AstTests
{
    #region PluralRule Tests

    [Fact]
    public void PluralRule_Constructor_SetsProperties()
    {
        var locales = new[] { "en", "en-US" };
        var conditions = new List<Condition>();

        var rule = new PluralRule(locales, conditions);

        Assert.Equal(locales, rule.Locales);
        Assert.Same(conditions, rule.Conditions);
    }

    [Fact]
    public void PluralRule_MultipleLocales_PreservesAll()
    {
        var locales = new[] { "en", "en-US", "en-GB", "en-AU" };
        var rule = new PluralRule(locales, []);

        Assert.Equal(4, rule.Locales.Length);
        Assert.Contains("en", rule.Locales);
        Assert.Contains("en-US", rule.Locales);
        Assert.Contains("en-GB", rule.Locales);
        Assert.Contains("en-AU", rule.Locales);
    }

    #endregion

    #region Condition Tests

    [Fact]
    public void Condition_Constructor_SetsProperties()
    {
        var orConditions = new List<OrCondition>();

        var condition = new Condition("one", "n = 1", orConditions);

        Assert.Equal("one", condition.Count);
        Assert.Equal("n = 1", condition.RuleDescription);
        Assert.Same(orConditions, condition.OrConditions);
    }

    [Theory]
    [InlineData("zero")]
    [InlineData("one")]
    [InlineData("two")]
    [InlineData("few")]
    [InlineData("many")]
    [InlineData("other")]
    public void Condition_AllPluralCategories_Supported(string count)
    {
        var condition = new Condition(count, "n = 0", []);

        Assert.Equal(count, condition.Count);
    }

    #endregion

    #region OrCondition Tests

    [Fact]
    public void OrCondition_Constructor_SetsAndConditions()
    {
        var andConditions = new List<Operation>();

        var orCondition = new OrCondition(andConditions);

        Assert.Same(andConditions, orCondition.AndConditions);
    }

    [Fact]
    public void OrCondition_MultipleAndConditions_PreservesAll()
    {
        var op1 = new Operation(
            new VariableOperand(OperandSymbol.AbsoluteValue),
            Relation.Equals,
            [new NumberOperand(1)]);
        var op2 = new Operation(
            new VariableOperand(OperandSymbol.VisibleFractionDigitNumber),
            Relation.Equals,
            [new NumberOperand(0)]);

        var orCondition = new OrCondition([op1, op2]);

        Assert.Equal(2, orCondition.AndConditions.Count);
    }

    #endregion

    #region Operation Tests

    [Fact]
    public void Operation_Constructor_SetsProperties()
    {
        var leftOperand = new VariableOperand(OperandSymbol.AbsoluteValue);
        var rightOperands = new List<IRightOperand> { new NumberOperand(1) };

        var operation = new Operation(leftOperand, Relation.Equals, rightOperands);

        Assert.Same(leftOperand, operation.OperandLeft);
        Assert.Equal(Relation.Equals, operation.Relation);
        Assert.Same(rightOperands, operation.OperandRight);
    }

    [Theory]
    [InlineData(Relation.Equals)]
    [InlineData(Relation.NotEquals)]
    public void Operation_AllRelations_Supported(Relation relation)
    {
        var operation = new Operation(
            new VariableOperand(OperandSymbol.AbsoluteValue),
            relation,
            [new NumberOperand(1)]);

        Assert.Equal(relation, operation.Relation);
    }

    #endregion

    #region VariableOperand Tests

    [Theory]
    [InlineData(OperandSymbol.AbsoluteValue)]
    [InlineData(OperandSymbol.IntegerDigits)]
    [InlineData(OperandSymbol.VisibleFractionDigitNumber)]
    [InlineData(OperandSymbol.VisibleFractionDigitNumberWithoutTrailingZeroes)]
    [InlineData(OperandSymbol.VisibleFractionDigits)]
    [InlineData(OperandSymbol.VisibleFractionDigitsWithoutTrailingZeroes)]
    [InlineData(OperandSymbol.ExponentC)]
    [InlineData(OperandSymbol.ExponentE)]
    public void VariableOperand_AllOperandSymbols_Supported(OperandSymbol symbol)
    {
        var operand = new VariableOperand(symbol);

        Assert.Equal(symbol, operand.Operand);
    }

    #endregion

    #region ModuloOperand Tests

    [Fact]
    public void ModuloOperand_Constructor_SetsProperties()
    {
        var operand = new ModuloOperand(OperandSymbol.AbsoluteValue, 10);

        Assert.Equal(OperandSymbol.AbsoluteValue, operand.Operand);
        Assert.Equal(10, operand.ModValue);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void ModuloOperand_VariousModValues_Supported(int modValue)
    {
        var operand = new ModuloOperand(OperandSymbol.IntegerDigits, modValue);

        Assert.Equal(modValue, operand.ModValue);
    }

    #endregion

    #region NumberOperand Tests

    [Fact]
    public void NumberOperand_Constructor_SetsNumber()
    {
        var operand = new NumberOperand(42);

        Assert.Equal(42, operand.Number);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000000)]
    public void NumberOperand_VariousNumbers_Supported(int number)
    {
        var operand = new NumberOperand(number);

        Assert.Equal(number, operand.Number);
    }

    #endregion

    #region RangeOperand Tests

    [Fact]
    public void RangeOperand_Constructor_SetsStartAndEnd()
    {
        var operand = new RangeOperand(2, 4);

        Assert.Equal(2, operand.Start);
        Assert.Equal(4, operand.End);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(2, 4)]
    [InlineData(11, 14)]
    [InlineData(100, 999)]
    public void RangeOperand_VariousRanges_Supported(int start, int end)
    {
        var operand = new RangeOperand(start, end);

        Assert.Equal(start, operand.Start);
        Assert.Equal(end, operand.End);
    }

    #endregion

    #region OperandSymbol Tests

    [Fact]
    public void OperandSymbol_AllValues_AreDefined()
    {
        var values = Enum.GetValues<OperandSymbol>();

        Assert.Equal(8, values.Length);
        Assert.Contains(OperandSymbol.AbsoluteValue, values);
        Assert.Contains(OperandSymbol.IntegerDigits, values);
        Assert.Contains(OperandSymbol.VisibleFractionDigitNumber, values);
        Assert.Contains(OperandSymbol.VisibleFractionDigitNumberWithoutTrailingZeroes, values);
        Assert.Contains(OperandSymbol.VisibleFractionDigits, values);
        Assert.Contains(OperandSymbol.VisibleFractionDigitsWithoutTrailingZeroes, values);
        Assert.Contains(OperandSymbol.ExponentC, values);
        Assert.Contains(OperandSymbol.ExponentE, values);
    }

    #endregion

    #region Relation Tests

    [Fact]
    public void Relation_AllValues_AreDefined()
    {
        var values = Enum.GetValues<Relation>();

        Assert.Equal(2, values.Length);
        Assert.Contains(Relation.Equals, values);
        Assert.Contains(Relation.NotEquals, values);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void CompleteAst_EnglishOneRule_StructuredCorrectly()
    {
        // English "one" rule: i = 1 and v = 0
        var op1 = new Operation(
            new VariableOperand(OperandSymbol.IntegerDigits),
            Relation.Equals,
            [new NumberOperand(1)]);

        var op2 = new Operation(
            new VariableOperand(OperandSymbol.VisibleFractionDigitNumber),
            Relation.Equals,
            [new NumberOperand(0)]);

        var orCondition = new OrCondition([op1, op2]);
        var condition = new Condition("one", "i = 1 and v = 0", [orCondition]);
        var rule = new PluralRule(["en"], [condition]);

        Assert.Single(rule.Conditions);
        Assert.Equal("one", rule.Conditions[0].Count);
        Assert.Single(rule.Conditions[0].OrConditions);
        Assert.Equal(2, rule.Conditions[0].OrConditions[0].AndConditions.Count);
    }

    [Fact]
    public void CompleteAst_RussianFewRule_StructuredCorrectly()
    {
        // Russian "few" rule: v = 0 and i % 10 = 2..4 and i % 100 != 12..14
        var op1 = new Operation(
            new VariableOperand(OperandSymbol.VisibleFractionDigitNumber),
            Relation.Equals,
            [new NumberOperand(0)]);

        var op2 = new Operation(
            new ModuloOperand(OperandSymbol.IntegerDigits, 10),
            Relation.Equals,
            [new RangeOperand(2, 4)]);

        var op3 = new Operation(
            new ModuloOperand(OperandSymbol.IntegerDigits, 100),
            Relation.NotEquals,
            [new RangeOperand(12, 14)]);

        var orCondition = new OrCondition([op1, op2, op3]);
        var condition = new Condition("few", "v = 0 and i % 10 = 2..4 and i % 100 != 12..14", [orCondition]);
        var rule = new PluralRule(["ru"], [condition]);

        Assert.Single(rule.Conditions);
        Assert.Equal("few", rule.Conditions[0].Count);
        Assert.Equal(3, rule.Conditions[0].OrConditions[0].AndConditions.Count);

        // Verify modulo operand
        var secondOp = rule.Conditions[0].OrConditions[0].AndConditions[1];
        Assert.IsType<ModuloOperand>(secondOp.OperandLeft);
        Assert.Equal(10, ((ModuloOperand)secondOp.OperandLeft).ModValue);
    }

    [Fact]
    public void CompleteAst_ArabicRule_WithMultipleOrConditions()
    {
        // Simplified Arabic rule with multiple or-conditions
        var op1 = new Operation(
            new VariableOperand(OperandSymbol.AbsoluteValue),
            Relation.Equals,
            [new NumberOperand(0)]);

        var op2 = new Operation(
            new VariableOperand(OperandSymbol.AbsoluteValue),
            Relation.Equals,
            [new NumberOperand(1)]);

        var orCondition1 = new OrCondition([op1]);
        var orCondition2 = new OrCondition([op2]);
        var condition = new Condition("one", "n = 0 or n = 1", [orCondition1, orCondition2]);
        var rule = new PluralRule(["ar"], [condition]);

        Assert.Equal(2, rule.Conditions[0].OrConditions.Count);
    }

    #endregion
}
