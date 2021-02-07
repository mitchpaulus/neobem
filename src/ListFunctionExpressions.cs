using System;
using System.Collections.Generic;
using System.Linq;

namespace src
{
    public class ListHeadFunctionExpression : FunctionExpression
    {
        public ListHeadFunctionExpression() : base(new List<Dictionary<string, Expression>>(), new List<string> { "list" })
        {
        }

        public override (string, Expression) Evaluate(List<Expression> inputs)
        {
            if (inputs.Count != 1) throw new ArgumentException($"'head' expects one parameter, saw {inputs.Count}");
            if (!(inputs[0] is ListExpression listExpression)) throw new NotImplementedException($"'head' is not defined for types other than list.");
            if (listExpression.Expressions.Any()) return ("", listExpression.Expressions[0]);
            throw new IndexOutOfRangeException($"The list has no elements.");
        }
    }

    public class ListTailFunctionExpression : FunctionExpression
    {
        public ListTailFunctionExpression() : base(new List<Dictionary<string, Expression>>(), new List<string> { "list" })
        {
        }

        public override (string, Expression) Evaluate(List<Expression> inputs)
        {
            if (inputs.Count != 1) throw new ArgumentException($"'tail' expects one parameter, saw {inputs.Count}");
            if (!(inputs[0] is ListExpression listExpression)) throw new NotImplementedException($"'tail' is not defined for types other than list.");
            if (!listExpression.Expressions.Any()) throw new IndexOutOfRangeException($"The list has no elements.");
            return ("", new ListExpression(listExpression.Expressions.Skip(1).ToList()));
        }
    }

    public class ListIndexFunctionExpression : FunctionExpression
    {
        public ListIndexFunctionExpression() : base(new List<Dictionary<string, Expression>>(), new List<string> { "list", "index" })
        {
        }

        public override (string, Expression) Evaluate(List<Expression> inputs)
        {
            if (inputs.Count != 2)
                throw new ArgumentException(
                    $"'index' expects 2 parameters: a list and a numeric, saw {inputs.Count} parameters");
            if (!(inputs[0] is ListExpression listExpression)) throw new NotImplementedException($"First parameter of 'index' should be a list");
            if (!(inputs[1] is NumericExpression numericExpression))
                throw new NotImplementedException("Second parameter of 'index' should be a numeric");
            if (!listExpression.Expressions.Any()) throw new IndexOutOfRangeException($"The list has no elements.");

            int index = Convert.ToInt32(numericExpression.Value);

            if (index >= 0)
            {
                if (index > listExpression.Expressions.Count)
                {
                    throw new IndexOutOfRangeException($"Attempted to access index {index} of list. List only has {listExpression.Expressions.Count} items.");
                }

                return ("", listExpression.Expressions[index]);
            }

            if (Math.Abs(index) > listExpression.Expressions.Count)
            {
                throw new IndexOutOfRangeException($"Attempted to access index {index} of list. List only has {listExpression.Expressions.Count} items.");
            }

            return ("", listExpression.Expressions[index + listExpression.Expressions.Count]);
        }
    }

    public class ListLengthFunctionExpression : FunctionExpression
    {
        public ListLengthFunctionExpression() : base(new List<Dictionary<string, Expression>>(), new List<string> { "list" })
        {
        }

        public override (string, Expression) Evaluate(List<Expression> inputs)
        {
            if (inputs.Count != 1) throw new ArgumentException( $"'length' expects 1 list parameter, saw {inputs.Count} parameters.");
            if (!(inputs[0] is ListExpression listExpression)) throw new ArgumentException( $"'length' expects a list as the parameter.");
            return ("", new NumericExpression(listExpression.Expressions.Count));
        }
    }
}