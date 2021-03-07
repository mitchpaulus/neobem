using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Antlr4.Runtime.Tree;

namespace src
{
    public class IdfPlusVisitor : NeobemParserBaseVisitor<string>
    {
        private readonly string _baseDirectory;
        private readonly List<Dictionary<string, Expression>> _environments;
        private readonly ObjectVariableReplacer _objectVariableReplacer;

        public HashSet<string> exports = new HashSet<string>();

        public IdfPlusVisitor(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
            _environments = new List<Dictionary<string, Expression>>()
            {
                new Dictionary<string, Expression>(MathematicalFunction.FunctionDict)
            };
            _environments[0]["map"] = new MapFunctionExpression();
            _environments[0]["filter"] = new FilterFunctionExpression();
            _environments[0]["load"] = new LoadFunctionExpression(baseDirectory);
            _environments[0]["head"] = new ListHeadFunctionExpression();
            _environments[0]["tail"] = new ListTailFunctionExpression();
            _environments[0]["init"] = new ListInitFunctionExpression();
            _environments[0]["last"] = new ListLastFunctionExpression();
            _environments[0]["index"] = new ListIndexFunctionExpression();
            _environments[0]["length"] = new ListLengthFunctionExpression();
            _environments[0]["join"] = new StringJoinFunctionExpression();

            _objectVariableReplacer = new ObjectVariableReplacer(baseDirectory);
        }

        public override string VisitIdf(NeobemParser.IdfContext context)
        {
            var items = context.base_idf().Select(Visit).ToList();

            return string.Join("", items);
        }

        public override string VisitIdfComment(NeobemParser.IdfCommentContext context)
        {
            return _objectVariableReplacer.Replace(context.GetText(), _environments);
        }

        public override string VisitObjectDeclaration(NeobemParser.ObjectDeclarationContext context)
        {
            var replaced = _objectVariableReplacer.Replace(context.GetText(), _environments);
            // ITerminalNode comment = context.@object().COMMENT();
            return replaced + "\n\n";
        }

        public override string VisitVariable_declaration(NeobemParser.Variable_declarationContext context)
        {
            IdfPlusExpVisitor expressionVisitor = new IdfPlusExpVisitor(_environments, _baseDirectory);
            Expression expression = expressionVisitor.Visit(context.expression());

            var identifier = context.IDENTIFIER().GetText();
            _environments[0][identifier] = expression;

            return expressionVisitor.output.ToString();
        }

        public override string VisitPrint_statment(NeobemParser.Print_statmentContext context)
        {
            IdfPlusExpVisitor expressionVisitor = new IdfPlusExpVisitor(_environments, _baseDirectory);
            expressionVisitor.Visit(context.expression());
            return expressionVisitor.output.ToString();
        }

        public override string VisitImport_statement(NeobemParser.Import_statementContext context)
        {
            var fullStringWithQuotes = context.STRING().GetText();
            string filePath = fullStringWithQuotes.Substring(1, fullStringWithQuotes.Length - 2);
            string contents;

            IdfPlusVisitor visitor;
            if (filePath.Contains("http"))
            {
                // See: https://stackoverflow.com/a/943875/5932184
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
                catch (FileNotFoundException)
                {
                    throw new Exception($"Could not find file {filePath} for import statement, line {context.Start.Line}.");
                }
                catch (Exception)
                {
                    throw new Exception( $"Could not read contents of {filePath} in import statement, line {context.Start.Line}.");
                }
                visitor = new IdfPlusVisitor(fileInfo.DirectoryName);
            }

            var options = context.import_option();

            string prefix = "";
            List<string> onlyOptions = new List<string>();
            foreach (NeobemParser.Import_optionContext importOptionContext in options)
            {
                if (importOptionContext is NeobemParser.AsOptionContext asOption)
                {
                    prefix = asOption.STRING().GetText().Substring(1, asOption.STRING().GetText().Length - 2);
                }

                if (importOptionContext is NeobemParser.OnlyOptionContext onlyOptionContext)
                {
                    onlyOptions.AddRange(onlyOptionContext.IDENTIFIER().Select(node => node.GetText()).ToList());
                }
            }

            NeobemParser parser =  contents.ToParser();
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

        public override string VisitExport_statement(NeobemParser.Export_statementContext context)
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