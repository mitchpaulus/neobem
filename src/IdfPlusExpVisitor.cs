using System;
using System.Collections.Generic;
using System.Linq;

namespace src
{
    public class IdfPlusExpVisitor : IdfplusBaseVisitor<Expression>
    {
        private readonly Dictionary<string, Expression> _variables;

        public IdfPlusExpVisitor(Dictionary<string, Expression> variables) => _variables = variables;
        public IdfPlusExpVisitor() => _variables = new Dictionary<string, Expression>();

        private readonly Dictionary<string, Func<double, double, double>> _numericOperatorMapping =
            new Dictionary<string, Func<double, double, double>>
            {
                {"+", (lhs, rhs) => lhs + rhs},
                {"-", (lhs, rhs) => lhs - rhs},
                {"*", (lhs, rhs) => lhs * rhs},
                {"/", (lhs, rhs) => lhs / rhs},
            };


        public override Expression VisitExponientiate(IdfplusParser.ExponientiateContext context)
        {
            IdfplusParser.ExpressionContext lhs = context.expression(0);

            NumericExpression lhsValue = (NumericExpression)Visit(lhs);
            NumericExpression rhsValue = (NumericExpression)Visit(context.expression(1));

            double exponentValue = Math.Pow(lhsValue.Value, rhsValue.Value);

            return new NumericExpression(exponentValue, exponentValue.ToString());
        }

        public override Expression VisitVariableExp(IdfplusParser.VariableExpContext context) => _variables[context.GetText()];

        public override Expression VisitNumericExp(IdfplusParser.NumericExpContext context)
        {
            string numericText = context.GetText();
            double value = double.Parse(numericText);
            return new NumericExpression(value, numericText);
        }

        public override Expression VisitStringExp(IdfplusParser.StringExpContext context) =>
            // Remove the surrounding quotes
            new StringExpression(context.GetText().Substring(1, context.GetText().Length - 2));

        public override Expression VisitParensExp(IdfplusParser.ParensExpContext context) => Visit(context.expression());

        public override Expression VisitMultDivide(IdfplusParser.MultDivideContext context)
        {
            var op = context.op.Text;

            Expression lhs = Visit(context.expression(0));
            Expression rhs = Visit(context.expression(1));

            var operatorFunction = _numericOperatorMapping[op];

            double newValue = operatorFunction(((NumericExpression)lhs).Value, ((NumericExpression) rhs).Value);
            return new NumericExpression(newValue);
        }

        public override Expression VisitListExp(IdfplusParser.ListExpContext context)
        {
            var expressions = context.list().expression().Select(Visit).ToList();
            return new ListExpression(expressions);
        }

        public override Expression VisitAddSub(IdfplusParser.AddSubContext context)
        {
            var op = context.op.Text;

            Expression lhs = Visit(context.expression(0));
            Expression rhs = Visit(context.expression(1));

            if (lhs is NumericExpression numericLhs && rhs is NumericExpression numericRhs)
            {
                var operatorFunction = _numericOperatorMapping[op];
                double newValue = operatorFunction(numericLhs.Value, numericRhs.Value);
                return new NumericExpression(newValue);
            }

            // Concatenate lists on '+' operation
            if (lhs is ListExpression lhsList && rhs is ListExpression rhsList && op == "+")
            {
                var expressions = new List<Expression>();
                expressions.AddRange(lhsList.Expressions);
                expressions.AddRange(rhsList.Expressions);
                return new ListExpression(expressions);
            }

            // Concatenate Strings on '+' operation
            if (lhs is StringExpression lhsString && rhs is StringExpression rhsString && op == "+")
                return new StringExpression(lhsString.Text + rhsString.Text);

            throw new NotImplementedException(
                $"The operation of {op} with types {lhs.GetType()} and {rhs.GetType()} is not defined.");
        }
    }
}