using System;
using System.Collections.Generic;
using System.Globalization;
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

    public class PartialApplicationFunctionExpression : FunctionExpression
    {
        private readonly FunctionExpression _original;
        private readonly Dictionary<int, Expression> _partiallyAppliedParameters;
        public PartialApplicationFunctionExpression(
            FunctionExpression original,
            List<Dictionary<string, Expression>> environments,
            List<string> remainingParameters,
            Dictionary<int, Expression> partiallyAppliedParameters) : base(environments, remainingParameters)
        {
            _original = original;
            _partiallyAppliedParameters = partiallyAppliedParameters;
        }

        /// <summary>
        /// This is an evaluation of the already partially applied function. The number of inputs here should be less than the original expression it
        /// was formed from.
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="baseDirectory"></param>
        /// <returns></returns>
        public override (string, Expression) Evaluate(List<Expression> inputs, string baseDirectory)
        {
            if (inputs.Count + _partiallyAppliedParameters.Count > _original.Parameters.Count)
            {
                string message =
                    $"The number of parameters supplied to this already partially applied function was too many." +
                    $" The original function had {_original.Parameters.Count} parameter{_original.Parameters.Count.Pluralize()}, " +
                    $"{_partiallyAppliedParameters.Count} parameter{_partiallyAppliedParameters.Count.Pluralize()} were partially applied, " +
                    $"and {inputs.Count} parameter{inputs.Count.Pluralize()} were passed.";
                throw new ArgumentException(message);
            }

            var inputQueue = new Queue<Expression>(inputs);
            List<Expression> newInputs = new();
            for (int i = 0; i < _original.Parameters.Count; i++)
            {
                newInputs.Add(_partiallyAppliedParameters.TryGetValue(i, out Expression partiallyAppliedExpression)
                    ? partiallyAppliedExpression
                    : inputQueue.Dequeue());
            }

            return _original.Evaluate(newInputs, baseDirectory);
        }
    }

    public class FunctionExpression : Expression
    {
        private readonly NeobemParser.LambdaExpContext _context;
        private readonly List<Dictionary<string, Expression>> _environments;

        public readonly List<string> Parameters;

        public override string AsString() => "";
        public override string AsErrorString() => _context.GetText();

        public override string TypeName() => "function";

        public FunctionExpression(NeobemParser.LambdaExpContext lambdaDefContext, List<Dictionary<string, Expression>> environments, string baseDirectory = null)
        {
            _context = lambdaDefContext;
            _environments = environments;
            Parameters = lambdaDefContext.lambda_def().IDENTIFIER().Select(node => node.GetText()).ToList();
        }

        public FunctionExpression(List<Dictionary<string, Expression>> environments, List<string> parameters, string baseDirectory = null)
        {
            _environments = environments;
            Parameters = parameters;
        }

        public virtual (string, Expression) Evaluate(List<Expression> inputs, string baseDirectory)
        {
            Dictionary<string, Expression> locals = new Dictionary<string, Expression>();

            if (inputs.Count != Parameters.Count)
            {
                string message =
                    $"Expected {Parameters.Count} parameter{Parameters.Count.Pluralize()} ({string.Join(", ", Parameters)}), received {inputs.Count}. Types: [{string.Join(", ", inputs.Select(expression => expression.TypeName()))}]";
                throw new InvalidOperationException(message);
            }

            for (int i = 0; i < inputs.Count; i++) locals[Parameters[i]] = inputs[i];

            var updatedEnvironments = new List<Dictionary<string, Expression>>(_environments);
            updatedEnvironments.Insert(0, locals);

            IdfPlusExpVisitor visitor = new(updatedEnvironments, baseDirectory);

            StringBuilder builder = new StringBuilder();

            ObjectVariableReplacer replacer = new ObjectVariableReplacer(baseDirectory);
            var prettyPrinter = new IdfObjectPrettyPrinter();

            // In a function declaration, you can have either a lone single expression,
            // or a set of valid function statements.
            if (_context.lambda_def().expression() != null)
            {
                NeobemParser.ExpressionContext expressionContext = _context.lambda_def().expression();
                Expression expression = visitor.Visit(expressionContext);
                return (visitor.output.ToString(), expression);
            }
            else
            {
                foreach (NeobemParser.Function_statementContext item in _context.lambda_def().function_statement())
                {
                    switch (item)
                    {
                        case NeobemParser.FunctionIdfCommentContext commentContext:
                            var replacedComment = replacer.Replace(commentContext.GetText(), updatedEnvironments);
                            builder.Append(replacedComment);
                            break;
                        case NeobemParser.FunctionObjectDeclarationContext objectDeclarationContext:
                            string objectText = objectDeclarationContext.GetText();
                            if (objectText.EndsWith("$")) objectText = objectText.Remove(objectText.Length - 1);
                            var replacedObject = replacer.Replace(objectText, updatedEnvironments);
                            var prettyPrinted = prettyPrinter.ObjectPrettyPrinter(replacedObject, 0, Consts.IndentSpaces);
                            builder.AppendLine(prettyPrinted);
                            break;
                        case NeobemParser.FunctionVariableDeclarationContext variableDeclarationContext:
                            var expressionResult = visitor.Visit(variableDeclarationContext.variable_declaration().expression());
                            var identifier = variableDeclarationContext.variable_declaration().IDENTIFIER().GetText();
                            locals[identifier] = expressionResult;
                            break;
                        case NeobemParser.ReturnStatementContext returnStatementContext:
                            Expression returnExpression = visitor.Visit(returnStatementContext.return_statement().expression());
                            return (builder.ToString(), returnExpression);
                        case NeobemParser.FunctionPrintStatementContext printStatmentContext:
                            // Need a clean visitor, so that only the output from evaluating the single expression
                            // is appended.
                            IdfPlusExpVisitor printStatementExpressionVisitor = new(updatedEnvironments, baseDirectory);
                            printStatementExpressionVisitor.Visit(printStatmentContext.print_statment().expression());
                            builder.Append(printStatementExpressionVisitor.output.ToString());
                            break;
                        case NeobemParser.FunctionLogStatementContext logStatementContext:
                            IdfPlusExpVisitor logStatementVisitor = new(updatedEnvironments, baseDirectory);
                            Expression resultingExpression = logStatementVisitor.Visit(logStatementContext.log_statement().expression());
                            Console.Error.Write(resultingExpression.AsString() + "\n");
                            break;
                        default:
                            // A user should never reach this, as all syntactically valid programs should be handled here.
                            throw new NotImplementedException(
                                $"The statement of type {item.GetType()} has not been implemented in function evaluation.");
                    }
                }

                return (builder.ToString(), new StringExpression(""));
            }
        }
    }

    public class StringExpression : Expression, IEquatable<StringExpression>
    {
        public readonly string Text;

        public StringExpression(string text)
        {
            Text = StringEscape.Escape(text);
        }

        public override string ToString() => Text ?? "";
        public override string AsString() => Text ?? "";
        public override string AsErrorString() => $"'{Text}'";

        public override string TypeName() => "string";

        public bool Equals(StringExpression other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Text == other.Text;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StringExpression) obj);
        }

        public override int GetHashCode() => Text.GetHashCode();
        public static bool operator ==(StringExpression left, StringExpression right) => Equals(left, right);
        public static bool operator !=(StringExpression left, StringExpression right) => !Equals(left, right);

        /// <summary>
        /// Case insensitive equality to the value of the text string
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool TextEqualsCaseIns(string other) => string.Equals(Text, other, StringComparison.OrdinalIgnoreCase);
    }

    public static class StringEscape
    {
        private static Dictionary<char, char> escapes = new Dictionary<char, char>()
        {
            {'n', '\n'},
            {'t', '\t'},
            {'\\', '\\'},
            {'\'', '\''},
            {'r', '\r'},
        };

        public static string Escape(string input)
        {
            StringBuilder builder = new StringBuilder(input);

            bool inEscape = false;

            List<int> indiciesToRemove = new List<int>();

            for (int i = 0; i < builder.Length; i++)
            {
                if (!inEscape)
                {
                    if (builder[i] == '\\')
                    {
                        inEscape = true;
                        indiciesToRemove.Add(i);
                    }
                }
                else
                {
                    if (escapes.ContainsKey(builder[i]))
                    {
                        builder[i] = escapes[builder[i]];
                        inEscape = false;
                    }
                    else
                    {
                        throw new NotImplementedException($"The escape sequence \\{builder[i]} is not valid.");
                    }
                }
            }

            foreach (var i in indiciesToRemove)
            {
                builder.Remove(i, 1);
            }
            return builder.ToString();
        }
    }

    public class ListExpression : Expression
    {
        public readonly List<Expression> Expressions;

        public ListExpression(List<Expression> expressions)
        {
            Expressions = expressions;
        }

        public override string AsString() => string.Join(",", Expressions.Select(expression => expression.AsString()));
        public override string AsErrorString() => "[" + AsString() + ']';

        public override string TypeName() => "list";
    }

    public class NumericExpression : Expression
    {
        public readonly double Value;
        public readonly string Text;

        public NumericExpression(double value, string text)
        {
            Value = value;
            Text = text;
        }

        public NumericExpression(double value)
        {
            Value = value;
            Text = value.ToSigFigs(4);
        }

        public override string ToString() => Text;
        public override string AsString() => Value.ToSigFigs(4);
        public override string AsErrorString() => Value.ToString(CultureInfo.InvariantCulture);

        public override string TypeName() => "numeric";
    }

    public class IdfPlusObjectExpression : Expression
    {
        public Dictionary<string, Expression> Members = new();
        public override string AsString() => string.Join(",", Members.Keys.Select(s => Members[s].AsString()));

        public override string AsErrorString()
        {
            return '{' + string.Join(", ", Members.Select(ErrorMemberString)) + '}';
        }

        public IdfPlusObjectExpression Add(IdfPlusObjectExpression right) => StructureAdd.Add(this, right);

        public override string TypeName() => "structure";

        private string ErrorMemberString(KeyValuePair<string, Expression> keyValuePair)
        {
            return $"'{keyValuePair.Key}': {keyValuePair.Value.AsErrorString()}";
        }
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
        public override string AsErrorString() => AsString();
        public override string TypeName() => "boolean";
    }


    public abstract class Expression
    {
        public abstract string AsString();
        public abstract string TypeName();

        // For error messages, we may want to present the current value
        // differently than what would show up in a replacement.
        public abstract string AsErrorString();

        public static Expression Parse(string input)
        {
            if (input == null) return new StringExpression("null");
            if (bool.TryParse(input, out bool parsedBoolean)) return new BooleanExpression(parsedBoolean);
            if (double.TryParse(input, out double parsedDouble)) return new NumericExpression(parsedDouble);
            return new StringExpression(input);
        }
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