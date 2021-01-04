using System;
using System.Collections.Generic;
using System.Linq;

namespace src
{
    public interface IFunction
    {
        Expression Evaluate(List<Expression> inputs);
    }

    public class MathematicalFunction : IFunction
    {
        public readonly string Name;
        public readonly Func<List<double>, double> Function;

        public static MathematicalFunction Abs = new MathematicalFunction("abs", inputs => Math.Abs(inputs[0]));
        public static MathematicalFunction Acos = new MathematicalFunction("acos", inputs => Math.Acos(inputs[0]));
        public static MathematicalFunction Asin = new MathematicalFunction("asin", inputs => Math.Asin(inputs[0]));
        public static MathematicalFunction Atan2 = new MathematicalFunction("atan2", inputs => Math.Atan2(inputs[0], inputs[1]));
        public static MathematicalFunction Ceiling = new MathematicalFunction("ceiling", inputs => Math.Ceiling(inputs[0]));
        public static MathematicalFunction Cos = new MathematicalFunction("cos", inputs => Math.Cos(inputs[0]));
        public static MathematicalFunction Log = new MathematicalFunction("ln", inputs => Math.Log(inputs[0]));
        public static MathematicalFunction Log10 = new MathematicalFunction("log10", inputs => Math.Log10(inputs[0]));
        public static MathematicalFunction Log2 = new MathematicalFunction("log2", inputs => Math.Log2(inputs[0]));
        public static MathematicalFunction Sin = new MathematicalFunction("sin", inputs => Math.Sin(inputs[0]));
        public static MathematicalFunction Sqrt = new MathematicalFunction("sqrt", inputs => Math.Sqrt(inputs[0]));

        public static MathematicalFunction Tan = new MathematicalFunction("tan", inputs => Math.Tan(inputs[0]));

        public static List<MathematicalFunction> List = new List<MathematicalFunction>();
        public static Dictionary<string, IFunction> FunctionDict = new Dictionary<string, IFunction>();

        public MathematicalFunction(string name, Func<List<double>, double> function)
        {
            Name = name;
            Function = function;

            List.Add(this);
            FunctionDict[name] = this;
        }

        public Expression Evaluate(List<Expression> inputs)
        {
            var value = Function(inputs.Cast<NumericExpression>().Select(expression => expression.Value).ToList());
            return new NumericExpression(value);
        }
    }
    public class SinFunction : IFunction
    {
        public Expression Evaluate(List<Expression> inputs)
        {
            var sin = Math.Sin(((NumericExpression) inputs[0]).Value);
            return new NumericExpression(sin);
        }
    }

    public class FunctionExpression : Expression
    {

        public override string AsString() => "";
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

        public override string AsString() => string.Join(",", Expressions);

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
    }

    public class BooleanExpression : Expression
    {
        private bool Value;

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