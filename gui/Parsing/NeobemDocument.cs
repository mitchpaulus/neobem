using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    // Goto-definition, references, hover and completion, backed by the same
    // code as `nbem --lsp`.
    public LanguageService Language { get; }

    public static NeobemDocument ParseFile(string filePath) =>
        new(filePath, File.ReadAllText(filePath));

    public NeobemDocument(string filePath, string sourceText)
    {
        FilePath = filePath;
        SourceText = sourceText;

        // The LSP document state parses the file and builds the definition,
        // reference and completion indexes in one pass, so the GUI parses once
        // and gets both the structure tree and the language features from it.
        LanguageServer.LoggingEnabled = false;
        LanguageServer.DocumentState state = new(sourceText, DetermineFileType(filePath));

        Diagnostics.AddRange(state.LexerErrors.Select(e => new Diagnostic(e.Line, e.CharPositionInLine, e.Msg, "lexer")));
        Diagnostics.AddRange(state.ParserErrors.Select(e => new Diagnostic(e.Line, e.CharPositionInLine, e.Msg, "parser")));
        if (state.LastParseException is not null)
        {
            Diagnostics.Add(new Diagnostic(1, 0, state.LastParseException.Message, "parser"));
        }

        Symbols = state.ParseTree is null ? new List<SymbolNode>() : StructureWalker.Walk(state.ParseTree);
        Language = new LanguageService(state, sourceText, filePath);
    }

    // Matches the language server's rule, so the GUI lexes a .inp/.bdl file the
    // same way an editor would.
    private static FileType DetermineFileType(string filePath) =>
        Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".bdl" => FileType.Doe2,
            ".inp" => FileType.Doe2,
            _ => FileType.Idf,
        };
}
