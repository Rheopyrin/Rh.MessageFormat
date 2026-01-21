namespace Rh.MessageFormat.CldrGenerator.Plural.Parsing;

public class InvalidCharacterException : FormatException
{
    public InvalidCharacterException(char character) : base($"Invalid format, do not recognise character '{character}'")
    {
    }
}