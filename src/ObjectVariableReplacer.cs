using System;
using System.Collections.Generic;
using System.Text;

namespace src
{
    public class ObjectVariableReplacer
    {

        public string Replace(string objectText, List<Dictionary<string, Expression>> variables)
        {
            StringBuilder output = new StringBuilder();

            Stack<int> leftCurlyBraceIndicies = new Stack<int>();

            for (int i = 0; i < objectText.Length ; i++)
            {
                if (objectText[i] == '{' && objectText[i  + 1] != '{')
                {
                    leftCurlyBraceIndicies.Push(i);
                    if (leftCurlyBraceIndicies.Count > 1)
                        throw new NotSupportedException("Nested expression replacement is not supported.");
                }
                else if (objectText[i] == '{' && objectText[i + 1] == '{')
                {
                    output.Append('{');
                    i++;
                }
                else if (objectText[i] == '}' && objectText[i + 1] != '}')
                {
                    var startIndex = leftCurlyBraceIndicies.Pop();
                    var expressionText = objectText.Substring(startIndex + 1, i - startIndex - 1);

                    IdfPlusExpVisitor expVisitor = new IdfPlusExpVisitor(variables, new Dictionary<string, IFunction>());

                    var parser = expressionText.ToParser();
                    var tree = parser.expression();
                    var evaluatedExpression = expVisitor.Visit(tree);
                    output.Append(evaluatedExpression.AsString());
                }
                else if (objectText[i] == '}' && objectText[i + 1] == '}')
                {
                    output.Append('}');
                    i++;
                }
                else if (leftCurlyBraceIndicies.Count == 0)
                {
                    output.Append(objectText[i]);
                }
            }

            return output.ToString();
        }

    }
}