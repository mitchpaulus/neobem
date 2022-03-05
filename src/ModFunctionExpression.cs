using System;
using System.Collections.Generic;

namespace src;

public class ModFunctionExpression : FunctionExpression
{
    public ModFunctionExpression() : base(new List<Dictionary<string, Expression>>(), new List<string> { "number1", "number2"  }, FileType.Any)
    {

    }

    public override (string, Expression) Evaluate(List<Expression> inputs, string baseDirectory)
    {
        if (inputs.Count != 2)
            throw new ArgumentException($"The mod function expects 2 numeric inputs, received {inputs.Count}.");

        if (inputs[0] is not NumericExpression numericExpression1)
            throw new ArgumentException(
                $"The first parameter to the mod function is expected to be a numeric expression, received a {inputs[0].TypeName()}");
        if (inputs[1] is not NumericExpression numericExpression2)
            throw new ArgumentException(
                $"The second parameter to the mod function is expected to be a numeric expression, received a {inputs[1].TypeName()}");

        return ("", new NumericExpression(numericExpression1.Value % numericExpression2.Value));
    }
}