using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using src;

namespace gui.Parsing;

public record Diagnostic(int Line, int Column, string Message, string Source)
{
    public string Display => $"[{Source}] Line {Line}:{Column} {Message}";
}

// Result of parsing a single .nbem file: source text, structure tree, and any
// lexer/parser diagnostics. Parsing never throws on bad input - whatever
// parses shows up in Symbols, the rest shows up in Diagnostics.
public class NeobemDocument
{
    public string FilePath { get; }
    public string SourceText { get; }
    public List<Diagnostic> Diagnostics { get; } = new();
    public List<SymbolNode> Symbols { get; private set; } = new();

    public static NeobemDocument ParseFile(string filePath) =>
        new(filePath, File.ReadAllText(filePath));

    public NeobemDocument(string filePath, string sourceText)
    {
        FilePath = filePath;
        SourceText = sourceText;

        var lexer = new NeobemLexer(new AntlrInputStream(sourceText)) { FileType = FileType.Idf };
        lexer.RemoveErrorListeners();
        var lexerErrors = new SimpleAntlrErrorListener();
        lexer.AddErrorListener(lexerErrors);

        var tokens = new CommonTokenStream(lexer);
        var parser = new NeobemParser(tokens);
        parser.RemoveErrorListeners();
        var parserErrors = new SimpleAntlrErrorListener();
        parser.AddErrorListener(parserErrors);

        NeobemParser.IdfContext tree = parser.idf();

        Diagnostics.AddRange(lexerErrors.Errors.Select(e => new Diagnostic(e.Line, e.CharPositionInLine, e.Msg, "lexer")));
        Diagnostics.AddRange(parserErrors.Errors.Select(e => new Diagnostic(e.Line, e.CharPositionInLine, e.Msg, "parser")));

        Symbols = StructureWalker.Walk(tree);
    }
}
