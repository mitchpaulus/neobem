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
            var openingChar = '<';
            var endingChar = '>';
            StringBuilder output = new StringBuilder();

            Stack<int> leftCurlyBraceIndicies = new Stack<int>();

            for (int i = 0; i < objectText.Length ; i++)
            {
                if (objectText[i] == openingChar && objectText[i  + 1] != openingChar)
                {
                    leftCurlyBraceIndicies.Push(i);
                    if (leftCurlyBraceIndicies.Count > 1)
                        throw new NotSupportedException($"Nested expression replacement is not supported. Occurred at index {i} in '{objectText}'.");
                }
                else if (objectText[i] == openingChar && objectText[i + 1] == openingChar)
                {
                    output.Append(openingChar);
                    i++;
                }
                else
                {
                    if (objectText[i] == endingChar && ((i + 1) >= objectText.Length || objectText[i + 1] != endingChar))
                    {
                        if (leftCurlyBraceIndicies.TryPop(out int startIndex))
                        {
                            var expressionText = objectText.Substring(startIndex + 1, i - startIndex - 1);

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
                            // var parser = expressionText.ToParser(fileType);
                            var tree = parser.expression();

                            if (eListener.Errors.Any())
                            {
                                return ("", eListener.Errors);
                            }

                            var evaluatedExpression = expVisitor.Visit(tree);
                            output.Append(evaluatedExpression.AsString());
                        }
                    }
                    else if (objectText[i] == endingChar && objectText[i + 1] == endingChar)
                    {
                        output.Append(endingChar);
                        i++;
                    }
                    else if (leftCurlyBraceIndicies.Count == 0)
                    {
                        output.Append(objectText[i]);
                    }
                }
            }

            return (output.ToString(), new List<AntlrError>(0));
        }
    }
}
