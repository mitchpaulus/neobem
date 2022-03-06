using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Antlr4.Runtime.Atn;
using src.Functions;

namespace src
{
    public class IdfPlusExpVisitor : NeobemParserBaseVisitor<Expression>
    {
        private readonly List<Dictionary<string, Expression>> _variables;
        private readonly FileType _fileType;
        private readonly string _baseDirectory;

        public StringBuilder output = new StringBuilder();

        public IdfPlusExpVisitor(List<Dictionary<string, Expression>> variables, FileType fileType, string baseDirectory)
        {
            _variables = variables;
            _fileType = fileType;
            _baseDirectory = baseDirectory;
        }

        public IdfPlusExpVisitor(string baseDirectory, FileType fileType)
        {
            _baseDirectory = baseDirectory;
            _fileType = fileType;
            _variables = new List<Dictionary<string, Expression>>
            {
                new Dictionary<string, Expression>(MathematicalFunction.FunctionDict)
            };
            _variables[0]["map"] = new MapFunctionExpression();
            _variables[0]["filter"] = new FilterFunctionExpression();
            _variables[0]["fold"] = new FoldFunctionExpression();
            _variables[0]["keys"] = new KeysFunctionExpression();
            _variables[0]["has"] = new HasFunctionExpression();
            _variables[0]["load"] = new LoadFunctionExpression(baseDirectory);
            _variables[0]["head"] = new ListHeadFunctionExpression();
            _variables[0]["tail"] = new ListTailFunctionExpression();
            _variables[0]["init"] = new ListInitFunctionExpression();
            _variables[0]["last"] = new ListLastFunctionExpression();

            _variables[0]["index"] = new ListIndexFunctionExpression();
            _variables[0]["length"] = new ListLengthFunctionExpression();

            _variables[0]["join"] = new StringJoinFunctionExpression();
            _variables[0]["replace"] = new StringReplaceFunctionExpression();

            _variables[0]["type"] = new TypeFunctionExpression();
            _variables[0]["guid"] = new GuidFunctionExpression();

            _variables[0]["mod"] = new ModFunctionExpression();
        }

        private readonly Dictionary<string, Func<double, double, double>> _numericOperatorMapping =
            new Dictionary<string, Func<double, double, double>>
            {
                { "+", (lhs, rhs) => lhs + rhs },
                { "-", (lhs, rhs) => lhs - rhs },
                { "*", (lhs, rhs) => lhs * rhs },
                { "/", (lhs, rhs) => lhs / rhs },
            };


        public override Expression VisitExponientiate(NeobemParser.ExponientiateContext context)
        {
            NeobemParser.ExpressionContext lhs = context.expression(0);

            NumericExpression lhsValue = (NumericExpression)Visit(lhs);
            NumericExpression rhsValue = (NumericExpression)Visit(context.expression(1));

            double exponentValue = Math.Pow(lhsValue.Value, rhsValue.Value);

            return new NumericExpression(exponentValue, exponentValue.ToString());
        }

        public override Expression VisitVariableExp(NeobemParser.VariableExpContext context)
        {
            foreach (var scope in _variables)
            {
                var found = scope.TryGetValue(context.GetText(), out Expression value);
                if (found) return value;
            }

            // Try to give more helpful error message if it appears like the variable should be coming from an import.
            if (context.GetText().Contains("@"))
            {
                throw new InvalidOperationException(
                    $"Line {context.Start.Line}: Could not find variable '{context.GetText()}' in scope. Possible reasons include missing import or missing export statements within imported file.");
            }

            throw new InvalidOperationException(
                $"Line {context.Start.Line}: Could not find variable '{context.GetText()}' in scope.");
        }

        public override Expression VisitLogicExp(NeobemParser.LogicExpContext context)
        {
            Dictionary<string, Func<bool, bool, bool>> conditional = new Dictionary<string, Func<bool, bool, bool>>()
            {
                { "and", (b, b1) => b && b1 },
                { "or", (b, b1) => b || b1 },
            };

            var lhs = Visit(context.expression(0));
            var rhs = Visit(context.expression(1));

            if (lhs is BooleanExpression lhsBooleanExp && rhs is BooleanExpression rhsBooleanExp)
            {
                return new BooleanExpression(conditional[context.op.Text](lhsBooleanExp.Value, rhsBooleanExp.Value));
            }

            throw new NotImplementedException(
                $"Line {context.Start.Line}: The logic expression is not defined for types {lhs.TypeName()} and {rhs.TypeName()}");
        }

        public override Expression VisitNumericExp(NeobemParser.NumericExpContext context)
        {
            string numericText = context.GetText();
            double value = double.Parse(numericText);
            return new NumericExpression(value, numericText);
        }

        public override Expression VisitBclExp(NeobemParser.BclExpContext context)
        {
            // There are lots of things that could go wrong here, catch em.
            try
            {
                Bcl bcl = new();
                string uuid = context.UUID().GetText();
                string jsonResponse = bcl.GetByUUID(uuid);
                Expression expression = bcl.ParseUUIDResponse(jsonResponse);
                return expression;
            }
            catch (Exception)
            {
                throw new Exception($"Line {context.Start.Line}: There was an issue parsing the BCL expression: '{context.GetText()}'");
            }
        }

        public override Expression VisitStringExp(NeobemParser.StringExpContext context) =>
            // Remove the surrounding quotes
            new StringExpression(context.GetText().Substring(1, context.GetText().Length - 2));

        public override Expression VisitParensExp(NeobemParser.ParensExpContext context) => Visit(context.expression());

        public override Expression VisitMemberAccessExp(NeobemParser.MemberAccessExpContext context)
        {
            Expression expression = Visit(context.expression(0));
            if (expression is IdfPlusObjectExpression objectExpression)
            {
                Expression memberAccessExpression = Visit(context.expression(1));

                if (memberAccessExpression is not StringExpression stringMemberExpression)
                {
                    throw new InvalidOperationException(
                        $"Line {context.Start.Line}: Attempted a member access with a non string expression. The original expression was {context.expression(1).GetText()}.");
                }

                var memberName = stringMemberExpression.Text;
                if (objectExpression.Members.TryGetValue(memberName, out Expression memberExpression))
                {
                    return memberExpression;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Line {context.Start.Line}: '{memberName}' is not a member of the dictionary {context.expression(0).GetText()}. Possible members include:\n\n{string.Join("", objectExpression.Members.Keys.Select(s => $"'{s}'\n"))}");
                }
            }
            else
            {
                throw new InvalidOperationException(
                    $"Line {context.Start.Line}: {context.expression(0).GetText()} is not an object.");
            }
        }

        public override Expression VisitMultDivide(NeobemParser.MultDivideContext context)
        {
            var op = context.op.Text;

            Expression lhs = Visit(context.expression(0));
            Expression rhs = Visit(context.expression(1));

            var operatorFunction = _numericOperatorMapping[op];

            double newValue = operatorFunction(((NumericExpression)lhs).Value, ((NumericExpression)rhs).Value);
            return new NumericExpression(newValue);
        }

        public override Expression VisitListExp(NeobemParser.ListExpContext context)
        {
            var expressions = context.list().expression().Select(Visit).ToList();
            return new ListExpression(expressions);
        }

        public override Expression VisitAddSub(NeobemParser.AddSubContext context)
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

            // Merge/Update dictionary on '+' operation
            if (lhs is IdfPlusObjectExpression lhsDictionary && rhs is IdfPlusObjectExpression rhsDictionary)
                return lhsDictionary.Add(rhsDictionary);

            // If you have one string, one numeric, cast numeric to string and concatenate.
            if (lhs is StringExpression lhsString1 && rhs is NumericExpression rhsNumeric1 && op == "+")
                return new StringExpression(lhsString1.Text + rhsNumeric1.Text);

            if (lhs is NumericExpression lhsNumeric1 && rhs is StringExpression rhsString1 && op == "+")
                return new StringExpression(lhsNumeric1.Text + rhsString1.Text);

            throw new NotImplementedException(
                $"Line {context.Start.Line}: The operation of {op} with types {lhs.TypeName()} and {rhs.TypeName()} is not defined.");
        }

        public override Expression VisitFunctionExp(NeobemParser.FunctionExpContext functionExpContext)
        {
            Expression func = Visit(functionExpContext.funcexp);

            if (!(func is FunctionExpression functionExpression))
                throw new InvalidOperationException(
                    $"Line {functionExpContext.Start.Line}: Attempt to apply function to non function expression.");

            // string functionName = functionExpContext.function_application().IDENTIFIER().GetText();
            // IFunction function = _functions[functionName];
            //
            string text;
            Expression expression;
            try
            {
                NeobemParser.Function_parameterContext[] functionParameters = functionExpContext.function_application().function_parameter();

                // Allow this situation to create a partial function application. TODO: Verify length isn't 0 or something
                if (functionParameters.Length == functionExpression.Parameters.Count - 1)
                {
                    List<string> remainingParameters = functionExpression.Parameters.Take(1).ToList();

                    var partiallyAppliedExpressions = functionExpContext.function_application().function_parameter().Select(Visit).ToList();
                    Dictionary<int, Expression> expressionDictionary = new();
                    for (int i = 1; i < functionExpression.Parameters.Count ; i++)
                    {
                        expressionDictionary[i] = partiallyAppliedExpressions[i - 1];
                    }

                    return new PartialApplicationFunctionExpression(functionExpression, _variables, remainingParameters, expressionDictionary);
                }

                var expressions = functionExpContext.function_application().function_parameter().Select(Visit).ToList();
                (text, expression) = functionExpression.Evaluate(expressions, _baseDirectory);
            }
            catch (Exception exception)
            {
                throw new Exception($"Line {functionExpContext.Start.Line}: {exception.Message}");
            }

            output.Append(text);

            return expression;
        }

        public override Expression VisitLambdaExp(NeobemParser.LambdaExpContext context)
        {
            return new FunctionExpression(context, _variables, _fileType, _baseDirectory);
        }

        public override Expression VisitMapPipeFilterExp(NeobemParser.MapPipeFilterExpContext context)
        {
            if (context.functional_operator().MAP_OPERATOR() != null) return EvaluateMapOperator(context);
            if (context.functional_operator().PIPE_OPERATOR() != null) return VisitPipeExp(context);
            if (context.functional_operator().FILTER_OPERATOR() != null) return EvaluateFilterOperator(context);
            throw new NotImplementedException($"The operator {context.functional_operator().GetText()} has not been implemented.");
        }

        public override Expression VisitRangeExp(NeobemParser.RangeExpContext context)
        {
            var lhsExpression = Visit(context.expression(0));
            if (lhsExpression is not NumericExpression lhsNumeric)
                throw new ArgumentException(
                    $"Line {context.Start.Line}: The left hand expression in the range operator is not a numeric. Received a {lhsExpression.TypeName()}.");
            var rhsExpression = Visit(context.expression(1));
            if (rhsExpression is not NumericExpression rhsNumeric)
                throw new ArgumentException(
                    $"Line {context.Start.Line}: The right hand expression in the range operator is not a numeric. Received a {rhsExpression.TypeName()}.");

            var initialInteger = Convert.ToInt32(lhsNumeric.Value);
            var finalInteger =   Convert.ToInt32(rhsNumeric.Value);

            if (initialInteger == finalInteger) return new ListExpression(new List<Expression>());

            List<Expression> expressions = new();
            if (initialInteger < finalInteger)
                for (int i = initialInteger; i <= finalInteger; i++)
                    expressions.Add(new NumericExpression(i));
            else
                for (int i = finalInteger; i >= initialInteger; i--)
                    expressions.Add(new NumericExpression(i));
            return new ListExpression(expressions);
        }

        private Expression EvaluateFilterOperator(NeobemParser.MapPipeFilterExpContext context)
        {
            var lhsExpression = Visit(context.expression(0));
            var rhsExpression = Visit(context.expression(1));

            // Do preliminary checks on the input.
            if (lhsExpression is not ListExpression listExpression)
                throw new ArgumentException(
                    $"Line {context.Start.Line}: The left hand side to the filter operator is not a list. Received a {lhsExpression.TypeName()}");

            if (rhsExpression is not FunctionExpression functionExpression) throw new ArgumentException(
                    $"Line {context.Start.Line}: The right hand side of the filter operator is not a function. Received a {rhsExpression.TypeName()}");

            if (functionExpression.Parameters.Count != 1)
                throw new ArgumentException(
                    $"Line {context.Start.Line}: The function for the filter operator should have one parameter. The function supplied has {functionExpression.Parameters.Count} parameter{functionExpression.Parameters.Count.Pluralize()}, named {string.Join(", ", functionExpression.Parameters)}.");

            // Delegate to existing filter function.
            FilterFunctionExpression filterFunctionExpression = new();

            string text;
            Expression expression;
            try
            {
                (text, expression) =
                    filterFunctionExpression.Evaluate(new List<Expression>() { listExpression, functionExpression },
                        _baseDirectory);
            }
            catch (Exception exception)
            {
                throw new Exception($"Line {context.Start.Line}: {exception.Message}");
            }

            output.Append(text);
            return expression;
        }
        private Expression EvaluateMapOperator(NeobemParser.MapPipeFilterExpContext context)
        {
            var lhsExpression = Visit(context.expression(0));
            var rhsExpression = Visit(context.expression(1));

            // Do preliminary checks on the input.
            if (lhsExpression is not ListExpression listExpression)
                throw new ArgumentException(
                    $"Line {context.Start.Line}: The left hand side to the map operator is not a list. Received a {lhsExpression.TypeName()}");

            if (rhsExpression is not FunctionExpression functionExpression) throw new ArgumentException(
                    $"Line {context.Start.Line}: The right hand side of the map operator is not a function. Received a {rhsExpression.TypeName()}");

            if (functionExpression.Parameters.Count != 1)
                throw new ArgumentException(
                    $"Line {context.Start.Line}: The function for the map operator should have one parameter. The function supplied has {functionExpression.Parameters.Count} parameter{functionExpression.Parameters.Count.Pluralize()}, named {string.Join(", ", functionExpression.Parameters)}.");

            // Delegate to existing map function.
            MapFunctionExpression mapFunctionExpression = new();

            string text;
            Expression expression;
            try
            {
                (text, expression) =
                    mapFunctionExpression.Evaluate(new List<Expression>() { listExpression, functionExpression },
                        _baseDirectory);
            }
            catch (Exception exception)
            {
                throw new Exception($"Line {context.Start.Line}: {exception.Message}");
            }

            output.Append(text);
            return expression;
        }

        private Expression VisitPipeExp(NeobemParser.MapPipeFilterExpContext context)
        {
            var inputExpression = Visit(context.expression(0));
            var afterPipeExpression = Visit(context.expression(1));

            if (afterPipeExpression is not FunctionExpression functionExpression)
                throw new InvalidOperationException(
                    $"Line {context.Start.Line}: The function to the right of a pipe operation may only be a function expression.");

            if (functionExpression.Parameters.Count != 1)
                throw new InvalidOperationException(
                    $"Line {context.Start.Line}: The function to the right of a pipe operation should take 1 parameter. Got a function with {functionExpression.Parameters.Count} parameter{functionExpression.Parameters.Count.Pluralize()}.");

            List<Expression> expressionList = new() { inputExpression };

            string text;
            Expression expression;
            try
            {
                (text, expression) = functionExpression.Evaluate(expressionList, _baseDirectory);
            }
            catch (Exception exception)
            {
                throw new Exception($"Line {context.Start.Line}: {exception.Message}");
            }

            output.Append(text);
            return expression;
        }

        public override Expression VisitIfExp(NeobemParser.IfExpContext context)
        {
            var expressionOutput = Visit(context.if_exp().expression(0));

            if (!(expressionOutput is BooleanExpression booleanExpression))
            {
                var expressionText = context.if_exp().expression(0).GetText();
                throw new NotSupportedException($"Line {context.Start.Line}: '{expressionText}' is not a boolean expression");
            }

            return Visit(booleanExpression.Value ? context.if_exp().expression(1) : context.if_exp().expression(2));
        }

        public override Expression VisitBooleanExp(NeobemParser.BooleanExpContext context)
        {
            var lhs = Visit(context.expression(0));
            var rhs = Visit(context.expression(1));

            string oper = context.boolean_exp_operator().GetText();

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

            if (lhs is BooleanExpression lhsBooleanExpression && rhs is BooleanExpression rhsBooleanExpression)
            {
                return oper switch
                {
                    "==" => new BooleanExpression(lhsBooleanExpression.Value == rhsBooleanExpression.Value),
                    "!=" => new BooleanExpression(lhsBooleanExpression.Value != rhsBooleanExpression.Value),
                    _ => throw new NotImplementedException(
                        $"Line {context.Start.Line}: Boolean expression like '{context.GetText()}' ({lhs.TypeName()} {oper} {rhs.TypeName()}) not implemented.")
                };
            }

            if (lhs is ListExpression lhsListExpression && rhs is ListExpression rhsListExpression)
            {
                if (lhsListExpression.Expressions.Count == 0 && rhsListExpression.Expressions.Count == 0)
                    return new BooleanExpression(true);
                if (lhsListExpression.Expressions.Count != rhsListExpression.Expressions.Count)
                    return new BooleanExpression(false);
                throw new NotImplementedException($"Line {context.Start.Line}: Boolean expression like '{context.GetText()}' ({lhs.TypeName()} {oper} {rhs.TypeName()}) not implemented for non empty lists.");
            }

            throw new NotImplementedException($"Line {context.Start.Line}: Boolean expression like '{context.GetText()}' ({lhs.TypeName()} {oper} {rhs.TypeName()}) not implemented.");
        }

        public override Expression VisitObjExp(NeobemParser.ObjExpContext context)
        {
            var props = context.idfplus_object().idfplus_object_property_def();

            IdfPlusObjectExpression objectExpression = new IdfPlusObjectExpression();
            foreach (NeobemParser.Idfplus_object_property_defContext prop in props)
            {
                var stringExpression = Visit(prop.expression(0));

                if (!(stringExpression is StringExpression nameExpression))
                {
                    throw new InvalidOperationException(
                        $"Line {context.Start.Line}: Tried to set member '{prop.expression(0).GetText()}', which is not a string expression.");
                }

                Expression expression = Visit(prop.expression(1));
                objectExpression.Members[nameExpression.Text] = expression;
            }

            return objectExpression;
        }

        public override Expression VisitInlineTable(NeobemParser.InlineTableContext context)
        {
            var names = context.inline_table().inline_table_header().STRING().Select(node => node.GetText().Substring(1,node.GetText().Length - 2)).ToList();

            var expressions = context.inline_table()
                .inline_table_data_row()
                .SelectMany(rowContext => rowContext.expression())
                .Select(Visit)
                .ToList();


            if (expressions.Count % names.Count != 0)
            {
                throw new InvalidDataException(
                    $"Line {context.Start.Line}: The number of expressions in the inline table need to be divisible by the number of identifiers.");
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

        public override Expression VisitLet_binding(NeobemParser.Let_bindingContext context)
        {
            var boundExpressions = context.expression().Select(Visit);
            var names = context.IDENTIFIER().Select(node => node.GetText());

            var dictionary = boundExpressions.Zip(names).ToDictionary(tuple => tuple.Second, tuple => tuple.First);

            List<Dictionary<string, Expression>> variableContext = new();
            foreach (var dict in _variables) variableContext.Add(dict);
            variableContext.Insert(0, dictionary);

            IdfPlusExpVisitor newVisitor = new IdfPlusExpVisitor(variableContext, _fileType, _baseDirectory);
            Expression evaluatedExpression = newVisitor.Visit(context.let_expression());
            output.Append(newVisitor.output.ToString());
            return evaluatedExpression;
        }

        public override Expression VisitBooleanLiteralTrueExp(NeobemParser.BooleanLiteralTrueExpContext context) =>
            new BooleanExpression(true);

        public override Expression VisitBooleanLiteralFalseExp(NeobemParser.BooleanLiteralFalseExpContext context) =>
            new BooleanExpression(false);
    }
}
