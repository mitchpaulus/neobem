using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime.Tree;

namespace src
{
    public class IdfPlusVisitor : IdfplusBaseVisitor<string>
    {
        private readonly List<Dictionary<string, Expression>> _environments;
        private readonly ObjectVariableReplacer _objectVariableReplacer = new ObjectVariableReplacer();

        public IdfPlusVisitor()
        {
            _environments = new List<Dictionary<string, Expression>>()
            {
                new Dictionary<string, Expression>(MathematicalFunction.FunctionDict)
            };
            _environments[0]["map"] = new MapFunctionExpression();
            _environments[0]["load"] = new LoadFunctionExpression();
            _environments[0]["head"] = new ListHeadFunctionExpression();
            _environments[0]["tail"] = new ListTailFunctionExpression();
            _environments[0]["index"] = new ListIndexFunctionExpression();
            _environments[0]["length"] = new ListLengthFunctionExpression();
        }

        public IdfPlusVisitor(List<Dictionary<string, Expression>> environments)
        {
            _environments = environments;
        }

        public override string VisitIdf(IdfplusParser.IdfContext context)
        {
            var items = context.base_idf().Select(Visit).ToList();

            return string.Join("", items);
        }

        public override string VisitIdfComment(IdfplusParser.IdfCommentContext context)
        {
            return _objectVariableReplacer.Replace(context.GetText(), _environments);
        }

        public override string VisitObjectDeclaration(IdfplusParser.ObjectDeclarationContext context)
        {
            var replaced = _objectVariableReplacer.Replace(context.GetText(), _environments);
            ITerminalNode comment = context.@object().COMMENT();
            return replaced + "\n\n";
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

            return expressionVisitor.output.ToString();
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
            var fullStringWithQuotes = context.STRING().GetText();
            string filePath = fullStringWithQuotes.Substring(1, fullStringWithQuotes.Length - 2);

            FileInfo fileInfo = new FileInfo(filePath);

            string contents;
            try
            {
                contents = File.ReadAllText(fileInfo.FullName);
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                throw new Exception($"Could not find file {filePath} for import statement, line {context.Start.Line}.");
            }
            catch (Exception exception)
            {
                throw new Exception( $"Could not read contents of {filePath} in import statement, line {context.Start.Line}.");
            }

            IdfPlusVisitor visitor = new IdfPlusVisitor(_environments);

            IdfplusParser parser =  contents.ToParser();
            var tree = parser.idf();
            return visitor.VisitIdf(tree);
        }
    }
}