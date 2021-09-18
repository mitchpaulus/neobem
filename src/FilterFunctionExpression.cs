using System;
using System.Collections.Generic;
using System.Linq;

namespace src
{
    public class FilterFunctionExpression : FunctionExpression
    {
        public FilterFunctionExpression() : base(new List<Dictionary<string, Expression>>(), new List<string>{ "list", "func" })
        {

        }

        public override (string, Expression) Evaluate(List<Expression> inputs, string baseDirectory)
        {
            if (inputs.Count != 2)
                throw new ArgumentException($"filter function expects 2 parameters, function and list, received {inputs.Count}.");

            if (inputs[0] is not ListExpression list || inputs[1] is not FunctionExpression function)
                throw new ArgumentException(
                    $"filter expects a list and function, received a {inputs[0].TypeName()} and {inputs[1].TypeName()}");

            List<(string, Expression)> mappedList = list.Expressions
                .Select(expression => function.Evaluate(new List<Expression> {expression}, baseDirectory)).ToList();

            var returnString = string.Join("", mappedList.Select(tuple => tuple.Item1));

            if (mappedList.Select((tuple, i) => tuple.Item2 is not BooleanExpression).Any(b => b))
            {
                IEnumerable<string> distinctTypes = mappedList.Select(tuple => tuple.Item2.TypeName()).Distinct();
                throw new ArgumentException(
                    $"filter function expects function to return a boolean expression. Received back types including: {string.Join(", ", distinctTypes)}.");
            }

            List<Expression> filteredList = mappedList
                .Zip(list.Expressions, (tuple, expression) => (BooleanExpression: tuple.Item2, OriginalExp: expression))
                .Where(tuple => ((BooleanExpression) tuple.BooleanExpression).Value)
                .Select(tuple => tuple.OriginalExp)
                .ToList();

            ListExpression newListExpression = new ListExpression(filteredList);

            return (returnString, newListExpression);
        }
    }
}
