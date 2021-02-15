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
        public static Regex ExpressionReplacement = new Regex(@"\{.*?\}");
    }

    public class IdfPlusListener : NeobemParserBaseListener
    {
        readonly Dictionary<string, Expression> _variables = new Dictionary<string, Expression>();

        public StringBuilder Output = new StringBuilder();
        private readonly VariableRegex _variableRegex = new VariableRegex();

        private readonly Dictionary<string, IFunction> _functions = new Dictionary<string, IFunction>();

        public override void EnterVariable_declaration(NeobemParser.Variable_declarationContext context)
        {
            IdfPlusExpVisitor visitor = new IdfPlusExpVisitor(new List<Dictionary<string, Expression>>()
            {
                _variables
            }, null);
            var name = context.IDENTIFIER().GetText();
            var expression = visitor.Visit(context.expression());

            _variables[name] = expression;
            // Expression value = ExpressionEvaluator(context.expression());
            // variables[name] = value;
        }

        public override void EnterPrint_statment(NeobemParser.Print_statmentContext context)
        {
            IdfPlusExpVisitor visitor = new IdfPlusExpVisitor( new List<Dictionary<string, Expression>>() { _variables }, null);
            var expression = visitor.Visit(context.expression());
            Output.Append(expression.AsString() + '\n');
        }

        public override void EnterObject(NeobemParser.ObjectContext context)
        {
            var text = context.GetText();

            StringBuilder builder = new StringBuilder();

            MatchCollection matches = VariableRegex.Regex.Matches(text);

            int currentIndex = 0;

            foreach (Match match in matches)
            {
                builder.Append(text.Substring(currentIndex, match.Index - currentIndex));

                if (_variables.ContainsKey(match.Value.Substring(1))) builder.Append(_variables[match.Value.Substring(1)]);

                currentIndex = match.Index + match.Length;
            }

            builder.Append(text.Substring(currentIndex));

            Output.Append(builder.ToString() + '\n');
        }

        public override void EnterLambda_def(NeobemParser.Lambda_defContext context)
        {
            // FunctionExpression funcExpression = new FunctionExpression(context);
        }
    }
}