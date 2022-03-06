using System;
using System.Collections.Generic;
using System.Text;

namespace src.Functions;

public class FoldFunctionExpression : FunctionExpression
{
    public FoldFunctionExpression() : base(new List<Dictionary<string, Expression>>(), new List<string>{ "list", "func", "initial" }, FileType.Any)
    {

    }

    public override (string, Expression) Evaluate(List<Expression> inputs, string baseDirectory)
    {
        if (inputs.Count != 3)
        {
            throw new ArgumentException($"map requires 2 parameters, a function and list. Received {inputs.Count} parameter{(inputs.Count.Pluralize())}.");
        }
        if (inputs[0] is not ListExpression list)
        {
            throw new ArgumentException($"Expected a list as the first parameter to map, received a {inputs[0].TypeName()} of value '{inputs[0].AsErrorString()}'");
        }

        if (inputs[1] is not FunctionExpression function)
        {
            throw new ArgumentException($"Expected a function as the second parameter to map, received a {inputs[1].TypeName()} of value '{inputs[1].AsErrorString()}'");
        }

        // The third input can be any arbitrary expression.
        if (function.Parameters.Count != 2)
        {
            throw new ArgumentException(
                $"The function parameter for fold is expected to have 2 parameters. The function passed has {function.Parameters.Count} parameter{function.Parameters.Count.Pluralize()}");
        }

        Expression accumulator = inputs[2];
        StringBuilder builder = new();
        foreach (Expression expression in list.Expressions)
        {
            // Each evaluation could potentially produce output. We'll capture it all.
            (string outputString, Expression newExpression) =
                function.Evaluate(new List<Expression> { accumulator, expression }, baseDirectory);

            builder.Append(outputString);
            accumulator = newExpression;
        }

        return (builder.ToString(), accumulator);
    }
}