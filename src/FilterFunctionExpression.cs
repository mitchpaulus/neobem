using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace src
{
    public class FilterFunctionExpression : FunctionExpression
    {
        public FilterFunctionExpression() : base(new List<Dictionary<string, Expression>>(), new List<string>{ "func", "list" })
        {

        }

        public override (string, Expression) Evaluate(List<Expression> inputs)
        {
            if (inputs.Count != 2)
                throw new ArgumentException($"filter function expects 2 parameters, function and list, received {inputs.Count}.");
            if (inputs[0] is FunctionExpression function && inputs[1] is ListExpression list)
            {
                List<(string, Expression)> mappedList = list.Expressions
                    .Select(expression => function.Evaluate(new List<Expression>() {expression})).ToList();

                var returnString = string.Join("", mappedList.Select(tuple => tuple.Item1));

                if (mappedList.Select((tuple, i) => !(tuple.Item2 is BooleanExpression)).Any(b => b))
                {
                    var distinctTypes = mappedList.Select(tuple => tuple.Item2.GetType()).Distinct();
                    throw new ArgumentException(
                        $"filter function expects function to return a boolean expression. Received back {string.Join(", ", distinctTypes)}.");
                }

                List<Expression> filteredList = mappedList
                    .Zip(list.Expressions, (tuple, expression) => (BooleanExpression: tuple.Item2, OriginalExp: expression))
                    .Where(tuple => ((BooleanExpression) tuple.BooleanExpression).Value)
                    .Select(tuple => tuple.OriginalExp)
                    .ToList();

                ListExpression newListExpression = new ListExpression(filteredList);

                return (returnString, newListExpression);
            }
            throw new ArgumentException(
                    $"filter expects a function and list, received a {inputs[0].GetType()} and {inputs[1].GetType()}");
        }
    }
}
