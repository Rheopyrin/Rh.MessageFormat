using Rh.MessageFormat.CldrGenerator.Plural.Parsing;
using Xunit;

namespace Rh.MessageFormat.CldrGenerator.Tests.Plural.Parsing;

/// <summary>
/// Tests for InvalidCharacterException.
/// </summary>
public class InvalidCharacterExceptionTests
{
    [Fact]
    public void Constructor_SetsMessage()
    {
        var exception = new InvalidCharacterException('x');

        Assert.Contains("x", exception.Message);
        Assert.Contains("Invalid format", exception.Message);
    }

    [Theory]
    [InlineData('x')]
    [InlineData('>')]
    [InlineData('<')]
    [InlineData('$')]
    public void Constructor_IncludesCharacterInMessage(char character)
    {
        var exception = new InvalidCharacterException(character);

        Assert.Contains(character.ToString(), exception.Message);
    }

    [Fact]
    public void Exception_IsFormatException()
    {
        var exception = new InvalidCharacterException('x');

        Assert.IsAssignableFrom<FormatException>(exception);
    }
}
