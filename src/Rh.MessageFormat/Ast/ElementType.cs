namespace Rh.MessageFormat.Ast;

/// <summary>
/// The type of message element.
/// </summary>
internal enum ElementType : byte
{
    Literal = 0,
    Argument = 1,
    Number = 2,
    Date = 3,
    Time = 4,
    DateTime = 5,
    Plural = 6,
    Select = 7,
    SelectOrdinal = 8,
    Custom = 9,
    Tag = 10,
    List = 11,
    RelativeTime = 12,
    DateRange = 13
}
