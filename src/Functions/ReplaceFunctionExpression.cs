using System;
using System.Collections.Generic;

namespace src.Functions;

public class ReplaceFunctionExpression : FunctionExpression
{
    private const string Name = "replace";

    public ReplaceFunctionExpression(NeobemParser.LambdaExpContext lambdaDefContext, List<Dictionary<string, Expression>> environments, FileType fileType, string baseDirectory = null) : base(lambdaDefContext, environments, fileType, baseDirectory)
    {
    }

    public ReplaceFunctionExpression() : base(new(), new List<string>() { "string", "toReplaceString", "replacmentString" }, FileType.Any)
    {
    }

    public override (string, Expression) Evaluate(List<Expression> inputs, string baseDirectory)
    {
        if (inputs.Count != 3)
        {
            throw new ArgumentException($"{Name} expects 3 inputs, the input string, the string to replace, and the string to replace with. Received {inputs.Count} inputs.");
        }

        if (inputs[0] is not StringExpression inputStr)
        {
            throw new ArgumentException($"First input provided to {Name} expected to be a string. Received a {inputs[0].TypeName()}");
        }

        if (inputs[1] is not StringExpression replaceStr)
        {
            throw new ArgumentException($"Second input provided to {Name} expected to be a string. Received a {inputs[1].TypeName()}");
        }

        if (inputs[2] is not StringExpression replaceWithStr)
        {
            throw new ArgumentException($"Third input provided to {Name} expected to be a string. Received a {inputs[2].TypeName()}");
        }

        string replaced = inputStr.Text.Replace(replaceStr.Text, replaceWithStr.Text);
        return ("", new StringExpression(replaced));
    }
}
