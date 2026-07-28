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

    public ObservableCollection<SymbolNode> RootSymbols { get; } = new();
    public ObservableCollection<Diagnostic> Diagnostics { get; } = new();
    public bool HasDiagnostics => Diagnostics.Count > 0;

    private List<SymbolNode> _allSymbols = new();
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
            SourceText = doc.SourceText;
            _allSymbols = doc.Symbols;

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
