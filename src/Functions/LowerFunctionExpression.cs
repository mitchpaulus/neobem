using System;
using System.Collections.Generic;

namespace src.Functions;

public class LowerFunctionExpression : FunctionExpression
{
    private const string Name = "lower";

    public LowerFunctionExpression(NeobemParser.LambdaExpContext lambdaDefContext, List<Dictionary<string, Expression>> environments, FileType fileType, string baseDirectory = null) : base(lambdaDefContext, environments, fileType, baseDirectory)
    {
    }

    public LowerFunctionExpression() : base(new(), new List<string>() { "string" }, FileType.Any)
    {
    }

    public override (string, Expression) Evaluate(List<Expression> inputs, string baseDirectory)
    {
        if (inputs.Count != 1)
        {
            throw new ArgumentException($"{Name} expects 1 input, the input string. Received {inputs.Count} inputs.");
        }

        if (inputs[0] is not StringExpression inputStr)
        {
            throw new ArgumentException($"Input provided to {Name} expected to be a string. Received a {inputs[0].TypeName()}");
        }

        return ("", new StringExpression(inputStr.Text.ToLower()));
    }
}