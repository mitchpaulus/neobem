using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace src
{
    public interface IFunction
    {
        Expression Evaluate(List<Expression> inputs);
    }

    public class MathematicalFunction : FunctionExpression
    {
        public static List<MathematicalFunction> List = new List<MathematicalFunction>();
        public static Dictionary<string, Expression> FunctionDict = new Dictionary<string, Expression>();

        public readonly string Name;
        public readonly Func<List<double>, double> Function;

        public static MathematicalFunction Abs = new MathematicalFunction("abs", inputs => Math.Abs(inputs[0]));
        public static MathematicalFunction Acos = new MathematicalFunction("acos", inputs => Math.Acos(inputs[0]));
        public static MathematicalFunction Asin = new MathematicalFunction("asin", inputs => Math.Asin(inputs[0]));
        public static MathematicalFunction Atan2 = new MathematicalFunction("atan2", inputs => Math.Atan2(inputs[0], inputs[1]));
        public static MathematicalFunction Ceiling = new MathematicalFunction("ceiling", inputs => Math.Ceiling(inputs[0]));
        public static MathematicalFunction Cos = new MathematicalFunction("cos", inputs => Math.Cos(inputs[0]));
        public static MathematicalFunction Floor = new MathematicalFunction("floor", inputs => Math.Floor(inputs[0]));
        public static MathematicalFunction Log = new MathematicalFunction("ln", inputs => Math.Log(inputs[0]));
        public static MathematicalFunction Log10 = new MathematicalFunction("log10", inputs => Math.Log10(inputs[0]));
        public static MathematicalFunction Log2 = new MathematicalFunction("log2", inputs => Math.Log2(inputs[0]));
        public static MathematicalFunction Sin = new MathematicalFunction("sin", inputs => Math.Sin(inputs[0]));
        public static MathematicalFunction Sqrt = new MathematicalFunction("sqrt", inputs => Math.Sqrt(inputs[0]));

        public static MathematicalFunction Tan = new MathematicalFunction("tan", inputs => Math.Tan(inputs[0]));


        public MathematicalFunction(string name, Func<List<double>, double> function) : base(new List<Dictionary<string, Expression>>(), new List<string>(), null)
        {
            Name = name;
            Function = function;

            List.Add(this);
            FunctionDict[name] = this;
        }

        public override (string, Expression) Evaluate(List<Expression> inputs, string baseDirectory)
        {
            var value = Function(inputs.Cast<NumericExpression>().Select(expression => expression.Value).ToList());
            return ("", new NumericExpression(value));
        }
    }

    public class FunctionExpression : Expression
    {
        private readonly NeobemParser.LambdaExpContext _context;
        private readonly List<Dictionary<string, Expression>> _environments;


        private readonly List<string> _parameters;

        public override string AsString() => "";

        public FunctionExpression(NeobemParser.LambdaExpContext lambdaDefContext, List<Dictionary<string, Expression>> environments, string baseDirectory = null)
        {
            _context = lambdaDefContext;
            _environments = environments;
            _parameters = lambdaDefContext.lambda_def().IDENTIFIER().Select(node => node.GetText()).ToList();
        }

        public FunctionExpression(List<Dictionary<string, Expression>> environments, List<string> parameters, string baseDirectory = null)
        {
            _environments = environments;
            _parameters = parameters;
        }

        public virtual (string, Expression) Evaluate(List<Expression> inputs, string baseDirectory)
        {
            Dictionary<string, Expression> locals = new Dictionary<string, Expression>();

            if (inputs.Count != _parameters.Count) throw new InvalidOperationException( $"Expected {_parameters.Count} parameters, received {inputs.Count}");

            for (int i = 0; i < inputs.Count; i++) locals[_parameters[i]] = inputs[i];

            var updatedEnvironments = new List<Dictionary<string, Expression>>(_environments);
            updatedEnvironments.Insert(0, locals);

            IdfPlusExpVisitor visitor = new IdfPlusExpVisitor(updatedEnvironments, baseDirectory);

            StringBuilder builder = new StringBuilder();

            ObjectVariableReplacer replacer = new ObjectVariableReplacer(baseDirectory);

            if (_context.lambda_def().expression() != null)
            {
                Expression expression = visitor.Visit(_context.lambda_def().expression());
                return (visitor.output.ToString(), expression);
            }
            else
            {
                foreach (NeobemParser.Function_statementContext item in _context.lambda_def().function_statement())
                {
                    switch (item)
                    {
                        case NeobemParser.FunctionIdfCommentContext commentContext:
                            builder.Append(commentContext.GetText());
                            break;
                        case NeobemParser.FunctionObjectDeclarationContext objectDeclarationContext:
                            var replacedObject = replacer.Replace(objectDeclarationContext.GetText(), updatedEnvironments);
                            builder.AppendLine(replacedObject);
                            break;
                        case NeobemParser.FunctionVariableDeclarationContext variableDeclarationContext:
                            var expressionResult = visitor.Visit(variableDeclarationContext.variable_declaration().expression());
                            var identifier = variableDeclarationContext.variable_declaration().IDENTIFIER().GetText();
                            locals[identifier] = expressionResult;
                            break;
                        case NeobemParser.ReturnStatementContext returnStatementContext:
                            Expression returnExpression = visitor.Visit(returnStatementContext.return_statement().expression());
                            return (builder.ToString(), returnExpression);
                        default:
                            throw new NotImplementedException();
                    }
                }

                return (builder.ToString(), new StringExpression(""));
            }
        }
    }

    public class StringExpression : Expression
    {
        public string Text;

        public StringExpression(string text)
        {
            Text = text;
        }

        public override string ToString() => Text;
        public override string AsString() => Text;
    }

    public class ListExpression : Expression
    {
        public readonly List<Expression> Expressions;

        public ListExpression(List<Expression> expressions)
        {
            Expressions = expressions;
        }

        public override string AsString() => string.Join(",", Expressions.Select(expression => expression.AsString()));

    }

    public class NumericExpression : Expression
    {
        public double Value;
        public string Text;

        public NumericExpression(double value, string text)
        {
            Value = value;
            Text = text;
        }

        public NumericExpression(double value)
        {
            Value = value;
            Text = value.ToString();
        }

        public override string ToString() => Text;
        public override string AsString() => Value.ToString();
    }

    public class IdfPlusObjectExpression : Expression
    {
        public Dictionary<string, Expression> Members = new Dictionary<string, Expression>();
        public override string AsString() => string.Join(",", Members.Keys.Select(s => Members[s].AsString()));

        public IdfPlusObjectExpression Add(IdfPlusObjectExpression right) => StructureAdd.Add(this, right);
    }

    public static class StructureAdd
    {
        public static IdfPlusObjectExpression Add(IdfPlusObjectExpression left, IdfPlusObjectExpression right)
        {
            IdfPlusObjectExpression newStructure = new IdfPlusObjectExpression();

            foreach (var member in left.Members.Keys)
            {
                if (!right.Members.ContainsKey(member))
                {
                    newStructure.Members[member] = left.Members[member];
                }
                else
                {
                    if (left.Members[member] is IdfPlusObjectExpression subLeftExpression &&
                        right.Members[member] is IdfPlusObjectExpression subRightExpression)
                    {
                        newStructure.Members[member] = Add(subLeftExpression, subRightExpression);
                    }
                    else
                    {
                        // If both items have the same key, the right side overrides.
                        newStructure.Members[member] = right.Members[member];
                    }
                }
            }

            foreach (var member in right.Members.Keys)
            {
                if (!left.Members.ContainsKey(member))
                {
                    newStructure.Members[member] = right.Members[member];
                }
            }

            return newStructure;
        }
    }

    public class BooleanExpression : Expression
    {
        public bool Value;

        public BooleanExpression(bool value) => Value = value;
        public override string AsString() => Value.ToString();
    }


    public abstract class Expression
    {
        public abstract string AsString();
    }

    public class IdfType
    {
        public int Id { get; }
        public string Description { get; }
        public static IdfType String = new IdfType(1, "String");
        public static IdfType Numeric = new IdfType(2, "Numeric");
        public static IdfType List = new IdfType(3, "List");
        public static IdfType Object = new IdfType(4, "Object");

        public IdfType(int id, string description)
        {
            Id = id;
            Description = description;
        }
    }

}