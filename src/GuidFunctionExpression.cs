using System;
using System.Collections.Generic;

namespace src
{
    public class GuidFunctionExpression : FunctionExpression
    {
        public GuidFunctionExpression() : base(new List<Dictionary<string, Expression>>(), new List<string>())
        {

        }

        public override (string, Expression) Evaluate(List<Expression> inputs, string baseDirectory) => ("", new StringExpression(Guid.NewGuid().ToString()));
    }
}