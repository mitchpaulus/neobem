using System.Collections.Generic;
using System.Linq;

namespace src
{
    public class TypeFunctionExpression : FunctionExpression
    {
        public TypeFunctionExpression() : base(new List<Dictionary<string, Expression>>(), new List<string> { "expression" })
        {
        }

        public override (string, Expression) Evaluate(List<Expression> inputs, string baseDirectory)
        {
            StringExpression stringExpression = new(string.Join(",", inputs.Select(expression => expression.TypeName())));
            return ("", stringExpression);
        }
    }
}