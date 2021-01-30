using System.Collections.Generic;
using System.Linq;

namespace src
{
    public class IdfPlusVisitor : IdfplusBaseVisitor<string>
    {
        private readonly List<Dictionary<string, Expression>> _environments;

        public IdfPlusVisitor()
        {
            _environments = new List<Dictionary<string, Expression>>()
            {
                new Dictionary<string, Expression>(MathematicalFunction.FunctionDict)
            };
            _environments[0]["map"] = new MapFunctionExpression();
        }

        public override string VisitIdf(IdfplusParser.IdfContext context)
        {
            var items = context.base_idf().Select(Visit).ToList();

            return string.Join("", items);
        }

        public override string VisitIdfComment(IdfplusParser.IdfCommentContext context)
        {
            return context.GetText();
        }

        public override string VisitObjectDeclaration(IdfplusParser.ObjectDeclarationContext context)
        {
            ObjectVariableReplacer replacer = new ObjectVariableReplacer();
            return replacer.Replace(context.GetText(), _environments);
        }

        public override string VisitVariable_declaration(IdfplusParser.Variable_declarationContext context)
        {
            IdfPlusExpVisitor expressionVisitor = new IdfPlusExpVisitor(_environments);
            Expression expression = expressionVisitor.Visit(context.expression());

            var members = context.member_access();
            var identifier = context.IDENTIFIER().GetText();
            if (members.Length == 0)
            {
                _environments[0][identifier] = expression;
            }
            else
            {
                if (!_environments[0].ContainsKey(identifier))
                {
                    IdfPlusObjectExpression objectExpression = new IdfPlusObjectExpression();
                    foreach (IdfplusParser.Member_accessContext memberAccessContext in members)
                    {
                        var memberName = memberAccessContext.IDENTIFIER().GetText();
                        // objectExpression.Members.Add(memberName,     );
                    }
                }
            }

            return "";
        }

        public IdfPlusObjectExpression AddObjectMembers(IdfPlusObjectExpression objectExpression,
            List<IdfplusParser.Member_accessContext> remainingMembers)
        {
            // if (!remainingMembers.Any()) return objectExpression;
            // var memberName = remainingMembers.First().IDENTIFIER().GetText();
            // objectExpression.Members.Add(memberName,  AddObjectMembers() );
            return objectExpression;
        }


        public override string VisitPrint_statment(IdfplusParser.Print_statmentContext context)
        {
            IdfPlusExpVisitor expressionVisitor = new IdfPlusExpVisitor(_environments);
            expressionVisitor.Visit(context.expression());
            return expressionVisitor.output.ToString();
        }

        public override string VisitImport_statement(IdfplusParser.Import_statementContext context)
        {
            return "";
        }
    }
}