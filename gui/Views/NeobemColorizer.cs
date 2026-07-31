using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using gui.Parsing;

namespace gui.Views;

// Paints the source editor from lexer token spans. AvaloniaEdit asks for one
// visible line at a time, so this keeps the spans sorted by start offset and
// binary searches into them rather than rescanning the document.
public sealed class NeobemColorizer : DocumentColorizingTransformer
{
    private IReadOnlyList<SyntaxSpan> _spans = Array.Empty<SyntaxSpan>();
    private int[] _starts = Array.Empty<int>();

    public void SetSpans(IReadOnlyList<SyntaxSpan> spans)
    {
        _spans = spans;
        _starts = new int[spans.Count];
        for (int i = 0; i < spans.Count; i++) _starts[i] = spans[i].Start;
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        if (_spans.Count == 0 || line.Length == 0) return;

        bool dark = Application.Current?.ActualThemeVariant == ThemeVariant.Dark;

        // First span that could touch this line. Spans never overlap, so the
        // one starting at or before the line start is the only earlier
        // candidate - a multi-line string or comment, say.
        int index = Array.BinarySearch(_starts, line.Offset);
        if (index < 0) index = Math.Max(~index - 1, 0);

        for (; index < _spans.Count; index++)
        {
            SyntaxSpan span = _spans[index];
            if (span.Start >= line.EndOffset) break;
            if (span.End <= line.Offset) continue;

            IBrush brush = BrushFor(span.Category, dark);
            ChangeLinePart(
                Math.Max(span.Start, line.Offset),
                Math.Min(span.End, line.EndOffset),
                element => element.TextRunProperties.SetForegroundBrush(brush));
        }
    }

    private static readonly Dictionary<TokenCategory, IBrush> LightBrushes = CreateBrushes(
        comment: "#6A737D",
        keyword: "#AF00DB",
        constant: "#0F7B6C",
        number: "#098658",
        text: "#A31515",
        objectType: "#0550AE",
        field: "#0A3069",
        builtIn: "#795E26",
        op: "#5C5C5C");

    private static readonly Dictionary<TokenCategory, IBrush> DarkBrushes = CreateBrushes(
        comment: "#7F8C98",
        keyword: "#C586C0",
        constant: "#4EC9B0",
        number: "#B5CEA8",
        text: "#CE9178",
        objectType: "#569CD6",
        field: "#9CDCFE",
        builtIn: "#DCDCAA",
        op: "#B0B0B0");

    private static IBrush BrushFor(TokenCategory category, bool dark)
    {
        Dictionary<TokenCategory, IBrush> palette = dark ? DarkBrushes : LightBrushes;
        return palette.TryGetValue(category, out IBrush? brush) ? brush : Brushes.Gray;
    }

    private static Dictionary<TokenCategory, IBrush> CreateBrushes(
        string comment,
        string keyword,
        string constant,
        string number,
        string text,
        string objectType,
        string field,
        string builtIn,
        string op)
    {
        return new Dictionary<TokenCategory, IBrush>
        {
            [TokenCategory.Comment] = Parse(comment),
            [TokenCategory.Keyword] = Parse(keyword),
            [TokenCategory.Constant] = Parse(constant),
            [TokenCategory.Number] = Parse(number),
            [TokenCategory.String] = Parse(text),
            [TokenCategory.ObjectType] = Parse(objectType),
            [TokenCategory.Field] = Parse(field),
            [TokenCategory.BuiltIn] = Parse(builtIn),
            [TokenCategory.Operator] = Parse(op),
        };
    }

    private static IBrush Parse(string hex) => new SolidColorBrush(Color.Parse(hex)).ToImmutable();
}
