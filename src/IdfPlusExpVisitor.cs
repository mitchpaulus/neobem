using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace src
{
    public class IdfPlusExpVisitor : IdfplusBaseVisitor<Expression>
    {
        private readonly List<Dictionary<string, Expression>> _variables;

        private readonly Dictionary<string, IFunction> _functions;

        public StringBuilder output = new StringBuilder();

        public IdfPlusExpVisitor(List<Dictionary<string, Expression>> variables, Dictionary<string, IFunction> functions)
        {
            _variables = variables;
            _functions = MathematicalFunction.FunctionDict;
        }
        public IdfPlusExpVisitor()
        {
            _variables = new List<Dictionary<string, Expression>>() { new Dictionary<string, Expression>() };
            _functions = MathematicalFunction.FunctionDict;
        }

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

        public override Expression VisitVariableExp(IdfplusParser.VariableExpContext context)
        {

            foreach (var scope in _variables)
            {
                var found = scope.TryGetValue(context.GetText(), out Expression value);
                if (found) return value;
            }
            throw new InvalidOperationException($"Could not find variable {context.GetText()} in scope.");
        }

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

        public override Expression VisitFunctionExp(IdfplusParser.FunctionExpContext functionExpContext)
        {
            Expression func = Visit(functionExpContext.funcexp);

            if (!(func is FunctionExpression functionExpression))
                throw new InvalidOperationException("Attempt to apply function to non function application.");

            // string functionName = functionExpContext.function_application().IDENTIFIER().GetText();
            // IFunction function = _functions[functionName];
            //
            var expressions = functionExpContext.expression().Skip(1).Select(Visit).ToList();
            (string text, Expression expression) = functionExpression.Evaluate(expressions);

            output.Append(text);

            return expression;
        }

        public override Expression VisitLambdaExp(IdfplusParser.LambdaExpContext context)
        {
            return new FunctionExpression(context, _variables);
        }

        public override Expression VisitIfExp(IdfplusParser.IfExpContext context)
        {
            var expressionOutput = Visit(context.if_exp().expression(0));

            if (!(expressionOutput is BooleanExpression booleanExpression))
            {
                var expressionText = context.if_exp().expression(0).GetText();
                throw new NotSupportedException($"'{expressionText}' is not a boolean expression");
            }

            return Visit(booleanExpression.Value ? context.if_exp().expression(1) : context.if_exp().expression(2));
        }

        public override Expression VisitBooleanExp(IdfplusParser.BooleanExpContext context)
        {
            var lhs = Visit(context.expression(0));
            var rhs = Visit(context.expression(1));

            string oper = context.op.Text;

            switch (oper)
            {
                case "==":

                    if (lhs is NumericExpression numericLhs && rhs is NumericExpression numericRhs)
                    {
                        bool areEqual = Math.Abs(numericLhs.Value - numericRhs.Value) < 0.000000001;
                        return new BooleanExpression(areEqual);
                    }
                    break;
            }

            throw new NotImplementedException($"{context.GetText()} not implemented.");
        }
    }
}