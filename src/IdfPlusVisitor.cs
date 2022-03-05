using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace src
{
    public class IdfPlusVisitor : NeobemParserBaseVisitor<string>
    {
        private readonly string _baseDirectory;
        private readonly FileType _fileType;
        private readonly List<Dictionary<string, Expression>> _environments;
        private readonly ObjectVariableReplacer _objectVariableReplacer;

        public HashSet<string> exports = new HashSet<string>();
        private readonly IdfObjectPrettyPrinter _idfObjectPrettyPrinter = new IdfObjectPrettyPrinter();

        public IdfPlusVisitor(string baseDirectory, FileType fileType)
        {
            _baseDirectory = baseDirectory;
            _fileType = fileType;
            _environments = new List<Dictionary<string, Expression>>()
            {
                new Dictionary<string, Expression>(MathematicalFunction.FunctionDict)
            };
            _environments[0]["map"] = new MapFunctionExpression();
            _environments[0]["filter"] = new FilterFunctionExpression();
            _environments[0]["keys"] = new KeysFunctionExpression();
            _environments[0]["has"] = new HasFunctionExpression();
            _environments[0]["load"] = new LoadFunctionExpression(baseDirectory);
            _environments[0]["head"] = new ListHeadFunctionExpression();
            _environments[0]["tail"] = new ListTailFunctionExpression();
            _environments[0]["init"] = new ListInitFunctionExpression();
            _environments[0]["last"] = new ListLastFunctionExpression();
            _environments[0]["index"] = new ListIndexFunctionExpression();
            _environments[0]["length"] = new ListLengthFunctionExpression();
            _environments[0]["join"] = new StringJoinFunctionExpression();
            _environments[0]["replace"] = new StringReplaceFunctionExpression();
            _environments[0]["mod"] = new ModFunctionExpression();

            _environments[0]["type"] = new TypeFunctionExpression();

            _environments[0]["guid"] = new GuidFunctionExpression();

            _objectVariableReplacer = new ObjectVariableReplacer(baseDirectory);
        }

        public override string VisitIdf(NeobemParser.IdfContext context)
        {
            // var items = context.base_idf().Select(Visit).Select(s => s.Replace("\r\n", "\n")).ToList();
            List<string> items = new();
            foreach (var item in context.base_idf())
            {
                string visitResult = Visit(item);

                if (visitResult == null)
                    throw new NotImplementedException(
                        $"{item.Start.Line}:{item.Start.Column}: Input file item not implemented.");

                items.Add(visitResult.Replace("\r\n", "\n"));
            }

            string joined = string.Join("", items);
            // Make sure we end with a single newline.
            return joined.AddNewLines(1);
        }

        public override string VisitDoe2Comment(NeobemParser.Doe2CommentContext context)
        {
            try
            {
                return _objectVariableReplacer.Replace(context.GetText(), _environments, _fileType);
            }
            catch (Exception exception)
            {
                throw new Exception($"Line {context.Start.Line}: {exception.Message}");
            }
        }

        public override string VisitDoe2object(NeobemParser.Doe2objectContext context)
        {
            try
            {
                Doe2Printer printer = new(_objectVariableReplacer, _environments, _fileType);
                return printer.PrettyPrint(context) + "\n\n";
            }
            catch (Exception exception)
            {
                throw new Exception($"Line {context.Start.Line}: {exception.Message}");
            }
        }

        public override string VisitIdfComment(NeobemParser.IdfCommentContext context)
        {
            return _objectVariableReplacer.Replace(context.GetText(), _environments, _fileType);
        }

        public override string VisitObjectDeclaration(NeobemParser.ObjectDeclarationContext context)
        {
            string objectText = context.GetText();

            if (objectText.EndsWith("$")) objectText = objectText.Remove(objectText.Length - 1);

            var replaced = _objectVariableReplacer.Replace(objectText, _environments, _fileType);
            var prettyPrinted = _idfObjectPrettyPrinter.ObjectPrettyPrinter(replaced, 0, Consts.IndentSpaces);
            return prettyPrinted + "\n\n";
        }

        public override string VisitVariable_declaration(NeobemParser.Variable_declarationContext context)
        {
            IdfPlusExpVisitor expressionVisitor = new IdfPlusExpVisitor(_environments, _fileType, _baseDirectory);
            Expression expression = expressionVisitor.Visit(context.expression());

            var identifier = context.IDENTIFIER().GetText();
            _environments[0][identifier] = expression;

            return expressionVisitor.output.ToString();
        }

        public override string VisitPrint_statment(NeobemParser.Print_statmentContext context)
        {
            IdfPlusExpVisitor expressionVisitor = new IdfPlusExpVisitor(_environments, _fileType, _baseDirectory);
            expressionVisitor.Visit(context.expression());
            return expressionVisitor.output.ToString();
        }

        public override string VisitImport_statement(NeobemParser.Import_statementContext context)
        {
            IdfPlusExpVisitor expressionVisitor = new IdfPlusExpVisitor(_environments, _fileType, _baseDirectory);
            Expression uriExpression =  expressionVisitor.Visit(context.expression());

            string filePath;
            if (uriExpression is StringExpression uriStringExpression)
            {
                filePath = uriStringExpression.Text;
            }
            else
            {
                throw new Exception($"The import statement expects a string expression, received a {uriExpression.TypeName()}");
            }

            string contents;
            IdfPlusVisitor visitor;

            // Set the contents and the visitor based on the type of URI.
            // Right now we support http{s} or files.
            if (filePath.StartsWith("http"))
            {
                // See: https://stackoverflow.com/a/943875/5932184
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(filePath);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream resStream = response.GetResponseStream();
                StreamReader reader = new(resStream);
                contents = reader.ReadToEnd();

                // Many of the idf files in the BCL include a 'Version' object. This causes issues when
                // more than a single import occurs. So we have this hardcoded check to remove that
                // in the case it appears that we are loading an idf file from the BCL.
                if (filePath.StartsWith("https://bcl.nrel.gov/api/file") && filePath.ToLower().EndsWith(".idf"))
                {
                    contents = Bcl.RemoveVersion(contents);
                }

                // When reading from a web URI, the concept of a base directory doesn't apply.
                visitor = new IdfPlusVisitor(null, _fileType);
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

                // Read the imported file, with the current directory set to directory of the input file.
                visitor = new IdfPlusVisitor(fileInfo.DirectoryName, _fileType);
            }

            NeobemParser.Import_optionContext[] options = context.import_option();

            string prefix = "";
            List<string> onlyOptions = new();

            // An import statement can have n number of options appended after it. This loop is
            // parsing those options in order. Can be an "as" option, "only" option, or "not" option.
            foreach (NeobemParser.Import_optionContext importOptionContext in options)
            {
                if (importOptionContext is NeobemParser.AsOptionContext asOption)
                {
                    prefix = asOption.IDENTIFIER().GetText();
                }
                else if (importOptionContext is NeobemParser.OnlyOptionContext onlyOptionContext)
                {
                    onlyOptions.AddRange(onlyOptionContext.IDENTIFIER().Select(node => node.GetText()).ToList());
                }
            }

            NeobemParser parser = contents.ToParser(_fileType);
            NeobemParser.IdfContext tree = parser.idf();

            string  outputResult = visitor.VisitIdf(tree);

            foreach (string item in visitor.exports)
            {
                if (!onlyOptions.Any() || onlyOptions.Contains(item))
                {
                    string updatedName = string.IsNullOrWhiteSpace(prefix) ? item : $"{prefix}@{item}";
                    _environments.Last()[updatedName] = visitor._environments.Last()[item];
                }
            }

            // Make a paragraph between imported stuff.
            return outputResult.AddNewLines(2);
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

        public override string VisitLog_statement(NeobemParser.Log_statementContext context)
        {
            IdfPlusExpVisitor expressionVisitor = new(_environments, _fileType, _baseDirectory);
            Expression resultExpression = expressionVisitor.Visit(context.expression());
            string stringResult = resultExpression.AsString();
            if (stringResult.Any() && stringResult.Last() != '\n') stringResult = stringResult + '\n';
            Console.Error.Write(stringResult);
            return "";
        }
    }
}