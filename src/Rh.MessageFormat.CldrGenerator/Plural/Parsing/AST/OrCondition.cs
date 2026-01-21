using System.Collections.Generic;

namespace Rh.MessageFormat.CldrGenerator.Plural.Parsing.AST;

public class OrCondition
{
    public OrCondition(IReadOnlyList<Operation> andConditions)
    {
        AndConditions = andConditions;
    }

    public IReadOnlyList<Operation> AndConditions { get; }
}