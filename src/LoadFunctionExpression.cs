using System;
using System.Collections.Generic;
using System.IO;

namespace src
{
    public class LoadFunctionExpression : FunctionExpression
    {
        public LoadFunctionExpression() : base(new List<Dictionary<string, Expression>>(), new List<string>{ "options"} )
        {
        }

        public override (string, Expression) Evaluate(List<Expression> inputs)
        {
            if (inputs[0] is StringExpression stringExpression)
            {
                var fullPath = Path.GetFullPath(stringExpression.Text);

                if (File.Exists(fullPath))
                {
                    DelimitedFileReader reader = new DelimitedFileReader();
                    string contents = File.ReadAllText(fullPath);

                    var listExpression = reader.ReadFile(contents);

                    return ("", listExpression);
                }
                else
                {
                    throw new FileNotFoundException($"The file {stringExpression.Text} could not be found.");
                }
            }
            else
            {
                throw new NotImplementedException($"Non string input for load function not implemented yet.");
            }
        }

        public override string AsString() => "Load";
    }
}