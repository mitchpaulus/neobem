using System;
using System.Collections.Generic;

namespace src.Functions;

public class HandleFunctionExpression : FunctionExpression
{
    public HandleFunctionExpression() : base(new List<Dictionary<string, Expression>>(), new List<string>(), FileType.Any)
    {
    }

    public override (string, Expression) Evaluate(List<Expression> inputs, string baseDirectory)
    {
        if (inputs.Count != 0)
        {
            throw new ArgumentException($"Handle takes 0 parameters. Received {inputs.Count} parameter{inputs.Count.Pluralize()}.");
        }

        // Check whether string name exists in file

        string uuid = Guid.NewGuid().ToString("B");
        return (string.Empty, new StringExpression(uuid));
    }
}