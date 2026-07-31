using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using gui.Parsing;

namespace gui.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private string? _filePath;
    [ObservableProperty] private string _sourceText = "";
    [ObservableProperty] private string _filterText = "";
    [ObservableProperty] private SymbolNode? _selectedSymbol;
    [ObservableProperty] private string _statusText = "Open a .nbem file to begin.";

    [ObservableProperty] private IReadOnlyList<SyntaxSpan> _syntaxSpans = Array.Empty<SyntaxSpan>();
    [ObservableProperty] private string _resultsTitle = "";
    [ObservableProperty] private string _hoverMarkdown = "";
    [ObservableProperty] private string _caretInfo = "";

    public ObservableCollection<SymbolNode> RootSymbols { get; } = new();
    public ObservableCollection<Diagnostic> Diagnostics { get; } = new();
    public bool HasDiagnostics => Diagnostics.Count > 0;

    // Goto-definition / find-references hits and completion candidates for the
    // current caret position. Only one of the two is populated at a time.
    public ObservableCollection<LocationResult> Results { get; } = new();
    public ObservableCollection<CompletionEntry> Completions { get; } = new();
    public bool HasResults => Results.Count > 0;
    public bool HasCompletions => Completions.Count > 0;
    public bool HasResultsPane => HasResults || HasCompletions;
    public bool HasHover => HoverMarkdown.Length > 0;

    private List<SymbolNode> _allSymbols = new();
    private LanguageService? _language;
    private FileSystemWatcher? _watcher;
    private DispatcherTimer? _reloadDebounce;

    public void LoadFile(string path)
    {
        FilePath = path;
        Reload();
        WatchFile(path);
    }

    public void Reload()
    {
        if (FilePath is null) return;
        try
        {
            NeobemDocument doc = NeobemDocument.ParseFile(FilePath);
            // Spans first: the view repaints the editor when SourceText changes
            // and reads the spans for the new text at that point.
            SyntaxSpans = doc.SyntaxSpans;
            SourceText = doc.SourceText;
            _allSymbols = doc.Symbols;
            _language = doc.Language;

            // Offsets in the old text mean nothing after a reload.
            ClearResults();

            Diagnostics.Clear();
            foreach (Diagnostic diagnostic in doc.Diagnostics) Diagnostics.Add(diagnostic);
            OnPropertyChanged(nameof(HasDiagnostics));

            ApplyFilter();
            StatusText = $"{Path.GetFileName(FilePath)} — {CountNodes(_allSymbols)} symbols" +
                         (doc.Diagnostics.Count > 0 ? $", {doc.Diagnostics.Count} parse errors" : "") +
                         $" — reloaded {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception e)
        {
            StatusText = $"Failed to load {FilePath}: {e.Message}";
        }
    }

    partial void OnFilterTextChanged(string value) => ApplyFilter();

    // ---- Language features -------------------------------------------------
    // All of these take a char offset into SourceText (the source pane's caret)
    // and delegate to the same language service that backs `nbem --lsp`.

    // Returns the span to navigate to when there is exactly one definition;
    // with several, they land in the results pane for the user to pick.
    public SourceSpan? GoToDefinition(int offset)
    {
        if (_language is null) return null;
        ClearResults();

        IReadOnlyList<LocationResult> definitions = _language.FindDefinitions(offset);
        if (definitions.Count == 0)
        {
            StatusText = $"No definition found for {DescribeCaret(offset)}.";
            return null;
        }

        if (definitions.Count == 1)
        {
            StatusText = $"Definition: line {definitions[0].Span.Line}.";
            return definitions[0].Span;
        }

        ShowResults($"{definitions.Count} definitions", definitions);
        return definitions[0].Span;
    }

    public void FindReferences(int offset)
    {
        if (_language is null) return;
        ClearResults();

        IReadOnlyList<LocationResult> references = _language.FindReferences(offset, includeDeclaration: true);
        if (references.Count == 0)
        {
            StatusText = $"No references found for {DescribeCaret(offset)}.";
            return;
        }

        ShowResults($"{references.Count} references", references);
    }

    // The source pane is read-only, so completions are shown as the set of
    // values EnergyPlus accepts in the field under the caret rather than as an
    // insertion popup.
    public void ShowCompletions(int offset)
    {
        if (_language is null) return;
        ClearResults();

        IReadOnlyList<CompletionEntry> completions = _language.FindCompletions(offset);
        if (completions.Count == 0)
        {
            StatusText = $"No completions available at {DescribeCaret(offset)}.";
            return;
        }

        foreach (CompletionEntry entry in completions) Completions.Add(entry);
        ResultsTitle = $"{completions.Count} valid values";
        RaiseResultsChanged();
        StatusText = ResultsTitle + " for the field under the caret.";
    }

    // Hover is driven by the caret rather than the mouse: the source pane is a
    // plain TextBox with no character hit-testing.
    public void UpdateCaret(int offset)
    {
        if (_language is null) return;

        HoverInfo? hover = _language.FindHover(offset);
        HoverMarkdown = hover?.Markdown ?? "";
        OnPropertyChanged(nameof(HasHover));
        CaretInfo = DescribeCaret(offset);
    }

    private string DescribeCaret(int offset)
    {
        if (_language is null) return "";
        (int line, int character) = _language.ToPosition(offset);
        Antlr4.Runtime.IToken? token = _language.FindToken(offset);
        string where = $"line {line + 1}:{character + 1}";
        return token is null ? where : $"{where} ({LanguageService.DescribeTokenType(token)} '{token.Text?.Trim()}')";
    }

    private void ShowResults(string title, IEnumerable<LocationResult> results)
    {
        foreach (LocationResult result in results) Results.Add(result);
        ResultsTitle = title;
        RaiseResultsChanged();
        StatusText = title + ".";
    }

    public void ClearResults()
    {
        Results.Clear();
        Completions.Clear();
        ResultsTitle = "";
        RaiseResultsChanged();
    }

    private void RaiseResultsChanged()
    {
        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(HasCompletions));
        OnPropertyChanged(nameof(HasResultsPane));
    }

    private void ApplyFilter()
    {
        RootSymbols.Clear();
        foreach (SymbolNode node in _allSymbols)
        {
            SymbolNode? filtered = Filter(node, FilterText.Trim());
            if (filtered is not null) RootSymbols.Add(filtered);
        }
    }

    // A node matching the filter keeps its whole subtree; a non-matching node
    // survives only if some descendant matches.
    private static SymbolNode? Filter(SymbolNode node, string filter)
    {
        if (filter.Length == 0) return node;
        bool selfMatch = node.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                         node.Detail.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                         node.Glyph.Contains(filter, StringComparison.OrdinalIgnoreCase);
        if (selfMatch) return node;

        List<SymbolNode> children = node.Children
            .Select(c => Filter(c, filter))
            .Where(c => c is not null)
            .Cast<SymbolNode>()
            .ToList();
        if (children.Count == 0) return null;

        return new SymbolNode
        {
            Kind = node.Kind,
            Name = node.Name,
            Detail = node.Detail,
            Line = node.Line,
            StartIndex = node.StartIndex,
            StopIndex = node.StopIndex,
            Children = children,
        };
    }

    private static int CountNodes(IEnumerable<SymbolNode> nodes) =>
        nodes.Sum(n => 1 + CountNodes(n.Children));

    // Auto-reload when the file changes on disk, so the GUI acts as a live
    // outline next to a real editor. Watching can fail on some filesystems
    // (e.g. network mounts) - in that case manual Reload still works.
    private void WatchFile(string path)
    {
        _watcher?.Dispose();
        _watcher = null;
        try
        {
            string? dir = Path.GetDirectoryName(path);
            if (dir is null) return;
            _watcher = new FileSystemWatcher(dir, Path.GetFileName(path))
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
                EnableRaisingEvents = true,
            };
            _watcher.Changed += OnFileChanged;
            _watcher.Created += OnFileChanged;
            _watcher.Renamed += OnFileChanged;
        }
        catch (Exception)
        {
            _watcher = null;
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _reloadDebounce ??= new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _reloadDebounce.Tick -= ReloadTick;
            _reloadDebounce.Tick += ReloadTick;
            _reloadDebounce.Stop();
            _reloadDebounce.Start();
        });
    }

    private void ReloadTick(object? sender, EventArgs e)
    {
        _reloadDebounce?.Stop();
        Reload();
    }
}
