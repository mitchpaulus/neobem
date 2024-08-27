using System;
using System.Collections.Generic;

namespace src.Functions;

public class ContainsFunctionExpression : FunctionExpression
{
    private const string Name = "contains";

    public ContainsFunctionExpression(NeobemParser.LambdaExpContext lambdaDefContext, List<Dictionary<string, Expression>> environments, FileType fileType, string baseDirectory = null) : base(lambdaDefContext, environments, fileType, baseDirectory)
    {
    }

    public ContainsFunctionExpression() : base(new(), new List<string>() { "string", "search_string" }, FileType.Any)
    {
    }

    public override (string, Expression) Evaluate(List<Expression> inputs, string baseDirectory)
    {
        if (inputs.Count != 2)
        {
            throw new ArgumentException($"{Name} expects 2 inputs, the input string, and the string to search for. Received {inputs.Count} inputs.");
        }

        if (inputs[0] is not StringExpression inputStr)
        {
            throw new ArgumentException($"First input provided to {Name} expected to be a string. Received a {inputs[0].TypeName()}");
        }

        if (inputs[1] is not StringExpression searchStr)
        {
            throw new ArgumentException($"Second input provided to {Name} expected to be a string. Received a {inputs[1].TypeName()}");
        }

        bool contains = inputStr.Text.Contains(searchStr.Text);
        return ("", new BooleanExpression(contains));
    }
}
