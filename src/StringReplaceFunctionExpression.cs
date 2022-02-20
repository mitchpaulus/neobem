using System;
using System.Collections.Generic;

namespace src
{
    public class StringReplaceFunctionExpression : FunctionExpression
    {
        public StringReplaceFunctionExpression() : base(new List<Dictionary<string, Expression>>(), new List<string>{ "originalString", "oldString", "newString" }, FileType.Any)
        {
        }

        public override (string, Expression) Evaluate(List<Expression> inputs, string baseDirectory)
        {
            if (inputs.Count != 3) throw new ArgumentException($"replace expects 3 parameters, {inputs.Count} received.");

            if (inputs[0] is not StringExpression originalString)
                throw new ArgumentException(
                    $"The first parameter of replace (original string) is expected to be a string expression, received a {inputs[0].TypeName()} with value {inputs[0].AsErrorString()}");

            if (inputs[1] is not StringExpression oldValue)
                throw new ArgumentException(
                    $"The second parameter of replace (old value) is expected to be a string expression, received a {inputs[1].TypeName()} with value {inputs[1].AsErrorString()}");

            if (inputs[2] is not StringExpression newValue)
                throw new ArgumentException(
                    $"The third parameter of replace (new value) is expected to be a string expression, received a {inputs[2].TypeName()} with value {inputs[2].AsErrorString()}");

            return ("", new StringExpression(originalString.Text.Replace(oldValue.Text, newValue.Text)));
        }
    }
}