using System;
using System.Collections.Generic;
using System.Text;

namespace src
{
    public class ObjectVariableReplacer
    {
        private readonly string _baseDirectory;

        public ObjectVariableReplacer(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }

        public string Replace(string objectText, List<Dictionary<string, Expression>> variables)
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
                        throw new NotSupportedException("Nested expression replacement is not supported.");
                }
                else if (objectText[i] == openingChar && objectText[i + 1] == openingChar)
                {
                    output.Append(openingChar);
                    i++;
                }
                else
                {
                    if (objectText[i] == endingChar && objectText[i + 1] != endingChar)
                    {
                        if (leftCurlyBraceIndicies.TryPop(out int startIndex))
                        {
                            var expressionText = objectText.Substring(startIndex + 1, i - startIndex - 1);

                            IdfPlusExpVisitor expVisitor = new IdfPlusExpVisitor(variables, _baseDirectory);

                            var parser = expressionText.ToParser();
                            var tree = parser.expression();
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

            return output.ToString();
        }

    }
}