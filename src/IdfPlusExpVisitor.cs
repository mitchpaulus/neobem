using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace src
{
    public class IdfPlusExpVisitor : IdfplusBaseVisitor<Expression>
    {
        private readonly List<Dictionary<string, Expression>> _variables;

        public StringBuilder output = new StringBuilder();

        public IdfPlusExpVisitor(List<Dictionary<string, Expression>> variables)
        {
            _variables = variables;
        }
        public IdfPlusExpVisitor()
        {
            _variables = new List<Dictionary<string, Expression>>
            {
                new Dictionary<string, Expression>(MathematicalFunction.FunctionDict)
            };
            _variables[0]["map"] = new MapFunctionExpression();
            _variables[0]["load"] = new LoadFunctionExpression();
            _variables[0]["head"] = new ListHeadFunctionExpression();
            _variables[0]["tail"] = new ListTailFunctionExpression();
            _variables[0]["index"] = new ListIndexFunctionExpression();
            _variables[0]["length"] = new ListLengthFunctionExpression();
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
            throw new InvalidOperationException($"Line {context.Start.Line}: Could not find variable {context.GetText()} in scope.");
        }

        public override Expression VisitLogicExp(IdfplusParser.LogicExpContext context)
        {
            Dictionary<string, Func<bool, bool, bool>> conditional = new Dictionary<string, Func<bool, bool, bool>>()
            {
                {"and", (b, b1) => b && b1},
                {"or", (b, b1) => b || b1},
            };

            var lhs = Visit(context.expression(0));
            var rhs = Visit(context.expression(1));

            if (lhs is BooleanExpression lhsBooleanExp && rhs is BooleanExpression rhsBooleanExp)
            {
                return new BooleanExpression(conditional[context.op.Text](lhsBooleanExp.Value, rhsBooleanExp.Value));
            }

            throw new NotImplementedException($"Line {context.Start.Line}: The logic expression is not defined for types {lhs.GetType()} and {rhs.GetType()}.");
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

        public override Expression VisitMemberAccessExp(IdfplusParser.MemberAccessExpContext context)
        {
            Expression expression = Visit(context.expression());
            if (expression is IdfPlusObjectExpression objectExpression)
            {
                var memberName = context.member_access().IDENTIFIER().GetText();
                if (objectExpression.Members.TryGetValue(memberName, out Expression memberExpression))
                {
                    return memberExpression;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"{memberName} is not a member of the object {context.expression().GetText()}");
                }
            }
            else
            {
                throw new InvalidOperationException($"{context.expression().GetText()} is not an object.");
            }
        }

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
                $"Line {context.Start.Line}: The operation of {op} with types {lhs.GetType()} and {rhs.GetType()} is not defined.");
        }

        public override Expression VisitFunctionExp(IdfplusParser.FunctionExpContext functionExpContext)
        {
            Expression func = Visit(functionExpContext.funcexp);

            if (!(func is FunctionExpression functionExpression))
                throw new InvalidOperationException($"Line {functionExpContext.Start.Line}: Attempt to apply function to non function application.");

            // string functionName = functionExpContext.function_application().IDENTIFIER().GetText();
            // IFunction function = _functions[functionName];
            //
            string text;
            Expression expression;
            try
            {
                var expressions = functionExpContext.expression().Skip(1).Select(Visit).ToList();
                (text, expression) = functionExpression.Evaluate(expressions);
            }
            catch (Exception exception)
            {
                throw new Exception($"Line {functionExpContext.Start.Line}: {exception.Message}");
            }

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

            Dictionary<string, Func<double, double, bool>> numericOperations =
                new Dictionary<string, Func<double, double, bool>>()
                {
                    {"==", (lhs, rhs) => Math.Abs(lhs - rhs) < 0.000000001},
                    {"!=", (lhs, rhs) => Math.Abs(lhs - rhs) > 0.000000001},
                    {"<=", (lhs, rhs) => lhs <= rhs },
                    {"<", (lhs, rhs) => lhs < rhs },
                    {">=", (lhs, rhs) => lhs >= rhs },
                    {">", (lhs, rhs) => lhs > rhs }
                };

            if (lhs is NumericExpression numericLhs && rhs is NumericExpression numericRhs)
                return new BooleanExpression(numericOperations[oper](numericLhs.Value, numericRhs.Value));

            Dictionary<string, Func<string, string, bool>> stringOperations =
                new Dictionary<string, Func<string, string, bool>>()
                {
                    { "==", (s, s1) => s == s1 },
                    { "!=", (s, s1) => s != s1 },
                    { "<=", (s, s1) => (s == s1) || string.Compare(s, s1, StringComparison.Ordinal) <= 0 },
                    { "<", (s, s1) =>  string.Compare(s, s1, StringComparison.Ordinal) < 0 },
                    { ">=", (s, s1) =>  (s == s1 ) || string.Compare(s, s1, StringComparison.Ordinal) >= 0 },
                    { ">", (s, s1) =>  string.Compare(s, s1, StringComparison.Ordinal) > 0 },
                };

            if (lhs is StringExpression lhsStringExpression && rhs is StringExpression rhsStringExpression)
            {
                return new BooleanExpression(stringOperations[oper](lhsStringExpression.Text,
                    rhsStringExpression.Text));
            }

            throw new NotImplementedException($"Boolean expression like '{context.GetText()}' not implemented.");
        }

        public override Expression VisitObjExp(IdfplusParser.ObjExpContext context)
        {
            var props = context.idfplus_object().idfplus_object_property_def();

            IdfPlusObjectExpression objectExpression = new IdfPlusObjectExpression();
            foreach (IdfplusParser.Idfplus_object_property_defContext prop in props)
            {
                var name = prop.IDENTIFIER().GetText();
                Expression expression = Visit(prop.expression());
                objectExpression.Members[name] = expression;
            }

            return objectExpression;
        }

        public override Expression VisitInlineTable(IdfplusParser.InlineTableContext context)
        {
            var names = context.inline_table().inline_table_header().IDENTIFIER().Select(node => node.GetText()).ToList();

            var expressions = context.inline_table()
                .inline_table_data_row()
                .SelectMany(rowContext => rowContext.expression())
                .Select(Visit)
                .ToList();


            if (expressions.Count % names.Count != 0)
            {
                throw new InvalidDataException(
                    "The number of expressions in the inline table need to be divisible by the number of identifiers.");
            }

            var objectExpressions = new List<Expression>();
            for (var rowIndex = 0; rowIndex < expressions.Count / names.Count; rowIndex++)
            {
                IdfPlusObjectExpression expression = new IdfPlusObjectExpression();
                for (var colIndex = 0; colIndex < names.Count; colIndex++)
                {
                    expression.Members[names[colIndex]] = expressions[rowIndex * names.Count + colIndex];
                }
                objectExpressions.Add(expression);
            }
            return new ListExpression(objectExpressions);
        }
    }
}