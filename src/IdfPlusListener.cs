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
        readonly Dictionary<string, Expression> _variables = new Dictionary<string, Expression>();

        public StringBuilder Output = new StringBuilder();
        private readonly VariableRegex _variableRegex = new VariableRegex();

        Dictionary<string, Template> templates = new Dictionary<string, Template>();
        private readonly Dictionary<string, IFunction> _functions = new Dictionary<string, IFunction>();

        public override void EnterVariable_declaration(IdfplusParser.Variable_declarationContext context)
        {
            IdfPlusExpVisitor visitor = new IdfPlusExpVisitor(_variables, _functions);
            var name = context.VARIABLE().GetText();
            var expression = visitor.Visit(context.expression());

            _variables[name] = expression;
            // Expression value = ExpressionEvaluator(context.expression());
            // variables[name] = value;
        }

        public override void EnterPrint_statment(IdfplusParser.Print_statmentContext context)
        {
            IdfPlusExpVisitor visitor = new IdfPlusExpVisitor(_variables, _functions);
            var expression = visitor.Visit(context.expression());
            Output.Append(expression.AsString() + '\n');
        }

        public override void EnterObject(IdfplusParser.ObjectContext context)
        {
            var text = context.GetText();

            StringBuilder builder = new StringBuilder();

            MatchCollection matches = VariableRegex.Regex.Matches(text);

            int currentIndex = 0;

            foreach (Match match in matches)
            {
                builder.Append(text.Substring(currentIndex, match.Index - currentIndex));

                if (_variables.ContainsKey(match.Value)) builder.Append(_variables[match.Value]);

                currentIndex = match.Index + match.Length;
            }

            builder.Append(text.Substring(currentIndex));

            Output.Append(builder.ToString() + '\n');
        }

        public override void EnterTemplate_statement(IdfplusParser.Template_statementContext context)
        {
            string name = context.IDENTIFIER().GetText();
            string contents = context.STRING().GetText();
            var parameters = context.VARIABLE().Select(node => node.ToString()).ToList();
            Template template = new Template( name, contents, parameters );

            templates[name] = template;
        }

        // public Expression ExpressionEvaluator(IdfplusParser.ExpressionContext context)
        // {
        //     if (context.STRING() != null)
        //     {
        //         var text = context.STRING().GetText();
        //         return new StringExpression(text.Substring(1, text.Length - 2));
        //     }
        //
        //     if (context.NUMERIC() != null)
        //     {
        //         return new NumericExpression(double.Parse(context.NUMERIC().GetText()), context.NUMERIC().GetText());
        //     };
        //
        //     if (context.VARIABLE() != null) return variables[context.VARIABLE().GetText()];
        //
        //     if (context.list() != null)
        //         return new ListExpression(context.list().expression().Select(ExpressionEvaluator).ToList());
        //
        //     if (context.map_statement() != null)
        //     {
        //         Template template = templates[context.map_statement().FUNCTION_NAME().GetText()];
        //
        //         if (context.map_statement().list() != null)
        //         {
        //             foreach (IdfplusParser.ExpressionContext expression in context.map_statement().list().expression())
        //             {
        //                 Expression evaluatedExpression = ExpressionEvaluator(expression);
        //             }
        //         }
        //     }
        //
        //     throw new NotImplementedException();
        // }

    }


    public class TemplateListener : IdfplusBaseListener
    {
        Dictionary<string, Template> templates = new Dictionary<string, Template>();
        public override void EnterTemplate_statement(IdfplusParser.Template_statementContext context)
        {
            var name = context.IDENTIFIER().GetText();

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