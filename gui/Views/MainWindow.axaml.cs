using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Document;
using gui.Parsing;
using gui.ViewModels;

namespace gui.Views;

public partial class MainWindow : Window
{
    private readonly NeobemColorizer _colorizer = new();
    private MainWindowViewModel? _observedViewModel;

    public MainWindow()
    {
        InitializeComponent();

        SourceBox.TextArea.TextView.LineTransformers.Add(_colorizer);

        // Tunneling, so the editor cannot swallow the shortcuts before the
        // window sees them.
        AddHandler(KeyDownEvent, WindowKeyDown, RoutingStrategies.Tunnel);

        // Hover and the "at caret" pane follow the caret rather than the mouse.
        SourceBox.TextArea.Caret.PositionChanged += (_, _) => ViewModel?.UpdateCaret(SourceBox.CaretOffset);

        DataContextChanged += (_, _) => ObserveViewModel();
        ObserveViewModel();
    }

    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    // The editor is not bound to SourceText: text and highlight spans have to
    // land together, or one repaint runs with the other document's colors.
    private void ObserveViewModel()
    {
        if (_observedViewModel is not null) _observedViewModel.PropertyChanged -= ViewModelPropertyChanged;

        _observedViewModel = ViewModel;
        if (_observedViewModel is null) return;

        _observedViewModel.PropertyChanged += ViewModelPropertyChanged;
        RefreshSource();
    }

    private void ViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SourceText)) RefreshSource();
    }

    private void RefreshSource()
    {
        if (ViewModel is null) return;

        _colorizer.SetSpans(ViewModel.SyntaxSpans);
        if (SourceBox.Text != ViewModel.SourceText) SourceBox.Text = ViewModel.SourceText;
        SourceBox.TextArea.TextView.Redraw();
    }

    private async void OpenClicked(object? sender, RoutedEventArgs e)
    {
        IReadOnlyList<IStorageFile> files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Neobem file",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Neobem files") { Patterns = new[] { "*.nbem" } },
                FilePickerFileTypes.All,
            },
        });

        string? path = files.FirstOrDefault()?.TryGetLocalPath();
        if (path is not null) ViewModel?.LoadFile(path);
    }

    private void ReloadClicked(object? sender, RoutedEventArgs e) => ViewModel?.Reload();

    private void SymbolTreeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SymbolTree.SelectedItem is not SymbolNode node || ViewModel is null) return;
        ViewModel.SelectedSymbol = node;
        HighlightSpan(node.StartIndex, node.StopIndex + 1);
    }

    private void DiagnosticDoubleTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as ListBox)?.SelectedItem is not Diagnostic diagnostic || ViewModel is null) return;
        if (diagnostic.Line < 1 || diagnostic.Line > SourceBox.Document.LineCount) return;

        DocumentLine line = SourceBox.Document.GetLineByNumber(diagnostic.Line);
        int offset = Math.Min(line.Offset + diagnostic.Column, line.EndOffset);
        HighlightSpan(offset, offset);
    }

    // ---- Language features -------------------------------------------------

    private void WindowKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.F12 when e.KeyModifiers.HasFlag(KeyModifiers.Shift):
                FindReferences();
                break;
            case Key.F12:
                GoToDefinition();
                break;
            case Key.Space when e.KeyModifiers.HasFlag(KeyModifiers.Control):
                ShowCompletions();
                break;
            case Key.Escape:
                ViewModel?.ClearResults();
                break;
            default:
                return;
        }

        e.Handled = true;
    }

    private void DefinitionClicked(object? sender, RoutedEventArgs e) => GoToDefinition();

    private void ReferencesClicked(object? sender, RoutedEventArgs e) => FindReferences();

    private void CompletionsClicked(object? sender, RoutedEventArgs e) => ShowCompletions();

    private void CloseResultsClicked(object? sender, RoutedEventArgs e) => ViewModel?.ClearResults();

    private void GoToDefinition()
    {
        SourceSpan? span = ViewModel?.GoToDefinition(SourceBox.CaretOffset);
        if (span is not null) HighlightSpan(span.Start, span.End);
    }

    private void FindReferences() => ViewModel?.FindReferences(SourceBox.CaretOffset);

    private void ShowCompletions() => ViewModel?.ShowCompletions(SourceBox.CaretOffset);

    private void ResultSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if ((sender as ListBox)?.SelectedItem is not LocationResult result) return;
        HighlightSpan(result.Span.Start, result.Span.End);
    }

    private void HighlightSpan(int start, int end)
    {
        int length = SourceBox.Document?.TextLength ?? 0;
        start = Math.Clamp(start, 0, length);
        end = Math.Clamp(end, start, length);

        SourceBox.CaretOffset = start;
        SourceBox.Select(start, end - start);

        TextLocation location = SourceBox.Document!.GetLocation(start);
        SourceBox.ScrollTo(location.Line, location.Column);
    }
}
