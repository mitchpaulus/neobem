using System.Collections.Generic;

namespace gui.Parsing;

public enum SymbolKind
{
    Object,
    Doe2Object,
    IdfPlusObject,
    Variable,
    Function,
    Import,
    Export,
    Print,
    Log,
    Let,
    Return,
}

// One node in the source structure tree. Spans are 0-based char offsets into
// the original source text; Line is 1-based to match ANTLR/editor conventions.
public class SymbolNode
{
    public SymbolKind Kind { get; init; }
    public string Name { get; init; } = "";
    public string Detail { get; init; } = "";
    public int Line { get; init; }
    public int StartIndex { get; init; }
    public int StopIndex { get; init; }
    public List<SymbolNode> Children { get; init; } = new();

    public string Glyph => Kind switch
    {
        SymbolKind.Object => "OBJ",
        SymbolKind.Doe2Object => "DOE2",
        SymbolKind.IdfPlusObject => "{ }",
        SymbolKind.Variable => "VAR",
        SymbolKind.Function => "FN",
        SymbolKind.Import => "IMP",
        SymbolKind.Export => "EXP",
        SymbolKind.Print => "PRN",
        SymbolKind.Log => "LOG",
        SymbolKind.Let => "LET",
        SymbolKind.Return => "RET",
        _ => "?",
    };
}
