using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;

namespace src
{
    public class ObjectVariableReplacer
    {
        private readonly string _baseDirectory;

        public ObjectVariableReplacer(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }

        public (string, List<AntlrError>) Replace(string objectText, List<Dictionary<string, Expression>> variables, FileType fileType)
        {
            (List<ReplacementSpan> spans, int? nestedOpenIndex) = ReplacementScanner.Scan(objectText);
            if (nestedOpenIndex is int nestedIndex)
                throw new NotSupportedException($"Nested expression replacement is not supported. Occurred at index {nestedIndex} in '{objectText}'.");

            StringBuilder output = new StringBuilder();
            int cursor = 0;

            foreach (ReplacementSpan span in spans)
            {
                AppendLiteral(output, objectText, cursor, span.OpenIndex);
                cursor = span.CloseIndex + 1;

                var expressionText = span.ExpressionText(objectText);

                IdfPlusExpVisitor expVisitor = new(variables, fileType, _baseDirectory);

                AntlrInputStream inputStream = new AntlrInputStream(expressionText);
                NeobemLexer lexer = new NeobemLexer(inputStream);
                var eListener = new SimpleAntlrErrorListener();
                lexer.RemoveErrorListeners();
                lexer.AddErrorListener(eListener);
                lexer.FileType = fileType;
                CommonTokenStream tokens = new CommonTokenStream(lexer);
                var parser = new NeobemParser(tokens);
                parser.RemoveErrorListeners();
                parser.AddErrorListener(eListener);
                var tree = parser.expression();

                if (eListener.Errors.Any())
                {
                    return ("", eListener.Errors);
                }

                var evaluatedExpression = expVisitor.Visit(tree);
                output.Append(evaluatedExpression.AsString());
            }

            AppendLiteral(output, objectText, cursor, objectText.Length);

            return (output.ToString(), new List<AntlrError>(0));
        }

        // '<<' and '>>' collapse to single literal characters, a bare '>' is dropped,
        // and an unmatched '<' swallows the rest of the text.
        private static void AppendLiteral(StringBuilder output, string text, int start, int endExclusive)
        {
            for (int i = start; i < endExclusive; i++)
            {
                char current = text[i];
                if (current == '<')
                {
                    if (i + 1 < endExclusive && text[i + 1] == '<')
                    {
                        output.Append('<');
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }
                else if (current == '>')
                {
                    if (i + 1 < endExclusive && text[i + 1] == '>')
                    {
                        output.Append('>');
                        i++;
                    }
                }
                else
                {
                    output.Append(current);
                }
            }
        }
    }
}
