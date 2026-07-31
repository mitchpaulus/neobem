using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using src;
using LspLocation = Microsoft.VisualStudio.LanguageServer.Protocol.Location;

namespace gui.Parsing;

// A span of the source document expressed in char offsets (what the source
// TextBox works in) plus the 1-based line/column (what humans read).
public record SourceSpan(int Start, int End, int Line, int Column);

// A goto-definition / find-references hit, with the source line it came from
// so the results list is readable without jumping to each one.
public record LocationResult(SourceSpan Span, string Preview)
{
    public string Display => $"Line {Span.Line}:{Span.Column}  {Preview}";
}

public record CompletionEntry(string Label, string Detail)
{
    public string Display => string.IsNullOrEmpty(Detail) ? Label : $"{Label}  —  {Detail}";
}

public record HoverInfo(SourceSpan Span, string Markdown);

// Wraps the language-service logic that backs `nbem --lsp` so the GUI can call
// it in-process. The LSP handlers in src/Lsp.cs are only JSON-RPC plumbing over
// LanguageServer.DocumentState, so everything here is the same code path the
// editor integrations use - the difference is char offsets in, char offsets out,
// instead of LSP line/character positions.
public sealed class LanguageService
{
    private readonly LanguageServer.DocumentState _document;
    private readonly string _text;
    private readonly Uri _uri;

    // Offset of the start of each line, so offset <-> (line, character)
    // conversion is a binary search rather than a scan of the whole document.
    private readonly int[] _lineStarts;

    internal LanguageService(LanguageServer.DocumentState document, string text, string filePath)
    {
        _document = document;
        _text = text;
        _uri = ToUri(filePath);
        _lineStarts = ComputeLineStarts(text);
    }

    public IReadOnlyList<LocationResult> FindDefinitions(int offset)
    {
        (int line, int character) = ToPosition(offset);
        return _document.FindDefinitions(_uri, line, character).Select(ToResult).ToList();
    }

    public IReadOnlyList<LocationResult> FindReferences(int offset, bool includeDeclaration = true)
    {
        (int line, int character) = ToPosition(offset);
        return _document.FindReferences(_uri, line, character, includeDeclaration).Select(ToResult).ToList();
    }

    public IReadOnlyList<CompletionEntry> FindCompletions(int offset)
    {
        (int line, int character) = ToPosition(offset);
        return _document.FindCompletions(line, character)
            .Select(item => new CompletionEntry(item.Label ?? "", item.Detail ?? ""))
            .ToList();
    }

    public HoverInfo? FindHover(int offset)
    {
        IToken? token = FindToken(offset);
        if (token is null) return null;

        string? markdown = LanguageServer.TryGetBuiltInHoverMarkdown(token.Text);
        if (markdown is null) return null;

        return new HoverInfo(SpanOfToken(token), markdown);
    }

    // The token under the caret, used for the status line and to tell the user
    // why a request came back empty.
    public IToken? FindToken(int offset)
    {
        (int line, int character) = ToPosition(offset);
        return _document.FindToken(line, character);
    }

    public static string DescribeTokenType(IToken token) =>
        NeobemLexer.DefaultVocabulary.GetSymbolicName(token.Type) ?? token.Type.ToString();

    private LocationResult ToResult(LspLocation location) =>
        new(ToSpan(location.Range), LineTextAt(location.Range.Start.Line));

    private SourceSpan ToSpan(Microsoft.VisualStudio.LanguageServer.Protocol.Range range)
    {
        int start = ToOffset(range.Start.Line, range.Start.Character);
        int end = ToOffset(range.End.Line, range.End.Character);
        return new SourceSpan(start, Math.Max(start, end), range.Start.Line + 1, range.Start.Character + 1);
    }

    private SourceSpan SpanOfToken(IToken token) =>
        new(token.StartIndex, token.StopIndex + 1, token.Line, token.Column + 1);

    private string LineTextAt(int zeroBasedLine)
    {
        if (zeroBasedLine < 0 || zeroBasedLine >= _lineStarts.Length) return "";
        int start = _lineStarts[zeroBasedLine];
        int end = zeroBasedLine + 1 < _lineStarts.Length ? _lineStarts[zeroBasedLine + 1] : _text.Length;
        return _text[start..end].TrimEnd('\n', '\r').Trim();
    }

    // ANTLR counts lines on '\n' only and columns in chars since that newline,
    // so the conversions here have to do the same or spans land off by the '\r'
    // count on CRLF files.
    private static int[] ComputeLineStarts(string text)
    {
        List<int> starts = new() { 0 };
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n') starts.Add(i + 1);
        }
        return starts.ToArray();
    }

    public (int Line, int Character) ToPosition(int offset)
    {
        offset = Math.Clamp(offset, 0, _text.Length);
        int index = Array.BinarySearch(_lineStarts, offset);
        int line = index >= 0 ? index : ~index - 1;
        return (line, offset - _lineStarts[line]);
    }

    public int ToOffset(int zeroBasedLine, int zeroBasedCharacter)
    {
        if (zeroBasedLine < 0) return 0;
        if (zeroBasedLine >= _lineStarts.Length) return _text.Length;

        int lineEnd = zeroBasedLine + 1 < _lineStarts.Length ? _lineStarts[zeroBasedLine + 1] : _text.Length;
        return Math.Clamp(_lineStarts[zeroBasedLine] + zeroBasedCharacter, 0, lineEnd);
    }

    private static Uri ToUri(string filePath)
    {
        try
        {
            return new Uri(System.IO.Path.GetFullPath(filePath));
        }
        catch (Exception)
        {
            // Unsaved or odd paths still need *some* uri - definitions are
            // all within the one document, so its exact value does not matter.
            return new Uri("untitled:document");
        }
    }
}
