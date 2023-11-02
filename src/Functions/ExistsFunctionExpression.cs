using System;
using System.Collections.Generic;
using System.Linq;

namespace src.Functions;

public class ExistsFunctionExpression : FunctionExpression
{
    public ExistsFunctionExpression() : base(new List<Dictionary<string, Expression>>(), new List<string>() { "name" }, FileType.Any)
    {
    }

    public override (string, Expression) Evaluate(List<Expression> inputs, string baseDirectory)
    {
        if (inputs.Count != 1)
        {
            throw new ArgumentException($"Exists requires 1 string parameter. Received {inputs.Count} parameter{(inputs.Count.Pluralize())}.");
        }
        if (inputs[0] is not StringExpression stringExp)
        {
            throw new ArgumentException($"Expected a list as the first parameter to map, received a {inputs[0].TypeName()} of value '{inputs[0].AsErrorString()}'");
        }

        // Check whether string name exists in file
        var exists = Environments.Any(dictionary => dictionary.ContainsKey(stringExp.Text));
        return (string.Empty, new BooleanExpression(exists));
    }
}