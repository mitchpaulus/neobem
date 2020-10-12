using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace src
{
    public class VariableRegex
    {
        public static Regex Regex = new Regex(@"\$[a-zA-Z0-9]+(\.[a-zA-Z0-9]+)*");
    }

    public class IdfPlusListener : IdfplusBaseListener
    {
        Dictionary<string, Expression> variables = new Dictionary<string, Expression>();

        public StringBuilder Output = new StringBuilder();
        private readonly VariableRegex _variableRegex = new VariableRegex();

        public override void EnterVariable_declaration(IdfplusParser.Variable_declarationContext context)
        {
            var name = context.VARIABLE().GetText();
            Expression value = ExpressionEvaluator(context.expression());
            variables[name] = value;
        }

        public override void EnterObject(IdfplusParser.ObjectContext context)
        {
            var text = context.OBJECT().GetText();

            StringBuilder builder = new StringBuilder();

            MatchCollection matches = VariableRegex.Regex.Matches(text);

            int currentIndex = 0;

            foreach (Match match in matches)
            {
                builder.Append(text.Substring(currentIndex, match.Index - currentIndex));

                if (variables.ContainsKey(match.Value)) builder.Append(variables[match.Value]);

                currentIndex = match.Index + match.Length;
            }

            builder.Append(text.Substring(currentIndex));

            Output.Append(builder.ToString() + '\n');
        }


        public Expression ExpressionEvaluator(IdfplusParser.ExpressionContext context)
        {
            if (context.STRING() != null)
            {
                var text = context.STRING().GetText();
                return new StringExpression(text.Substring(1, text.Length - 2));
            }

            if (context.NUMERIC() != null)
            {
                return new NumericExpression(double.Parse(context.NUMERIC().GetText()), context.NUMERIC().GetText());
            };

            if (context.VARIABLE() != null) return variables[context.VARIABLE().GetText()];

            if (context.list() != null)
                return new ListExpression(context.list().expression().Select(ExpressionEvaluator).ToList());

            throw new NotImplementedException();
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
    }

    public class ListExpression : Expression
    {
        public List<Expression> Expressions;

        public ListExpression(List<Expression> expressions)
        {
            Expressions = expressions;
        }
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

        public override string ToString() => Text;
    }

    public class Expression {  }

    public class IdfType
    {
        public int Id { get; }
        public string Description { get; }
        public static IdfType String = new IdfType(1, "String");
        public static IdfType Numeric = new IdfType(2, "Numeric");
        public static IdfType List = new IdfType(3, "List");

        public IdfType(int id, string description)
        {
            Id = id;
            Description = description;
        }
    }

    public class TemplateListener : IdfplusBaseListener
    {
        Dictionary<string, Template> templates = new Dictionary<string, Template>();
        public override void EnterFunction_definition(IdfplusParser.Function_definitionContext context)
        {
            var name = context.FUNCTION_NAME().GetText();

            var parameters = context.VARIABLE().Select(node => node.GetText()).ToList();

            var templateString = context.STRING().GetText();

            Template template = new Template(name, templateString, parameters);

            templates[name] = template;
        }
    }

    public class Template
    {
        public List<string> Parameters;
        public string TemplateString;
        public string Name;

        public Template(string name, string templateString, List<string> parameters)
        {
            TemplateString = templateString;
            Name = name;
            Parameters = parameters;
        }
    }
}