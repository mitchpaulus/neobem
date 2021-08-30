using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Antlr4.Runtime;

namespace src
{
    public class FormatVisitor : NeobemParserBaseVisitor<string>
    {
        private readonly int _currentIndentLevel;
        private int _indentSpacing = 2;

        // This is useful variable for maintaining a non standard indentation when breaking expressions across multiple lines.
        private readonly int _currentPosition = 0;
        private readonly BufferedTokenStream _tokens;

        private IdfObjectPrettyPrinter _prettyPrinter = new();

        public FormatVisitor(int currentIndentLevel, int currentPosition, BufferedTokenStream tokens)
        {
            _currentIndentLevel = currentIndentLevel;
            _currentPosition = currentPosition;
            _tokens = tokens;
        }

        public override string VisitIdfComment(NeobemParser.IdfCommentContext context) => context.GetText();

        public override string VisitFunctionIdfComment(NeobemParser.FunctionIdfCommentContext context) => context.GetText();

        public override string VisitIdfplus_object(NeobemParser.Idfplus_objectContext context)
        {
            // Handle the empty structure
            if (!context.idfplus_object_property_def().Any()) return $"{context.LCURLY().GetText()} {context.RCURLY().GetText()}";

            // Handle the single item structure
            if (context.idfplus_object_property_def().Length == 1)
            {
                // Add two to the current position for the left curly brace plus a space.
                FormatVisitor singleLineVisitor = new(_currentIndentLevel, _currentPosition + 2, _tokens);
                return $"{{ {singleLineVisitor.Visit(context.idfplus_object_property_def().Single())} }}";
            }

            // Handle the n-item structure
            FormatVisitor subVisitor = new(_currentIndentLevel + 1, (_currentIndentLevel + 1) * _indentSpacing, _tokens);
            List<string> formattedStructs = context.idfplus_object_property_def().Select(def => subVisitor.IndentSpaces + subVisitor.Visit(def) + ",\n").ToList();

            return $"{{\n{string.Join("", formattedStructs)}{IndentSpaces}}}";
        }

        public override string VisitIdfplus_object_property_def(NeobemParser.Idfplus_object_property_defContext context)
        {
            string keyExpression = Visit(context.expression(0));
            string firstPortion = $"{keyExpression}{context.STRUCT_SEP().GetText()} ";

            var valueExpressionStartPosition = _currentPosition + firstPortion.SplitLines().Last().Length;
            FormatVisitor subVisitor = new(_currentIndentLevel, valueExpressionStartPosition, _tokens);
            string valueExpression = subVisitor.Visit(context.expression(1));

            return $"{firstPortion}{valueExpression}";
        }

        public override string VisitVariable_declaration(NeobemParser.Variable_declarationContext context)
        {
            string identifierPortion = $"{context.IDENTIFIER().GetText()} = ";
            FormatVisitor visitor = new(_currentIndentLevel,  _currentPosition + identifierPortion.Length, _tokens);
            return $"{context.IDENTIFIER().GetText()} = {visitor.Visit(context.expression())}";
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
                FormatVisitor ifVisitor = new(_currentIndentLevel, _currentPosition + ifPortion.Length, _tokens);

                string ifExpression = ifVisitor.Visit(ifExpressionContext);

                string thenText = $"{CurrentPositionSpaces}{exp.THEN().GetText()} ";
                FormatVisitor thenVisitor = new(_currentIndentLevel, thenText.Length, _tokens);

                string thenExpression = thenVisitor.Visit(thenExpressionContext);

                string elseText = $"{CurrentPositionSpaces}{exp.ELSE().GetText()} ";
                FormatVisitor elseVisitor = new(_currentIndentLevel, elseText.Length, _tokens);

                string elseExpression = elseVisitor.Visit(elseExpressionContext);

                string multilineExp = $"{ifPortion}{ifExpression}\n{thenText}{thenExpression}\n{elseText}{elseExpression}";

                return multilineExp;
            }
        }

        private string VisitMain<T1, T>(T1 context, Func<T1, T[]> selector) where T1 : ParserRuleContext where T : ParserRuleContext
        {
            var items = selector(context);
            StringBuilder builder = new();
            for (int i = 0; i < items.Length; i++)
            {
                T baseIdfContext = context.GetRuleContext<T>(i);

                int startTokenIndex = baseIdfContext.Start.TokenIndex;

                IList<IToken> commentTokens = _tokens.GetHiddenTokensToLeft(startTokenIndex) ?? new List<IToken>();

                // Remove beginning of file whitespace.
                if (i == 0) commentTokens = commentTokens.SkipWhile(token => token.Channel == 2).ToList();

                foreach (IToken commentToken in commentTokens)
                {
                    if (commentToken.Channel == 1)
                    {
                        // The following is a check to determine whether the comment is inline, or standalone.
                        int idx = commentToken.TokenIndex - 1;
                        bool isInlineComment = false;
                        while (idx >= 0)
                        {
                            IToken aPreviousToken = _tokens.Get(idx);
                            if (aPreviousToken.Channel == 0 && aPreviousToken.Line == commentToken.Line)
                            {
                                isInlineComment = aPreviousToken.Line == commentToken.Line;
                                break;
                            }

                            if (aPreviousToken.Channel == 0 && aPreviousToken.Line != commentToken.Line)
                                break;

                            idx--;
                        }

                        string inlineCommentSpace = isInlineComment && builder.ToString().Last() != ' ' ? " " : "";
                        builder.Append($"{inlineCommentSpace}{commentToken.Text.FixComment()}");
                    }
                    else if (commentToken.Channel == 2)
                    {
                        // If we are followed by normal content, we want to make sure that there
                        // is at least a single newline separating main statements.
                        int minNewLines = 0;
                        // minus 1 is for the EOF symbol that is tacked on in the size count.
                        if (commentToken.TokenIndex + 1 < _tokens.Size - 1)
                        {
                            var followingToken = _tokens.Get(commentToken.TokenIndex + 1);
                            if (followingToken.Channel != 1) minNewLines = 1;
                        }

                        int maxNewlines = 2;
                        string whiteSpace = HandleWhiteSpace(commentToken, maxNewlines, 0, minNewLines, builder);
                        builder.Append(whiteSpace);
                    }
                }

                string formattedContext = Visit(baseIdfContext);
                builder.Append($"{formattedContext}");
            }

            // Add any final comments
            if (items.Length > 0)
            {
                var endingComments = _tokens.GetHiddenTokensToRight(items.Last().Stop.TokenIndex) ?? new Collection<IToken>();

                // Remove whitespace at end of file.
                while (endingComments.Any() && endingComments.Last().Channel == 2)
                {
                    endingComments.RemoveAt(endingComments.Count -1);
                }

                foreach ((IToken endingComment, int index) in endingComments.WithIndex())
                {
                    if (endingComment.Channel == 1)
                    {
                        // If the first token is a comment, that means that there was no whitespace between it and the previous
                        // item. Ex:
                        // \ name {#comment
                        // So, force an extra space. Otherwise, whitespace should handle itself below.
                        string extraSpace = index == 0 ? " " : "";
                        builder.Append($"{extraSpace}{endingComment.Text.FixComment()}");
                    }
                    else if (endingComment.Channel == 2)
                    {
                        int maxNewlines = 2;
                        string endingWhiteSpace = HandleWhiteSpace(endingComment, maxNewlines, 0, 0, builder);
                        builder.Append(endingWhiteSpace);
                    }
                }
            }

            // Make sure we are ending with a newline
            if (builder[^1] != '\n') builder.Append("\n");

            return builder.ToString();
        }

        public override string VisitIdf(NeobemParser.IdfContext context)
        {
            StringBuilder builder = new();
            for (int i = 0; i < context.base_idf().Length; i++)
            {
                NeobemParser.Base_idfContext baseIdfContext = context.base_idf(i);

                int startTokenIndex = baseIdfContext.Start.TokenIndex;

                IList<IToken> commentTokens = _tokens.GetHiddenTokensToLeft(startTokenIndex) ?? new List<IToken>();

                // Remove beginning of file whitespace.
                if (i == 0) commentTokens = commentTokens.SkipWhile(token => token.Channel == 2).ToList();

                foreach (IToken commentToken in commentTokens)
                {
                    if (commentToken.Channel == 1)
                    {
                        // The following is a check to determine whether the comment is inline, or standalone.
                        int idx = commentToken.TokenIndex - 1;
                        bool isInlineComment = false;
                        while (idx >= 0)
                        {
                            IToken aPreviousToken = _tokens.Get(idx);
                            if (aPreviousToken.Channel == 0 && aPreviousToken.Line == commentToken.Line)
                            {
                                isInlineComment = aPreviousToken.Line == commentToken.Line;
                                break;
                            }

                            if (aPreviousToken.Channel == 0 && aPreviousToken.Line != commentToken.Line)
                                break;

                            idx--;
                        }

                        string inlineCommentSpace = isInlineComment && builder.ToString().Last() != ' ' ? " " : "";
                        builder.Append($"{inlineCommentSpace}{commentToken.Text.FixComment()}");
                    }
                    else if (commentToken.Channel == 2)
                    {
                        // If we are followed by normal content, we want to make sure that there
                        // is at least a single newline separating main statements.
                        int minNewLines = 0;
                        // minus 1 is for the EOF symbol that is tacked on in the size count.
                        if (commentToken.TokenIndex + 1 < _tokens.Size - 1)
                        {
                            var followingToken = _tokens.Get(commentToken.TokenIndex + 1);
                            if (followingToken.Channel != 1) minNewLines = 1;
                        }

                        int maxNewlines = 2;
                        string whiteSpace = HandleWhiteSpace(commentToken, maxNewlines, 0, minNewLines, builder);
                        builder.Append(whiteSpace);
                    }
                }

                string formattedContext = Visit(baseIdfContext);
                builder.Append($"{formattedContext}");
            }

            // Add any final comments
            if (context.base_idf().Any())
            {
                var endingComments = _tokens.GetHiddenTokensToRight(context.base_idf().Last().Stop.TokenIndex) ?? new Collection<IToken>();

                // Remove whitespace at end of file.
                while (endingComments.Any() && endingComments.Last().Channel == 2)
                {
                    endingComments.RemoveAt(endingComments.Count -1);
                }

                foreach ((IToken endingComment, int index) in endingComments.WithIndex())
                {
                    if (endingComment.Channel == 1)
                    {
                        // If the first token is a comment, that means that there was no whitespace between it and the previous
                        // item. Ex:
                        // \ name {#comment
                        // So, force an extra space. Otherwise, whitespace should handle itself below.
                        string extraSpace = index == 0 ? " " : "";
                        builder.Append($"{extraSpace}{endingComment.Text.FixComment()}");
                    }
                    else if (endingComment.Channel == 2)
                    {
                        int maxNewlines = 2;
                        string endingWhiteSpace = HandleWhiteSpace(endingComment, maxNewlines, 0, 0, builder);
                        builder.Append(endingWhiteSpace);
                    }
                }
            }

            // Make sure we are ending with a newline
            if (builder[^1] != '\n') builder.Append("\n");

            return builder.ToString();
        }

        public override string VisitLambda_def(NeobemParser.Lambda_defContext context)
        {
            FormatVisitor subVisitor = new(_currentIndentLevel + 1, (_currentIndentLevel + 1) * _indentSpacing, _tokens);

            // identifier portion - simply separate tokens with single space.
            string identifierContents = context.IDENTIFIER().Any() ? $" {string.Join(" ", context.IDENTIFIER().Select(node => node.GetText()))} " : " ";

            // Single line for expression version
            if (context.expression() is not null)
            {
                List<IToken> comments = _tokens.GetTokens(context.LCURLY().Symbol.TokenIndex,
                    context.RCURLY().Symbol.TokenIndex, NeobemLexer.NEOBEM_COMMENT)?.ToList() ?? new List<IToken>() ;

                string expressionContents = Visit(context.expression());
                // If the expression comes back longer than a single line, we need to use slightly different visitor with
                // different starting position.
                if (expressionContents.NumLines() > 1)
                {
                    expressionContents = subVisitor.Visit(context.expression());
                }
                bool multipleLineContents = expressionContents.Any(s => s == '\n');

                if (multipleLineContents)
                {
                    return
                        $"λ{identifierContents}{{{(expressionContents.StartsWith('\n') ? "" : $"\n{subVisitor.IndentSpaces}")}{expressionContents}\n{IndentSpaces}}}";
                }
                else
                {
                    if (!comments.Any()) return $"λ{identifierContents}{{ {expressionContents} }}";
                    if (comments.Count == 1)
                        return $"λ{identifierContents}{{ {expressionContents} }} {comments.First().Text.FixComment()}";

                    string commentString = string.Join("", comments.Select(token => $"\n{subVisitor.IndentSpaces}" + token.Text));

                    return
                        $"λ{identifierContents}{{{commentString}{(expressionContents.StartsWith('\n') ? "" : $"\n{subVisitor.IndentSpaces}")}{expressionContents}\n{IndentSpaces}}}";
                }

                return multipleLineContents ?
                    $"λ{identifierContents}{{{(expressionContents.StartsWith('\n') ? "" : $"\n{subVisitor.IndentSpaces}")}{expressionContents}\n{IndentSpaces}}}" :
                    $"λ{identifierContents}{{ {expressionContents} }}";
            }
            else
            {
                StringBuilder statementBuilder = new();

                // If we have statements instead of expression, we don't put it on a single line.
                int minNumberOfNewlines = 1;

                for (int i = 0; i < context.function_statement().Length; i++)
                {
                    var functionStatementContext = context.function_statement(i);
                    int startTokenIndex = functionStatementContext.Start.TokenIndex;

                    IList<IToken> commentTokens = _tokens.GetHiddenTokensToLeft(startTokenIndex) ?? new List<IToken>();

                    // This is usually a rare edge case, but if there were no whitespace tokens before statement, force a newline or space.
                    // If there are any comments, we know that a newline exists.
                    if (!commentTokens.Any())
                    {
                        statementBuilder.Append($"\n{subVisitor.IndentSpaces}");
                    }
                    else
                    {
                        foreach ((IToken commentToken, int index) in commentTokens.WithIndex())
                        {
                            if (commentToken.Channel == 1)
                            {
                                // If the first token is a comment, that means that there was no whitespace between it and the previous
                                // item. Ex:
                                // \ name {#comment
                                // So, force an extra space. Otherwise, whitespace should handle itself below.
                                string extraSpace = index == 0 ? " " : "";

                                // If we have two comment tokens in a row, we need to add proper indentation that a whitespace
                                // token would normally take of.
                                if (PreviousToken(commentToken).Channel == 1) extraSpace = subVisitor.IndentSpaces;

                                statementBuilder.Append($"{extraSpace}{commentToken.Text.FixComment()}");
                            }
                            else if (commentToken.Channel == 2)
                            {
                                // The first statement is not allowed to have extra space at the top of the function.
                                int maxNewlines = i == 0 ? 1 : 2;
                                string whiteSpace = HandleWhiteSpace(commentToken, maxNewlines, subVisitor.IndentSpaces.Length, minNumberOfNewlines, statementBuilder);
                                statementBuilder.Append(whiteSpace);
                            }
                        }

                        // If the final token is a comment, we need to add in the proper indentation from a whitespace token
                        // that we don't have.
                        if (commentTokens.Last().Channel == 1) statementBuilder.Append(subVisitor.IndentSpaces);
                    }


                    string statement = subVisitor.Visit(functionStatementContext);
                    // This is one of the invariants of the design, that the statement do not have a newline associated with them,
                    // it is handled by the whitespace tokens only. This comes into play with tokens like the IDF comment.
                    if (statement.EndsWith("\n")) statement = statement.Substring(0, statement.Length - 1);

                    statementBuilder.Append(statement);
                }

                // Add any final comments/whitespace
                var endingHiddenTokens = _tokens.GetHiddenTokensToLeft(context.RCURLY().Symbol.TokenIndex) ?? new Collection<IToken>();

                // If there are no end whitespace/comment tokens, we have a situation in which the curly brace is right next to the final
                // common token like:
                // Ex: \ name { value}
                // So add in proper indentation or space.
                if (!endingHiddenTokens.Any()) statementBuilder.Append($"\n{IndentSpaces}");
                else
                {
                    foreach ((IToken commentToken, int index) in endingHiddenTokens.WithIndex())
                    {
                        if (commentToken.Channel == 1)
                        {
                            // If the first token is a comment, that means that there was no whitespace between it and the previous
                            // item. Ex:
                            // \ name {#comment
                            // So, force an extra space. Otherwise, whitespace should handle itself below.
                            string extraSpace = index == 0 ? " " : "";
                            statementBuilder.Append($"{extraSpace}{commentToken.Text.FixComment()}");
                        }
                        else if (commentToken.Channel == 2)
                        {
                            string whitespace = HandleWhiteSpace(commentToken, 1, IndentSpaces.Length, minNumberOfNewlines, statementBuilder);
                            statementBuilder.Append(whitespace);
                        }
                    }
                }
                statementBuilder.Append(context.RCURLY().GetText());

                return $"λ{identifierContents}{{{statementBuilder}";
            }
        }
        public override string VisitObjectDeclaration(NeobemParser.ObjectDeclarationContext context)
        {
            return _prettyPrinter.ObjectPrettyPrinter(context.GetText(), _currentIndentLevel, _indentSpacing);
        }

        public override string VisitFunctionObjectDeclaration(NeobemParser.FunctionObjectDeclarationContext context)
        {
            return _prettyPrinter.ObjectPrettyPrinter(context.GetText(), _currentIndentLevel, _indentSpacing);
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
            FormatVisitor subVisitor = new(_currentIndentLevel, _currentPosition + 1, _tokens);
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
            string returnPortion = $"{context.return_statement().RETURN().GetText()} ";
            FormatVisitor subVisitor = new(_currentIndentLevel, _currentPosition + returnPortion.Length, _tokens);
            string expressionPortion = subVisitor.Visit(context.return_statement().expression());
            return $"{returnPortion}{expressionPortion}";
        }

        public override string VisitPrint_statment(NeobemParser.Print_statmentContext context)
        {
            string printPortion = $"{context.PRINT().GetText()} ";
            FormatVisitor subVisitor = new(_currentIndentLevel, _currentPosition + printPortion.Length, _tokens);
            string expressionPortion = subVisitor.Visit(context.expression());
            return $"{printPortion}{expressionPortion}";
        }

        public override string VisitLog_statement(NeobemParser.Log_statementContext context)
        {
            string logPortion = $"{context.LOG().GetText()} ";
            FormatVisitor subVisitor = new(_currentIndentLevel, _currentPosition + logPortion.Length, _tokens);
            string expressionPortion = subVisitor.Visit(context.expression());
            return $"{logPortion}{expressionPortion}";
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

            return $"\n{IndentSpaces}{topBorder}\n{IndentSpaces}{headerRow}\n{IndentSpaces}{separatorRow}\n{IndentSpaces}{string.Join($"\n{IndentSpaces}", dataRows)}\n{IndentSpaces}{bottomBorder}";
        }

        public override string VisitLet_binding(NeobemParser.Let_bindingContext context)
        {
            StringBuilder builder = new();
            var firstPortion = $"{context.LET().GetText()} ";
            builder.Append(firstPortion);
            FormatVisitor identifierSubVisitor =
                new(_currentIndentLevel, _currentPosition + firstPortion.Length, _tokens);

            for (int i = 0; i < context.expression().Length; i++)
            {
                string startSpace = i == 0 ? "" : new string(' ', _currentPosition + firstPortion.Length);
                if (i != 0) builder.Append(",\n");
                builder.Append($"{startSpace}{context.IDENTIFIER(i).GetText()} = ");
                int expStartPosition = _currentPosition + firstPortion.Length + context.IDENTIFIER(i).GetText().Length + 3;
                FormatVisitor expSubVisitor = new FormatVisitor(_currentIndentLevel, expStartPosition, _tokens);
                builder.Append(expSubVisitor.Visit(context.expression(i)));
            }

            builder.Append($"\n{new string(' ', _currentPosition)}in ");
            FormatVisitor finalLetExpressionVisitor = new(_currentIndentLevel, _currentPosition + 3, _tokens);
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

            var expressionVisitor = new FormatVisitor(_currentIndentLevel, importText.Length + 2, _tokens);
            string expression = expressionVisitor.Visit(context.expression());
            builder.Append(expression);

            foreach (NeobemParser.Import_optionContext importOptionContext in context.import_option())
            {
                FormatVisitor visitor = new(_currentIndentLevel, builder.ToString().CurrentPosition() + 1, _tokens);
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
                FormatVisitor subVisitor = new(_currentIndentLevel + 1, (_currentIndentLevel + 1) * _indentSpacing, _tokens);
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

        private IToken PreviousToken(IToken token)
        {
            if (token.TokenIndex == 0) throw new ArgumentException("No previous token for the first token");
            return _tokens.Get(token.TokenIndex - 1);
        }

        private string HandleWhiteSpace(IToken whitespaceToken, int maxNewLines, int numberOfIndentSpaces, int minNewLines = 0)
        {
            // If the previous token was a comment (neobem or idf) or idf object, it has a newline bound to it. Need to account for that here
            // when dealing with whitespace.
            int extraNewlineFromPreviousToken = _tokens.PreviousTokenEndsWithNewline(whitespaceToken) ? 1 : 0;

            int numNewlines = Math.Max(Math.Min(whitespaceToken.Text.Count(c => c == '\n') + extraNewlineFromPreviousToken, maxNewLines), minNewLines);
            return numNewlines > 0
                ? $"{new string('\n', numNewlines - extraNewlineFromPreviousToken)}{new string(' ', numberOfIndentSpaces)}"
                : " ";
        }

        private string HandleWhiteSpace(IToken whitespaceToken, int maxNewLines, int numberOfIndentSpaces, int minNewLines, StringBuilder builder)
        {
            // If the previous token was a comment (neobem or idf) or idf object, it has a newline bound to it. Need to account for that here
            // when dealing with whitespace.
            int extraNewlineFromPreviousToken = builder.ToString().EndsWith("\n") ? 1 : 0;

            int numNewlines = Math.Max(Math.Min(whitespaceToken.Text.Count(c => c == '\n') + extraNewlineFromPreviousToken, maxNewLines), minNewLines);
            return numNewlines > 0
                ? $"{new string('\n', numNewlines - extraNewlineFromPreviousToken)}{new string(' ', numberOfIndentSpaces)}"
                : " ";
        }


    }
}