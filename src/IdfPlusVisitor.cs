using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Antlr4.Runtime.Tree;

namespace src
{
    public class IdfPlusVisitor : IdfplusBaseVisitor<string>
    {
        private readonly string _baseDirectory;
        private readonly List<Dictionary<string, Expression>> _environments;
        private readonly ObjectVariableReplacer _objectVariableReplacer = new ObjectVariableReplacer();

        public HashSet<string> exports = new HashSet<string>();

        public IdfPlusVisitor(string baseDirectory = null)
        {
            _baseDirectory = baseDirectory;
            _environments = new List<Dictionary<string, Expression>>()
            {
                new Dictionary<string, Expression>(MathematicalFunction.FunctionDict)
            };
            _environments[0]["map"] = new MapFunctionExpression();
            _environments[0]["filter"] = new FilterFunctionExpression();
            _environments[0]["load"] = new LoadFunctionExpression();
            _environments[0]["head"] = new ListHeadFunctionExpression();
            _environments[0]["tail"] = new ListTailFunctionExpression();
            _environments[0]["index"] = new ListIndexFunctionExpression();
            _environments[0]["length"] = new ListLengthFunctionExpression();
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
            string contents;

            IdfPlusVisitor visitor;
            if (filePath.Contains("http"))
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(filePath);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream resStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                contents = reader.ReadToEnd();
                visitor = new IdfPlusVisitor(null);
            }
            else
            {
                string fullFilePath = Path.GetFullPath(filePath, _baseDirectory);
                FileInfo fileInfo = new FileInfo(fullFilePath);

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
                visitor = new IdfPlusVisitor(fileInfo.DirectoryName);
            }

            var options = context.import_option();

            string prefix = "";
            List<string> onlyOptions = new List<string>();
            foreach (IdfplusParser.Import_optionContext importOptionContext in options)
            {
                if (importOptionContext is IdfplusParser.AsOptionContext asOption)
                {
                    prefix = asOption.STRING().GetText().Substring(1, asOption.STRING().GetText().Length - 2);
                }

                if (importOptionContext is IdfplusParser.OnlyOptionContext onlyOptionContext)
                {
                    onlyOptions.AddRange(onlyOptionContext.IDENTIFIER().Select(node => node.GetText()).ToList());
                }
            }

            IdfplusParser parser =  contents.ToParser();
            var tree = parser.idf();
            string  outputResult = visitor.VisitIdf(tree);

            foreach (var item in visitor.exports)
            {
                if (!onlyOptions.Any() || onlyOptions.Contains(item))
                {
                    string updatedName = string.IsNullOrWhiteSpace(prefix) ? item : $"{prefix}@{item}";
                    _environments.Last()[updatedName] = visitor._environments.Last()[item];
                }
            }

            return outputResult;
        }

        public override string VisitExport_statement(IdfplusParser.Export_statementContext context)
        {
            foreach (var identifierNode in context.IDENTIFIER())
            {
                string identifier = identifierNode.GetText();
                if (_environments.Last().ContainsKey(identifier))
                {
                    exports.Add(identifier);
                }
            }

            return "";
        }
    }
}