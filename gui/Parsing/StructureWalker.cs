using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using src;

namespace gui.Parsing;

// Walks a Neobem parse tree and produces the source structure tree shown in
// the GUI. Deliberately GUI-agnostic so it can later back an LSP
// textDocument/documentSymbol implementation.
public static class StructureWalker
{
    public static List<SymbolNode> Walk(NeobemParser.IdfContext idf)
    {
        var result = new List<SymbolNode>();
        foreach (NeobemParser.Base_idfContext statement in idf.base_idf())
        {
            switch (statement)
            {
                case NeobemParser.ObjectDeclarationContext c:
                    result.Add(FromObject(c.@object()));
                    break;
                case NeobemParser.Doe2ObjectDeclarationContext c:
                    result.Add(FromDoe2Object(c.doe2object()));
                    break;
                case NeobemParser.VariableDeclarationContext c:
                    result.Add(FromVariableDeclaration(c.variable_declaration()));
                    break;
                case NeobemParser.ImportStatementContext c:
                    result.Add(FromImport(c.import_statement()));
                    break;
                case NeobemParser.ExportStatmentContext c:
                    result.Add(FromExport(c.export_statement()));
                    break;
                case NeobemParser.PrintStatmentContext c:
                    result.Add(FromSimple(SymbolKind.Print, "print", c.print_statment().expression(), c.print_statment()));
                    break;
                case NeobemParser.LogStatementContext c:
                    result.Add(FromSimple(SymbolKind.Log, "log", c.log_statement().expression(), c.log_statement()));
                    break;
                // Comments are skipped.
            }
        }
        return result;
    }

    private static SymbolNode FromObject(NeobemParser.ObjectContext ctx)
    {
        string name = ctx.OBJECT_TYPE().GetText().Trim();
        ITerminalNode[] fields = ctx.FIELD();
        string detail = fields.Length > 0 ? Truncate(fields[0].GetText().Trim(), 40) : "";
        return Node(SymbolKind.Object, name, detail, ctx);
    }

    private static SymbolNode FromDoe2Object(NeobemParser.Doe2objectContext ctx)
    {
        string name = (ctx.DOE2IDENTIFIER() ?? ctx.DOE2STRING_UNAME())?.GetText().Trim() ?? "DOE-2 object";
        return Node(SymbolKind.Doe2Object, name, "", ctx);
    }

    private static SymbolNode FromVariableDeclaration(NeobemParser.Variable_declarationContext ctx)
    {
        string name = ctx.IDENTIFIER().GetText();
        NeobemParser.ExpressionContext expr = ctx.expression();

        if (expr is NeobemParser.LambdaExpContext lambdaExp)
        {
            NeobemParser.Lambda_defContext lambda = lambdaExp.lambda_def();
            return Node(SymbolKind.Function, name, LambdaParams(lambda), ctx, CollectNested(LambdaBody(lambda)));
        }

        return Node(SymbolKind.Variable, name, Truncate(SourceText(expr), 40), ctx, CollectNested(expr));
    }

    private static SymbolNode FromImport(NeobemParser.Import_statementContext ctx)
    {
        string name = StripQuotes(ctx.expression().GetText());
        string detail = string.Join(" ", ctx.import_option().Select(o => SourceText(o)));
        return Node(SymbolKind.Import, name, detail, ctx);
    }

    private static SymbolNode FromExport(NeobemParser.Export_statementContext ctx)
    {
        string detail = string.Join(", ", ctx.IDENTIFIER().Select(i => i.GetText()));
        return Node(SymbolKind.Export, "export", detail, ctx);
    }

    private static SymbolNode FromSimple(SymbolKind kind, string name, NeobemParser.ExpressionContext expr, ParserRuleContext ctx) =>
        Node(kind, name, Truncate(SourceText(expr), 40), ctx, CollectNested(expr));

    private static SymbolNode FromLambda(string name, NeobemParser.Lambda_defContext ctx) =>
        Node(SymbolKind.Function, name, LambdaParams(ctx), ctx, CollectNested(LambdaBody(ctx)));

    private static SymbolNode FromIdfPlusObject(NeobemParser.Idfplus_objectContext ctx)
    {
        NeobemParser.Idfplus_object_property_defContext[] props = ctx.idfplus_object_property_def();
        string detail = props.Length > 0
            ? Truncate(string.Join(", ", props.Select(p => StripQuotes(p.expression(0).GetText()))), 40)
            : "";
        var children = new List<SymbolNode>();
        foreach (NeobemParser.Idfplus_object_property_defContext prop in props)
            Collect(prop.expression(1), children);
        return Node(SymbolKind.IdfPlusObject, "object", detail, ctx, children);
    }

    private static SymbolNode FromLet(NeobemParser.Let_bindingContext ctx)
    {
        var children = new List<SymbolNode>();
        ITerminalNode[] names = ctx.IDENTIFIER();
        NeobemParser.ExpressionContext[] exprs = ctx.expression();
        for (int i = 0; i < names.Length && i < exprs.Length; i++)
        {
            children.Add(Node(SymbolKind.Variable, names[i].GetText(), Truncate(SourceText(exprs[i]), 40),
                Span(names[i].Symbol, exprs[i].Stop), CollectNested(exprs[i])));
        }
        Collect(ctx.let_expression(), children);
        return Node(SymbolKind.Let, "let", string.Join(", ", names.Select(n => n.GetText())), ctx, children);
    }

    // Recursive descent that stops at any construct that gets its own node,
    // so nesting in the tree mirrors nesting in the source.
    private static void Collect(IParseTree tree, List<SymbolNode> sink)
    {
        switch (tree)
        {
            case NeobemParser.Variable_declarationContext c:
                sink.Add(FromVariableDeclaration(c));
                return;
            case NeobemParser.Lambda_defContext c:
                sink.Add(FromLambda("λ", c));
                return;
            case NeobemParser.Idfplus_objectContext c:
                sink.Add(FromIdfPlusObject(c));
                return;
            case NeobemParser.Let_bindingContext c:
                sink.Add(FromLet(c));
                return;
            case NeobemParser.ObjectContext c:
                sink.Add(FromObject(c));
                return;
            case NeobemParser.Doe2objectContext c:
                sink.Add(FromDoe2Object(c));
                return;
            case ParserRuleContext ctx:
                for (int i = 0; i < ctx.ChildCount; i++) Collect(ctx.GetChild(i), sink);
                return;
        }
    }

    private static List<SymbolNode> CollectNested(IEnumerable<IParseTree> trees)
    {
        var result = new List<SymbolNode>();
        foreach (IParseTree tree in trees) Collect(tree, result);
        return result;
    }

    private static List<SymbolNode> CollectNested(IParseTree tree) => CollectNested(new[] { tree });

    private static IEnumerable<IParseTree> LambdaBody(NeobemParser.Lambda_defContext ctx)
    {
        if (ctx.expression() is { } expr) yield return expr;
        foreach (NeobemParser.Function_statementContext statement in ctx.function_statement()) yield return statement;
    }

    private static string LambdaParams(NeobemParser.Lambda_defContext ctx) =>
        "λ " + string.Join(" ", ctx.IDENTIFIER().Select(i => i.GetText()));

    private static SymbolNode Node(SymbolKind kind, string name, string detail, ParserRuleContext ctx, List<SymbolNode>? children = null) =>
        Node(kind, name, detail, (ctx.Start.Line, ctx.Start.StartIndex, ctx.Stop?.StopIndex ?? ctx.Start.StopIndex), children);

    private static SymbolNode Node(SymbolKind kind, string name, string detail, (int Line, int Start, int Stop) span, List<SymbolNode>? children = null) =>
        new()
        {
            Kind = kind,
            Name = name,
            Detail = detail,
            Line = span.Line,
            StartIndex = span.Start,
            StopIndex = span.Stop,
            Children = children ?? new List<SymbolNode>(),
        };

    // GetText() concatenates tokens with whitespace stripped; this reads the
    // original source span so details render as written.
    private static string SourceText(ParserRuleContext ctx) =>
        ctx.Start.InputStream is { } stream && ctx.Stop is not null
            ? stream.GetText(new Interval(ctx.Start.StartIndex, ctx.Stop.StopIndex))
            : ctx.GetText();

    private static (int, int, int) Span(IToken start, IToken? stop) =>
        (start.Line, start.StartIndex, stop?.StopIndex ?? start.StopIndex);

    private static string StripQuotes(string s)
    {
        s = s.Trim();
        if (s.Length >= 2 && (s[0] == '\'' && s[^1] == '\'' || s[0] == '"' && s[^1] == '"'))
            return s[1..^1];
        return s;
    }

    private static string Truncate(string s, int max)
    {
        s = s.Replace("\r", "").Replace("\n", " ");
        return s.Length <= max ? s : s[..(max - 1)] + "…";
    }
}
