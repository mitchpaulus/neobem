using System;

namespace src
{
    public class IdfPlusExpVisitor : IdfplusBaseVisitor<Expression>
    {
        public override Expression VisitExponientiate(IdfplusParser.ExponientiateContext context)
        {
            IdfplusParser.ExpressionContext lhs = context.expression(0);

            NumericExpression lhsValue = (NumericExpression)Visit(lhs);

            NumericExpression rhsValue = (NumericExpression)Visit(context.expression(1));

            var exponentValue = Math.Pow(lhsValue.Value, rhsValue.Value);
            return new NumericExpression(exponentValue, exponentValue.ToString());
        }



    }
}