using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace src
{
    public class FormatVisitor : NeobemParserBaseVisitor<string>
    {
        private readonly int _currentIndentLevel;
        private int _indentSpacing = 2;

        // This is useful variable for maintaining a non standard indentation when breaking expressions across multiple lines.
        private readonly int _currentPosition = 0;

        private IdfObjectPrettyPrinter _prettyPrinter = new();

        public FormatVisitor(int currentIndentLevel, int currentPosition)
        {
            _currentIndentLevel = currentIndentLevel;
            _currentPosition = currentPosition;
        }

        public override string VisitIdfComment(NeobemParser.IdfCommentContext context)
        {
            // The TrimEnd is required here, since a newline is handled at the root idf level.
            return Indent(_currentIndentLevel, context.GetText().TrimEnd());
        }

        public override string VisitIdfplus_object(NeobemParser.Idfplus_objectContext context)
        {
            // Handle the empty structure
            if (!context.idfplus_object_property_def().Any()) return $"{context.LCURLY().GetText()} {context.RCURLY().GetText()}";

            // Handle the single item structure
            if (context.idfplus_object_property_def().Length == 1)
            {
                // Add two to the current position for the left curly brace plus a space.
                FormatVisitor singleLineVisitor = new(_currentIndentLevel, _currentPosition + 2);
                return $"{{ {singleLineVisitor.Visit(context.idfplus_object_property_def().Single())} }}";
            }

            // Handle the n-item structure
            FormatVisitor subVisitor = new(_currentIndentLevel + 1, (_currentIndentLevel + 1) * _indentSpacing);
            List<string> formattedStructs = context.idfplus_object_property_def().Select(def => subVisitor.IndentSpaces + subVisitor.Visit(def) + ",\n").ToList();

            return $"{{\n{string.Join("", formattedStructs)}{IndentSpaces}}}";
        }

        public override string VisitIdfplus_object_property_def(NeobemParser.Idfplus_object_property_defContext context)
        {
            string keyExpression = Visit(context.expression(0));
            string firstPortion = $"{keyExpression}{context.STRUCT_SEP().GetText()} ";

            var valueExpressionStartPosition = _currentPosition + firstPortion.SplitLines().Last().Length;
            FormatVisitor subVisitor = new(_currentIndentLevel, valueExpressionStartPosition);
            string valueExpression = subVisitor.Visit(context.expression(1));

            return $"{firstPortion}{valueExpression}";
        }

        public override string VisitVariable_declaration(NeobemParser.Variable_declarationContext context)
        {
            string identifierPortion = $"{IndentSpaces}{context.IDENTIFIER().GetText()} = ";
            FormatVisitor visitor = new(_currentIndentLevel, identifierPortion.Length);
            return $"{IndentSpaces}{context.IDENTIFIER().GetText()} = {visitor.Visit(context.expression())}";
        }

        public override string VisitIfExp(NeobemParser.IfExpContext context)
        {
            var exp = context.if_exp();

            NeobemParser.ExpressionContext ifExpressionContext = exp.expression(0);
            NeobemParser.ExpressionContext thenExpressionContext = exp.expression(1);
            NeobemParser.ExpressionContext elseExpressionContext  = exp.expression(2);

            string onelineExpression = $"{exp.IF().GetText()} {Visit(ifExpressionContext)} {exp.THEN().GetText()} {Visit(thenExpressionContext)} {exp.ELSE().GetText()} {Visit(elseExpressionContext)}";

            // If the if expression is long, break it up.
            if (onelineExpression.Length <= 72)
            {
                return onelineExpression;
            }
            else
            {
                string ifPortion = $"{exp.IF().GetText()} ";
                FormatVisitor ifVisitor = new(_currentIndentLevel, _currentPosition + ifPortion.Length);

                string ifExpression = ifVisitor.Visit(ifExpressionContext);

                string thenText = $"{CurrentPositionSpaces}{exp.THEN().GetText()} ";
                FormatVisitor thenVisitor = new(_currentIndentLevel, thenText.Length);

                string thenExpression = thenVisitor.Visit(thenExpressionContext);

                string elseText = $"{CurrentPositionSpaces}{exp.ELSE().GetText()} ";
                FormatVisitor elseVisitor = new(_currentIndentLevel, elseText.Length);

                string elseExpression = elseVisitor.Visit(elseExpressionContext);

                string multilineExp = $"{ifPortion}{ifExpression}\n{thenText}{thenExpression}\n{elseText}{elseExpression}";

                return multilineExp;
            }
        }

        public override string VisitIdf(NeobemParser.IdfContext context)
        {
            StringBuilder builder = new();
            for (int i = 0; i < context.base_idf().Length; i++)
            {
                bool extraSpace;
                bool isLastItem = i == context.base_idf().Length - 1;

                if (isLastItem)
                {
                    extraSpace = false;
                }
                else if (context.base_idf(i) is NeobemParser.IdfCommentContext)
                {
                    extraSpace = false;
                }
                else if (Visit(context.base_idf(i)).SplitLines().Count == 1 && Visit(context.base_idf(i + 1)).SplitLines().Count == 1)
                {
                    extraSpace = false;
                }
                else
                {
                    extraSpace = true;
                }

                var end = extraSpace ? "\n\n" : "\n";

                builder.Append($"{Visit(context.base_idf(i))}{end}");
            }

            return builder.ToString();
        }

        public override string VisitLambda_def(NeobemParser.Lambda_defContext context)
        {
            FormatVisitor subVisitor = new(_currentIndentLevel + 1, (_currentIndentLevel + 1) * _indentSpacing);

            FormatVisitor multiLineSingleExpressionVisitor = new(_currentIndentLevel + 1, (_currentIndentLevel + 1) * _indentSpacing);

            string identifierContents = context.IDENTIFIER().Any() ? $" {string.Join(" ", context.IDENTIFIER().Select(node => node.GetText()))} " : " ";

            // Single line for expression version
            string expressionContents;
            if (context.expression() is not null)
            {
                expressionContents = subVisitor.Visit(context.expression());
                if (expressionContents.NumLines() > 1)
                {
                    // An expression doesn't add leading spaces or a final newline, so we need to be sure to add those here.
                    expressionContents =
                        $"{multiLineSingleExpressionVisitor.IndentSpaces}{multiLineSingleExpressionVisitor.Visit(context.expression())}\n";
                }
            }
            else
            {
                StringBuilder statementBuilder = new();
                for (int i = 0; i < context.function_statement().Length; i++)
                {
                    bool addSpace;
                    bool isLastItem = i == context.function_statement().Length - 1;

                    if (isLastItem) addSpace = false;
                    // If we have multiple variable declarations, I think it looks better to not have extra space between them.
                    else if (context.function_statement(i) is NeobemParser.FunctionVariableDeclarationContext &&
                             context.function_statement(i + 1) is NeobemParser.FunctionVariableDeclarationContext)
                    {
                        addSpace = false;
                    }
                    else
                    {
                        addSpace = true;
                    }

                    // The trim end is to make sure that we get the correct number of newlines after, even if the Visitor isn't quite right.
                    string statement = $"{subVisitor.Visit(context.function_statement(i))}".TrimEnd();

                    statementBuilder.Append($"{statement}{(addSpace ? "\n\n" : "\n")}");
                }

                // List<string> functionContents = context.function_statement().Select(Visit).ToList();
                expressionContents = statementBuilder.ToString();
            }

            bool multipleLineContents = expressionContents.Any(s => s == '\n');

            return multipleLineContents ?
                $"λ{identifierContents}{{\n{expressionContents}{IndentSpaces}}}" :
                $"λ{identifierContents}{{ {expressionContents} }}";
        }
        public override string VisitObjectDeclaration(NeobemParser.ObjectDeclarationContext context)
        {
            return _prettyPrinter.ObjectPrettyPrinter(context.GetText(), _currentIndentLevel, _indentSpacing).TrimEnd();
        }

        public override string VisitFunctionObjectDeclaration(NeobemParser.FunctionObjectDeclarationContext context)
        {
            return _prettyPrinter.ObjectPrettyPrinter(context.GetText(), _currentIndentLevel, _indentSpacing).TrimEnd();
        }

        public override string VisitNumericExp(NeobemParser.NumericExpContext context) => context.GetText();
        public override string VisitMemberAccessExp(NeobemParser.MemberAccessExpContext context) => $"{Visit(context.expression(0))}{context.MEMBER_ACCESS().GetText()}{Visit(context.expression(1))}";
        public override string VisitStringExp(NeobemParser.StringExpContext context) => context.GetText();
        public override string VisitAddSub(NeobemParser.AddSubContext context) => $"{Visit(context.expression(0))} {context.op.Text} {Visit(context.expression(1))}";
        public override string VisitMultDivide(NeobemParser.MultDivideContext context) => $"{Visit(context.expression(0))}{context.op.Text}{Visit(context.expression(1))}";
        public override string VisitLogicExp(NeobemParser.LogicExpContext context) => $"{Visit(context.expression(0))} {context.op.Text} {Visit(context.expression(1))}";
        public override string VisitExponientiate(NeobemParser.ExponientiateContext context) => $"{Visit(context.expression(0))}{context.CARET().GetText()}{Visit(context.expression(1))}";
        public override string VisitParensExp(NeobemParser.ParensExpContext context)
        {
            FormatVisitor subVisitor = new(_currentIndentLevel, _currentPosition + 1);
            return $"{context.LPAREN().GetText()}{subVisitor.Visit(context.expression())}{context.RPAREN().GetText()}";
        }

        public override string VisitVariableExp(NeobemParser.VariableExpContext context) => context.IDENTIFIER().GetText();
        public override string VisitBooleanExp(NeobemParser.BooleanExpContext context)
        {
            return $"{Visit(context.expression(0))} {context.boolean_exp_operator().GetText()} {Visit(context.expression(1))}";
        }

        public override string VisitBooleanLiteralTrueExp(NeobemParser.BooleanLiteralTrueExpContext context) => "✓";
        public override string VisitBooleanLiteralFalseExp(NeobemParser.BooleanLiteralFalseExpContext context) => "✗";

        public override string VisitReturnStatement(NeobemParser.ReturnStatementContext context)
        {
            string returnPortion = $"{IndentSpaces}{context.return_statement().RETURN().GetText()} ";
            FormatVisitor subVisitor = new(_currentIndentLevel, returnPortion.Length);
            string expressionPortion = subVisitor.Visit(context.return_statement().expression());
            return $"{returnPortion}{expressionPortion}";
        }

        public override string VisitPrint_statment(NeobemParser.Print_statmentContext context)
        {
            string printPortion = $"{IndentSpaces}{context.PRINT().GetText()} ";
            FormatVisitor subVisitor = new(_currentIndentLevel, printPortion.Length);
            string expressionPortion = subVisitor.Visit(context.expression());
            return $"{printPortion}{expressionPortion}";
        }

        public override string VisitFunctionExp(NeobemParser.FunctionExpContext context)
        {
            // Print everything on single line for now.
            return $"{context.funcexp.GetText()}{context.LPAREN().GetText()}{string.Join(", ", context.function_parameter().Select(Visit))}{context.RPAREN().GetText()}";
        }

        public override string VisitInline_table(NeobemParser.Inline_tableContext context)
        {
            var headerLengths = context.inline_table_header().STRING().Select(node => node.GetText().Length).ToList();

            var rowLengths = context.inline_table_data_row().Select(rowContext =>
                    rowContext.expression().Select(expressionContext => expressionContext.GetText().Length).ToList())
                .ToList();


            // Build up the column widths = not necessarily guaranteed that all records have the same number of columns,
            // that's a check that doesn't happen at the parser level.
            var colIndex = 0;

            List<int> columnWidths = new();

            while (colIndex < headerLengths.Count || rowLengths.Any(row => colIndex < row.Count))
            {
                int currentMaxWidth = -1;

                if (colIndex < headerLengths.Count && headerLengths[colIndex] > currentMaxWidth)
                    currentMaxWidth = headerLengths[colIndex];

                foreach (var row in rowLengths)
                {
                    if (colIndex < row.Count && row[colIndex] > currentMaxWidth) currentMaxWidth = row[colIndex];
                }

                columnWidths.Add(currentMaxWidth);
                colIndex++;
            }

            string PadCell(NeobemParser.ExpressionContext expressionContext, int idx) => expressionContext.GetText().PadRight(columnWidths[idx]);

            List<string> horizontals = columnWidths.Select(i => new string('─', i)).ToList();
            string topBorder = string.Join("─┬─", horizontals);
            string bottomBorder = string.Join("─┴─", horizontals);

            string headerRow = string.Join(" │ ",
                context.inline_table_header().STRING().Select((node, i) => node.GetText().PadRight(columnWidths[i])));

            string separatorRow = string.Join("─┼─", horizontals);

            List<string> dataRows = context.inline_table_data_row().Select(rowContext  =>
            {
                string separator = " │ ";
                return string.Join(separator,rowContext.expression().Select(PadCell));
            }).ToList();

            return $"\n{topBorder}\n{headerRow}\n{separatorRow}\n{string.Join("\n", dataRows)}\n{bottomBorder}";
        }

        public override string VisitLet_binding(NeobemParser.Let_bindingContext context)
        {
            StringBuilder builder = new();
            var firstPortion = $"{context.LET().GetText()} ";
            builder.Append(firstPortion);
            FormatVisitor identifierSubVisitor =
                new(_currentIndentLevel, _currentPosition + firstPortion.Length);

            for (int i = 0; i < context.expression().Length; i++)
            {
                string startSpace = i == 0 ? "" : new string(' ', _currentPosition + firstPortion.Length);
                if (i != 0) builder.Append("\n");
                builder.Append($"{startSpace}{context.IDENTIFIER(i).GetText()} = ");
                int expStartPosition = _currentPosition + firstPortion.Length + context.IDENTIFIER(i).GetText().Length + 3;
                FormatVisitor expSubVisitor = new FormatVisitor(_currentIndentLevel, expStartPosition);
                builder.Append(expSubVisitor.Visit(context.expression(i)));
            }

            builder.Append($"\n{new string(' ', _currentPosition)}in ");
            FormatVisitor finalLetExpressionVisitor = new(_currentIndentLevel, _currentPosition + 3);
            builder.Append(finalLetExpressionVisitor.Visit(context.let_expression()));

            return builder.ToString();
        }

        public override string VisitExport_statement(NeobemParser.Export_statementContext context)
        {
            if (context.IDENTIFIER().Length < 4)
            {
                return $"export ({string.Join(" ", context.IDENTIFIER().Select(node => node.GetText()).ToList())})";
            }
            else
            {
                string identifierList = string.Join("", context.IDENTIFIER().Select(node => $"{Indent(1, node.GetText())}\n"));
                return $"export (\n{identifierList})";
            }
        }

        public override string VisitImport_statement(NeobemParser.Import_statementContext context)
        {
            StringBuilder builder = new();
            string importText = context.IMPORT().GetText();
            builder.Append(importText + " ");

            var expressionVisitor = new FormatVisitor(_currentIndentLevel, importText.Length + 2);
            string expression = expressionVisitor.Visit(context.expression());
            builder.Append(expression);

            foreach (NeobemParser.Import_optionContext importOptionContext in context.import_option())
            {
                FormatVisitor visitor = new(_currentIndentLevel, builder.ToString().CurrentPosition() + 1);
                string formattedOption = visitor.Visit(importOptionContext);
                builder.Append(" " + formattedOption);
            }

            return builder.ToString();
        }

        public override string VisitAsOption(NeobemParser.AsOptionContext context) => $"{context.AS().GetText()} {context.IDENTIFIER().GetText()}";

        public override string VisitOnlyOption(NeobemParser.OnlyOptionContext context)
        {
            string identifiers = string.Join(" ", context.IDENTIFIER().Select(node => node.GetText()));
            return $"{context.ONLY().GetText()} ({identifiers})";
        }

        public override string VisitNotOption(NeobemParser.NotOptionContext context)
        {
            string identifiers = string.Join(" ", context.IDENTIFIER().Select(node => node.GetText()));
            return $"{context.NOT().GetText()} ({identifiers})";
        }

        public override string VisitListExp(NeobemParser.ListExpContext context)
        {
            string leftBracket = context.list().LSQUARE().GetText();
            string rightBracket = context.list().RSQUARE().GetText();
            // Handle the empty list
            if (!context.list().expression().Any())
            {
                return $"{leftBracket}{rightBracket}";
            }

            StringBuilder builder = new();
            for (int i = 0; i < context.list().expression().Length; i++)
            {
                FormatVisitor subVisitor = new(_currentIndentLevel + 1, (_currentIndentLevel + 1) * _indentSpacing);
                if (context.list().expression(i) is NeobemParser.ObjExpContext || context.list().expression(i) is NeobemParser.ListExpContext)
                {
                    builder.Append($"\n{subVisitor.IndentSpaces}");
                }

                builder.Append(subVisitor.Visit(context.list().expression(i)));

                if (i != context.list().expression().Length - 1)
                {
                    // don't add space after comma if we know a newline is coming for next structure.
                    builder.Append(context.list().expression(i + 1) is NeobemParser.ObjExpContext || context.list().expression(i + 1) is NeobemParser.ListExpContext ? "," : ", ");
                }
                else
                {
                    // If we are more than a single line, append trailing comma
                    if (builder.NumLines() > 1) builder.Append(',');
                }
            }

            // If we have more than a single line, end the list on it's own line.
            var endBracket = builder.ToString().SplitLines().Count > 1 ? $"\n{IndentSpaces}{rightBracket}" : rightBracket;

            return $"{leftBracket}{builder}{endBracket}";
        }

        private string IndentSpaces => new(' ', _currentIndentLevel * _indentSpacing);

        private string CurrentPositionSpaces => new(' ', _currentPosition);

        private string Indent(int indentLevel, string line) => $"{new string(' ', indentLevel * _indentSpacing)}{line}";
    }
}