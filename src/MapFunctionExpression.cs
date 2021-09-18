using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace src
{
    public class MapFunctionExpression : FunctionExpression
    {
        public MapFunctionExpression() : base(new List<Dictionary<string, Expression>>(), new List<string>{ "func", "list" })
        {

        }

        public override (string, Expression) Evaluate(List<Expression> inputs, string baseDirectory)
        {
            if (inputs.Count != 2)
            {
                throw new ArgumentException(
                    $"map requires 2 parameters, a function and list. Received {inputs.Count} parameter{(inputs.Count.Pluralize())}.");
            }
            if (inputs[0] is not FunctionExpression function)
            {
                throw new ArgumentException(
                    $"Expected a function as the first parameter to map, received a {inputs[0].TypeName()} of value '{inputs[0].AsErrorString()}'");
            }

            if (inputs[1] is not ListExpression list)
            {
                throw new ArgumentException(
                    $"Expected a list as the second parameter to map, received a {inputs[1].TypeName()} of value '{inputs[1].AsErrorString()}'");
            }

            List<(string, Expression)> mappedList = list.Expressions
                .Select(expression => function.Evaluate(new List<Expression>() {expression}, baseDirectory)).ToList();

            var returnString = string.Join("", mappedList.Select(tuple => tuple.Item1));
            ListExpression newListExpression = new ListExpression(mappedList.Select(tuple => tuple.Item2).ToList());

            return (returnString, newListExpression);
        }
    }

    public class KeysFunctionExpression : FunctionExpression
    {
        public KeysFunctionExpression() : base(new List<Dictionary<string, Expression>>(), new List<string>{ "structure" })
        {
        }

        public override (string, Expression) Evaluate(List<Expression> inputs, string baseDirectory)
        {
            if (inputs.Count != 1)
                throw new ArgumentException(
                    $"'keys' function expects one structure parameter, received {inputs.Count} parameters.");

            if (!(inputs[0] is IdfPlusObjectExpression structure))
            {
                throw new ArgumentException($"The parameter to 'keys' is expected to be a structure, received a {inputs[0].TypeName()}");
            }

            ListExpression listExpression = new ListExpression(structure.Members.Keys.Select(s => new StringExpression(s)).Cast<Expression>().ToList());
            return ("",  listExpression);
        }
    }

    public class HasFunctionExpression : FunctionExpression
    {
        public HasFunctionExpression() : base(new List<Dictionary<string, Expression>>(), new List<string>{ "structure", "key" })
        {
        }

        public override (string, Expression) Evaluate(List<Expression> inputs, string baseDirectory)
        {
            if (inputs.Count != 2)
                throw new ArgumentException(
                    $"'has' function expects 2 parameters, a structure parameter and string, received {inputs.Count} parameters.");

            if (inputs[0] is not IdfPlusObjectExpression structure)
            {
                throw new ArgumentException($"The first parameter to 'has' is expected to be a structure, received a {inputs[0].TypeName()}");
            }

            if (inputs[1] is not StringExpression stringExpression)
            {
                throw new ArgumentException($"The second parameter to 'has' is expected to be a string, received a {inputs[1].TypeName()}");
            }

            bool hasMember = structure.Members.ContainsKey(stringExpression.Text);
            return ("",  new BooleanExpression(hasMember));
        }
    }
}