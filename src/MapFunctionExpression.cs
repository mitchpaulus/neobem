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
            FunctionExpression function = inputs[0] as FunctionExpression;
            ListExpression list = inputs[1] as ListExpression;

            List<(string, Expression)> mappedList = list.Expressions
                .Select(expression => function.Evaluate(new List<Expression>() {expression}, baseDirectory)).ToList();

            var returnString = string.Join("", mappedList.Select(tuple => tuple.Item1));
            ListExpression newListExpression = new ListExpression(mappedList.Select(tuple => tuple.Item2).ToList());

            return (returnString, newListExpression);
        }
    }
}